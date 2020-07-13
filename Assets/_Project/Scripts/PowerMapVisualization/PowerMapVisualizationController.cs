using Cysharp.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;

public class PowerMapVisualizationController : MonoBehaviour
{
    [SerializeField] private int _resolutionX = 100;
    [SerializeField] private int _resolutionY = 100;
    [SerializeField] private int _intervalMsec = 10;
    [SerializeField] private GameObject _targetObj = null;

    private Material _targetMaterial = null;
    private float[] _powerArr;
    private ComputeBuffer _powerBuffer = null;

    private int _bufferLength => _resolutionX * _resolutionY;

    private void Start()
    {
        InitPowerArrAndBuffer();
        InitMaterial();

        UpdatePowerData().Forget();
    }

    private void InitPowerArrAndBuffer()
    {
        _powerArr = new float[_bufferLength];
        _powerBuffer = new ComputeBuffer(_bufferLength, Marshal.SizeOf(typeof(float)));
    }

    private void InitMaterial()
    {
        _targetMaterial = _targetObj.GetComponent<Renderer>().material;
        _targetMaterial.SetInt("_ResolutionX", _resolutionX);
        _targetMaterial.SetInt("_ResolutionY", _resolutionY);
    }

    private async UniTaskVoid UpdatePowerData()
    {
        while (true)
        {
            for (int x = 0; x < _resolutionX; x++)
            {
                for (int y = 0; y < _resolutionY; y++)
                {
                    int index = x + y * _resolutionX;
                    //_powerArr[index] = Mathf.Abs(Mathf.Sin(x / _resolutionX * Mathf.PI + Time.time));
                    _powerArr[index] = UnityEngine.Random.Range(0f, 1f);
                }
            }

            _powerBuffer.SetData(_powerArr);
            _targetMaterial.SetBuffer("_PowerBuffer", _powerBuffer);

            await UniTask.Delay(_intervalMsec);
        }
    }

    private void OnDisable()
    {
        _powerBuffer.Release();
    }
}