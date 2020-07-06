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



    private void Update()
    {
        if (_texture == null)
        {
            return;
        }
        var viewMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1)) * transform.worldToLocalMatrix;
        Matrix4x4 projectionMatrix;
        if (_orthographic)
        {
            var orthographicWidth = _orthographicSize * _aspect;
            projectionMatrix = Matrix4x4.Ortho(-orthographicWidth, orthographicWidth, -_orthographicSize, _orthographicSize, _nearClipPlane, _farClipPlane);
        }
        else
        {
            projectionMatrix = Matrix4x4.Perspective(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);
        }
        projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, true);
        Shader.SetGlobalMatrix("_ProjectorMatrixVP", projectionMatrix * viewMatrix);
        Shader.SetGlobalTexture("_ProjectorTexture", _texture);
        // プロジェクターの位置を渡す
        // _ObjectSpaceLightPosのような感じでwに0が入っていたらOrthographicの前方方向とみなす
        var projectorPos = Vector4.zero;
        projectorPos = _orthographic ? transform.forward : transform.position;
        projectorPos.w = _orthographic ? 0 : 1;
        Shader.SetGlobalVector("_ProjectorPos", projectorPos);
    }

    private void OnDrawGizmos()
    {
        var gizmosMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        if (_orthographic)
        {
            var orthographicWidth = _orthographicSize * _aspect;
            var length = _farClipPlane - _nearClipPlane;
            var start = _nearClipPlane + length / 2;
            Gizmos.DrawWireCube(Vector3.forward * start, new Vector3(orthographicWidth * 2, _orthographicSize * 2, length));
        }
        else
        {
            Gizmos.DrawFrustum(Vector3.zero, _fieldOfView, _farClipPlane, _nearClipPlane, _aspect);
        }

        Gizmos.matrix = gizmosMatrix;
    }
}