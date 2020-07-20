using UnityEngine;

public class ProjectorTargetTexture : MonoBehaviour
{
    [SerializeField]
    private Texture2D _texture;

    private void Start()
    {
        Shader.SetGlobalTexture("_ProjectorTexture", _texture);
    }
}