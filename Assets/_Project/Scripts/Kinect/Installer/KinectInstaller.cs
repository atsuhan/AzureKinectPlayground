using KinectPlayGround.Kinect.Application;
using KinectPlayGround.Kinect.Infrastructure;
using Zenject;

namespace KinectPlayGround.Kinect.Installer
{
    public class KinectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<KinectManager>()
                .AsCached();

            Container
                .Bind<KinectService>()
                .AsCached();
        }
    }
}