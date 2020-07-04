using Cysharp.Threading.Tasks;
using KinectPlayGround.Kinect.Domain;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace KinectPlayGround.Kinect.Infrastructure
{
    public class KinectManager : IKinectManager, IInitializable, IDisposable
    {
        private Device _kinect;
        private Transformation _kinectTransformation;

        private KinectResultData _resultData = new KinectResultData();
        private Subject<KinectResultData> _subjectResultData = new Subject<KinectResultData>();


        public KinectDeviceInfo DeviceInfo { get; private set; }
        public IObservable<KinectResultData> OnUpdateResultData => _subjectResultData.AsObservable();


        #region ZenjectInterfaces
        public void Initialize()
        {
            InitDevice();
            InitDeviceInfo();
            InitResultData(DeviceInfo);
            LoopResultDataUpdater().Forget();
        }

        public void Dispose()
        {
            _kinect.StopCameras();
        }
        #endregion


        private void InitDevice()
        {
            _kinect = Device.Open(0);
            _kinect.StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R1080p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS30
            });
            _kinectTransformation = _kinect.GetCalibration().CreateTransformation();
        }

        private void InitDeviceInfo()
        {
            int width = _kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
            int height = _kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
            DeviceInfo = new KinectDeviceInfo(width, height);
        }

        private void InitResultData(KinectDeviceInfo deviceInfo)
        {
            _resultData.Vertexes = new Vector3[deviceInfo.TotalPixelNum];
            _resultData.Colors = new Color32[deviceInfo.TotalPixelNum];
            _resultData.RGBTexture = new Texture2D(deviceInfo.Width, deviceInfo.Height);
        }

        private async UniTaskVoid LoopResultDataUpdater()
        {
            while (true)
            {
                using (Capture capture = await Task.Run(() => _kinect.GetCapture()).ConfigureAwait(true))
                {
                    Image colorImage = _kinectTransformation.ColorImageToDepthCamera(capture);
                    BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                    Image xyzImage = _kinectTransformation.DepthImageToPointCloud(capture.Depth);
                    Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                    for (int i = 0; i < DeviceInfo.TotalPixelNum; i++)
                    {
                        _resultData.Vertexes[i] = new Vector3(
                            xyzArray[i].X * 0.001f,
                            -xyzArray[i].Y * 0.001f,
                            xyzArray[i].Z * 0.001f
                        );

                        _resultData.Colors[i] = new Color32(
                            colorArray[i].R,
                            colorArray[i].G,
                            colorArray[i].B,
                            colorArray[i].A
                        );
                    }
                    _resultData.RGBTexture.SetPixels32(_resultData.Colors);

                    _subjectResultData.OnNext(_resultData);
                }
            }
        }
    }
}
