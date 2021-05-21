using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
    private SerializedProperty _randomHeightRange;
    private SerializedProperty _heightMapScale;
    private SerializedProperty _heightMapImage;
    private SerializedProperty _perlinXScale;
    private SerializedProperty _perlinYScale;
    private SerializedProperty _perlinXOffset;
    private SerializedProperty _perlinYOffset;
    private SerializedProperty _perlinOctaves;
    private SerializedProperty _perlinPersistence;
    private SerializedProperty _perlinHeightScale;
    private SerializedProperty _resetTerrain;
    private SerializedProperty _peaksCount;
    private SerializedProperty _fallOff;
    private SerializedProperty _dropOff;
    private SerializedProperty _minHeight;
    private SerializedProperty _maxHeight;
    private SerializedProperty _voronoiType;
    private SerializedProperty _MPD_minHeight;
    private SerializedProperty _MPD_maxHeight;
    private SerializedProperty _MPD_roughness;
    private SerializedProperty _MPD_heightDampenerPower;
    private SerializedProperty _smoothAmount;
    //public SerializedProperty _splatOffset;
    //public SerializedProperty _noiseXScale;
    //public SerializedProperty _noiseYScale;
    //public SerializedProperty _noiseScaler;

    private GUITableState _perlinParametersTable;
    private SerializedProperty _perlinParameters;

    private GUITableState _splatMapsParametersTable;
    private SerializedProperty _splatMapsParameters;

    private GUITableState _vegetationParametersTable;
    private SerializedProperty _vegetationParameters;
    private SerializedProperty _maxTrees;
    private SerializedProperty _treeSpacing;

    private GUITableState _detailsParametersTable;
    private SerializedProperty _detailsParameters;
    private SerializedProperty _maxDetails;
    private SerializedProperty _detailsSpacing;

    private SerializedProperty _waterHeight;
    private SerializedProperty _waterGameObject;
    private SerializedProperty _shorelineMaterial;
    private SerializedProperty _shorelineScale;

    private bool _showRandom = false;
    private bool _showImage = false;
    private bool _showSinglePerlin = false;
    private bool _showMultiplePerlin = false;
    private bool _showVoronoi = false;
    private bool _midpointDisplacement = false;
    private bool _smooth = false;
    private bool _splatMap = false;
    private bool _heightMap = false;
    private bool _vegetation = false;
    private bool _details = false;
    private bool _water = false;

    private Vector2 _scrollPos;
    private Texture2D _currHeightMap;
    private float imageSize;

    private void OnEnable()
    {
        _randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        _heightMapScale = serializedObject.FindProperty("heightMapScale");
        _heightMapImage = serializedObject.FindProperty("heightMapImage");
        _perlinXScale = serializedObject.FindProperty("perlinXScale");
        _perlinYScale = serializedObject.FindProperty("perlinYScale");
        _perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        _perlinYOffset = serializedObject.FindProperty("perlinYOffset");
        _perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        _perlinPersistence = serializedObject.FindProperty("perlinPersistence");
        _perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        _resetTerrain = serializedObject.FindProperty("resetTerrain");
        _perlinParametersTable = new GUITableState("perlinParametersTable");
        _perlinParameters = serializedObject.FindProperty("perlinParameters");
        _peaksCount = serializedObject.FindProperty("peaksCount");
        _fallOff = serializedObject.FindProperty("fallOff");
        _dropOff = serializedObject.FindProperty("dropOff");
        _minHeight = serializedObject.FindProperty("minHeight");
        _maxHeight = serializedObject.FindProperty("maxHeight");
        _voronoiType = serializedObject.FindProperty("voronoiType");
        _MPD_minHeight = serializedObject.FindProperty("MPD_minHeight");
        _MPD_maxHeight = serializedObject.FindProperty("MPD_maxHeight");
        _MPD_roughness = serializedObject.FindProperty("MPD_roughness");
        _MPD_heightDampenerPower = serializedObject.FindProperty("MPD_heightDampenerPower");
        _smoothAmount = serializedObject.FindProperty("smoothAmount");

        _splatMapsParametersTable = new GUITableState("splatMapsParametersTable");
        _splatMapsParameters = serializedObject.FindProperty("splatMapsParameters");

        _currHeightMap = new Texture2D(513, 513, TextureFormat.ARGB32, false);
        
        _maxTrees = serializedObject.FindProperty("maxTrees");
        _treeSpacing = serializedObject.FindProperty("treeSpacing");
        _vegetationParametersTable = new GUITableState("vegetationParametersTable");
        _vegetationParameters = serializedObject.FindProperty("vegetationParameters");

        _maxDetails = serializedObject.FindProperty("maxDetails");
        _detailsSpacing = serializedObject.FindProperty("detailsSpacing");
        _detailsParametersTable = new GUITableState("detailsParametersTable");
        _detailsParameters = serializedObject.FindProperty("detailsParameters");
        
        _waterHeight = serializedObject.FindProperty("waterHeight");
        _waterGameObject = serializedObject.FindProperty("waterGameObject");
        _shorelineMaterial = serializedObject.FindProperty("shorelineMaterial");
        _shorelineScale = serializedObject.FindProperty("shorelineScale");

        //_splatOffset = serializedObject.FindProperty("splatOffset");
        //_noiseXScale = serializedObject.FindProperty("noiseXScale");
        //_noiseYScale = serializedObject.FindProperty("noiseYScale");
        //_noiseScaler = serializedObject.FindProperty("noiseScaler");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CustomTerrain customTerrain = (CustomTerrain)target;

        //scrollbar
        Rect r = EditorGUILayout.BeginVertical();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(r.width), GUILayout.Height(r.height));
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(_resetTerrain);

        _showRandom = EditorGUILayout.Foldout(_showRandom, "Random");
        if (_showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Height Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_randomHeightRange);
            if (GUILayout.Button("Random Height"))
            {
                customTerrain.RandomTerrain();
            }
        }

        _showImage = EditorGUILayout.Foldout(_showImage, "Texture");
        if (_showImage)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From a Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_heightMapImage);
            EditorGUILayout.PropertyField(_heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                customTerrain.LoadTexture();
            }
        }

        _showSinglePerlin = EditorGUILayout.Foldout(_showSinglePerlin, "Single Perlin Noise");
        if (_showSinglePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Perlin X and Y Scale", EditorStyles.boldLabel);
            EditorGUILayout.Slider(_perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(_perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(_perlinXOffset, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(_perlinYOffset, 0, 10000, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(_perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(_perlinPersistence, 1, 10, new GUIContent("Persistence"));
            EditorGUILayout.Slider(_perlinHeightScale, 0, 1, new GUIContent("Height Scale"));

            if (GUILayout.Button("Apply Single Perlin"))
            {
                customTerrain.SinglePerlin();
            }
        }

        _showMultiplePerlin = EditorGUILayout.Foldout(_showMultiplePerlin, "Multiple Perlin Noise");
        if (_showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise Table", EditorStyles.boldLabel);
            _perlinParametersTable = GUITableLayout.DrawTable(_perlinParametersTable, _perlinParameters);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
                customTerrain.AddNewPerlin();
            if (GUILayout.Button("-"))
                customTerrain.RemovePerlin();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                customTerrain.MultiplePerlin();
            }
        }

        _showVoronoi = EditorGUILayout.Foldout(_showVoronoi, "Voronoi");
        if (_showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Voroni Parameters", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(_peaksCount, 1, 10, new GUIContent("Peaks Count"));
            EditorGUILayout.Slider(_fallOff, 0, 10, new GUIContent("Fall Off"));
            EditorGUILayout.Slider(_dropOff, 0, 10, new GUIContent("Drop Off"));
            EditorGUILayout.Slider(_minHeight, 0, 1, new GUIContent("Minimum Height"));
            EditorGUILayout.Slider(_maxHeight, 0, 1, new GUIContent("Maximum Height"));
            EditorGUILayout.PropertyField(_voronoiType);

            if (GUILayout.Button("Apply Voronoi"))
            {
                customTerrain.Voronoi();
            }
        }

        _midpointDisplacement = EditorGUILayout.Foldout(_midpointDisplacement, "Midpoint Displacement");
        if (_midpointDisplacement)
        {
            EditorGUILayout.PropertyField(_MPD_minHeight);
            EditorGUILayout.PropertyField(_MPD_maxHeight);
            EditorGUILayout.PropertyField(_MPD_heightDampenerPower);
            EditorGUILayout.PropertyField(_MPD_roughness);

            if (GUILayout.Button("MPD"))
            {
                customTerrain.MidpointDisplacement();
            }
        }

        _splatMap = EditorGUILayout.Foldout(_splatMap, "Splat Maps");
        if (_splatMap)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);

            //EditorGUILayout.Slider(_splatOffset, 0, 0.1f, new GUIContent("Offset"));
            //EditorGUILayout.Slider(_noiseXScale, 0.001f, 1, new GUIContent("Noise X Scale"));
            //EditorGUILayout.Slider(_noiseYScale, 0.001f, 1, new GUIContent("Noise Y Scale"));
            //EditorGUILayout.Slider(_noiseScaler, 0, 1, new GUIContent("Noise Scaler"));


            _splatMapsParametersTable = GUITableLayout.DrawTable(_splatMapsParametersTable, _splatMapsParameters);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
                customTerrain.AddNewSplatMap();
            if (GUILayout.Button("-"))
                customTerrain.RemoveSplatMap();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Splat Maps"))
            {
                customTerrain.SplatMaps();
            }
        }

        _vegetation = EditorGUILayout.Foldout(_vegetation, "Vegetation");
        if(_vegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(_maxTrees, 0, 10000, new GUIContent("Max Trees"));
            EditorGUILayout.IntSlider(_treeSpacing, 2, 20, new GUIContent("Tree Spacing"));

            _vegetationParametersTable = GUITableLayout.DrawTable(_vegetationParametersTable, _vegetationParameters);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
                customTerrain.AddNewVegetation();
            if (GUILayout.Button("-"))
                customTerrain.RemoveVegetation();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Vegetation"))
            {
                customTerrain.Vegetation();
            }
        }

        _details = EditorGUILayout.Foldout(_details, "Details");
        if (_details)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Details", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(_maxDetails, 0, 10000, new GUIContent("Max Details"));
            EditorGUILayout.IntSlider(_detailsSpacing, 2, 20, new GUIContent("Details Spacing"));

            customTerrain.GetComponent<Terrain>().detailObjectDistance = _maxDetails.intValue;

            _detailsParametersTable = GUITableLayout.DrawTable(_detailsParametersTable, _detailsParameters);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
                customTerrain.AddNewDetails();
            if (GUILayout.Button("-"))
                customTerrain.RemoveDetails();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Details"))
            {
                customTerrain.Details();
            }
        }

        _water = EditorGUILayout.Foldout(_water, "Water");
        if (_water)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);

            EditorGUILayout.Slider(_waterHeight, 0, 1f, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(_waterGameObject);

            if (GUILayout.Button("Add Water"))
            {
                customTerrain.Water();
            }

            EditorGUILayout.Slider(_shorelineScale, 0, 100, new GUIContent("Shoreline Scale"));
            EditorGUILayout.PropertyField(_shorelineMaterial);

            if (GUILayout.Button("Add Shoreline"))
            {
                customTerrain.Shoreline();
            }
        }

            _smooth = EditorGUILayout.Foldout(_smooth, "Smooth Terrain");
        if (_smooth)
        {
            EditorGUILayout.IntSlider(_smoothAmount, 0, 10, new GUIContent("Smooth Amount"));
            if (GUILayout.Button("Smooth"))
            {
                customTerrain.Smooth();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Height"))
        {
            customTerrain.ResetTerrain();
        }

        _heightMap = EditorGUILayout.Foldout(_heightMap, "HeightMap");
        if(_heightMap)
        {
            imageSize = (int)(EditorGUIUtility.currentViewWidth - 100);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(_currHeightMap, GUILayout.Width(imageSize), GUILayout.Height(imageSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(imageSize)))
            {
                _currHeightMap = customTerrain.GetCurrentHeightMap();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
