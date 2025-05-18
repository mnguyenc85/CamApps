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

namespace TestOpenCVSharp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Mat _frame = new Mat();
        private WriteableBitmap _camBuf;
        private VideoCapture? _capture;
        private CancellationTokenSource? _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();

            _camBuf = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr32, null);
            CameraImage.Source = _camBuf;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            _capture = new VideoCapture(0); // 0 = default camera
            // Set resolution to 1920x1080
            _capture.Set(VideoCaptureProperties.FrameWidth, 1920);
            _capture.Set(VideoCaptureProperties.FrameHeight, 1080);

            // Optionally verify
            int width = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
            int height = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
            Console.WriteLine($"Resolution: {width}x{height}");
            _capture.Set(VideoCaptureProperties.Fps, 20);

            if (!_capture.IsOpened())
            {
                MessageBox.Show("Camera not found.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            await Task.Run(() => CaptureLoop(_cancellationTokenSource.Token));
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _capture?.Release();
            _capture?.Dispose();
        }

        private void CaptureLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _capture?.Read(_frame);
                Dispatcher.Invoke(() =>
                {
                    OpenCVSharpExt.MatToWritableBitmap(_frame, _camBuf);
                });
            }
        }
    }
}