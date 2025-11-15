using UnityEngine;
using UnityEngine.UI;

public class SetStreamingRawImage : MonoBehaviour
{
    public RawImage streamingRawImage;

    private void Awake()
    {
#if !UNITY_EDITOR
        streamingRawImage.uvRect = new Rect(0, 1, 1, -1);
#endif
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
