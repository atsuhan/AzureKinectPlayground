using KinectPlayGround.Kinect.Application;
using KinectPlayGround.Kinect.Domain;
using UniRx;
using UnityEngine;
using Zenject;

namespace KinectPlayGround.Kinect.Presentation
{
    [RequireComponent(typeof(MeshFilter))]
    public class KinectMeshComputeGenerator : MonoBehaviour
    {
        [Inject] private KinectService _kinectService = null;

        [SerializeField] private MeshTopology _meshTopology = MeshTopology.Points;
        [SerializeField] private ComputeShader _computeShader = null;


        private Mesh _mesh = null;

        private int _indicateKernelNum = 0;
        private int[] _indicateArr = null;
        private ComputeBuffer _indicateBuffer;

        private void Start()
        {
            InitMesh();
            InitIndicate();
            InitUpdaterStream();
        }

        private void InitMesh()
        {
            _mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void InitIndicate()
        {
            _indicateKernelNum = _computeShader.FindKernel("CSMain");
            _indicateBuffer = new ComputeBuffer(_kinectService.DeviceInfo.TotalPixelNum, sizeof(int));

            _indicateArr = new int[_kinectService.DeviceInfo.TotalPixelNum];
            for (int i = 0; i < _indicateArr.Length; i++)
            {
                _indicateArr[i] = i;
            }
        }

        private void InitUpdaterStream()
        {
            _kinectService
                .OnUpdateResultData
                .Subscribe(UpdatePointCloud);
        }

        private void UpdatePointCloud(KinectResultData resultData)
        {
            _mesh.vertices = resultData.Vertexes;
            _mesh.colors32 = resultData.Colors;
            _mesh.RecalculateBounds();

            _indicateBuffer.SetData(_indicateArr);
            _computeShader.SetBuffer(_indicateKernelNum, "IndicateBuffer", _indicateBuffer);
            _computeShader.Dispatch(_indicateKernelNum, 1, 1, 1);
            _indicateBuffer.GetData(_indicateArr);

            Debug.Log(_indicateArr[10]);

            _mesh.SetIndices(_indicateArr, _meshTopology, 0);
        }
    }
}