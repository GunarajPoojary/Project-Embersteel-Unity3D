using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Editor tool for Baking multiple UI elements into a single optimized sprite and GameObject
/// Enhanced with pixel-perfect dimension calculation based on actual UI element bounds
/// </summary>
public class UIBaker : EditorWindow
{
    private GameObject _targetUI;
    private string _savePath = "Assets/Generated/BakedUI/";
    private string _fileName = "BakedSprite";
    private int _textureWidth = 2048;
    private int _textureHeight = 2048;
    private bool _includeInactiveElements = false;
    private bool _preserveOriginalHierarchy = true;
    private bool _useNativeResolution = false;

    private Vector2 _scrollPosition;
    private Vector2 _elementListScrollPosition;
    private readonly List<UIElementData> _detectedElements = new();
    private bool _showPreview = true;
    private Texture2D _previewTexture;
    private Rect _originalCanvasBounds;
    private Vector2 _calculatedPixelDimensions;

    [System.Serializable]
    private class UIElementData
    {
        public GameObject gameObject;
        public Component component;
        public Rect screenRect;
        public bool includeInBaker = true;
        public UIElementType elementType;

        public enum UIElementType
        {
            Image,
            Text,
            RawImage,
            Button,
            Other
        }
    }

    [MenuItem("Tools/Custom/UGUI/UIBaker")]
    public static void ShowWindow()
    {
        UIBaker window = GetWindow<UIBaker>("UI Baker");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.Space();

        DrawConfigurationSection();
        EditorGUILayout.Space();

        DrawDetectionSection();
        EditorGUILayout.Space();

        DrawElementListSection();
        EditorGUILayout.Space();

        DrawPreviewSection();
        EditorGUILayout.Space();

        DrawBakerSection();

        EditorGUILayout.EndScrollView();
    }

    private void OnInspectorUpdate()
    {
        // Force repaint when window is resized to ensure proper scaling
        if (Event.current != null && Event.current.type == EventType.Layout)
        {
            Repaint();
        }
    }

    private void DrawConfigurationSection()
    {
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

        _targetUI = (GameObject)EditorGUILayout.ObjectField(
            "Target UI", _targetUI, typeof(GameObject), true);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path", GUILayout.Width(EditorGUIUtility.labelWidth));
        _savePath = EditorPrefs.GetString("UIBakerSavePath", _savePath);
        _savePath = EditorGUILayout.TextField(_savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Save Directory", _savePath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                _savePath = "Assets" + selectedPath[Application.dataPath.Length..];
                EditorPrefs.SetString("UIBakerSavePath", _savePath);
            }
        }
        EditorGUILayout.EndHorizontal();

        _fileName = EditorGUILayout.TextField("Asset Name", _fileName);

        EditorGUILayout.BeginHorizontal();
        _textureWidth = EditorGUILayout.IntField("Texture Width", _textureWidth, GUILayout.MinWidth(100));
        _textureHeight = EditorGUILayout.IntField("Texture Height", _textureHeight, GUILayout.MinWidth(100));
        EditorGUILayout.EndHorizontal();

        _useNativeResolution = EditorGUILayout.Toggle("Use Pixel-Perfect Dimensions", _useNativeResolution);
        if (_useNativeResolution)
        {
            EditorGUILayout.HelpBox("Pixel-perfect mode calculates texture dimensions based on actual UI element pixel bounds for precise scaling.", MessageType.Info);

            if (_calculatedPixelDimensions.x > 0 && _calculatedPixelDimensions.y > 0)
            {
                EditorGUILayout.LabelField($"Calculated Dimensions: {_calculatedPixelDimensions.x:F0} x {_calculatedPixelDimensions.y:F0} pixels");
            }
        }

