using KinectPlayGround.Kinect.Application;
using KinectPlayGround.Kinect.Domain;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace KinectPlayGround.Kinect.Presentation
{
    [RequireComponent(typeof(MeshFilter))]
    public class KinectMeshGenerator : MonoBehaviour
    {
        [Inject] private KinectService _kinectService = null;

        [SerializeField] private MeshTopology _meshTopology = MeshTopology.Points;

        private Mesh _mesh = null;

        private void Start()
        {
            InitMesh();
            InitUpdaterStream();
        }

        private void InitMesh()
        {
            _mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.GetComponent<MeshFilter>().mesh = _mesh;
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

            List<int> indicateList = GetIndicateList(_kinectService.DeviceInfo, resultData);
            _mesh.SetIndices(indicateList, _meshTopology, 0);

            _mesh.RecalculateBounds();
        }

        private List<int> GetIndicateList(KinectDeviceInfo deviceInfo, KinectResultData resultData)
        {
            List<int> indicateList = new List<int>();

            if (_meshTopology == MeshTopology.Points)
            {
                for (int i = 0; i < deviceInfo.TotalPixelNum; i++)
                {
                    bool isVaridPoint = resultData.Vertexes[i].magnitude != 0;
                    if (isVaridPoint)
                    {
                        indicateList.Add(i);
                    }
                }
                return indicateList;
            }

            for (int y = 0; y < deviceInfo.Height - 1; y++)
            {
                for (int x = 0; x < deviceInfo.Width - 1; x++)
                {
                    int index = y * deviceInfo.Width + x;
                    int a = index;
                    int b = index + 1;
                    int c = index + deviceInfo.Width;
                    int d = index + deviceInfo.Width + 1;

                    bool isVaridA = resultData.Vertexes[a].magnitude != 0;
                    bool isVaridB = resultData.Vertexes[b].magnitude != 0;
                    bool isVaridC = resultData.Vertexes[c].magnitude != 0;
                    bool isVaridD = resultData.Vertexes[d].magnitude != 0;

                    switch (_meshTopology)
                    {
                        case MeshTopology.Triangles:
                            if (isVaridA & isVaridD & isVaridC)
                            {
                                indicateList.Add(a);
                                indicateList.Add(d);
                                indicateList.Add(c);
                            }
                            if (isVaridA & isVaridB & isVaridD)
                            {
                                indicateList.Add(a);
                                indicateList.Add(b);
                                indicateList.Add(d);
                            }
                            break;

                        case MeshTopology.Quads:
                            if (!(isVaridA && isVaridB && isVaridC && isVaridD)) continue;
                            indicateList.Add(a);
                            indicateList.Add(b);
                            indicateList.Add(d);
                            indicateList.Add(c);
                            break;

                        default:
                            if (isVaridA & isVaridB)
                            {
                                indicateList.Add(a);
                                indicateList.Add(b);
                            }
                            if (isVaridC & isVaridD)
                            {
                                indicateList.Add(c);
                                indicateList.Add(d);
                            }
                            break;
                    }
                }
            }
            return indicateList;
        }
    }
}