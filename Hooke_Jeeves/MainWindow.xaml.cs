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
        delegate void update_plot();
        public double f(double x, double y)
        {
            try
            {
                return (double)function_textbox.Text.Substitute("x", x).Substitute("y", y).EvalNumerical();
            }
            catch(Exception)
            {
                return 10000000;
            }
        }

        public double Coef_a()
        {
            try
            {
                return (double)coef_a_textbox.Text.EvalNumerical();
            }
            catch (Exception)
            {
                return 2;
            }
        }
        public double Coef_b()
        {
            try
            {
                return (double)coef_b_textbox.Text.EvalNumerical();
            }
            catch (Exception)
            {
                return 2;
            }
        }
        public double Coef_e()
        {
            try
            {
                return (double)coef_e_textbox.Text.EvalNumerical();
            }
            catch (Exception)
            {
                return 0.1;
            }
        }
        public int Max_iter()
        {
            try
            {
                return (int)iter_max_textbox.Text.EvalNumerical();
            }
            catch (Exception)
            {
                return 100;
            }
        }
        public double[] Coef_h()
        {
            double[] coefh = { 1, 1 };
            try
            {
                coefh[0] = (double)hx_textbox.Text.EvalNumerical();
                coefh[1] = (double)hy_textbox.Text.EvalNumerical();
                return coefh;
            }
            catch (Exception)
            {
                return coefh;
            }
        }
        public double[] Zero_point()
        {
            double[] pointzero = { 0, 0 };
            try
            {
                pointzero[0] = (double)x0_textbox.Text.EvalNumerical();
                pointzero[1] = (double)y0_textbox.Text.EvalNumerical();
                return pointzero;
            }
            catch (Exception)
            {
                return pointzero;
            }
        }

        public void Add_heat_map()
        {
            int width = (int)map_size.Value;
            int height = width;

            double[,] intensities = new double[width*2, height*2];

            for (int x = 0; x < width*2; x++)
                for (int y = 0; y < height*2; y++)
                    intensities[height*2-y-1, x] = f(x - width, y - height);

            var hm = Plot.Plot.AddHeatmap(intensities);//, ScottPlot.Drawing.Colormap.Turbo
            hm.OffsetX = -width;
            hm.OffsetY = -height;
            hm.UseParallel = true; //должно ускорить работу
            Plot.Plot.AddColorbar(hm);

            Plot.Plot.SetAxisLimits(-width*1.05, width * 1.05, -height * 1.05, height * 1.05);
            Plot.Refresh();
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
        public void add_cross(double[]  point, double[] h)
        {
            List<double> crossX = new List<double>();
            List<double> crossY = new List<double>();

            crossX.Add(point[0] - h[0]);
            crossY.Add(point[1]);
            crossX.Add(point[0] + h[0]);
            crossY.Add(point[1]);
            crossX.Add(double.NaN);
            crossY.Add(double.NaN);
            crossX.Add(point[0]);
            crossY.Add(point[1] - h[1]);
            crossX.Add(point[0]);
            crossY.Add(point[1] + h[1]);

            var sp = Plot.Plot.AddScatter(crossX.ToArray(), crossY.ToArray(), lineStyle: LineStyle.Dot, lineWidth: 2);
            sp.OnNaN = ScottPlot.Plottable.ScatterPlot.NanBehavior.Gap;
            crossX.Clear();
            crossY.Clear();
        }
        public void add_label(double[] point, int iter)
        {
            var marker = Plot.Plot.AddMarker(point[0], point[1], color: System.Drawing.Color.FromName("Transparent"));
            marker.Text = iter.ToString();
            marker.TextFont.Alignment = Alignment.UpperCenter;
            marker.TextFont.Color = System.Drawing.Color.FromName("SandyBrown");
            marker.TextFont.Size = 16;
        }


        public void hook()
        {

            double a = Coef_a(), b = Coef_b(), epsilon = Coef_e();
            double[] h = Coef_h();
            double[] point1 = Zero_point();
            double[] point2 = Zero_point();
            double[] point3 = Zero_point();
            double[] point3_old = Zero_point();
            double[] point3_new = Zero_point();

            double f0, f_left, f_right, f_top, f_bottom;

            List<double> dataX = new List<double>();
            List<double> dataY = new List<double>();
            List<double> crossX = new List<double>();
            List<double> crossY = new List<double>();

            int iter = 1, maxiter = Max_iter();

            add_cross(point3, h);
            add_label(point3, iter);

            while (h[1] > epsilon)
            {
                iter++;
                //Dispatcher.BeginInvoke(new update_plot(Plot.RefreshUI), new object {});
                TextBox_out.Text += "\n\n";

                f0 = f(point3[0], point3[1]);
                f_left = f(point3[0] - h[0], point3[1]);
                f_right = f(point3[0] + h[0], point3[1]);
                f_top = f(point3[0], point3[1] + h[1]);
                f_bottom = f(point3[0], point3[1] - h[1]);

                TextBox_out.Text += "\nИтерация " + iter;
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

                point3_new[0] = (point2[0] - point1[0]) * b + point1[0];
                point3_new[1] = (point2[1] - point1[1]) * b + point1[1];

                if (point2[0] == point3[0] && point2[1] == point3[1])
                {
                    TextBox_out.Text += "не найдено";
                    h[0] /= a;
                    h[1] /= a;
                }
                else
                if (f(point2[0], point2[1]) > f(point1[0], point1[1]))
                {
                    TextBox_out.Text += "\nНеверное направление - функция выросла";
                    Plot.Plot.AddArrow(point3_old[0], point3_old[1], point3[0], point3[1], lineWidth: 1, color: System.Drawing.Color.FromArgb(255, 255, 0, 0));

                    point3[0] = point3_old[0];
                    point3[1] = point3_old[1];
                    point1[0] = point3_old[0];
                    point1[1] = point3_old[1];

                    point2[0] = point3[0];
                    point2[1] = point3[1];

                    add_cross(point3, h);

                    Plot.Refresh();
                    
                }
                else
                {
                    TextBox_out.Text += "\nТочка минимума в окрестности ( " + point2[0] + ", " + point2[1] + " )";
                    TextBox_out.Text += "\nПредыдущая точка ( " + point1[0] + ", " + point1[1] + " )";
                    TextBox_out.Text += "\nНовая точка ( " + point3_new[0] + ", " + point3_new[1] + " )";

                    point3_old[0] = point3[0];
                    point3_old[1] = point3[1];
                    point3[0] = point3_new[0];
                    point3[1] = point3_new[1];

                    dataX.Add(point3[0]);
                    dataY.Add(point3[1]);

                    Plot.Plot.AddArrow(point3[0], point3[1], point1[0], point1[1], lineWidth: 2, color: System.Drawing.Color.FromArgb(255,0,200,200));

                    point1[0] = point2[0];
                    point1[1] = point2[1];

                    Plot.Plot.AddArrow(point2[0], point2[1], point3_old[0], point3_old[1], lineWidth: 2, color: System.Drawing.Color.FromName("SandyBrown"));

                    point2[0] = point3[0];
                    point2[1] = point3[1];

                    add_cross(point3, h);
                    add_label(point3, iter);

                    Plot.Refresh();
                }

                progress.Value = Math.Log2(1 / h[1]) / Math.Log2(1 / epsilon) * 100;
                Plot.RefreshUI();
                progress.RefreshUI();
                //Thread.Sleep(200);

                if (iter == maxiter)
                    break;
            }
            Plot.Refresh();

            min_textbox.Text = Math.Round(f(point3[0], point3[1]),(int)Math.Log10(1/epsilon)+1).ToString();
            xmin_textbox.Text = Math.Round(point3[0], (int)Math.Log10(1 / epsilon) + 1).ToString();
            ymin_textbox.Text = Math.Round(point3[0], (int)Math.Log10(1 / epsilon) + 1).ToString();
            iter_textbox.Text = iter.ToString();
            return;
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            progress.Value = 0;
            TextBox_out.Text = "";
            Plot.Plot.Clear();
            Add_heat_map();
        }

        parameters parameter = new parameters();

        public void set_params_from_ui()
        {
            parameter.a = Coef_a();
            parameter.b = Coef_b();
            parameter.e = Coef_e();
            parameter.h = Coef_h();
            parameter.point0 = Zero_point();
            parameter.function = function_textbox.Text;
        }

        public void set_ui_from_params()
        {
            x0_textbox.Text = parameter.x0_string();
            y0_textbox.Text = parameter.y0_string();
            hx_textbox.Text = parameter.hx_string();
            hy_textbox.Text = parameter.hy_string();
            coef_a_textbox.Text = parameter.a_string();
            coef_b_textbox.Text = parameter.b_string();
            coef_e_textbox.Text = parameter.e_string();
            function_textbox.Text = parameter.function;
        }

        private void btn_open_Click(object sender, RoutedEventArgs e)
        {
            TextRange range;
            FileStream fStream;
            DateTime date = new DateTime();
            string _fileName = date.ToString("HH_mm__dd_MM_yyyy") + "__Hooke_Jeeves_params";

            set_ui_from_params();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            set_params_from_ui();
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yaml = serializer.Serialize(parameter);
            DateTime date = new DateTime();
            string _fileName = date.ToString("HH_mm__dd_MM_yyyy") + "__Hooke_Jeeves_params";
            string path = @"C:\Users\uwm16\";
            File.WriteAllText(path + _fileName+".txt", yaml);
        }

    }
}
