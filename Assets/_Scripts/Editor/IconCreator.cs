using UnityEngine;
using UnityEditor;
using System.IO;

public class IconCreator : EditorWindow
{
    [Header("Icon Settings")]
    private GameObject _targetObject;
    private int _iconSize = 2048;
    private string _fileName = "Icon";
    private string _savePath = "Assets/Game/Sprites/Icons/Items/";

    [Header("Camera Settings")]
    private bool _useOrthographicCamera = false;
    private Vector3 _cameraPosition = new(0, 0, -5);
    private Vector3 _cameraRotation = new(0, 0, 0);
    private float _fieldOfView = 60f;
    private float _orthographicSize = 5f;
    private Color _backgroundColor = Color.clear;

    [Header("Lighting")]
    private bool _useCustomLighting = true;
    private Color _lightColor = Color.white;
    private float _lightIntensity = 1f;
    private Vector3 _lightDirection = new(-30, 30, 0);

    private Camera _iconCamera;
    private Light _iconLight;
    private RenderTexture _renderTexture;
    private Vector2 _scrollPosition;
    private Texture2D _previewTexture;
    private bool _showPreview = false;
    private RenderTexture _previewRenderTexture;

    [MenuItem("Tools/Custom/Icon Creator")]
    public static void ShowWindow() => GetWindow<IconCreator>("Icon Creator");

    private void OnEnable()
    {
        minSize = new Vector2(350, 500);
        maxSize = new Vector2(400, 800);
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.Space();

        DrawIconConfigurationSection();
        EditorGUILayout.Space();

        DrawCameraSettingsSection();
        EditorGUILayout.Space();

        DrawLightingSettingsSection();
        EditorGUILayout.Space();

        DrawActionButtons();
        EditorGUILayout.Space();

        DrawPreviewSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawPreviewSection()
    {
        if (_showPreview && _previewTexture != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(false));
            previewRect.width = 256;
            previewRect.height = 256;

            GUI.DrawTexture(previewRect, _previewTexture, ScaleMode.ScaleToFit, true);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Preview"))
            {
                UpdatePreview();
            }
            if (GUILayout.Button("Hide Preview"))
            {
                _showPreview = false;
            }
            EditorGUILayout.EndHorizontal();
        }
        // Help Section with improved formatting
        string helpText = _useOrthographicCamera
            ? "1. Select a GameObject from the scene\n2. Adjust camera and lighting settings\n3. Use 'Setup Preview' to position the camera\n4. Adjust Orthographic Size to frame the object\n5. Click 'Generate Icon' to create the PNG"
            : "1. Select a GameObject from the scene\n2. Adjust camera and lighting settings\n3. Use 'Setup Preview' to position the camera\n4. Click 'Generate Icon' to create the PNG";

        EditorGUILayout.HelpBox(helpText, MessageType.Info);
    }


    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Setup Preview", GUILayout.Height(25)))
        {
            SetupPreviewCamera();
        }
        if (GUILayout.Button("Generate Icon", GUILayout.Height(25)))
        {
            GenerateIcon();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Cleanup Preview", GUILayout.Height(25)))
        {
            CleanupPreview();
        }
    }


    private void DrawLightingSettingsSection()
    {
        EditorGUILayout.LabelField("Lighting Settings", EditorStyles.boldLabel);
        _useCustomLighting = EditorGUILayout.Toggle("Use Custom Lighting", _useCustomLighting);

        if (_useCustomLighting)
        {
            _lightColor = EditorGUILayout.ColorField("Light Color", _lightColor);
            _lightIntensity = EditorGUILayout.Slider("Light Intensity", _lightIntensity, 0f, 3f);
            _lightDirection = EditorGUILayout.Vector3Field("Light Direction", _lightDirection);
        }
    }

