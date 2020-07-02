using UnityEngine;

namespace KinectPlayGround.Kinect.Domain
{
    public struct CaptureData
    {
        public int Width { get; }
        public int Height { get; }
        public int TotalPixelNum { get; }

        public CaptureData(int width, int height)
        {
            Width = width;
            Height = height;
            TotalPixelNum = width * height;
        }
    }
    public struct PointCloudData
    {
        public Vector3[] Vertexes { get; }
        public Color32[] Colors { get; }

        public PointCloudData(Vector3[] vertexPositions, Color32[] vertexColors)
        {
            Vertexes = vertexPositions;
            Colors = vertexColors;
        }
    }

    public interface IKinectManager
    {
        CaptureData CaptureData { get; }
        PointCloudData PointCloudData { get; }

    }
}
