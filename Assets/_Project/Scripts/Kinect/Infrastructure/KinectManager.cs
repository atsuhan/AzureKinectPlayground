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
        private Subject<PointCloudData> _subjectPointCloudData = new Subject<PointCloudData>();

        private Vector3[] _pointVertexes;
        private Color32[] _pointColors;

        public CaptureInfo CaptureData { get; private set; }
        public IObservable<PointCloudData> OnUpdatePointCloudData => _subjectPointCloudData.AsObservable();

        public void Initialize()
        {
            InitKinect();
            InitCaptureData();
            InitPointCloudArr();
            PointCloudDataUpdater().Forget();
        }

        private void InitKinect()
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

        private void InitCaptureData()
        {
            int width = _kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
            int height = _kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
            CaptureData = new CaptureInfo(width, height);
        }

        private void InitPointCloudArr()
        {
            _pointVertexes = new Vector3[CaptureData.TotalPixelNum];
            _pointColors = new Color32[CaptureData.TotalPixelNum];
        }

        private async UniTaskVoid PointCloudDataUpdater()
        {
            while (true)
            {
                using (Capture capture = await Task.Run(() => _kinect.GetCapture()).ConfigureAwait(true))
                {
                    Image colorImage = _kinectTransformation.ColorImageToDepthCamera(capture);
                    BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                    Image xyzImage = _kinectTransformation.DepthImageToPointCloud(capture.Depth);
                    Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                    for (int i = 0; i < _pointVertexes.Length; i++)
                    {
                        _pointVertexes[i] = new Vector3(
                            xyzArray[i].X * 0.001f,
                            -xyzArray[i].Y * 0.001f,
                            xyzArray[i].Z * 0.001f
                        );

                        _pointColors[i] = new Color32(
                            colorArray[i].R,
                            colorArray[i].G,
                            colorArray[i].B,
                            colorArray[i].A
                        );
                    }

                    PointCloudData resultPointCloud = new PointCloudData(_pointVertexes, _pointColors);
                    _subjectPointCloudData.OnNext(resultPointCloud);
                }
            }
        }

        public void Dispose()
        {
            _kinect.StopCameras();
        }
    }
}
