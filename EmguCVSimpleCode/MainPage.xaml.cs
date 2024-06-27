using Emgu.CV;

namespace EmguCVSimpleCode
{
    public partial class MainPage : ContentPage
    {
        private VideoCapture cap;
        private Mat frame;
        private bool playing;

        public MainPage()
        {
            InitializeComponent();
            _ = mainPage_load();
        }

        private async Task mainPage_load()
        {
            // RTSP URL for streaming video
            string rtspUrl = "rtsp://username:password@ip_address:port/Streaming/Channels/No_Channel";
            // URL for a sample video file
            string videoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

            cap = new VideoCapture(0);
            frame = new Mat();
            playing = true;

            // Start a timer that executes every 50 milliseconds
            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                if (playing)
                {
                    // Read a frame from the video capture
                    cap.Read(frame);
                    if (!frame.IsEmpty)
                    {
                        // Convert the frame to a bitmap
                        var bitmap = frame.ToBitmap();
                        var memoryStream = new MemoryStream();
                        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        var imageSource = ImageSource.FromStream(() => memoryStream);

                        // Set the ImageSource to the ImageView to display the frame
                        ImageView.Source = imageSource;
                    }
                    else
                    {
                        playing = false;
                    }
                }
                return playing;
            });
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