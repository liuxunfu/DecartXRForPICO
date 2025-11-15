using UnityEngine;

public class ShaderManager : MonoBehaviour
{
    public static ShaderManager instance = null;

    public Shader m_UIRect;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
