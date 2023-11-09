using AngouriMath.Extensions;
using ScottPlot;
using ScottPlot.MarkerShapes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using static AngouriMath.Entity;

namespace Hooke_Jeeves
{
    public class parameters
    {
        public double a, b, e, map_size;
        public double[] h = new double[2];
        public double[] point0 = new double[2];
        public string function = "";
        public int max_iter;


        public string a_string()
        {
            return a.ToString();
        }
        public string b_string()
        {
            return b.ToString();
        }
        public string e_string()
        {
            return e.ToString();
        }
        public string hx_string()
        {
            return h[0].ToString();
        }
        public string hy_string()
        {
            return h[1].ToString();
        }
        public string x0_string()
        {
            return point0[0].ToString();
        }
        public string y0_string()
        {
            return point0[1].ToString();
        }
        public string max_iter_string()
        {
            return max_iter.ToString();
        }
    }
}