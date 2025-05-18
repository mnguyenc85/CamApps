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
using Emgu.CV.CvEnum;
using Emgu.CV;
using TestEmguCV.Views;
using System.Windows.Threading;

namespace TestEmguCV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private WriteableBitmap _camFrame;
        private VideoCapture? _videoCapture;
        private DispatcherTimer? _timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromMilliseconds(40)
                };
                _timer.Tick += _timer_Tick;

                _videoCapture = new VideoCapture(0, VideoCapture.API.OpencvMjpeg);

                _videoCapture?.Start();
                _timer?.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            using (var frame = _videoCapture?.QueryFrame())
            {
                if (frame != null && !frame.IsEmpty)
                {
                    var imgSource = Mat2BitmapSource(frame);

                    // Ensure UI thread access
                    Dispatcher.Invoke(() =>
                    {
                        ImgCamera.Source = imgSource;
                    });
                }
            }
        }

        private BitmapSource? Mat2BitmapSource(Mat mat)
        {
            if (mat.IsEmpty) return null;

            // Ensure Mat is in BGR or BGRA format (common for OpenCV)
            Mat displayMat = mat;
            if (mat.NumberOfChannels == 1) // Convert grayscale to BGR if needed
            {
                displayMat = new Mat();
                CvInvoke.CvtColor(mat, displayMat, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr);
            }

            // Get pixel format and bytes per pixel
            PixelFormat pixelFormat = displayMat.NumberOfChannels == 4 ? PixelFormats.Bgra32 : PixelFormats.Bgr24;
            int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;

            // Calculate stride (bytes per row)
            int stride = mat.Width * bytesPerPixel;

            // Create BitmapSource
            BitmapSource bitmapSource = BitmapSource.Create(
                mat.Width,
                mat.Height,
                96, // DPI X
                96, // DPI Y
                pixelFormat,
                null, // Palette
                mat.DataPointer, // Pixel data
                mat.Height * stride, // Buffer size
                stride);

            return bitmapSource;
        }

        #region User interacts
        private void MniFileConfig_Click(object sender, RoutedEventArgs e)
        {
            SelectCamDeviceWnd wnd = new();
            wnd.ShowDialog();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
            _videoCapture?.Dispose();
        }
    }
}