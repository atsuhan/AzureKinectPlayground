using KinectPlayGround.Kinect.Application;
using KinectPlayGround.Kinect.Domain;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace KinectPlayGround.Kinect.Presentation
{
    [RequireComponent(typeof(MeshFilter))]
    public class KinectResultMesh : MonoBehaviour
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
            _mesh.RecalculateBounds();

            List<int> indicateList = GetIndicateList(_kinectService.DeviceInfo, resultData);
            _mesh.SetIndices(indicateList, _meshTopology, 0);
        }

        private List<int> GetIndicateList(KinectDeviceInfo deviceInfo, KinectResultData resultData)
        {
            List<int> indicateList = new List<int>();

            if (_meshTopology == MeshTopology.Points)
            {
                for (int i = 0; i < deviceInfo.TotalPixelNum; i++)
                {
                    indicateList.Add(i);
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

                    bool isMissingA = resultData.Vertexes[a].magnitude == 0;
                    bool isMissingB = resultData.Vertexes[b].magnitude == 0;
                    bool isMissingC = resultData.Vertexes[c].magnitude == 0;
                    bool isMissingD = resultData.Vertexes[d].magnitude == 0;

                    switch (_meshTopology)
                    {
                        case MeshTopology.Triangles:
                            if (!isMissingA & !isMissingD & !isMissingC)
                            {
                                indicateList.Add(a);
                                indicateList.Add(d);
                                indicateList.Add(c);
                            }
                            if (!isMissingA & !isMissingB & !isMissingD)
                            {
                                indicateList.Add(a);
                                indicateList.Add(b);
                                indicateList.Add(d);
                            }
                            break;

                        case MeshTopology.Quads:
                            if (isMissingA || isMissingB || isMissingC || isMissingD) continue;
                            indicateList.Add(a);
                            indicateList.Add(b);
                            indicateList.Add(d);
                            indicateList.Add(c);
                            break;

                        default:
                            if (!isMissingA & !isMissingC)
                            {
                                indicateList.Add(a);
                                indicateList.Add(c);
                            }
                            if (!isMissingB & !isMissingD)
                            {
                                indicateList.Add(b);
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