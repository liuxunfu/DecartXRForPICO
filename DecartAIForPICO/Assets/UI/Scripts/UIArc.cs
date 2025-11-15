using UnityEngine;
using UnityEngine.Serialization;
using Image = UnityEngine.UI.Image;


[ExecuteAlways]
[DisallowMultipleComponent]
public class UIArc : Image
{
    #region Static Cache

    /// Cached material & shader
    private static Material _material;
    private static Shader _shader;
    const string SHADER_NAME = "UI/UIArc";
    
    private static Material GetMaterial()
    {
        _shader ??= Shader.Find(SHADER_NAME);

        if (_material == null)
        {
            _material = new Material(_shader);
        }
        return _material;
    }

    #endregion

    #region Public
    
    public Vector2 Size => rectTransform.rect.size;

    // TODD make these properties private

    [FormerlySerializedAs("fillColor")] public Color FillColor = new(1, 1, 1, 1);

    // top-left | top-right | bottom-right | bottom-left
    public bool independentCorners = true;
    [FormerlySerializedAs("radius")] public Vector4 Radius = Vector4.zero;
    [FormerlySerializedAs("translate")] public Vector3 Translate = Vector3.zero;

    // Border
    [FormerlySerializedAs("borderColor")] public Color BorderColor = new(0, 0, 0, 1);
    [FormerlySerializedAs("borderWidth")] public float BorderWidth = 0;
    [FormerlySerializedAs("borderAlign")] public BorderAlign BorderAlign = BorderAlign.Inside;

    // Shadow
    [FormerlySerializedAs("hasShadow")] public bool HasShadow = false;
    [FormerlySerializedAs("shadowColor")] public Color ShadowColor = new(0, 0, 0, 0.5f);
    [FormerlySerializedAs("shadowSize")] public float ShadowSize = 10;
    [FormerlySerializedAs("shadowSpread")] public float ShadowSpread = 0;
    [FormerlySerializedAs("shadowOffset")] public Vector3 ShadowOffset = new Vector2(5, -5);

    // Bevel
    [FormerlySerializedAs("bevelWidth")] public float BevelWidth = 0;
    [FormerlySerializedAs("bevelStrength")] public float BevelStrength = 1;
    
    #endregion

    #region Private
    
    // private Sequence _sequence;
    private float _t;

    // The default shared material instance used for rendering
    public override Material defaultMaterial => GetMaterial();
    
    
    #endregion
}
