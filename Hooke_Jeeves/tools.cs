using AngouriMath.Extensions;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hooke_Jeeves
{
    public partial class MainWindow : Window
    {

        public double f(double x, double y)
        {
            try
            {
                return (double)function_textbox.Text.Substitute("x", x).Substitute("y", y).EvalNumerical();
            }
            catch (Exception)
            {
                return 10000000;
            }
        }
        public double f(double[] point)
        {
            try
            {
                return (double)function_textbox.Text.Substitute("x", point[0]).Substitute("y", point[1]).EvalNumerical();
            }
            catch (Exception)
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
        public void Add_cross(double[] point, double[] h)
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
        public void Add_label(double[] point, int iter)
        {
            var marker = Plot.Plot.AddMarker(point[0], point[1], color: System.Drawing.Color.FromName("Transparent"), size: 0);
            marker.Text = iter.ToString();
            marker.TextFont.Alignment = Alignment.UpperCenter;
            marker.TextFont.Color = System.Drawing.Color.FromName("SandyBrown");
            marker.TextFont.Size = 16;
        }
        public void Set_params_from_ui(parameters param)
        {
            param.a = Coef_a();
            param.b = Coef_b();
            param.e = Coef_e();
            param.h = Coef_h();
            param.point0 = Zero_point();
            param.function = function_textbox.Text;
            param.max_iter = Max_iter();
            param.map_size = map_size.Value;
        }

        public void Set_ui_from_params(parameters param)
        {
            x0_textbox.Text = param.x0_string();
            y0_textbox.Text = param.y0_string();
            hx_textbox.Text = param.hx_string();
            hy_textbox.Text = param.hy_string();
            coef_a_textbox.Text = param.a_string();
            coef_b_textbox.Text = param.b_string();
            coef_e_textbox.Text = param.e_string();
            function_textbox.Text = param.function;
            iter_max_textbox.Text = param.max_iter_string();
            map_size.Value = param.map_size;
        }
        public void Set_result(double[] point, double epsilon, int iter) //Вывод результатов поиска в интерфейс
        {
            min_textbox.Text = Math.Round(f(point), (int)Math.Log10(1 / epsilon) + 1).ToString();
            xmin_textbox.Text = Math.Round(point[0], (int)Math.Log10(1 / epsilon) + 1).ToString();
            ymin_textbox.Text = Math.Round(point[0], (int)Math.Log10(1 / epsilon) + 1).ToString();
            iter_textbox.Text = iter.ToString();
        }
        public void Add_heat_map() //Создание карты высот/температур
        {
            int width = (int)map_size.Value;
            int height = width;

            double[,] intensities = new double[width * 2, height * 2];

            for (int x = 0; x < width * 2; x++)
                for (int y = 0; y < height * 2; y++)
                    intensities[height * 2 - y - 1, x] = f(x - width, y - height);

            var hm = Plot.Plot.AddHeatmap(intensities);//, ScottPlot.Drawing.Colormap.Turbo   -- другая цветовая пальтра, более контрастная
            hm.OffsetX = -width;
            hm.OffsetY = -height;
            hm.UseParallel = true; //должно ускорить работу
            Plot.Plot.AddColorbar(hm);

            Plot.Plot.SetAxisLimits(-width * 1.05, width * 1.05, -height * 1.05, height * 1.05);
            Plot.Refresh();
        }
    }
}
