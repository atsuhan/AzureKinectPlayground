using Microsoft.Azure.Kinect.Sensor;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class KinectPointCloud : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image _uiImage = null;

    private Device _kinect;
    private Transformation _kinectTransformation;
    private int _pointNum;
    private Mesh _mesh;
    private int[] _pointIndices;
    private Vector3[] _pointVertices;
    private Color32[] _pointColors;

    private void Start()
    {
        InitKinect();
        InitMesh();
        Task t = KinectLoop();
    }

    //Kinectの初期化
    private void InitKinect()
    {
        _kinect = Device.Open(0);
        _kinect.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        });
        _kinectTransformation = _kinect.GetCalibration().CreateTransformation();
    }

    private void InitMesh()
    {
        int width = _kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        int height = _kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        _pointNum = width * height;

        _mesh = new Mesh();
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        _pointVertices = new Vector3[_pointNum];
        _pointColors = new Color32[_pointNum];
        _pointIndices = new int[_pointNum];

        for (int i = 0; i < _pointNum; i++)
        {
            _pointIndices[i] = i;
        }

        _mesh.vertices = _pointVertices;
        _mesh.colors32 = _pointColors;
        _mesh.SetIndices(_pointIndices, MeshTopology.Points, 0);

        gameObject.GetComponent<MeshFilter>().mesh = _mesh;
    }

    private async Task KinectLoop()
    {
        while (true)
        {
            using (Capture capture = await Task.Run(() => _kinect.GetCapture()).ConfigureAwait(true))
            {
                Image colorImage = _kinectTransformation.ColorImageToDepthCamera(capture);
                BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                Image xyzImage = _kinectTransformation.DepthImageToPointCloud(capture.Depth);
                Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                for (int i = 0; i < _pointNum; i++)
                {
                    _pointVertices[i].x = xyzArray[i].X * 0.001f;
                    _pointVertices[i].y = -xyzArray[i].Y * 0.001f;//上下反転
                    _pointVertices[i].z = xyzArray[i].Z * 0.001f;
                    _pointColors[i].b = colorArray[i].B;
                    _pointColors[i].g = colorArray[i].G;
                    _pointColors[i].r = colorArray[i].R;
                    _pointColors[i].a = 255;
                }
                _mesh.vertices = _pointVertices;
                _mesh.colors32 = _pointColors;
                _mesh.RecalculateBounds();
            }
        }
    }

    private void OnDestroy()
    {
        _kinect.StopCameras();
    }
}