using KinectPlayGround.Kinect.Domain;
using Zenject;

namespace KinectPlayGround.Kinect.Application
{
    public class KinectService
    {
        [Inject] private IKinectManager _kinectManager = null;

        public CaptureData CaptureData => _kinectManager.CaptureData;
        public PointCloudData PointCloudData => _kinectManager.PointCloudData;
    }
}
