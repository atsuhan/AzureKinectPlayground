using System;
using UniRx;
using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(10000)]
public class TextureProjector : MonoBehaviour
{
    [SerializeField, Range(0.0001f, 179)]
    private float _fieldOfView = 60;
    [SerializeField, Range(0.2f, 5.0f)]
    private float _aspect = 1.0f;
    [SerializeField, Range(0.0001f, 1000.0f)]
    private float _nearClipPlane = 0.01f;
    [SerializeField, Range(0.0001f, 1000.0f)]
    private float _farClipPlane = 100.0f;
    [SerializeField]
    private bool _orthographic = false;
    [SerializeField]
    private float _orthographicSize = 1.0f;
    [SerializeField]
    private Texture2D _texture;

    private void Start()
    {
        InitStream();
    }

    private void InitStream()
    {
        IObservable<Unit> moveStream = transform
            .ObserveEveryValueChanged(trans => trans.position)
            .Select(_ => Unit.Default);

        IObservable<Unit> rotationStream = transform
            .ObserveEveryValueChanged(trans => trans.rotation)
            .Select(_ => Unit.Default);

        Observable
            .Merge(moveStream, rotationStream)
            .Subscribe(_ => UpdateProjector())
            .AddTo(this);
    }

    private void UpdateProjector()
    {
        if (_texture == null) return;

        Matrix4x4 viewMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1)) * transform.worldToLocalMatrix;
        Matrix4x4 projectionMatrix;
        if (_orthographic)
        {
            float orthographicWidth = _orthographicSize * _aspect;
            projectionMatrix = Matrix4x4.Ortho(-orthographicWidth, orthographicWidth, -_orthographicSize, _orthographicSize, _nearClipPlane, _farClipPlane);
        }
        else
        {
            projectionMatrix = Matrix4x4.Perspective(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);
        }
        projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, true);
        Shader.SetGlobalMatrix("_ProjectorMatrixVP", projectionMatrix * viewMatrix);
        Shader.SetGlobalTexture("_ProjectorTexture", _texture);

        Vector4 projectorPos = Vector4.zero;
        projectorPos = _orthographic ? transform.forward : transform.position;
        projectorPos.w = _orthographic ? 0 : 1;
        Shader.SetGlobalVector("_ProjectorPos", projectorPos);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Matrix4x4 gizmosMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        if (_orthographic)
        {
            float orthographicWidth = _orthographicSize * _aspect;
            float length = _farClipPlane - _nearClipPlane;
            float start = _nearClipPlane + length / 2;
            Gizmos.DrawWireCube(Vector3.forward * start, new Vector3(orthographicWidth * 2, _orthographicSize * 2, length));
        }
        else
        {
            Gizmos.DrawFrustum(Vector3.zero, _fieldOfView, _farClipPlane, _nearClipPlane, _aspect);
        }

        Gizmos.matrix = gizmosMatrix;
    }
#endif
}