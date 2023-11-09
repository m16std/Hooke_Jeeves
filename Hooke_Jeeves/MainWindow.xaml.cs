using AngouriMath.Extensions;
using ScottPlot;
using ScottPlot.MarkerShapes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using YamlDotNet.Core;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using static AngouriMath.Entity;
using Microsoft.Win32;
using System.Runtime.Serialization.Json;
using System.Data.Common;
using static System.Net.WebRequestMethods;
using System.Linq;


namespace Hooke_Jeeves
{
    public static class ExtensionMethods
    {

        private static Action EmptyDelegate = delegate () { };


        public static void RefreshUI(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, delegate () { });
            //uiElement.Dispatcher.Invoke(delegate () { }, DispatcherPriority.Render);
        }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
       


        public void hook()
        {
            double a = Coef_a(), b = Coef_b(), epsilon = Coef_e();
            double[] h = Coef_h();
            double[] point1 = Zero_point(), point2 = Zero_point(), point3 = Zero_point(), point3_old = Zero_point();
            int iter = 1, maxiter = Max_iter();
            Add_label(point3, iter);

            while (h[1] > epsilon)
            {
                Add_cross(point3, h);

                Exploratory_search(ref point2, point3, h, iter);

                if (point2 == point3)   //Поиск завершился неудачей
                {
                    Constriction(ref h, a);
                }
                else
                if (f(point2) > f(point1)) //Найденная точка хуже старой
                {
                    Step_back(ref point1, ref point2, ref point3, point3_old);
                }
                else                                                    //Найденная точка лучше предыдущей
                {
                    Pattern_search(ref point1, ref point2, ref point3, ref point3_old, ref h, a, b, iter);
                }

                progress.Value = Math.Log2(1 / h[1]) / Math.Log2(1 / epsilon) * 100;
                Plot.Refresh();
                Plot.RefreshUI();
                progress.RefreshUI();
                //Thread.Sleep(200);

                if (iter == maxiter)
                    break;

                iter++;
            }
            Set_result(point3, epsilon, iter);
            return;
        }
        public void Exploratory_search(ref double[] point2, double[] point3, double[] h, int iter)
        {
            double f0, f_left, f_right, f_top, f_bottom;

            //Dispatcher.BeginInvoke(new update_plot(Plot.RefreshUI), new object {});

            f0 = f(point3);
            f_left = f(point3[0] - h[0], point3[1]);
            f_right = f(point3[0] + h[0], point3[1]);
            f_top = f(point3[0], point3[1] + h[1]);
            f_bottom = f(point3[0], point3[1] - h[1]);

            TextBox_out.Text += "\n\n\nИтерация " + iter;
            TextBox_out.Text += "\nТекущая точка    ( " + point3[0] + ", " + point3[1] + " )";
            TextBox_out.Text += "\nДистанция поиска ( " + h[0] + ", " + h[1] + " )";
            TextBox_out.Text += "\nЗначение функции в точке: " + f0;
            TextBox_out.Text += "\nЛев: " + f_left + "  Прав: " + f_right + "  Верх: " + f_top + "  Низ: " + f_bottom + " - Значение ";
            TextBox_out.Text += "\nЛев: " + (f_left - f0).ToString() + "  Прав: " + (f_right - f0).ToString() + "  Верх: " + (f_top - f0).ToString() + "  Низ: " + (f_bottom - f0).ToString() + " - Изменение";

            TextBox_out.Text += "\nВыбранное направление: ";

            if (f_top < f0)
            {
                point2[1] = point3[1] + h[1];
                TextBox_out.Text += "вверх ";
            }
            if (f_bottom < f0)
            {
                point2[1] = point3[1] - h[1];
                TextBox_out.Text += "вниз ";
            }
            if (f_left < f0)
            {
                point2[0] = point3[0] - h[0];
                TextBox_out.Text += "влево ";
            }
            if (f_right < f0)
            {
                point2[0] = point3[0] + h[0];
                TextBox_out.Text += "вправо ";
            }
            TextBox_out.Text += "\nТочка минимума в окрестности ( " + point2[0] + ", " + point2[1] + " )";
        }
        public void Pattern_search(ref double[] point1, ref double[] point2, ref double[] point3, ref double[] point3_old, ref double[] h, double a, double b, int iter)
        {
            double[] point3_new = new double[2];
            //Поиск по образцу
            point3_new[0] = (point2[0] - point1[0]) * b + point1[0];
            point3_new[1] = (point2[1] - point1[1]) * b + point1[1];

            
            TextBox_out.Text += "\nПредыдущая точка ( " + point1[0] + ", " + point1[1] + " )";
            TextBox_out.Text += "\nНовая точка ( " + point3_new[0] + ", " + point3_new[1] + " )";

            if (Enumerable.SequenceEqual(point3_new, point3_old) || Enumerable.SequenceEqual(point3_new, point3)) //Защита от вхождения в замкнутый цикл
            {
                TextBox_out.Text += "\nВхождение в замкнутый цикл - уменьшаем шаг";
                double[] h_new = new double[2];
                h_new = h;
                Constriction(ref h_new, a);
                h = h_new;
                return;
            }

            //Стелка из старой точки в новую
            Plot.Plot.AddArrow(point3_new[0], point3_new[1], point1[0], point1[1], lineWidth: 2, color: System.Drawing.Color.FromArgb(255, 0, 200, 200));
            //Стрелка - указатель направления минимума
            Plot.Plot.AddArrow(point2[0], point2[1], point3[0], point3[1], lineWidth: 2, color: System.Drawing.Color.FromName("SandyBrown"));
            //Указываем номер итерации
            Add_label(point3_new, iter);

            point3_old[0] = point3[0];
            point3_old[1] = point3[1];
            point3[0] = point3_new[0];
            point3[1] = point3_new[1];
            point1[0] = point2[0];
            point1[1] = point2[1];
            point2[0] = point3[0];
            point2[1] = point3[1];
        }
        public void Step_back(ref double[] point1, ref double[] point2, ref double[] point3, double[] point3_old)
        {
            TextBox_out.Text += "\nНеверное направление - функция выросла";
            //стрелка из текущей точки в предыдущую
            Plot.Plot.AddArrow(point3_old[0], point3_old[1], point3[0], point3[1], lineWidth: 1, color: System.Drawing.Color.FromArgb(255, 255, 0, 0));

            point3[0] = point3_old[0];
            point3[1] = point3_old[1];
            point1[0] = point3_old[0];
            point1[1] = point3_old[1];
            point2[0] = point3_old[0];
            point2[1] = point3_old[1];
        }
        public void Constriction(ref double[] h, double a)
        {
            TextBox_out.Text += "не найдено";
            h[0] /= a;
            h[1] /= a;
        }




        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            progress.Value = 0;
            Plot.Plot.Clear();
            TextBox_out.Text = "";
            try
            {
                Add_heat_map();
                hook();
            }
            catch (Exception)
            {

            }
        }
        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            progress.Value = 0;
            TextBox_out.Text = "";
            Plot.Plot.Clear();
            try
            {
                Add_heat_map();
            }
            catch (Exception)
            {

            }
        }
        private void Btn_open_Click(object sender, RoutedEventArgs e)
        {
            var yalm = @"";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                yalm = System.IO.File.ReadAllText(openFileDialog.FileName);

            var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var parameter = deserializer.Deserialize<parameters>(yalm);
            Set_ui_from_params(parameter);
        }
        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            parameters parameter = new parameters();
            Set_params_from_ui(parameter);

            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yaml = serializer.Serialize(parameter);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "Text file (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
                System.IO.File.WriteAllText(saveFileDialog.FileName, yaml);
        }
    }
}
