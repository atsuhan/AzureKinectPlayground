using KinectPlayGround.Kinect.Domain;
using System;
using Zenject;

namespace KinectPlayGround.Kinect.Application
{
    public class KinectService
    {
        [Inject] private IKinectManager _kinectManager = null;

        public KinectDeviceInfo DeviceInfo => _kinectManager.DeviceInfo;
        public IObservable<KinectResultData> OnUpdateResultData => _kinectManager.OnUpdateResultData;
    }
}
