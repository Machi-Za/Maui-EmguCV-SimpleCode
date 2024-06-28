using Emgu.CV;
using Emgu.CV.Structure;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace EmguCVSimpleCode
{
    public partial class MainPage : ContentPage
    {
        private VideoCapture cap;
        private Mat frame;
        private bool playing;

        private ConcurrentQueue<SKBitmap> _bitmapQueue = new ConcurrentQueue<SKBitmap>();
        private SKBitmap _bitmap;

        private static object lockObject = new object();

        public MainPage()
        {
            InitializeComponent();
            _ = MainPage_Load();
        }

        private async Task MainPage_Load()
        {
            // RTSP URL for streaming video
            string rtspUrl = "rtsp://username:password@ip_address:port/Streaming/Channels/No_Channel";
            // URL for a sample video file
            string videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
            cap = new VideoCapture(0);  // Initialize capture from default camera
            frame = new Mat();
            playing = true;

            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                if (playing)
                {
                    // Read a frame from the video capture
                    cap.Read(frame);
                    if (!frame.IsEmpty)
                    {
                        // Convert the frame to a bitmap
                        var image = frame.ToImage<Bgra, byte>();
                        var bitmap = new SKBitmap(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

                        byte[] imageData = image.Bytes;
                        IntPtr unmanagedPointer = Marshal.AllocHGlobal(imageData.Length);
                        Marshal.Copy(imageData, 0, unmanagedPointer, imageData.Length);

                        bitmap.InstallPixels(new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul), unmanagedPointer, image.Width * 4, (addr, ctx) => System.Runtime.InteropServices.Marshal.FreeHGlobal(addr), null);

                        if (_bitmapQueue.Count == 2) ClearQueue();
                        _bitmapQueue.Enqueue(bitmap);

                        Device.InvokeOnMainThreadAsync(() =>
                        {
                            ImageView.InvalidateSurface();
                        });
                    }
                    else
                    {
                        playing = false;
                    }
                }
                return playing;
            });
        }

        private void ClearQueue()
        {
            while (_bitmapQueue.TryDequeue(out var bitmap))
            {
                bitmap.Dispose();
            }
        }

        private void ImageView_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (_bitmapQueue.TryDequeue(out SKBitmap bitmap))
            {
                args.Surface.Canvas.Clear();
                args.Surface.Canvas.DrawBitmap(bitmap, new SKPoint(0, 0));
                bitmap.Dispose();
            }
        }

        protected override void OnDisappearing()
        {
            // Dispose of the VideoCapture object if it is not null
            base.OnDisappearing();
            playing = false;
            cap?.Dispose();
        }
    }
}
