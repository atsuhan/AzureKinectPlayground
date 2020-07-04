using KinectPlayGround.Kinect.Application;
using KinectPlayGround.Kinect.Domain;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace KinectPlayGround.Kinect.Presentation
{
    [RequireComponent(typeof(MeshFilter))]
    public class KinectPointCloud : MonoBehaviour
    {
        [Inject] private KinectService _kinectService = null;

        private Mesh _mesh = null;
        private List<int> _indicateList = new List<int>();

        private void Start()
        {
            InitMesh();
            InitIndicats();
            InitMeshUpdateStream();
        }

        private void InitMesh()
        {
            _mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            gameObject.GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void InitIndicats()
        {
            for (int y = 0; y < _kinectService.CaptureData.Height - 2; y++)
            {
                for (int x = 0; x < _kinectService.CaptureData.Width - 2; x++)
                {
                    int index = y * _kinectService.CaptureData.Width + x;
                    int a = index;
                    int b = index + 1;
                    int c = index + _kinectService.CaptureData.Width;
                    int d = index + _kinectService.CaptureData.Width + 1;

                    _indicateList.Add(a);
                    _indicateList.Add(d);
                    _indicateList.Add(c);
                    _indicateList.Add(a);
                    _indicateList.Add(b);
                    _indicateList.Add(d);
                }
            }
        }

        private void InitMeshUpdateStream()
        {
            _kinectService
                .OnUpdatePointCloudData
                .Subscribe(UpdateMeshByPointCloudData);
        }

        private void UpdateMeshByPointCloudData(PointCloudData pointCloudData)
        {
            _mesh.vertices = pointCloudData.Vertexes;
            _mesh.colors32 = pointCloudData.Colors;

            _mesh.RecalculateBounds();

            _mesh.SetTriangles(_indicateList, 0);
            _mesh.RecalculateNormals();
        }
    }
}