    private void DrawIconConfigurationSection()
    {
        EditorGUILayout.LabelField("Icon Settings", EditorStyles.boldLabel);
        _targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", _targetObject, typeof(GameObject), true);
        _iconSize = EditorGUILayout.IntSlider("Icon Size", _iconSize, 64, 2048);
        _fileName = EditorGUILayout.TextField("File Name", _fileName);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path", GUILayout.Width(EditorGUIUtility.labelWidth));
        _savePath = EditorPrefs.GetString("IconCreatorSavePath", _savePath);
        _savePath = EditorGUILayout.TextField(_savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Save Directory", _savePath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                _savePath = "Assets" + selectedPath[Application.dataPath.Length..];
                EditorPrefs.SetString("IconCreatorSavePath", _savePath);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawCameraSettingsSection()
    {
        EditorGUILayout.LabelField("Camera Settings", EditorStyles.boldLabel);
        _useOrthographicCamera = EditorGUILayout.Toggle("Use Orthographic Camera", _useOrthographicCamera);
        _cameraPosition = EditorGUILayout.Vector3Field("Camera Position", _cameraPosition);
        _cameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", _cameraRotation);

        if (_useOrthographicCamera)
        {
            _orthographicSize = EditorGUILayout.Slider("Orthographic Size", _orthographicSize, 0.1f, 20f);
        }
        else
        {
            _fieldOfView = EditorGUILayout.Slider("Field of View", _fieldOfView, 10f, 120f);
        }

        _backgroundColor = EditorGUILayout.ColorField("Background Color", _backgroundColor);
    }

    private void SetupPreviewCamera()
    {
        if (_targetObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a target object first.", "OK");
            return;
        }

        CleanupPreview();

        // Create camera for preview
        GameObject cameraObj = new("Icon Preview Camera");
        _iconCamera = cameraObj.AddComponent<Camera>();
        _iconCamera.transform.position = _cameraPosition;
        _iconCamera.transform.eulerAngles = _cameraRotation; // Preserve user-defined rotation

        // Configure camera projection
        _iconCamera.orthographic = _useOrthographicCamera;
        if (_useOrthographicCamera)
        {
            _iconCamera.orthographicSize = _orthographicSize;
        }
        else
        {
            _iconCamera.fieldOfView = _fieldOfView;
        }

        _iconCamera.backgroundColor = _backgroundColor;
        _iconCamera.clearFlags = CameraClearFlags.SolidColor;

        // Setup lighting if enabled
        if (_useCustomLighting)
        {
            GameObject lightObj = new GameObject("Icon Light");
            _iconLight = lightObj.AddComponent<Light>();
            _iconLight.type = LightType.Directional;
            _iconLight.color = _lightColor;
            _iconLight.intensity = _lightIntensity;
            _iconLight.transform.eulerAngles = _lightDirection;
        }

        // Focus scene view on the camera
        Selection.activeGameObject = cameraObj;
        SceneView.FrameLastActiveSceneView();
        // Add this line before the Debug.Log
        UpdatePreview();
        Debug.Log("Preview camera setup complete. Adjust position as needed, then generate the icon.");
    }

    private void UpdatePreview()
    {
        if (_iconCamera == null || _targetObject == null) return;

        // Create or update preview render texture
        if (_previewRenderTexture == null)
        {
            _previewRenderTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
        }

        // Store original camera settings
        RenderTexture originalTarget = _iconCamera.targetTexture;
        Color originalBackground = _iconCamera.backgroundColor;

        // Configure camera for preview
        _iconCamera.targetTexture = _previewRenderTexture;
        _iconCamera.backgroundColor = _backgroundColor;
        _iconCamera.Render();

        // Convert to Texture2D for display
        RenderTexture.active = _previewRenderTexture;
        if (_previewTexture == null)
        {
            _previewTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        }
        _previewTexture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        _previewTexture.Apply();

        // Restore camera settings
        _iconCamera.targetTexture = originalTarget;
        _iconCamera.backgroundColor = originalBackground;
        RenderTexture.active = null;

        _showPreview = true;
    }

    private void GenerateIcon()
    {
        if (_targetObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a target object first.", "OK");
            return;
        }

        if (_iconCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Please setup preview camera first.", "OK");
            return;
        }

        // Create directory if it doesn't exist
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
        }

        // Create render texture
        _renderTexture = new RenderTexture(_iconSize, _iconSize, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4
        };

        // Configure camera for final render
        Camera tempCamera = _iconCamera;
        tempCamera.targetTexture = _renderTexture;
        tempCamera.backgroundColor = Color.clear;
        tempCamera.clearFlags = CameraClearFlags.SolidColor;

        // Render the icon
        tempCamera.Render();

        // Convert to Texture2D
        RenderTexture.active = _renderTexture;
        Texture2D iconTexture = new Texture2D(_iconSize, _iconSize, TextureFormat.ARGB32, false);
        iconTexture.ReadPixels(new Rect(0, 0, _iconSize, _iconSize), 0, 0);
        iconTexture.Apply();

        // Save as PNG
        byte[] pngData = iconTexture.EncodeToPNG();
        string fullPath = Path.Combine(_savePath, _fileName + ".png");
        File.WriteAllBytes(fullPath, pngData);

        // Cleanup
        RenderTexture.active = null;
        tempCamera.targetTexture = null;
        DestroyImmediate(_renderTexture);
        DestroyImmediate(iconTexture);

        // Refresh asset database
        AssetDatabase.Refresh();

        // Configure import settings for transparency
        ConfigureTextureImportSettings(fullPath);

        EditorUtility.DisplayDialog("Success", $"Icon saved to: {fullPath}", "OK");

        // Ping the created asset
        Object createdAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
        EditorGUIUtility.PingObject(createdAsset);
    }

    private void ConfigureTextureImportSettings(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }
    }

    private void CleanupPreview()
    {
        if (_iconCamera != null)
        {
            DestroyImmediate(_iconCamera.gameObject);
            _iconCamera = null;
        }

        if (_iconLight != null)
        {
            DestroyImmediate(_iconLight.gameObject);
            _iconLight = null;
        }

        if (_renderTexture != null)
        {
            DestroyImmediate(_renderTexture);
            _renderTexture = null;
        }

        // Add these cleanup lines
        if (_previewRenderTexture != null)
        {
            DestroyImmediate(_previewRenderTexture);
            _previewRenderTexture = null;
        }

        if (_previewTexture != null)
        {
            DestroyImmediate(_previewTexture);
            _previewTexture = null;
        }

        _showPreview = false;
    }

    private void OnDestroy() => CleanupPreview();
}