using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using OpenCvSharp;
using System.Windows;
using System.Windows.Media.Media3D;
using System.IO;

namespace TestOpenCVSharp4.Core
{
    public static class OpenCVSharpExt
    {
        public static BitmapSource? ToBitmapSource(this Mat mat)
        {
            if (mat.Empty()) return null;

            bool requiredDispose = false;
            // Ensure Mat is in BGR or BGRA format (common for OpenCV)
            Mat bgra = mat;
            if (mat.Type() != MatType.CV_8UC4)
            {
                bgra = new Mat();
                Cv2.CvtColor(mat, bgra, ColorConversionCodes.BGR2BGRA);
                requiredDispose = true;
            }

            PixelFormat pixelFormat = PixelFormats.Bgra32;
            int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
            int stride = mat.Width * bytesPerPixel;

            // Create BitmapSource
            BitmapSource bitmapSource = BitmapSource.Create(
                bgra.Width, bgra.Height,
                96, 96,              // DPI
                pixelFormat,
                null,                // Palette
                bgra.Data,            // Pixel data
                bgra.Height * stride, // Buffer size
                stride);

            if (requiredDispose) { bgra.Dispose(); }

            return bitmapSource;
        }

        public static WriteableBitmap? ToWriteableBitmap(this Mat mat)
        {
            if (mat.Empty()) return null;

            bool requiredDispose = false;
            // Convert to 4-channel BGRA if not already
            Mat bgra = mat;
            if (mat.Type() != MatType.CV_8UC4)
            {
                bgra = new Mat();
                Cv2.CvtColor(mat, bgra, ColorConversionCodes.BGR2BGRA);
                requiredDispose = true;
            }

            PixelFormat pixelFormat = PixelFormats.Bgra32;
            int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
            int stride = mat.Width * bytesPerPixel;

            WriteableBitmap bitmap = new WriteableBitmap(
                bgra.Width, bgra.Height, 96, 96, // DPI
                PixelFormats.Bgra32,
                null);

            bitmap.Lock();

            // Copy Mat data to WriteableBitmap
            IntPtr srcPtr = bgra.Data;
            IntPtr destPtr = bitmap.BackBuffer;
            int length = stride * bgra.Height;
            unsafe
            {
                Buffer.MemoryCopy((void*)srcPtr, (void*)destPtr, length, length);
            }

            bitmap.AddDirtyRect(new Int32Rect(0, 0, bgra.Width, bgra.Height));
            bitmap.Unlock();

            if (requiredDispose) { bgra.Dispose(); }

            return bitmap;
        }

        public static void MatToWritableBitmap(Mat mat, WriteableBitmap bmp)
        {
            if (mat.Empty()) return;
            if (mat.Width != bmp.Width || mat.Height != bmp.Height) return;

            bool requiredDispose = false;
            // Convert to BGRA if needed
            Mat bgra = mat;
            if (mat.Type() != MatType.CV_8UC4)
            {
                Cv2.CvtColor(mat, bgra = new Mat(), ColorConversionCodes.BGR2BGRA);
                requiredDispose = true;
            }

            int width = bgra.Width;
            int height = bgra.Height;
            int stride = bmp.BackBufferStride;

            bmp.Lock();
            // Copy Mat data to WriteableBitmap
            IntPtr srcPtr = bgra.Data;
            IntPtr destPtr = bmp.BackBuffer;
            int length = stride * mat.Height;
            unsafe
            {
                Buffer.MemoryCopy((void*)srcPtr, (void*)destPtr, length, length);
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bmp.Unlock();

            if (requiredDispose) bgra.Dispose();
        }

        public static BitmapSource BitmapSourceFromStream(MemoryStream stream)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;  // Important: so the stream can be closed afterward
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();                                // Optional: makes it cross-thread accessible

            return bitmap;
        }
    }
}
