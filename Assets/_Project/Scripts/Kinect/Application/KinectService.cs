using KinectPlayGround.Kinect.Domain;
using System;
using Zenject;

namespace KinectPlayGround.Kinect.Application
{
    public class KinectService
    {
        [Inject] private IKinectManager _kinectManager = null;

        public CaptureInfo CaptureData => _kinectManager.CaptureData;
        public IObservable<PointCloudData> OnUpdatePointCloudData => _kinectManager.OnUpdatePointCloudData;
    }
}
