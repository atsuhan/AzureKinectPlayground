using System;
using UniRx;
using UnityEngine;

namespace KinectPlayGround.Kinect.Domain
{
    public struct CaptureInfo
    {
        public int Width { get; }
        public int Height { get; }
        public int TotalPixelNum => Width * Height;

        public CaptureInfo(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
    public struct PointCloudData
    {
        public Vector3[] Vertexes { get; }
        public Color32[] Colors { get; }

        public PointCloudData(Vector3[] vertexes, Color32[] colors)
        {
            Vertexes = vertexes;
            Colors = colors;
        }
    }

    public interface IKinectManager
    {
        CaptureInfo CaptureData { get; }
        IObservable<PointCloudData> OnUpdatePointCloudData { get; }
    }
}
