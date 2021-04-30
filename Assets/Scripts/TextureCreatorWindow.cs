using UnityEditor;
using UnityEngine;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    private string _fileName = "myProceduralTexture";
    private float _perlinXScale;
    private float _perlinYScale;
    private int _perlinOctaves;
    private float _perlinPersistance;
    private float _perlinHeightScale;
    private int _perlinOffsetX;
    private int _perlinOffsetY;
    private bool _alphaToggle;
    private bool _seamlessToggle;
    private bool _mapToggle;

    private float _brightness = 0.5f;
    private float _contrast = 0.5f;

    private Texture2D _perlinTexture;
    private Vector2 _scrollPos;

    [MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        _perlinTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    private void OnGUI()
    {
        //scrollbar
        Rect r = EditorGUILayout.BeginVertical();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        _fileName = EditorGUILayout.TextField("Texture Name", _fileName);

        int imageSize = (int)(EditorGUIUtility.currentViewWidth - 100);

        _perlinXScale = EditorGUILayout.Slider("X Scale", _perlinXScale, 0, 0.1f);
        _perlinYScale = EditorGUILayout.Slider("Y Scale", _perlinYScale, 0, 0.1f);
        _perlinOctaves = EditorGUILayout.IntSlider("Octaves", _perlinOctaves, 0, 10);
        _perlinPersistance = EditorGUILayout.Slider("Persistance", _perlinPersistance, 0, 10);
        _perlinHeightScale = EditorGUILayout.Slider("Height Scale", _perlinHeightScale, 0, 1);
        _perlinOffsetX = EditorGUILayout.IntSlider("X Offset", _perlinOffsetX, 0, 10000);
        _perlinOffsetY = EditorGUILayout.IntSlider("Y Offset", _perlinOffsetY, 0, 10000);
        _brightness = EditorGUILayout.Slider("Brightness", _brightness, 0, 2);
        _contrast = EditorGUILayout.Slider("Contrast", _contrast, 0, 2);
        _alphaToggle = EditorGUILayout.Toggle("Alpha?", _alphaToggle);
        _seamlessToggle = EditorGUILayout.Toggle("Seamless?", _seamlessToggle);
        _mapToggle = EditorGUILayout.Toggle("Map?", _mapToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate", GUILayout.Width(imageSize)))
        {
            GenerateImage();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(_perlinTexture, GUILayout.Width(imageSize), GUILayout.Height(imageSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(imageSize)))
        {
            SaveImage();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void GenerateImage()
    {
        int width = 512;
        int height = 512;
        float perlinValue = 0;
        Color pixelColor = Color.white;

        float minColor = 1;
        float maxColor = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (_seamlessToggle)
                {
                    float u = (float)x / (float)width;
                    float v = (float)y / (float)height;

                    float noise00 = Utils.FBM((x + _perlinOffsetX) * _perlinXScale, (y + _perlinOffsetY) * _perlinYScale, _perlinOctaves, _perlinPersistance) * _perlinHeightScale;
                    float noise01 = Utils.FBM((x + _perlinOffsetX) * _perlinXScale, (y + _perlinOffsetY + height) * _perlinYScale, _perlinOctaves, _perlinPersistance) * _perlinHeightScale;
                    float noise10 = Utils.FBM((x + _perlinOffsetX + width) * _perlinXScale, (y + _perlinOffsetY) * _perlinYScale, _perlinOctaves, _perlinPersistance) * _perlinHeightScale;
                    float noise11 = Utils.FBM((x + _perlinOffsetX + width) * _perlinXScale, (y + _perlinOffsetY + height) * _perlinYScale, _perlinOctaves, _perlinPersistance) * _perlinHeightScale;

                    float totalNoise = u * v * noise00 +
                        u * (1 - v) * noise01 +
                        (1 - u) * v * noise10 +
                        (1 - u) * (1 - v) * noise11;

                    float value = (int)(256 * totalNoise) + 50;
                    float r = Mathf.Clamp((int)noise00, 0, 255);
                    float g = Mathf.Clamp((int)value, 0, 255);
                    float b = Mathf.Clamp((int)value + 50, 0, 255);
                    float a = Mathf.Clamp((int)value + 100, 0, 255);

                    perlinValue = (r + g + b) / (3 * 255.0f);

                }
                else
                {
                    perlinValue = Utils.FBM((x + _perlinOffsetX) * _perlinXScale, (y + _perlinOffsetY) * _perlinYScale, _perlinOctaves, _perlinPersistance) * _perlinHeightScale;
                }

                float colorValue = _contrast * (perlinValue - 0.5f) + 0.5f * _brightness;

                if (minColor > colorValue) minColor = colorValue;
                if (maxColor < colorValue) maxColor = colorValue;

                pixelColor = new Color(colorValue, colorValue, colorValue, _alphaToggle ? colorValue : 1);
                _perlinTexture.SetPixel(x, y, pixelColor);
            }
        }

        if (_mapToggle)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixelColor = _perlinTexture.GetPixel(x, y);
                    float colorValue = pixelColor.r;
                    colorValue = Utils.Map(0, 1, minColor, maxColor, colorValue);
                    pixelColor.r = colorValue;
                    pixelColor.g = colorValue;
                    pixelColor.b = colorValue;
                    _perlinTexture.SetPixel(x, y, pixelColor);
                }
            }
        }

        _perlinTexture.Apply(false, false);
    }

    private void SaveImage()
    {
        string directoryPath = Application.dataPath + "/SavedTexture";
        byte[] bytes = _perlinTexture.EncodeToPNG();
        Directory.CreateDirectory(directoryPath);
        File.WriteAllBytes(directoryPath + "/" + _fileName + ".png", bytes);
    }
}
