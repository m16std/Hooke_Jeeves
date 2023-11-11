using ScottPlot;
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
            double a = Coef_a(), b = Coef_b(), epsilon = Coef_e();
            double[] h = Coef_h();
            double[] point1 = Zero_point(), point2 = Zero_point(), point3 = Zero_point(), point3_old = Zero_point();
            int iter = 0, maxiter = Max_iter();
            Add_label(point3, iter);

            while (h[1] > epsilon)
            {
                iter++;

                Add_cross(point3, h);

                Exploratory_search(ref point2, point3, h, iter);

                if (Enumerable.SequenceEqual(point2, point3))   //Поиск завершился неудачей
                {
                    Constriction(ref h, a);
                }
                else
                if (f(point2) > f(point1)) //Найденная точка хуже старой
                {
                    Step_back(ref point1, ref point2, ref point3, point3_old);
                }
                else                       //Найденная точка лучше предыдущей
                {
                    Pattern_search(ref point1, ref point2, ref point3, ref point3_old, ref h, a, b, iter);
                }

                progress.Value = Math.Log2(1 / h[1]) / Math.Log2(1 / epsilon) * 100;
                Plot.Refresh();
                DoEvents();
                Thread.Sleep(2000 - (int)anim_speed.Value);

                if (iter == maxiter)
                    break;

                Set_result(point3, epsilon, iter);
            }
            Set_result(point3, epsilon, iter);
            return;
        }


        public void Exploratory_search(ref double[] point2, double[] point3, double[] h, int iter)
        {
            double f0 = f(point3);

            TextBox_out.Text += "\n\n\nИтерация " + iter;
            TextBox_out.Text += "\nТекущая точка    ( " + point3[0] + ", " + point3[1] + " )";
            TextBox_out.Text += "\nДистанция поиска ( " + h[0] + ", " + h[1] + " )";
            TextBox_out.Text += "\nЗначение функции в точке: " + f0;
            //TextBox_out.Text += "\nЛев: " + f_left + "  Прав: " + f_right + "  Верх: " + f_top + "  Низ: " + f_bottom + " - Значение ";
            //TextBox_out.Text += "\nЛев: " + (f_left - f0).ToString() + "  Прав: " + (f_right - f0).ToString() + "  Верх: " + (f_top - f0).ToString() + "  Низ: " + (f_bottom - f0).ToString() + " - Изменение";

            TextBox_out.Text += "\nВыбранное направление: ";

            if (f(point3[0] + h[0], point3[1]) < f0)
            {
                point2[0] = point3[0] + h[0];
                TextBox_out.Text += "вправо ";
            }
            if (f(point3[0] - h[0], point3[1]) < f0)
            {
                point2[0] = point3[0] - h[0];
                TextBox_out.Text += "влево ";
            }
            if (f(point3[0], point3[1] + h[1]) < f0)
            {
                point2[1] = point3[1] + h[1];
                TextBox_out.Text += "вверх ";
            }
            if (f(point3[0], point3[1] - h[1]) < f0)
            {
                point2[1] = point3[1] - h[1];
                TextBox_out.Text += "вниз ";
            }

            TextBox_out.Text += "\nТочка минимума в окрестности ( " + point2[0] + ", " + point2[1] + " )";
            TextBox_out.Text += "\nЗначение функции в точке: " + f(point2).ToString();
        }


        public void Pattern_search(ref double[] point1, ref double[] point2, ref double[] point3, ref double[] point3_old, ref double[] h, double a, double b, int iter)
        {
            double[] point3_new = new double[2];
            //Поиск по образцу
            point3_new[0] = (point2[0] - point1[0]) * b + point1[0];
            point3_new[1] = (point2[1] - point1[1]) * b + point1[1];


            TextBox_out.Text += "\nФункция улучшилась, выполним поиск по образцу";
            TextBox_out.Text += "\nПредыдущая точка ( " + point1[0] + ", " + point1[1] + " )";
            TextBox_out.Text += "\nНовая точка ( " + point3_new[0] + ", " + point3_new[1] + " )";

            //Стрелка - указатель направления минимума
            Plot.Plot.AddArrow(point2[0], point2[1], point3[0], point3[1], lineWidth: 2, color: System.Drawing.Color.FromName("SandyBrown"));
            Plot.Refresh();
            DoEvents();
            Thread.Sleep(2000 - (int)anim_speed.Value);

            //Стелка из старой точки в новую
            Plot.Plot.AddArrow(point3_new[0], point3_new[1], point1[0], point1[1], lineWidth: 2, color: System.Drawing.Color.FromArgb(255, 0, 200, 200));
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
            TextBox_out.Text += "\nНеверное направление - функция выросла, возвращаемся";
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
            TextBox_out.Text += "не найдено, уменьшаем шаг";
            h[0] /= a;
            h[1] /= a;
        }
    }
}
