using Cysharp.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;

public class PowerMapVisualizationController : MonoBehaviour
{
    [SerializeField] private int _resolutionX = 100;
    [SerializeField] private int _resolutionY = 100;
    [SerializeField] private int _intervalMsec = 10;

    private float[] _powerArr;
    private ComputeBuffer _powerBuffer = null;

    private int _bufferLength => _resolutionX * _resolutionY;

    private void Start()
    {
        InitPowerArrAndBuffer();
        InitShaderResolution();

        UpdatePowerData().Forget();
    }

    private void InitPowerArrAndBuffer()
    {
        _powerArr = new float[_bufferLength];
        _powerBuffer = new ComputeBuffer(_bufferLength, Marshal.SizeOf(typeof(float)));
    }

    private void InitShaderResolution()
    {
        Shader.SetGlobalInt("_PowerBufferWidth", _resolutionX);
        Shader.SetGlobalInt("_PowerBufferHeight", _resolutionY);
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
                    _powerArr[index] = (((float)index / _bufferLength) + Time.time) % 1;
                }
            }

            _powerBuffer.SetData(_powerArr);
            Shader.SetGlobalBuffer("_PowerBuffer", _powerBuffer);

            await UniTask.Delay(_intervalMsec);
        }
    }

    private void OnDisable()
    {
        _powerBuffer.Release();
    }
}