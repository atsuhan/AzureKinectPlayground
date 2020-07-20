using KinectPlayGround.Kinect.Application;
using KinectPlayGround.Kinect.Domain;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace KinectPlayGround.Kinect.Presentation
{
    public class KinectCameraView : MonoBehaviour
    {
        [Inject] private KinectService _kinectService = null;

        [SerializeField] private KinectTextureType _textureType = KinectTextureType.RGBTexture;
        [SerializeField] private RawImage _rawImage = null;

        private void Start()
        {
            _kinectService
                .OnUpdateResultData
                .Subscribe(UpdateRawImage);
        }

        private void UpdateRawImage(KinectResultData resultData)
        {
            if (_textureType == 0)
            {
                _rawImage.texture = resultData.RGBTexture;
                return;
            }

            _rawImage.texture = resultData.DepthTexture;
        }
    }

    public enum KinectTextureType
    {
        RGBTexture = 0,
        DepthTexture = 1,
    }
}