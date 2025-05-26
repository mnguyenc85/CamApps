using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp;
using TestOpenCVSharp4.Core;
using TestOpenCVSharp4.Views;

namespace TestOpenCVSharp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private CameraController _cameraController = new CameraController(0, 1280, 720, 30);
        private long _t0;
        private double _tframe = 0;

        public MainWindow()
        {
            InitializeComponent();

            CameraImage.Source = _cameraController.Buffer;
            _cameraController.FrameReady += _cameraController_FrameReady;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void _cameraController_FrameReady(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (++_tframe >= 30)
                {
                    long t = DateTime.Now.Ticks;
                    double cycle = (t - _t0) / 10000000d;
                    _t0 = t;
                    if (cycle > 0)
                    {
                        double fps = _tframe / cycle;
                        LblCycle.Text = $"FPS: {fps:0.000}";
                    }
                    _tframe = 0;
                }

                _cameraController.UpdateBuffer();
            });
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _cameraController.Start();
            _t0 = DateTime.Now.Ticks;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _cameraController.Stop();
        }

        #region UI
        private void MniFileConfig_Click(object sender, RoutedEventArgs e)
        {
            SelectCameraWnd wnd = new();
            wnd.ShowDialog();
        }
        #endregion
    }
}