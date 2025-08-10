using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PatternCreatorWindow : EditorWindow
{
    [System.Serializable]
    public class PatternElement
    {
        public Texture2D image;
        public Vector2 position = Vector2.zero;
        public Vector2 scale = Vector2.one;
        public float rotation = 0f;
        public bool randomizePosition = false;
        public bool randomizeRotation = false;
        public bool randomizeScale = false;
        public Vector2 positionRange = new Vector2(0.1f, 0.1f);
        public Vector2 rotationRange = new Vector2(-45f, 45f);
        public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        public float opacity = 1f;
        public bool enabled = true;
    }

    public enum PatternType
    {
        Grid,
        HexGrid,
        Random,
        Radial,
        Spiral,
        Wave,
        Custom
    }

    public enum BlendMode
    {
        Normal,
        Multiply,
        Screen,
        Overlay,
        SoftLight
    }

    // Pattern settings
    private List<PatternElement> patternElements = new List<PatternElement>();
    private Texture2D backgroundImage;
    private Color backgroundColor = Color.white;
    private bool useBackgroundImage = false;
    
    // Pattern properties
    private PatternType patternType = PatternType.Grid;
    private Vector2Int patternSize = new Vector2Int(512, 512);
    private Vector2Int repetitions = new Vector2Int(4, 4);
    private float spacing = 1f;
    private float patternOffset = 0f;
    private BlendMode blendMode = BlendMode.Normal;
    
    // Randomization
    private int randomSeed = 0;
    private bool useRandomSeed = true;
    
    // Preview and output
    private Texture2D previewTexture;
    private Vector2 scrollPosition;
    private bool autoPreview = true;
    private string outputPath = "Assets/GeneratedPatterns/";
    private string fileName = "Pattern";

    [MenuItem("Tools/Pattern Creator")]
    public static void ShowWindow()
    {
        GetWindow<PatternCreatorWindow>("Pattern Creator");
    }

    private void OnEnable()
    {
        if (patternElements.Count == 0)
        {
            patternElements.Add(new PatternElement());
        }
        
        if (useRandomSeed)
        {
            randomSeed = Random.Range(0, 10000);
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawPatternSettings();
        EditorGUILayout.Space(10);
        
        DrawBackgroundSettings();
        EditorGUILayout.Space(10);
        
        DrawPatternElements();
        EditorGUILayout.Space(10);
        
        DrawRandomizationSettings();
        EditorGUILayout.Space(10);
        
        DrawPreviewSettings();
        EditorGUILayout.Space(10);
        
        DrawOutputSettings();

        EditorGUILayout.EndScrollView();

        if (autoPreview && GUI.changed)
        {
            GeneratePreview();
        }
    }

    private void DrawPatternSettings()
    {
        EditorGUILayout.LabelField("Pattern Settings", EditorStyles.boldLabel);
        
        patternType = (PatternType)EditorGUILayout.EnumPopup("Pattern Type", patternType);
        patternSize = EditorGUILayout.Vector2IntField("Texture Size", patternSize);
        repetitions = EditorGUILayout.Vector2IntField("Repetitions", repetitions);
        spacing = EditorGUILayout.Slider("Spacing", spacing, 0.1f, 3f);
        
        if (patternType == PatternType.Wave || patternType == PatternType.Spiral)
        {
            patternOffset = EditorGUILayout.Slider("Pattern Offset", patternOffset, 0f, 360f);
        }
        
        blendMode = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", blendMode);
    }

    private void DrawBackgroundSettings()
    {
        EditorGUILayout.LabelField("Background Settings", EditorStyles.boldLabel);
        
        useBackgroundImage = EditorGUILayout.Toggle("Use Background Image", useBackgroundImage);
        
        if (useBackgroundImage)
        {
            backgroundImage = (Texture2D)EditorGUILayout.ObjectField("Background Image", backgroundImage, typeof(Texture2D), false);
        }
        else
        {
            backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
        }
    }

    private void DrawPatternElements()
    {
        EditorGUILayout.LabelField("Pattern Elements", EditorStyles.boldLabel);
        
        for (int i = 0; i < patternElements.Count; i++)
        {
            DrawPatternElement(i);
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Element"))
        {
            patternElements.Add(new PatternElement());
        }
        if (GUILayout.Button("Remove Last") && patternElements.Count > 1)
        {
            patternElements.RemoveAt(patternElements.Count - 1);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPatternElement(int index)
    {
        var element = patternElements[index];
        
        EditorGUILayout.BeginVertical("box");
        
        element.enabled = EditorGUILayout.Toggle($"Element {index + 1} Enabled", element.enabled);
        
        if (element.enabled)
        {
            element.image = (Texture2D)EditorGUILayout.ObjectField("Image", element.image, typeof(Texture2D), false);
            element.position = EditorGUILayout.Vector2Field("Position", element.position);
            element.scale = EditorGUILayout.Vector2Field("Scale", element.scale);
            element.rotation = EditorGUILayout.Slider("Rotation", element.rotation, 0f, 360f);
            element.opacity = EditorGUILayout.Slider("Opacity", element.opacity, 0f, 1f);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Randomization", EditorStyles.miniBoldLabel);
            
            element.randomizePosition = EditorGUILayout.Toggle("Randomize Position", element.randomizePosition);
            if (element.randomizePosition)
            {
                element.positionRange = EditorGUILayout.Vector2Field("Position Range", element.positionRange);
            }
            
            element.randomizeRotation = EditorGUILayout.Toggle("Randomize Rotation", element.randomizeRotation);
            if (element.randomizeRotation)
            {
                element.rotationRange = EditorGUILayout.Vector2Field("Rotation Range", element.rotationRange);
            }
            
            element.randomizeScale = EditorGUILayout.Toggle("Randomize Scale", element.randomizeScale);
            if (element.randomizeScale)
            {
                element.scaleRange = EditorGUILayout.Vector2Field("Scale Range", element.scaleRange);
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawRandomizationSettings()
    {
        EditorGUILayout.LabelField("Randomization Settings", EditorStyles.boldLabel);
        
        useRandomSeed = EditorGUILayout.Toggle("Use Random Seed", useRandomSeed);
        
        EditorGUILayout.BeginHorizontal();
        randomSeed = EditorGUILayout.IntField("Seed", randomSeed);
        if (GUILayout.Button("New Seed", GUILayout.Width(80)))
        {
            randomSeed = Random.Range(0, 10000);
            if (autoPreview) GeneratePreview();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPreviewSettings()
    {
        EditorGUILayout.LabelField("Preview Settings", EditorStyles.boldLabel);
        
        autoPreview = EditorGUILayout.Toggle("Auto Preview", autoPreview);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Preview"))
        {
            GeneratePreview();
        }
        if (GUILayout.Button("Clear Preview"))
        {
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
                previewTexture = null;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (previewTexture != null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            
            float previewSize = Mathf.Min(position.width - 40, 300);
            float aspectRatio = (float)previewTexture.height / previewTexture.width;
            
            Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize * aspectRatio);
            EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
        }
    }

    private void DrawOutputSettings()
    {
        EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputPath = "Assets" + path.Substring(Application.dataPath.Length) + "/";
            }
        }
        EditorGUILayout.EndHorizontal();
        
        fileName = EditorGUILayout.TextField("File Name", fileName);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Pattern"))
        {
            SavePattern();
        }
        if (GUILayout.Button("Save as PNG"))
        {
            SavePatternAsPNG();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void GeneratePreview()
    {
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
        }
        
        previewTexture = GeneratePattern();
        Repaint();
    }

    private Texture2D GeneratePattern()
    {
        Random.State oldState = Random.state;
        Random.InitState(randomSeed);
        
        Texture2D pattern = new Texture2D(patternSize.x, patternSize.y, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[patternSize.x * patternSize.y];
        
        // Initialize background
        Color bgColor = useBackgroundImage && backgroundImage != null ? Color.white : backgroundColor;
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgColor;
        }
        
        // Apply background image if specified
        if (useBackgroundImage && backgroundImage != null)
        {
            ApplyBackgroundImage(pixels, backgroundImage);
        }
        
        // Generate pattern positions based on pattern type
        List<Vector2> positions = GeneratePatternPositions();
        
        // Apply pattern elements
        foreach (var position in positions)
        {
            foreach (var element in patternElements)
            {
                if (element.enabled && element.image != null)
                {
                    ApplyPatternElement(pixels, element, position);
                }
            }
        }
        
        pattern.SetPixels(pixels);
        pattern.Apply();
        
        Random.state = oldState;
        return pattern;
    }

    private void ApplyBackgroundImage(Color[] pixels, Texture2D bgImage)
    {
        for (int y = 0; y < patternSize.y; y++)
        {
            for (int x = 0; x < patternSize.x; x++)
            {
                float u = (float)x / patternSize.x;
                float v = (float)y / patternSize.y;
                
                Color bgPixel = bgImage.GetPixelBilinear(u, v);
                pixels[y * patternSize.x + x] = bgPixel;
            }
        }
    }

    private List<Vector2> GeneratePatternPositions()
    {
        List<Vector2> positions = new List<Vector2>();
        
        switch (patternType)
        {
            case PatternType.Grid:
                positions = GenerateGridPositions();
                break;
            case PatternType.HexGrid:
                positions = GenerateHexGridPositions();
                break;
            case PatternType.Random:
                positions = GenerateRandomPositions();
                break;
            case PatternType.Radial:
                positions = GenerateRadialPositions();
                break;
            case PatternType.Spiral:
                positions = GenerateSpiralPositions();
                break;
            case PatternType.Wave:
                positions = GenerateWavePositions();
                break;
        }
        
        return positions;
    }

    private List<Vector2> GenerateGridPositions()
    {
        List<Vector2> positions = new List<Vector2>();
        
        for (int y = 0; y < repetitions.y; y++)
        {
            for (int x = 0; x < repetitions.x; x++)
            {
                float posX = (float)x / (repetitions.x - 1) * patternSize.x;
                float posY = (float)y / (repetitions.y - 1) * patternSize.y;
                positions.Add(new Vector2(posX, posY));
            }
        }
        
        return positions;
    }

    private List<Vector2> GenerateHexGridPositions()
    {
        List<Vector2> positions = new List<Vector2>();
        
        float hexWidth = (float)patternSize.x / repetitions.x;
        float hexHeight = (float)patternSize.y / repetitions.y;
        
        for (int y = 0; y < repetitions.y; y++)
        {
            for (int x = 0; x < repetitions.x; x++)
            {
                float posX = x * hexWidth;
                float posY = y * hexHeight;
                
                if (y % 2 == 1)
                {
                    posX += hexWidth * 0.5f;
                }
                
                positions.Add(new Vector2(posX, posY));
            }
        }
        
        return positions;
    }

    private List<Vector2> GenerateRandomPositions()
    {
        List<Vector2> positions = new List<Vector2>();
        
        int totalElements = repetitions.x * repetitions.y;
        
        for (int i = 0; i < totalElements; i++)
        {
            float posX = Random.Range(0f, patternSize.x);
            float posY = Random.Range(0f, patternSize.y);
            positions.Add(new Vector2(posX, posY));
        }
        
        return positions;
    }

    private List<Vector2> GenerateRadialPositions()
    {
        List<Vector2> positions = new List<Vector2>();
        
        Vector2 center = new Vector2(patternSize.x * 0.5f, patternSize.y * 0.5f);
        float maxRadius = Mathf.Min(patternSize.x, patternSize.y) * 0.4f;
        
        for (int ring = 0; ring < repetitions.x; ring++)
        {
            int elementsInRing = ring == 0 ? 1 : repetitions.y * ring;
            float radius = (float)ring / (repetitions.x - 1) * maxRadius;
            
            for (int i = 0; i < elementsInRing; i++)
            {
                float angle = (float)i / elementsInRing * 360f + patternOffset;
                float radians = angle * Mathf.Deg2Rad;
                
                float posX = center.x + Mathf.Cos(radians) * radius;
                float posY = center.y + Mathf.Sin(radians) * radius;
                
                positions.Add(new Vector2(posX, posY));
            }
        }
        
        return positions;
    }

    private List<Vector2> GenerateSpiralPositions()
    {
        List<Vector2> positions = new List<Vector2>();
        
        Vector2 center = new Vector2(patternSize.x * 0.5f, patternSize.y * 0.5f);
        float maxRadius = Mathf.Min(patternSize.x, patternSize.y) * 0.4f;
        int totalElements = repetitions.x * repetitions.y;
        
        for (int i = 0; i < totalElements; i++)
        {
            float t = (float)i / (totalElements - 1);
            float angle = t * 720f + patternOffset; // Two full rotations
            float radius = t * maxRadius;
            float radians = angle * Mathf.Deg2Rad;
            
            float posX = center.x + Mathf.Cos(radians) * radius;
            float posY = center.y + Mathf.Sin(radians) * radius;
            
            positions.Add(new Vector2(posX, posY));
        }
        
        return positions;
    }

    private List<Vector2> GenerateWavePositions()
    {
        List<Vector2> positions = new List<Vector2>();
        
        for (int y = 0; y < repetitions.y; y++)
        {
            for (int x = 0; x < repetitions.x; x++)
            {
                float posX = (float)x / (repetitions.x - 1) * patternSize.x;
                float baseY = (float)y / (repetitions.y - 1) * patternSize.y;
                
                float waveOffset = Mathf.Sin((posX / patternSize.x * 4f + patternOffset * Mathf.Deg2Rad)) * 20f;
                float posY = baseY + waveOffset;
                
                positions.Add(new Vector2(posX, posY));
            }
        }
        
        return positions;
    }

    private void ApplyPatternElement(Color[] pixels, PatternElement element, Vector2 basePosition)
    {
        Vector2 position = basePosition + element.position;
        Vector2 scale = element.scale;
        float rotation = element.rotation;
        
        // Apply randomization
        if (element.randomizePosition)
        {
            position.x += Random.Range(-element.positionRange.x, element.positionRange.x);
            position.y += Random.Range(-element.positionRange.y, element.positionRange.y);
        }
        
        if (element.randomizeRotation)
        {
            rotation += Random.Range(element.rotationRange.x, element.rotationRange.y);
        }
        
        if (element.randomizeScale)
        {
            float scaleMultiplier = Random.Range(element.scaleRange.x, element.scaleRange.y);
            scale *= scaleMultiplier;
        }
        
        // Calculate element bounds
        int elementWidth = Mathf.RoundToInt(element.image.width * scale.x);
        int elementHeight = Mathf.RoundToInt(element.image.height * scale.y);
        
        Vector2 center = new Vector2(elementWidth * 0.5f, elementHeight * 0.5f);
        float rotRad = rotation * Mathf.Deg2Rad;
        
        for (int y = 0; y < elementHeight; y++)
        {
            for (int x = 0; x < elementWidth; x++)
            {
                // Apply rotation
                Vector2 localPos = new Vector2(x - center.x, y - center.y);
                Vector2 rotatedPos = new Vector2(
                    localPos.x * Mathf.Cos(rotRad) - localPos.y * Mathf.Sin(rotRad),
                    localPos.x * Mathf.Sin(rotRad) + localPos.y * Mathf.Cos(rotRad)
                );
                
                int finalX = Mathf.RoundToInt(position.x + rotatedPos.x);
                int finalY = Mathf.RoundToInt(position.y + rotatedPos.y);
                
                if (finalX >= 0 && finalX < patternSize.x && finalY >= 0 && finalY < patternSize.y)
                {
                    float u = (float)x / elementWidth;
                    float v = (float)y / elementHeight;
                    
                    Color elementColor = element.image.GetPixelBilinear(u, v);
                    elementColor.a *= element.opacity;
                    
                    int pixelIndex = finalY * patternSize.x + finalX;
                    pixels[pixelIndex] = BlendColors(pixels[pixelIndex], elementColor, blendMode);
                }
            }
        }
    }

    private Color BlendColors(Color baseColor, Color blendColor, BlendMode mode)
    {
        Color result = baseColor;
        
        switch (mode)
        {
            case BlendMode.Normal:
                result = Color.Lerp(baseColor, blendColor, blendColor.a);
                break;
            case BlendMode.Multiply:
                result = baseColor * blendColor;
                result.a = Mathf.Max(baseColor.a, blendColor.a);
                break;
            case BlendMode.Screen:
                result = Color.white - (Color.white - baseColor) * (Color.white - blendColor);
                result.a = Mathf.Max(baseColor.a, blendColor.a);
                break;
            case BlendMode.Overlay:
                result.r = baseColor.r < 0.5f ? 2f * baseColor.r * blendColor.r : 1f - 2f * (1f - baseColor.r) * (1f - blendColor.r);
                result.g = baseColor.g < 0.5f ? 2f * baseColor.g * blendColor.g : 1f - 2f * (1f - baseColor.g) * (1f - blendColor.g);
                result.b = baseColor.b < 0.5f ? 2f * baseColor.b * blendColor.b : 1f - 2f * (1f - baseColor.b) * (1f - blendColor.b);
                result.a = Mathf.Max(baseColor.a, blendColor.a);
                break;
            case BlendMode.SoftLight:
                result.r = blendColor.r < 0.5f ? 2f * baseColor.r * blendColor.r + baseColor.r * baseColor.r * (1f - 2f * blendColor.r) : 
                           2f * baseColor.r * (1f - blendColor.r) + Mathf.Sqrt(baseColor.r) * (2f * blendColor.r - 1f);
                result.g = blendColor.g < 0.5f ? 2f * baseColor.g * blendColor.g + baseColor.g * baseColor.g * (1f - 2f * blendColor.g) : 
                           2f * baseColor.g * (1f - blendColor.g) + Mathf.Sqrt(baseColor.g) * (2f * blendColor.g - 1f);
                result.b = blendColor.b < 0.5f ? 2f * baseColor.b * blendColor.b + baseColor.b * baseColor.b * (1f - 2f * blendColor.b) : 
                           2f * baseColor.b * (1f - blendColor.b) + Mathf.Sqrt(baseColor.b) * (2f * blendColor.b - 1f);
                result.a = Mathf.Max(baseColor.a, blendColor.a);
                break;
        }
        
        return result;
    }

    private void SavePattern()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        Texture2D pattern = GeneratePattern();
        string fullPath = outputPath + fileName + ".asset";
        
        AssetDatabase.CreateAsset(pattern, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Pattern Saved", $"Pattern saved as {fullPath}", "OK");
    }

    private void SavePatternAsPNG()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        Texture2D pattern = GeneratePattern();
        byte[] pngData = pattern.EncodeToPNG();
        string fullPath = outputPath + fileName + ".png";
        
        File.WriteAllBytes(fullPath, pngData);
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Pattern Saved", $"Pattern saved as {fullPath}", "OK");
        
        if (pattern != previewTexture)
        {
            DestroyImmediate(pattern);
        }
    }

    private void OnDestroy()
    {
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
        }
    }
}