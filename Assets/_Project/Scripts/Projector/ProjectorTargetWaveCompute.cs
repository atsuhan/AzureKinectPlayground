using UnityEngine;

public class ProjectorTargetWaveCompute : MonoBehaviour
{
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private float _deltaSize = 0.1f;
    [SerializeField] private float _waveCoef = 1.0f;
    private RenderTexture _waveTexture;
    private RenderTexture _drawTexture;

    private int kernelInitialize, kernelAddWave, kernelUpdate, kernelDraw;
    private ThreadSize threadSizeInitialize, threadSizeUpdate, threadSizeDraw;

    private struct ThreadSize
    {
        public int x;
        public int y;
        public int z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }
    }

    private void Start()
    {
        // カーネルIdの取得
        kernelInitialize = _computeShader.FindKernel("Initialize");
        kernelAddWave = _computeShader.FindKernel("AddWave");
        kernelUpdate = _computeShader.FindKernel("Update");
        kernelDraw = _computeShader.FindKernel("Draw");

        // 波の高さを格納するテクスチャの作成
        _waveTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.RG32);
        _waveTexture.wrapMode = TextureWrapMode.Clamp;
        _waveTexture.enableRandomWrite = true;
        _waveTexture.Create();
        // レンダリング用のテクスチャの作成
        _drawTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
        _drawTexture.enableRandomWrite = true;
        _drawTexture.Create();

        // スレッド数の取得
        uint threadSizeX, threadSizeY, threadSizeZ;
        _computeShader.GetKernelThreadGroupSizes(kernelInitialize, out threadSizeX, out threadSizeY, out threadSizeZ);
        threadSizeInitialize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);
        _computeShader.GetKernelThreadGroupSizes(kernelUpdate, out threadSizeX, out threadSizeY, out threadSizeZ);
        threadSizeUpdate = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);
        _computeShader.GetKernelThreadGroupSizes(kernelDraw, out threadSizeX, out threadSizeY, out threadSizeZ);
        threadSizeDraw = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

        // 波の高さの初期化
        _computeShader.SetTexture(kernelInitialize, "waveTexture", _waveTexture);
        _computeShader.Dispatch(kernelInitialize, Mathf.CeilToInt(_waveTexture.width / threadSizeInitialize.x), Mathf.CeilToInt(_waveTexture.height / threadSizeInitialize.y), 1);
    }

    private void FixedUpdate()
    {
        // 波の追加
        _computeShader.SetFloat("time", Time.time);
        _computeShader.SetTexture(kernelAddWave, "waveTexture", _waveTexture);
        _computeShader.Dispatch(kernelAddWave, Mathf.CeilToInt(_waveTexture.width / threadSizeUpdate.x), Mathf.CeilToInt(_waveTexture.height / threadSizeUpdate.y), 1);

        // 波の高さの更新
        _computeShader.SetFloat("deltaSize", _deltaSize);
        _computeShader.SetFloat("deltaTime", Time.deltaTime * 2.0f);
        _computeShader.SetFloat("waveCoef", _waveCoef);
        _computeShader.SetTexture(kernelUpdate, "waveTexture", _waveTexture);
        _computeShader.Dispatch(kernelUpdate, Mathf.CeilToInt(_waveTexture.width / threadSizeUpdate.x), Mathf.CeilToInt(_waveTexture.height / threadSizeUpdate.y), 1);

        // 波の高さをもとにレンダリング用のテクスチャを作成
        _computeShader.SetTexture(kernelDraw, "waveTexture", _waveTexture);
        _computeShader.SetTexture(kernelDraw, "drawTexture", _drawTexture);
        _computeShader.Dispatch(kernelDraw, Mathf.CeilToInt(_waveTexture.width / threadSizeDraw.x), Mathf.CeilToInt(_waveTexture.height / threadSizeDraw.y), 1);
        Shader.SetGlobalTexture("_ProjectorTexture", _drawTexture);
    }
}