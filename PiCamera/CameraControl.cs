using System;
using System.Linq;
using System.IO;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;
using System.Threading;
using System.Threading.Tasks;

namespace PiCamera
{
    public class CameraControl : IDisposable
    {
        #region private fields

        private static CameraControl _current;

        private bool isRunning;
        private readonly string baseFolder;
        private CameraVideoSettings videoSettings;
        private BinaryWriter fileStream;
        private Timer timer;

        #endregion

        private static readonly CameraVideoSettings defaultVideoSettings =
            new CameraVideoSettings
            {
                CaptureTimeoutMilliseconds = 0,
                CaptureDisplayPreview = false,
                ImageFlipVertically = true,
                CaptureExposure = CameraExposureMode.Auto,
                CaptureWidth = 800,
                CaptureHeight = 600
            };


        private CameraControl(CameraVideoSettings settings)
        {
            videoSettings = settings;
            baseFolder = Path.Combine(GetHomePath(), "recordings");
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
        }

        #region static methods

        public static CameraControl Current => _current;

        public static CameraControl SetupCamera()
        {
            return SetupCamera(defaultVideoSettings);
        }

        public static CameraControl SetupCamera(CameraVideoSettings settings)
        {
            if (_current == null)
            {
                _current = new CameraControl(settings); 
            }

            return _current;
        }

        #endregion

        #region public methods and properties

        public bool IsRunning => isRunning;

        public CameraVideoSettings VideoSettings => videoSettings;

        public delegate void HandleFragment(byte[] fragment);

        public event HandleFragment OnVideoFragment;

        public void UpdateVideoSetting(CameraVideoSettings settings)
        {
            if (isRunning)
            {
                Stop();
                videoSettings = settings;
                Start();
            }
        }

        public void Start()
        {
            if (isRunning)
            {
                return;
            }

            timer = new Timer(HandleTimerCallback, null, Timeout.InfiniteTimeSpan, TimeSpan.FromMinutes(60));

            OpenNewFileStream();
            StartVideoStream();
            isRunning = true;
        }

        public void Stop()
        {
            if (!isRunning)
            {
                return;
            }
            Pi.Camera.CloseVideoStream();
            fileStream.Close();
            timer.Dispose();
            isRunning = false;
        }

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region private methods

        private void StartVideoStream()
        {
            Pi.Camera.OpenVideoStream(videoSettings, HandleDataReceived, HandleExit);
        }

        private void OpenNewFileStream()
        {
            if (fileStream != null)
            {
                fileStream.Dispose();
            }

            string file = GetRecordingFilePath();
            FileStream stream = File.OpenWrite(file);
            fileStream = new BinaryWriter(stream);
        }

        private void HandleDataReceived(byte[] data)
        {
            if (fileStream != null)
            {
                fileStream.Write(data);
            }

            OnVideoFragment(data);
        }

        private void HandleExit()
        {
            isRunning = false;
            Pi.Camera.CloseVideoStream();
            fileStream.Close();
        }

        private void HandleTimerCallback(object state)
        {
            if (!isRunning)
            {
                timer.Dispose();
                return;
            }

            OpenNewFileStream();
        }


        private string GetRecordingFilePath()
        {
            DateTime now = DateTime.Now;
            string prefix = string.Format("{0:0000}-{1:00}-{2:00}", now.Year, now.Month, now.Day);
            string fullDir = Path.Combine(baseFolder, prefix);
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }
            int index = GetIndex(fullDir);
            string filename = string.Format("{0}-{1}.h264", prefix, index + 1);
            return Path.Combine(fullDir, filename);
        }

        private static string GetHomePath()
        {
            return Environment.GetEnvironmentVariable("HOME");
        }
        
        private static int GetIndex(string dir)
        {
            return Directory.GetFiles(dir)
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .Select(filename => int.Parse(filename.Substring(11)))
                .OrderByDescending((nth) => nth)
                .FirstOrDefault();
        }

        #endregion
    }
}
