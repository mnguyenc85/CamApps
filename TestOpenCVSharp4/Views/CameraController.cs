using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using TestOpenCVSharp4.Core;

namespace TestOpenCVSharp4.Views
{
    public class CameraController
    {
        public event EventHandler? FrameReady;

        private int _index;
        private int _w, _h, _fps;

        private Mat _frame = new();
        private VideoCapture? _capture;
        private CancellationTokenSource? _cancellationTokenSource;

        private WriteableBitmap _camBuf;
        public WriteableBitmap Buffer { get { return _camBuf; } }

        public CameraController(int index, int w, int h, int fps)
        {
            _index = index; _w = w; _h = h; _fps = fps;
            _camBuf = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr24, null);
        }

        public void UpdateBuffer()
        {
            OpenCVSharpExt.MatToBgrWritableBitmap(_frame, _camBuf);
        }

        public async void Start()
        {
            Stop();

            _capture = new VideoCapture(_index);

            _capture.Set(VideoCaptureProperties.FrameWidth, _w);
            _capture.Set(VideoCaptureProperties.FrameHeight, _h);
            // Optionally verify
            int width = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
            int height = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
            Console.WriteLine($"Resolution: {width}x{height}");

            _capture.Set(VideoCaptureProperties.Fps, _fps);

            if (!_capture.IsOpened())
                throw new Exception("Camera not found.");

            _cancellationTokenSource = new CancellationTokenSource();
            await Task.Run(() => CaptureLoop(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (_capture != null && _capture.IsOpened())
            {
                _cancellationTokenSource?.Cancel();
                _capture?.Release();
                _capture?.Dispose();
            }
        }

        private void CaptureLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _capture?.Read(_frame);
                FrameReady?.Invoke(this, new EventArgs());
            }
        }
    }
}
