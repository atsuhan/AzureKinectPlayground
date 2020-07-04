using System;
using UnityEngine;

namespace KinectPlayGround.Kinect.Domain
{
    public struct KinectDeviceInfo
    {
        public int Width { get; }
        public int Height { get; }
        public int TotalPixelNum => Width * Height;

        public KinectDeviceInfo(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public struct KinectResultData
    {
        public Vector3[] Vertexes;
        public Color32[] Colors;
        public Texture2D RGBTexture;
    }

    public interface IKinectManager
    {
        KinectDeviceInfo DeviceInfo { get; }
        IObservable<KinectResultData> OnUpdateResultData { get; }
    }
}
