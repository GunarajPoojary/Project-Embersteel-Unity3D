using UnityEditor;
using UnityEngine;
using ProjectEmbersteel.Equipment;

[CustomEditor(typeof(EquipmentSO), true)]
public class EquipmentSOEditor : Editor
{
    SerializedProperty iconProp;

    private void OnEnable()
    {
        if (serializedObject != null)
            iconProp = serializedObject.FindProperty("_icon");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        bool expanded = true;

        while (property.NextVisible(expanded))
        {
            EditorGUILayout.PropertyField(property, true);
            expanded = false;

            // Draw preview right after the Icon field
            if (property.name == "_icon" && iconProp != null && iconProp.objectReferenceValue != null)
            {
                Texture2D sprite = AssetPreview.GetAssetPreview(iconProp.objectReferenceValue);
                if (sprite != null)
                {
                    Rect rect = GUILayoutUtility.GetRect(120, 120, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(rect, sprite, ScaleMode.ScaleToFit);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}