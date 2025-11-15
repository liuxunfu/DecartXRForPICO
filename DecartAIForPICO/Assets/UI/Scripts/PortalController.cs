using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PortalController : UIBehaviour
{
    [SerializeField] public Material portalMaterial;
    [SerializeField] private UIRect glow1;
    [SerializeField] private UIRect glow2;
    [SerializeField] private Transform glowParent;
    [SerializeField] private GameObject darkBG;
    [SerializeField] public float Radius = 1.0f;

    public UIFader fader;
    public Transform dummyPortal;
    
    private RectTransform _rectTransform;
    private Vector2 _lastSize;
    private float _lastRadius;

    private Vector2 _expandedSize = new (3300, 2200);
    
    private void Start()
    {
        Initialize();
        
        #if !UNITY_EDITOR
        if (darkBG != null)
        {
            darkBG.SetActive(false);
        }
        #endif
    }
    
    private void OnEnable()
    {
        Initialize();
        
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.update += EditorUpdate;
        }
#endif
    }
    
    private void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.update -= EditorUpdate;
        }
#endif
    }

    public void Show()
    {
        fader.FadeTo(1f, duration: 1f, delay: 0.5f);
    }

    public void Expand()
    {
        glowParent.transform.localScale = Vector3.one;
        dummyPortal.gameObject.SetActive(false);
        _rectTransform.DOSizeDelta(_expandedSize, 0.5f).SetEase(Ease.OutBack);
    }
    
    private void Initialize()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform == null)
        {
            Debug.LogError("PortalMaterial: No RectTransform found on this GameObject.");
            enabled = false;
            return;
        }
        
        UpdateRectSize();
        UpdateRadius();
    }
    
    private void Update()
    {
        if (Application.isPlaying)
        {
            CheckAndUpdateSize();
        }
    }
    
#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (this != null && !Application.isPlaying)
        {
            CheckAndUpdateSize();
        }
    }
#endif
    
    private void CheckAndUpdateSize()
    {
        if (_rectTransform != null && portalMaterial != null)
        {
            Vector2 currentSize = GetWorldSize();
            if (currentSize != _lastSize)
            {
                UpdateRectSize();
                _lastSize = currentSize;
            }
            
            if (Radius != _lastRadius)
            {
                UpdateRadius();
                _lastRadius = Radius;
            }
        }
    }
    
    private Vector2 GetWorldSize()
    {
        if (_rectTransform == null) return Vector2.zero;
        
        Vector3[] corners = new Vector3[4];
        _rectTransform.GetWorldCorners(corners);
        
        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);
        
        return new Vector2(width, height);
    }
    
    private void UpdateRectSize()
    {
        if (portalMaterial == null) return;
        
        Vector2 worldSize = GetWorldSize();
        portalMaterial.SetVector("_RectSize", new Vector4(worldSize.x, worldSize.y, 0, 0));
    }
    
    private void UpdateRadius()
    {
        if (portalMaterial == null) return;

        Vector2 worldSize = GetWorldSize();
        var maxRadius = Mathf.Min(worldSize.x, worldSize.y) / 2;
        var r = Mathf.Min(Radius, maxRadius);

        portalMaterial.SetVector("_Radius", Vector4.one*r);
        
        if (glow1 != null)
        {
            glow1.radius = Vector4.one*r*1000;
            glow1.SetVerticesDirty();
        }
        if (glow2 != null)
        {
            glow2.radius = Vector4.one*r*1000;
            glow2.SetVerticesDirty();
        }
    }
    
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        UpdateRectSize();
        UpdateRadius();
    }
    
    private void OnValidate()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();
            
        UpdateRectSize();
        UpdateRadius();
    }
}