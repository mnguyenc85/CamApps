using System;
using System.Collections.ObjectModel;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using OpenCvSharp;
using TestOpenCVSharp4.Core;
using TestOpenCVSharp4.Data.DO;

namespace TestOpenCVSharp4.Views
{
    /// <summary>
    /// Interaction logic for SelectCameraWnd.xaml
    /// </summary>
    public partial class SelectCameraWnd : System.Windows.Window
    {
        public ObservableCollection<CamInfoDO> LstCameras { get; set; } = [];
        public ObservableCollection<CamResolutionDO> LstResolutions { get; set; } = [];

        public SelectCameraWnd()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //EnumCameras();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void BtLvCamerasRefresh_Click(object sender, RoutedEventArgs e)
        {
            EnumCameras();
        }

        private void LvCameras_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void EnumCameras()
        {
            LstCameras.Clear();
            
            foreach (CamInfoDO camInfo in Utils.ListCameras())
            {
                LstCameras.Add(camInfo);
            }

            LvCamerasAutoSize(LvCameras);
        }

        private void LvCamerasAutoSize(ListView lv, int colIndex = 1)
        {
            var gridView = lv.View as GridView;
            if (gridView != null)
            {
                double totalWidth = lv.ActualWidth - SystemParameters.VerticalScrollBarWidth - 6;
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

        private void LvCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LvCameras.SelectedItem is CamInfoDO cam)
            {
                LstResolutions.Clear();
                foreach (var r in  cam.Resolutions)
                {
                    LstResolutions.Add(r);
                }
            }
        }
    }
}