        _includeInactiveElements = EditorGUILayout.Toggle("Include Inactive Elements", _includeInactiveElements);
        _preserveOriginalHierarchy = EditorGUILayout.Toggle("Preserve Original Hierarchy", _preserveOriginalHierarchy);
    }

    private void DrawDetectionSection()
    {
        EditorGUILayout.LabelField("Element Detection", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Scan UI Elements", GUILayout.MinWidth(100), GUILayout.ExpandWidth(true)))
        {
            ScanUIElements();
        }

        if (GUILayout.Button("Clear List", GUILayout.MinWidth(80), GUILayout.ExpandWidth(true)))
        {
            _detectedElements.Clear();
            ClearPreview();
            _calculatedPixelDimensions = Vector2.zero;
        }
        EditorGUILayout.EndHorizontal();

        if (_detectedElements.Count > 0)
        {
            EditorGUILayout.LabelField($"Detected {_detectedElements.Count} UI elements");
        }
    }

    private void DrawElementListSection()
    {
        if (_detectedElements.Count == 0) return;

        EditorGUILayout.LabelField("Detected Elements", EditorStyles.boldLabel);

        _elementListScrollPosition = EditorGUILayout.BeginScrollView(_elementListScrollPosition, GUILayout.Height(120));

        for (int i = 0; i < _detectedElements.Count; i++)
        {
            UIElementData element = _detectedElements[i];

            EditorGUILayout.BeginHorizontal();

            element.includeInBaker = EditorGUILayout.Toggle(element.includeInBaker, GUILayout.Width(20));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(element.gameObject, typeof(GameObject), true, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField(element.elementType.ToString(), GUILayout.Width(60));
            EditorGUILayout.LabelField($"{element.screenRect.width:F0}x{element.screenRect.height:F0}", GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            foreach (UIElementData element in _detectedElements)
                element.includeInBaker = true;
        }

        if (GUILayout.Button("Select None"))
        {
            foreach (UIElementData element in _detectedElements)
                element.includeInBaker = false;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPreviewSection()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        _showPreview = EditorGUILayout.Toggle("Show Preview", _showPreview);

        EditorGUILayout.BeginHorizontal();
        if (_previewTexture != null && _showPreview)
        {
            float aspectRatio = (float)_previewTexture.width / _previewTexture.height;
            float availableWidth = EditorGUIUtility.currentViewWidth - 40;
            float maxPreviewWidth = Mathf.Min(400, availableWidth);
            float previewWidth = Mathf.Max(200, maxPreviewWidth);
            float previewHeight = previewWidth / aspectRatio;

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Preview Texture:", EditorStyles.boldLabel);

            Rect previewRect = EditorGUILayout.GetControlRect(false, previewHeight);

            // Center the preview if the window is wider than the preview
            if (previewRect.width > previewWidth)
            {
                float offset = (previewRect.width - previewWidth) * 0.5f;
                previewRect.x += offset;
                previewRect.width = previewWidth;
            }

            EditorGUI.DrawPreviewTexture(previewRect, _previewTexture);
        }
        EditorGUILayout.EndHorizontal();

        if (_previewTexture != null && _showPreview)
        {
            float aspectRatio = (float)_previewTexture.width / _previewTexture.height;
            float previewWidth = Mathf.Min(300, EditorGUIUtility.currentViewWidth - 60);
            float previewHeight = previewWidth / aspectRatio;

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Preview Texture:", EditorStyles.boldLabel);

            Rect previewRect = EditorGUILayout.GetControlRect(false, previewHeight);
            EditorGUI.DrawPreviewTexture(previewRect, _previewTexture);
        }
    }

    private void DrawBakerSection()
    {
        EditorGUI.BeginDisabledGroup(_detectedElements.Count == 0 || _targetUI == null);

        if (GUILayout.Button("Bake UI Elements", GUILayout.Height(30)))
        {
            BakerUIElements();
        }

        EditorGUI.EndDisabledGroup();

        if (_targetUI == null)
        {
            EditorGUILayout.HelpBox("Please assign a Target UI to proceed with Baker.", MessageType.Warning);
        }
        else if (_detectedElements.Count == 0)
        {
            EditorGUILayout.HelpBox("Please scan for UI elements first.", MessageType.Warning);
        }
    }

    private void ScanUIElements()
    {
        if (_targetUI == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Target UI first.", "OK");
            return;
        }

        _detectedElements.Clear();

        Component[] components = _targetUI.GetComponentsInChildren<Component>(_includeInactiveElements);
        foreach (Component comp in components)
        {
            if (comp == null || comp.gameObject == null)
                continue;

            if (IsUIElement(comp))
            {
                UIElementData elementData = CreateElementData(comp);
                if (elementData != null)
                {
                    _detectedElements.Add(elementData);
                }
            }
        }

        // Calculate pixel dimensions after scanning
        CalculatePixelDimensions();

        Debug.Log($"Scanned and found {_detectedElements.Count} UI elements.");
        if (_useNativeResolution)
        {
            Debug.Log($"Calculated pixel dimensions: {_calculatedPixelDimensions.x} x {_calculatedPixelDimensions.y}");
        }
    }

    private bool IsUIElement(Component component)
    {
        return component is Image || component is Text || component is RawImage ||
               component is Button || component is Slider || component is Toggle;
    }

    private UIElementData CreateElementData(Component component)
    {
        RectTransform rectTransform = component.GetComponent<RectTransform>();
        if (rectTransform == null) return null;

        UIElementData elementData = new UIElementData
        {
            gameObject = component.gameObject,
            component = component,
            screenRect = GetScreenRect(rectTransform)
        };

        if (component is Image) elementData.elementType = UIElementData.UIElementType.Image;
        else if (component is Text) elementData.elementType = UIElementData.UIElementType.Text;
        else if (component is RawImage) elementData.elementType = UIElementData.UIElementType.RawImage;
        else if (component is Button) elementData.elementType = UIElementData.UIElementType.Button;
        else elementData.elementType = UIElementData.UIElementType.Other;

        return elementData;
    }

    private Rect GetScreenRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector2 min = corners[0];
        Vector2 max = corners[2];

        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    /// <summary>
    /// Calculate pixel-perfect dimensions based on actual UI element bounds
    /// This mimics Unity's Sprite Editor automatic slice functionality
    /// </summary>
    private void CalculatePixelDimensions()
    {
        if (_detectedElements.Count == 0)
        {
            _calculatedPixelDimensions = Vector2.zero;
            return;
        }

        Canvas canvas = _targetUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            _calculatedPixelDimensions = Vector2.zero;
            return;
        }

        // Calculate the actual content bounds in canvas space
        Rect contentBounds = CalculateContentBounds();

        if (contentBounds.width <= 0 || contentBounds.height <= 0)
        {
            _calculatedPixelDimensions = Vector2.zero;
            return;
        }

        // Get the canvas scale factor for pixel conversion
        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
        float scaleFactor = GetCanvasScaleFactor(canvas, canvasScaler);

        // Convert canvas units to actual pixels
        float pixelWidth = contentBounds.width * scaleFactor;
        float pixelHeight = contentBounds.height * scaleFactor;

        // Round to nearest integer pixels and ensure minimum size
        _calculatedPixelDimensions = new Vector2(
            Mathf.Max(1, Mathf.RoundToInt(pixelWidth)),
            Mathf.Max(1, Mathf.RoundToInt(pixelHeight))
        );

        Debug.Log($"Content bounds: {contentBounds}, Scale factor: {scaleFactor}, Final pixel dimensions: {_calculatedPixelDimensions}");
    }

    /// <summary>
    /// Calculate tight bounds around actual UI content
    /// </summary>
    private Rect CalculateContentBounds()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        bool boundsFound = false;
        Canvas canvas = _targetUI.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        foreach (UIElementData element in _detectedElements)
        {
            if (element.includeInBaker && element.gameObject.activeInHierarchy)
            {
                RectTransform rectTransform = element.gameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Get the actual rendered bounds of the element
                    Bounds renderedBounds = GetRenderedBounds(rectTransform);

                    Vector2 min = new Vector2(renderedBounds.min.x, renderedBounds.min.y);
                    Vector2 max = new Vector2(renderedBounds.max.x, renderedBounds.max.y);

                    minX = Mathf.Min(minX, min.x);
                    minY = Mathf.Min(minY, min.y);
                    maxX = Mathf.Max(maxX, max.x);
                    maxY = Mathf.Max(maxY, max.y);

                    boundsFound = true;
                }
            }
        }

        if (!boundsFound)
        {
            return new Rect(0, 0, 100, 100); // Fallback size
        }

        // Add minimal padding for edge precision
        float padding = 1f;
        return new Rect(
            minX - padding,
            minY - padding,
            (maxX - minX) + (padding * 2),
            (maxY - minY) + (padding * 2)
        );
    }

    /// <summary>
    /// Get the actual rendered bounds of a UI element
    /// </summary>
    private Bounds GetRenderedBounds(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector3 min = corners[0];
        Vector3 max = corners[0];

        for (int i = 1; i < 4; i++)
        {
            min = Vector3.Min(min, corners[i]);
            max = Vector3.Max(max, corners[i]);
        }

        return new Bounds((min + max) * 0.5f, max - min);
    }

    /// <summary>
    /// Get the effective canvas scale factor for pixel calculations
    /// </summary>
    private float GetCanvasScaleFactor(Canvas canvas, CanvasScaler canvasScaler)
    {
        if (canvasScaler == null)
        {
            return 1f; // No scaling
        }

        switch (canvasScaler.uiScaleMode)
        {
            case CanvasScaler.ScaleMode.ConstantPixelSize:
                return canvasScaler.scaleFactor;

            case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                Vector2 referenceResolution = canvasScaler.referenceResolution;
                Vector2 screenSize = new Vector2(Screen.width, Screen.height);

                float logWidth = Mathf.Log(screenSize.x / referenceResolution.x, 2);
                float logHeight = Mathf.Log(screenSize.y / referenceResolution.y, 2);
                float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, canvasScaler.matchWidthOrHeight);

                return Mathf.Pow(2, logWeightedAverage);

            case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                return Screen.dpi / canvasScaler.fallbackScreenDPI;

            default:
                return 1f;
        }
    }

    private void GeneratePreview()
    {
        if (_detectedElements.Count == 0) return;

        ClearPreview();
        _previewTexture = GenerateBakedTexture();
        Repaint();
    }

    /// <summary>
    /// Calculate canvas-relative bounds for all selected UI elements
    /// </summary>
    private Rect CalculateCanvasBounds()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        bool boundsFound = false;

        Canvas canvas = _targetUI.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        foreach (UIElementData element in _detectedElements)
        {
            if (element.includeInBaker && element.gameObject.activeInHierarchy)
            {
                RectTransform rectTransform = element.gameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);

                    foreach (Vector3 corner in corners)
                    {
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            canvasRect,
                            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corner),
                            canvas.worldCamera,
                            out localPoint
                        );

                        minX = Mathf.Min(minX, localPoint.x);
                        minY = Mathf.Min(minY, localPoint.y);
                        maxX = Mathf.Max(maxX, localPoint.x);
                        maxY = Mathf.Max(maxY, localPoint.y);

                        boundsFound = true;
                    }
                }
            }
        }

        if (!boundsFound)
        {
            Vector2 canvasSize = canvasRect.sizeDelta;
            return new Rect(-canvasSize.x * 0.5f, -canvasSize.y * 0.5f, canvasSize.x, canvasSize.y);
        }

        float padding = 1f;
        Rect bounds = new Rect(
            minX - padding,
            minY - padding,
            (maxX - minX) + (padding * 2),
            (maxY - minY) + (padding * 2)
        );

        _originalCanvasBounds = bounds;
        return bounds;
    }

    /// <summary>
    /// Prepare UI elements for screen space capture while preserving original alpha values
    /// </summary>
    private void PrepareUIElementsForCapture()
    {
        Canvas canvas = _targetUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas.ForceUpdateCanvases();

        foreach (UIElementData element in _detectedElements)
        {
            if (element.includeInBaker)
            {
                element.gameObject.SetActive(true);
                // Note: We no longer force alpha to 1.0f - preserve original alpha values
            }
            else
            {
                element.gameObject.SetActive(false);
            }
        }

        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Generate Baked texture using pixel-perfect dimensions
    /// </summary>
    private Texture2D GenerateBakedTexture()
    {
        Canvas canvas = _targetUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in parent hierarchy!");
            return null;
        }

        // Store original canvas state
        RenderMode originalRenderMode = canvas.renderMode;
        Camera originalCamera = canvas.worldCamera;
        int originalSortingOrder = canvas.sortingOrder;

        // Store original element states
        Dictionary<GameObject, bool> originalActiveStates = new Dictionary<GameObject, bool>();
        // Remove the originalColors dictionary since we're no longer modifying colors

        foreach (UIElementData element in _detectedElements)
        {
            originalActiveStates[element.gameObject] = element.gameObject.activeSelf;
            // Remove the color storage logic
        }

        try
        {
            PrepareUIElementsForCapture();

            Rect canvasBounds = CalculateCanvasBounds();

            // Use pixel-perfect dimensions if enabled, otherwise use specified dimensions
            int renderWidth, renderHeight;
            if (_useNativeResolution && _calculatedPixelDimensions.x > 0 && _calculatedPixelDimensions.y > 0)
            {
                renderWidth = (int)_calculatedPixelDimensions.x;
                renderHeight = (int)_calculatedPixelDimensions.y;
            }
            else
            {
                renderWidth = _textureWidth;
                renderHeight = _textureHeight;
            }

            RenderTexture renderTexture = new RenderTexture(renderWidth, renderHeight, 24, RenderTextureFormat.ARGB32);
            RenderTexture previousActive = RenderTexture.active;

            GameObject tempCameraObj = new GameObject("TempUICamera");
            Camera tempCamera = tempCameraObj.AddComponent<Camera>();

            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = Color.clear;
            tempCamera.orthographic = true;
            tempCamera.cullingMask = 1 << 5; // UI layer
            tempCamera.targetTexture = renderTexture;
            tempCamera.nearClipPlane = -1000f;
            tempCamera.farClipPlane = 1000f;

            // Enhanced settings for better alpha capture
            tempCamera.allowMSAA = false;
            tempCamera.allowHDR = false;
            tempCamera.useOcclusionCulling = false;

            tempCamera.transform.position = new Vector3(
                canvasBounds.center.x,
                canvasBounds.center.y,
                -100f
            );
            tempCamera.orthographicSize = canvasBounds.height * 0.5f;

            // Calculate aspect ratio for proper viewport
            float boundsAspect = canvasBounds.width / canvasBounds.height;
            float renderAspect = (float)renderWidth / renderHeight;

            if (boundsAspect > renderAspect)
            {
                float viewportHeight = renderAspect / boundsAspect;
                tempCamera.rect = new Rect(0, (1f - viewportHeight) * 0.5f, 1f, viewportHeight);
            }
            else
            {
                float viewportWidth = boundsAspect / renderAspect;
                tempCamera.rect = new Rect((1f - viewportWidth) * 0.5f, 0, viewportWidth, 1f);
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = tempCamera;
            canvas.planeDistance = 100f;

            Canvas.ForceUpdateCanvases();

            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);

            tempCamera.Render();

            Texture2D rawTexture = new Texture2D(renderWidth, renderHeight, TextureFormat.RGBA32, false);
            rawTexture.ReadPixels(new Rect(0, 0, renderWidth, renderHeight), 0, 0);

            // Process pixels to enhance alpha values before applying
            Color[] capturedPixels = rawTexture.GetPixels();
            Color[] processedPixels = ProcessCapturedPixels(capturedPixels, renderWidth, renderHeight);
            rawTexture.SetPixels(processedPixels);
            rawTexture.Apply();

            // Crop texture to actual content bounds for pixel-perfect dimensions
            Texture2D bakedTexture = CropTextureToContent(rawTexture);

            // Clean up the raw texture
            if (rawTexture != bakedTexture)
            {
                DestroyImmediate(rawTexture);
            }

            // Clean up
            RenderTexture.active = previousActive;
            DestroyImmediate(tempCameraObj);
            renderTexture.Release();

            SaveTextureAsset(bakedTexture);

            ClearPreview();
            _previewTexture = bakedTexture;

            return bakedTexture;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during texture generation: {ex.Message}");
            return null;
        }
        finally
        {
            RestoreOriginalStates(canvas, originalRenderMode, originalCamera, originalSortingOrder,
                                originalActiveStates);
        }
    }

    /// <summary>
    /// Process captured pixels to enhance alpha values and maintain visual consistency
    /// </summary>
    private Color[] ProcessCapturedPixels(Color[] pixels, int width, int height)
    {
        Color[] processedPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color originalPixel = pixels[i];

            // Skip fully transparent pixels
            if (originalPixel.a <= 0.01f)
            {
                processedPixels[i] = originalPixel;
                continue;
            }

            // Enhance alpha visibility while preserving the original alpha relationship
            // This compensates for the rendering pipeline differences
            float enhancedAlpha = Mathf.Pow(originalPixel.a, 0.7f); // Gamma-like correction for alpha

            // Preserve the color intensity relative to alpha
            float alphaRatio = originalPixel.a > 0 ? enhancedAlpha / originalPixel.a : 1f;

            processedPixels[i] = new Color(
                originalPixel.r * alphaRatio,
                originalPixel.g * alphaRatio,
                originalPixel.b * alphaRatio,
                enhancedAlpha
            );
        }

        return processedPixels;
    }

    /// <summary>
    /// Crop texture to actual content bounds, ensuring dimensions are multiples of 4 for compression compatibility
    /// </summary>
    private Texture2D CropTextureToContent(Texture2D sourceTexture)
    {
        Color[] pixels = sourceTexture.GetPixels();
        int width = sourceTexture.width;
        int height = sourceTexture.height;

        int minX = width, maxX = -1;
        int minY = height, maxY = -1;

        // Find actual content bounds by scanning for non-transparent pixels
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];
                if (pixel.a > 0.01f) // Consider nearly transparent pixels as empty
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        // If no content found, return original texture
        if (maxX == -1 || maxY == -1)
        {
            Debug.LogWarning("No visible content found in texture");
            return sourceTexture;
        }

        // Add small padding to prevent edge artifacts
        int padding = 2;
        minX = Mathf.Max(0, minX - padding);
        minY = Mathf.Max(0, minY - padding);
        maxX = Mathf.Min(width - 1, maxX + padding);
        maxY = Mathf.Min(height - 1, maxY + padding);

        int croppedWidth = maxX - minX + 1;
        int croppedHeight = maxY - minY + 1;

        // Ensure dimensions are multiples of 4 for compression compatibility
        croppedWidth = RoundToMultipleOf4(croppedWidth);
        croppedHeight = RoundToMultipleOf4(croppedHeight);

        // Recalculate bounds to center the content within the adjusted dimensions
        int widthAdjustment = croppedWidth - (maxX - minX + 1);
        int heightAdjustment = croppedHeight - (maxY - minY + 1);

        minX = Mathf.Max(0, minX - widthAdjustment / 2);
        minY = Mathf.Max(0, minY - heightAdjustment / 2);

        // Ensure we don't exceed source texture bounds
        if (minX + croppedWidth > width)
        {
            minX = width - croppedWidth;
        }
        if (minY + croppedHeight > height)
        {
            minY = height - croppedHeight;
        }

        // Create cropped texture with compression-compatible dimensions
        Texture2D croppedTexture = new Texture2D(croppedWidth, croppedHeight, TextureFormat.RGBA32, false);
        Color[] croppedPixels = new Color[croppedWidth * croppedHeight];

        // Fill with transparent pixels first
        for (int i = 0; i < croppedPixels.Length; i++)
        {
            croppedPixels[i] = Color.clear;
        }

        // Copy source pixels to the cropped texture
        for (int y = 0; y < croppedHeight; y++)
        {
            for (int x = 0; x < croppedWidth; x++)
            {
                int sourceX = minX + x;
                int sourceY = minY + y;

                if (sourceX >= 0 && sourceX < width && sourceY >= 0 && sourceY < height)
                {
                    croppedPixels[y * croppedWidth + x] = pixels[sourceY * width + sourceX];
                }
            }
        }

        croppedTexture.SetPixels(croppedPixels);
        croppedTexture.Apply();

        Debug.Log($"Texture cropped from {width}x{height} to {croppedWidth}x{croppedHeight} (compression-compatible)");

        return croppedTexture;
    }

    /// <summary>
    /// Round dimension to the nearest multiple of 4, ensuring minimum size of 4
    /// </summary>
    private int RoundToMultipleOf4(int value)
    {
        return Mathf.Max(4, ((value + 3) / 4) * 4);
    }

    /// <summary>
    /// Restore original canvas and element states after texture generation
    /// </summary>
    private void RestoreOriginalStates(Canvas canvas, RenderMode originalRenderMode, Camera originalCamera,
                                     int originalSortingOrder, Dictionary<GameObject, bool> originalActiveStates)
    {
        canvas.renderMode = originalRenderMode;
        canvas.worldCamera = originalCamera;
        canvas.sortingOrder = originalSortingOrder;

        foreach (KeyValuePair<GameObject, bool> kvp in originalActiveStates)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetActive(kvp.Value);
            }
        }

        Canvas.ForceUpdateCanvases();
    }

    private void SaveTextureAsset(Texture2D texture)
    {
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
        }

        byte[] textureBytes = texture.EncodeToPNG();
        string texturePath = Path.Combine(_savePath, $"{_fileName}.png");

        File.WriteAllBytes(texturePath, textureBytes);
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();

            // Set compression settings based on texture properties
            if (HasAlphaChannel(texture))
            {
                // For textures with alpha, use appropriate compression
                if (IsDimensionMultipleOfFour(texture.width) && IsDimensionMultipleOfFour(texture.height))
                {
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.compressionQuality = 100; // High quality for UI elements
                }
                else
                {
                    // Fallback to uncompressed for non-multiple-of-4 dimensions
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    Debug.LogWarning($"Texture dimensions ({texture.width}x{texture.height}) are not multiples of 4. Using uncompressed format to avoid compression warnings.");
                }
            }
            else
            {
                importer.textureCompression = TextureImporterCompression.Compressed;
            }

            // Set pixels per unit based on canvas scale for proper sizing
            Canvas canvas = _targetUI.GetComponentInParent<Canvas>();
            CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();

            float pixelsPerUnit = 100f; // Unity default
            if (canvasScaler != null && _useNativeResolution)
            {
                float scaleFactor = GetCanvasScaleFactor(canvas, canvasScaler);
                pixelsPerUnit = 100f * scaleFactor;
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit = pixelsPerUnit;
            importer.SetTextureSettings(settings);

            // Apply platform-specific settings for better optimization
            TextureImporterPlatformSettings platformSettings = new()
            {
                name = "Standalone",
                overridden = true,
                maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(texture.width, texture.height))
            };

            if (HasAlphaChannel(texture))
            {
                platformSettings.format = TextureImporterFormat.DXT5; // Supports alpha
            }
            else
            {
                platformSettings.format = TextureImporterFormat.DXT1; // No alpha, better compression
            }

            importer.SetPlatformTextureSettings(platformSettings);

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

            Debug.Log($"Texture saved with dimensions: {texture.width}x{texture.height}, Compression: {importer.textureCompression}");
        }
    }

    /// <summary>
    /// Check if texture has meaningful alpha channel content
    /// </summary>
    private bool HasAlphaChannel(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.99f) // Has transparency
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if dimension is a multiple of 4
    /// </summary>
    private bool IsDimensionMultipleOfFour(int dimension) => dimension % 4 == 0;

    private void BakerUIElements()
    {
        if (!ValidateBaking()) return;

        CreateOutputDirectory();

        Texture2D bakedTexture = GenerateBakedTexture();
        if (bakedTexture == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to generate baked texture!", "OK");
            return;
        }

        string texturePath = Path.Combine(_savePath, $"{_fileName}_Texture.png");
        Sprite bakedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);

        if (bakedSprite == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to load baked sprite!", "OK");
            return;
        }

        GameObject bakedObject = CreatebakedGameObject(bakedSprite);

        if (_preserveOriginalHierarchy)
        {
            DisableOriginalElements();
        }
        else
        {
            DestroyOriginalElements();
        }

        EditorUtility.DisplayDialog("Success", $"Sprite saved at: {texturePath}", "OK");

        Selection.activeGameObject = bakedObject;
    }

    private GameObject CreatebakedGameObject(Sprite sprite)
    {
        GameObject bakedObject = new($"{_fileName}_Baked");
        bakedObject.transform.SetParent(_targetUI.transform.parent);
        bakedObject.transform.SetSiblingIndex(_targetUI.transform.GetSiblingIndex());

        RectTransform rectTransform = bakedObject.AddComponent<RectTransform>();
        Image imageComponent = bakedObject.AddComponent<Image>();

        imageComponent.sprite = sprite;
        imageComponent.type = Image.Type.Simple;
        imageComponent.preserveAspect = false;

        Canvas parentCanvas = _targetUI.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

            if (_useNativeResolution && _originalCanvasBounds.width > 0 && _originalCanvasBounds.height > 0)
            {
                rectTransform.SetParent(canvasRect);

                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(_originalCanvasBounds.center.x, _originalCanvasBounds.center.y);

                rectTransform.sizeDelta = new Vector2(_originalCanvasBounds.width, _originalCanvasBounds.height);
            }
            else
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
        }

        return bakedObject;
    }

    private bool ValidateBaking()
    {
        if (_targetUI == null)
        {
            EditorUtility.DisplayDialog("Error", "Target UI is not assigned.", "OK");
            return false;
        }

        if (_detectedElements.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No UI elements detected. Please scan first.", "OK");
            return false;
        }

        if (_detectedElements.FindAll(e => e.includeInBaker).Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No elements selected for baking.", "OK");
            return false;
        }

        Canvas canvas = _targetUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Target UI must be a child of a Canvas.", "OK");
            return false;
        }

        return true;
    }

    private void CreateOutputDirectory()
    {
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
            AssetDatabase.Refresh();
        }
    }

    private void DisableOriginalElements()
    {
        foreach (UIElementData element in _detectedElements)
        {
            if (element.includeInBaker)
            {
                element.gameObject.SetActive(false);
            }
        }
    }

    private void DestroyOriginalElements()
    {
        foreach (UIElementData element in _detectedElements)
        {
            if (element.includeInBaker)
            {
                DestroyImmediate(element.gameObject);
            }
        }
    }

    private void ClearPreview()
    {
        if (_previewTexture != null)
        {
            DestroyImmediate(_previewTexture);
            _previewTexture = null;
        }
    }

    private void OnDestroy()
    {
        ClearPreview();
    }
}