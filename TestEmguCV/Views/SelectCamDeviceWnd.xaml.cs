using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestEmguCV.Views
{
    /// <summary>
    /// Interaction logic for SelectCamDeviceWnd.xaml
    /// </summary>
    public partial class SelectCamDeviceWnd : Window
    {
        public ObservableCollection<CameraInfo> CameraInfos { get; set; } = [];

        public SelectCamDeviceWnd()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null)
            {
                ListView_AutoSize(listView);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListView_AutoSize(LvCameras);
        }

        private void ListView_AutoSize(ListView lv, int colIndex = 1)
        {
            var gridView = lv.View as GridView;
            if (gridView != null)
            {
                double totalWidth = lv.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                double fixedColumnsWidth = 0;

                // Sum widths of all columns except the last
                for (int i = 0; i < gridView.Columns.Count; i++)
                {
                    if (i != colIndex)
                        fixedColumnsWidth += gridView.Columns[i].ActualWidth;
                }

                // Set remaining width to the last column
                gridView.Columns[colIndex].Width = Math.Max(0, totalWidth - fixedColumnsWidth);
            }
        }

        private void EnumCameras()
        {
            CameraInfos.Clear();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using var video = new VideoCapture(i);
                    if (video.Read(new Mat()))
                    {
                        CameraInfos.Add(new CameraInfo(i, video.BackendName, video.Width, video.Height));
                    }
                }
                catch { }
            }
        }

        public class CameraInfo(int index, string name, int width, int height)
        {
            public int Index { get; set; } = index;
            public string Name { get; set; } = name;
            public int Width { get; set; } = width;
            public int Height { get; set; } = height;
        }

        private void BtCamAutoDetect_Click(object sender, RoutedEventArgs e)
        {
            EnumCameras();
        }
    }
}
