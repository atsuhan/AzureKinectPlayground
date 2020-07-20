using Microsoft.Azure.Kinect.Sensor;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class KinectScript : MonoBehaviour
{
    [SerializeField] private MeshTopology _meshTopology = MeshTopology.Triangles;

    private Device _kinectDevice;
    private Transformation _kinectTransformation;

    private int _pointNum;
    private int _pointWidth;
    private int _pointHeight;

    private Mesh _mesh;
    private Vector3[] _meshVertices;
    private Color32[] _meshColors;

    private void Start()
    {
        InitKinect();
        InitMesh();
        Task task = KinectLoop();
    }

    private void OnDestroy()
    {
        _kinectDevice.StopCameras();
    }

    //Kinectの初期化
    private void InitKinect()
    {
        // kinect device
        _kinectDevice = Device.Open(0);
        _kinectDevice.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        });
        _kinectTransformation = _kinectDevice.GetCalibration().CreateTransformation();

        // point count
        _pointWidth = _kinectDevice.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        _pointHeight = _kinectDevice.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        _pointNum = _pointWidth * _pointHeight;
    }

    //Mesh情報の初期化
    private void InitMesh()
    {
        _mesh = new Mesh();
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _meshVertices = new Vector3[_pointNum];
        _meshColors = new Color32[_pointNum];

        _mesh.vertices = _meshVertices;
        _mesh.colors32 = _meshColors;

        gameObject.GetComponent<MeshFilter>().mesh = _mesh;
    }

    //Kinectからデータを取得→描画
    private async Task KinectLoop()
    {
        while (true)
        {
            using (Capture capture = await Task.Run(() => _kinectDevice.GetCapture()).ConfigureAwait(true))
            {
                Image colorImage = _kinectTransformation.ColorImageToDepthCamera(capture);
                BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                Image xyzImage = _kinectTransformation.DepthImageToPointCloud(capture.Depth);
                Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                for (int i = 0; i < _pointNum; i++)
                {
                    _meshVertices[i].x = xyzArray[i].X * 0.001f;
                    _meshVertices[i].y = -xyzArray[i].Y * 0.001f;
                    _meshVertices[i].z = xyzArray[i].Z * 0.001f;

                    _meshColors[i].b = colorArray[i].B;
                    _meshColors[i].g = colorArray[i].G;
                    _meshColors[i].r = colorArray[i].R;
                    _meshColors[i].a = 255;
                }

                // update vertices and colors
                _mesh.vertices = _meshVertices;
                _mesh.colors32 = _meshColors;

                // update indices 
                List<int> indiceList = GetIndiceList(_meshVertices);
                _mesh.SetIndices(indiceList, _meshTopology, 0);

                _mesh.RecalculateBounds();
            }
        }
    }

    // meshのindicesを取得
    private List<int> GetIndiceList(Vector3[] vertices)
    {
        List<int> indiceList = new List<int>();

        if (_meshTopology == MeshTopology.Points)
        {
            for (int i = 0; i < _pointNum; i++)
            {
                bool isVaridPoint = vertices[i].magnitude != 0;
                if (isVaridPoint)
                {
                    indiceList.Add(i);
                }
            }
            return indiceList;
        }

        for (int y = 0; y < _pointHeight - 1; y++)
        {
            for (int x = 0; x < _pointWidth - 1; x++)
            {
                int index = y * _pointWidth + x;
                int a = index;
                int b = index + 1;
                int c = index + _pointWidth;
                int d = index + _pointWidth + 1;

                bool isVaridA = vertices[a].magnitude != 0;
                bool isVaridB = vertices[b].magnitude != 0;
                bool isVaridC = vertices[c].magnitude != 0;
                bool isVaridD = vertices[d].magnitude != 0;

                switch (_meshTopology)
                {
                    case MeshTopology.Triangles:
                        if (isVaridA & isVaridB & isVaridC)
                        {
                            indiceList.Add(a);
                            indiceList.Add(b);
                            indiceList.Add(c);
                        }
                        if (isVaridC & isVaridB & isVaridD)
                        {
                            indiceList.Add(c);
                            indiceList.Add(b);
                            indiceList.Add(d);
                        }
                        break;

                    case MeshTopology.Quads:
                        if (isVaridA && isVaridB && isVaridC && isVaridD)
                        {
                            indiceList.Add(a);
                            indiceList.Add(b);
                            indiceList.Add(d);
                            indiceList.Add(c);
                        }
                        break;

                    default:
                        if (isVaridA & isVaridB)
                        {
                            indiceList.Add(a);
                            indiceList.Add(b);
                        }
                        if (isVaridC & isVaridD)
                        {
                            indiceList.Add(c);
                            indiceList.Add(d);
                        }
                        break;
                }
            }
        }
        return indiceList;
    }
}