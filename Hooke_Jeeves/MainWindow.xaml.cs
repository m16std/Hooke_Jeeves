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
using ScottPlot.Plottable;
using ScottPlot.Drawing.Colormaps;

namespace Hooke_Jeeves
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Clear_All_Click(object sender, RoutedEventArgs e)
        {
            progress.Value = 0;
            Plot.Plot.Clear();
            Plot.Refresh();
            TextBox_out.Text = "";
            Clear_result();
        }
        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            progress.Value = 0;
            Plot.Plot.Clear(typeof(ArrowCoordinated));
            Plot.Plot.Clear(typeof(ScatterPlot));
            Plot.Plot.Clear(typeof(Marker));
            Plot.Plot.Clear(typeof(MarkerPlot));
            Plot.Refresh();
            TextBox_out.Text = "";
            Clear_result();
            try
            {
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
            Clear_result();
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
        private void LostFocusFunction(object sender, RoutedEventArgs e)
        {
            Plot.Plot.Clear(typeof(Heatmap));
            Plot.Plot.Clear(typeof(Colorbar));
            try
            {
                Add_heat_map();
            }
            catch (Exception)
            {

            }
        }
        private void About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Метод Хука — Дживса (англ. Hooke — Jeeves), также известный как метод конфигураций — как и алгоритм Нелдера — Мида, служит для поиска безусловного локального " +
                "экстремума функции и относится к прямым методам, то есть опирается непосредственно на значения функции. Алгоритм делится на две фазы: исследующий поиск и поиск по образцу.\r\n\r\n" +
                "На начальном этапе задаётся стартовая точка (обозначим её 1) и шаги hi по координатам. Затем замораживаем значения всех координат кроме 1-й, вычисляем значения функции в точках x0+h0 и " +
                "x0-h0 (где x0 — первая координата точки, а h0 — соответственно значение шага по этой координате) и переходим в точку с наименьшим значением функции. В этой точке замораживаем значения всех " +
                "координат кроме 2-й, вычисляем значения функции в точках x1+h1 и x1-h1, переходим в точку с наименьшим значением функции и т. д. для всех координат. В случае, если для какой-нибудь " +
                "координаты значение в исходной точке меньше, чем значения для обоих направлений шага, то шаг по этой координате уменьшается. Когда шаги по всем координатам hi станут меньше соответствующих " +
                "значений ei, алгоритм завершается, и точка 1 признаётся точкой минимума. \r\n\r\n a - то, во сколько раз уменьшается шаг при неудачном поиске.\r\n b - коэффициент усиления в поиске по образцу." +
                "\r\n h - начальный шаг в исследующем поиске.\r\n e - значение шага поиска, на котором он остановится.");
        }
    }
}
