using ScottPlot;
using ScottPlot.MarkerShapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Hooke_Jeeves
{
    public partial class MainWindow : Window
    {
        public void hook()
        {
            stop_alog = false;
            double a = Coef_a(), b = Coef_b(), epsilon = Coef_e();
            double[] h = Coef_h();
            double[] point1 = Zero_point(), point2 = Zero_point();
            int iter = 0, maxiter = Max_iter();

            while (h[0] > epsilon || h[1] > epsilon)
            {
                iter++;
                Add_label(point2, iter);   //Указываем номер итерации

                Exploratory_search(ref point2, h, iter, a);

                if (f(point2) > f(point1)) //Найденная точка хуже старой
                {
                    Step_back(ref point1, ref point2, iter);
                }
                if (f(point2) < f(point1)) //Найденная точка лучше предыдущей
                {
                    Pattern_search(ref point1, ref point2, ref h, a, b, iter);
                }

                progress.Value = Math.Log2(1 / h[1]) / Math.Log2(1 / epsilon) * 100;
                Plot.Refresh();
                DoEvents();
                Set_result(point2, epsilon, iter);
                Thread.Sleep(2000 - (int)anim_speed.Value);

                if (iter == maxiter || stop_alog)
                    break;
            }
            return;
        }


        public void Exploratory_search(ref double[] point2, double[] h, int iter, double a)
        {
            TextBox_out.Text += "\n\n\nИтерация " + iter;
            TextBox_out.Text += "\nТекущая точка    ( " + point2[0] + ", " + point2[1] + " )";
            TextBox_out.Text += "\nДистанция поиска ( " + h[0] + ", " + h[1] + " )";
            TextBox_out.Text += "\nЗначение функции в точке: " + f(point2);
            //TextBox_out.Text += "\nЛев: " + f_left + "  Прав: " + f_right + "  Верх: " + f_top + "  Низ: " + f_bottom + " - Значение ";
            //TextBox_out.Text += "\nЛев: " + (f_left - f0).ToString() + "  Прав: " + (f_right - f0).ToString() + "  Верх: " + (f_top - f0).ToString() + "  Низ: " + (f_bottom - f0).ToString() + " - Изменение";

           
            TextBox_out.Text += "\nСправа: " + f(point2[0] + h[0], point2[1]) + "; Слева: " + f(point2[0] - h[0], point2[1]);
            TextBox_out.Text += "\nВыбранное направление: ";
            if (f(point2[0] + h[0], point2[1]) <  f(point2) && f(point2[0] + h[0], point2[1]) < f(point2[0] - h[0], point2[1]))
            {
                Plot.Plot.AddArrow(point2[0] + h[0], point2[1], point2[0], point2[1], lineWidth: 2, color: System.Drawing.Color.FromName("GreenYellow"));
                point2[0] = point2[0] + h[0];
                TextBox_out.Text += "вправо ";

            }
            else if (f(point2[0] - h[0], point2[1]) < f(point2) && f(point2[0] - h[0], point2[1]) < f(point2[0] + h[0], point2[1]))
            {
                Plot.Plot.AddArrow(point2[0] - h[0], point2[1], point2[0], point2[1], lineWidth: 2, color: System.Drawing.Color.FromName("GreenYellow"));
                point2[0] = point2[0] - h[0];
                TextBox_out.Text += "влево ";
            }
            else
            {
                List<double> crossX = new List<double>();
                List<double> crossY = new List<double>();
                crossX.Add(point2[0] - h[0]);
                crossY.Add(point2[1]);
                crossX.Add(point2[0] + h[0]);
                crossY.Add(point2[1]);
                Plot.Plot.AddScatter(crossX.ToArray(), crossY.ToArray(), lineStyle: LineStyle.Dot, lineWidth: 2, color: System.Drawing.Color.FromName("OrangeRed"));
                Constriction_X(ref h, a);
            }


            TextBox_out.Text += "\nСверху: " + f(point2[0], point2[1] + h[1]) + "; Снизу: " + f(point2[0], point2[1] - h[1]);
            TextBox_out.Text += "\nВыбранное направление: ";
            if (f(point2[0], point2[1] + h[1]) < f(point2) && f(point2[0], point2[1] + h[1]) < f(point2[0], point2[1] - h[1]))
            {
                Plot.Plot.AddArrow(point2[0], point2[1] + h[1], point2[0], point2[1], lineWidth: 2, color: System.Drawing.Color.FromName("GreenYellow"));
                point2[1] = point2[1] + h[1];
                TextBox_out.Text += "вверх ";
            }
            else if (f(point2[0], point2[1] - h[1]) < f(point2) && f(point2[0], point2[1] - h[1]) < f(point2[0], point2[1] + h[1]))
            {
                Plot.Plot.AddArrow(point2[0], point2[1] - h[1], point2[0], point2[1], lineWidth: 2, color: System.Drawing.Color.FromName("GreenYellow"));
                point2[1] = point2[1] - h[1];
                TextBox_out.Text += "вниз ";
            }
            else
            {
                List<double> crossX = new List<double>();
                List<double> crossY = new List<double>();
                crossX.Add(point2[0]);
                crossY.Add(point2[1] - h[1]);
                crossX.Add(point2[0]);
                crossY.Add(point2[1] + h[1]);
                Plot.Plot.AddScatter(crossX.ToArray(), crossY.ToArray(), lineStyle: LineStyle.Dot, lineWidth: 2, color: System.Drawing.Color.FromName("OrangeRed"));
                Constriction_Y(ref h, a);
            }
            Plot.Refresh();
            DoEvents();
            Thread.Sleep((2000 - (int)anim_speed.Value) / 2);

            TextBox_out.Text += "\nТочка минимума в окрестности ( " + point2[0] + ", " + point2[1] + " )";
            TextBox_out.Text += "\nЗначение функции в ней: " + f(point2).ToString();
        }


        public void Pattern_search(ref double[] point1, ref double[] point2, ref double[] h, double a, double b, int iter)
        {
            //Поиск по образцу
            double[] point3 = new double[2];
            point3[0] = (point2[0] - point1[0]) * b + point2[0];
            point3[1] = (point2[1] - point1[1]) * b + point2[1];

            TextBox_out.Text += "\nФункция улучшилась, выполним поиск по образцу";
            TextBox_out.Text += "\nПредыдущая точка ( " + point1[0] + ", " + point1[1] + " )";
            TextBox_out.Text += "\nНовая точка ( " + point3[0] + ", " + point3[1] + " )";
            
            //Стелка из старой точки в новую
            Plot.Plot.AddArrow(point3[0], point3[1], point1[0], point1[1], lineWidth: 2, color: System.Drawing.Color.FromArgb(255, 0, 200, 200));

            point1[0] = point2[0];
            point1[1] = point2[1];
            point2[0] = point3[0];
            point2[1] = point3[1];
        }
        public void Step_back(ref double[] point1, ref double[] point2, int iter)
        {
            TextBox_out.Text += "\nФункция выросла, возвращаемся";

            //стрелка из текущей точки в предыдущую
            Plot.Plot.AddArrow(point1[0], point1[1], point2[0], point2[1], lineWidth: 1, color: System.Drawing.Color.FromArgb(255, 255, 0, 0));

            point2[0] = point1[0];
            point2[1] = point1[1];
        }
        public void Constriction_X(ref double[] h, double a)
        {
            TextBox_out.Text += "по X не найдено, уменьшаем шаг";
            h[0] /= a;
        }
        public void Constriction_Y(ref double[] h, double a)
        {
            TextBox_out.Text += "по Y не найдено, уменьшаем шаг";
            h[1] /= a;
        }
    }
}
