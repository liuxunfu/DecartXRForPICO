#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(UIRect)), CanEditMultipleObjects]
public class UIRectEditor : Editor
{
    private static bool showBorder;
    private static bool showShadow;
    private static bool showBevel;
    private bool _hasShadow;
    private bool _hasBevel;

    SerializedProperty _color;
    SerializedProperty _sprite;
    SerializedProperty _radius;

    SerializedProperty _fillColor;
    SerializedProperty _borderColor;
    SerializedProperty _borderWidth;
    SerializedProperty _borderAlign;
    
    SerializedProperty _shadowEnabled;
    SerializedProperty _shadowColor;
    SerializedProperty _shadowSize;
    SerializedProperty _shadowSpread;
    SerializedProperty _shadowOffset;

    SerializedProperty _bevelWidth;
    SerializedProperty _bevelStrength;

    void OnEnable()
    {
        // Fetch the properties from the GameObject script to display in the inspector
        _color = serializedObject.FindProperty("m_Color");
        _sprite = serializedObject.FindProperty("m_Sprite");
        _radius = serializedObject.FindProperty("radius");

        _fillColor = serializedObject.FindProperty("fillColor");
        _borderColor = serializedObject.FindProperty("borderColor");
        _borderWidth = serializedObject.FindProperty("borderWidth");
        _borderAlign = serializedObject.FindProperty("borderAlign");
        
        _shadowEnabled = serializedObject.FindProperty("hasShadow");
        _shadowColor = serializedObject.FindProperty("shadowColor");
        _shadowSize = serializedObject.FindProperty("shadowSize");
        _shadowSpread = serializedObject.FindProperty("shadowSpread");
        _shadowOffset = serializedObject.FindProperty("shadowOffset");
        
        _bevelWidth = serializedObject.FindProperty("bevelWidth");
        _bevelStrength = serializedObject.FindProperty("bevelStrength");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var box = (UIRect)target;

        if (box.canvas == null ||
            !box.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord1) ||
            !box.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord2) ||
            !box.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord3))
        {
            EditorGUILayout.HelpBox("Make sure that \"TexCoord1\", \"TexCoord2\" and \"TexCoord3\" are enabled" +
                                    " in \"Additional Shader Channels\" on the Canvas", MessageType.Error, true);
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_color, new GUIContent("Tint Color"));
        EditorGUILayout.PropertyField(_fillColor);
        EditorGUILayout.PropertyField(_sprite);
        box.independentCorners = EditorGUILayout.ToggleLeft("Independent Corners", box.independentCorners);
        if (box.independentCorners)
        {
            _radius.vector4Value =
                Vector4.Max(EditorGUILayout.Vector4Field("Corner Radius", box.radius), Vector4.zero);
        }
        else
        {
            var radius = Mathf.Max(EditorGUILayout.FloatField("Corner Radius", box.radius.x), 0);
            _radius.vector4Value = Vector4.one * radius;
        }

        GUI.enabled = true;

        GUILayout.Space(10);

        BeginFoldOutGroup("Border", ref showBorder);
        if (showBorder)
        {
            // _borderColor.colorValue = EditorGUILayout.ColorField("Border", box.borderColor);
            EditorGUILayout.PropertyField(_borderColor);
            _borderWidth.floatValue =
                Mathf.Max(EditorGUILayout.FloatField("Border Thickness", box.borderWidth), 0);
            EditorGUILayout.PropertyField(_borderAlign);

            // _borderMode.enumValueIndex =  EditorGUILayout.EnumPopup("Border Mode", rect.borderMode);
        }
        EndFoldOutGroup();

        _hasShadow = _shadowEnabled.boolValue;
        BeginFoldOutGroup("Shadow / Glow", ref showShadow, ref _hasShadow);
        if (showShadow)
        {
            EditorGUILayout.PropertyField(_shadowColor);
            _shadowSize.floatValue = Mathf.Max(EditorGUILayout.FloatField("Shadow Size", box.shadowSize), 0);
            // _shadowSpread.floatValue = EditorGUILayout.FloatField("Shadow Spread", box.shadowSpread);
            EditorGUILayout.PropertyField(_shadowOffset);
        }
        EndFoldOutGroup();
    
        _shadowEnabled.boolValue = _hasShadow;
        
        // _hasBevel = _bevelEnabled.boolValue;
        BeginFoldOutGroup("Bevel", ref showBevel);
        if (showBevel)
        {
            EditorGUILayout.PropertyField(_bevelWidth);
            EditorGUILayout.PropertyField(_bevelStrength);
        }
        EndFoldOutGroup();
    
        // _bevelEnabled.boolValue = _hasBevel;    
        
        GUILayout.Space(5);

        if (EditorGUI.EndChangeCheck())
        {
            box.SetAllDirty();
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void BeginFoldOutGroup(string name, ref bool foldOut, ref bool isEnabled)
    {
        BeginFoldOutGroupBase(name, ref foldOut);
        GUILayout.Label("Enable");
        isEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(15));
        GUILayout.EndHorizontal();
        GUI.enabled = isEnabled;
    }

    private void BeginFoldOutGroup(string name, ref bool foldOut)
    {
        BeginFoldOutGroupBase(name, ref foldOut);
        GUILayout.EndHorizontal();
    }

    private void BeginFoldOutGroupBase(string name, ref bool foldOut)
    {
        var foldOutBodyStyle = EditorStyles.helpBox;
        foldOutBodyStyle.padding = new RectOffset(1, 1, 1, 0);
        GUILayout.BeginVertical(foldOutBodyStyle);
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Space(15);
        foldOut = EditorGUILayout.Foldout(foldOut, name, true);
        GUILayout.FlexibleSpace();
    }


    private void EndFoldOutGroup()
    {
        GUI.enabled = true;
        GUILayout.EndVertical();
    }
}

#endif