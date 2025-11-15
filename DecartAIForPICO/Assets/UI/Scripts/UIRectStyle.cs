using UnityEngine;

public enum BorderAlign
{
    Middle, Outside, Inside
}

public enum BoxRenderMode
{
    Fill, Shadow, Bevel
}

public struct UIRectStyle
{
    public Color? BackgroundColor;
    public Vector4? Radius;
    public Vector3? Translate;
    
    // Border
    public Color? BorderColor;
    public float? BorderWidth;
    public BorderAlign? BorderAlign;
    
    // Shadow
    public Color? ShadowColor;
    public float? ShadowSize;
    public float? ShadowSpread;
    public Vector3? ShadowOffset;
    
    // Bevel
    public float? BevelWidth;
    public float? BevelStrength;
    
    public static UIRectStyle Lerp(UIRectStyle s1, UIRectStyle s2, float t)
    {
        return new UIRectStyle()
        {
            BackgroundColor = (s1.BackgroundColor == null || s2.BackgroundColor == null) ? null :
                Color.Lerp((Color)s1.BackgroundColor, (Color)s2.BackgroundColor, t),
            Radius = (s1.Radius == null || s2.Radius == null) ? null :
                Vector4.Lerp((Vector4)s1.Radius, (Vector4)s2.Radius, t),
            Translate = (s1.Translate == null || s2.Translate == null) ? null :
                Vector3.Lerp((Vector3)s1.Translate, (Vector3)s2.Translate, t),

            
            BorderColor = (s1.BorderColor == null || s2.BorderColor == null) ? null :
                Color.Lerp((Color)s1.BorderColor, (Color)s2.BorderColor, t),
            BorderWidth = (s1.BorderWidth == null || s2.BorderWidth == null) ? null :
                Mathf.Lerp((float)s1.BorderWidth, (float)s2.BorderWidth, t),
            
            ShadowColor = (s1.ShadowColor == null || s2.ShadowColor == null) ? null :
                Color.Lerp((Color)s1.ShadowColor, (Color)s2.ShadowColor, t),
            ShadowSize = (s1.ShadowSize == null || s2.ShadowSize == null) ? null :
                Mathf.Lerp((float)s1.ShadowSize, (float)s2.ShadowSize, t),
            ShadowSpread = (s1.ShadowSpread == null || s2.ShadowSpread == null) ? null :
                Mathf.Lerp((float)s1.ShadowSpread, (float)s2.ShadowSpread, t),
            ShadowOffset = (s1.ShadowOffset == null || s2.ShadowOffset == null) ? null :
                Vector3.Lerp((Vector3)s1.ShadowOffset, (Vector3)s2.ShadowOffset, t),
            
            BevelWidth = (s1.BevelWidth == null || s2.BevelWidth == null) ? null :
                Mathf.Lerp((float)s1.BevelWidth, (float)s2.BevelWidth, t),
            BevelStrength = (s1.BevelStrength == null || s2.BevelStrength == null) ? null :
                Mathf.Lerp((float)s1.BevelStrength, (float)s2.BevelStrength, t),

        };
    }
}