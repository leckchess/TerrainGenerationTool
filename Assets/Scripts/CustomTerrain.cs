using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool resetTerrain = true;


    //SINGLE PERLIN NOISE ----------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinXOffset = 0;
    public int perlinYOffset = 0;
    public int perlinOctaves = 3;
    public float perlinPersistence = 8;
    public float perlinHeightScale = 0.09f;

    //MULTIPLE PERLIN NOISE ----------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float perlinXScale = 0.01f;
        public float perlinYScale = 0.01f;
        public int perlinXOffset = 0;
        public int perlinYOffset = 0;
        public int perlinOctaves = 3;
        public float perlinPersistence = 8;
        public float perlinHeightScale = 0.09f;
        public bool remove;
    }
    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>() { new PerlinParameters() };

    //SPLAT MAPS -----------------------------
    [System.Serializable]
    public class SplatMapsParameters
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0.0f;
        public float maxSlope = 1.5f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float splatOffset = 0.01f;
        public float noiseXScale = 0.01f;
        public float noiseYScale = 0.01f;
        public float noiseScaler = 0.5f;
        public bool remove;
    }
    public List<SplatMapsParameters> splatMapsParameters = new List<SplatMapsParameters>() { new SplatMapsParameters() };

    //VEGETATION -----------------------------
    [System.Serializable]
    public class VegetationParameters
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSloop = 0;
        public float maxSloop = 90;
        public float minScale = 0.5f;
        public float maxScale = 0.95f;
        public float minRotation = 0;
        public float maxRotation = 360;
        public float density = 0.5f;
        public Color color1 = Color.white;
        public Color color2 = Color.white;
        public Color lightmapColor = Color.white;
        public bool remove = false;
    }

    public List<VegetationParameters> vegetationParameters = new List<VegetationParameters>() { new VegetationParameters() };
    public int maxTrees = 5000;
    public int treeSpacing = 5;

    //PEAKS ----------------------------------
    public int peaksCount = 1;
    public float fallOff = 0.2f;
    public float dropOff = 0.6f;
    public float minHeight = 0;
    public float maxHeight = 0.5f;
    public enum VoronoiType { linear = 0, power = 1, combined = 2, sin = 3, macorine };
    public VoronoiType voronoiType = VoronoiType.linear;

    //MPD -------------------------------------
    public float MPD_minHeight = -2.0f;
    public float MPD_maxHeight = 2.0f;
    public float MPD_roughness = 2.0f;
    public float MPD_heightDampenerPower = 2.0f;

    //SMOOTH ----------------------------------
    public int smoothAmount = 1;

    public Terrain terrain;
    public TerrainData terrainData;

    public enum TagType { tag = 0, layer = 1 };
    [SerializeField]
    int terrainLayer = -1;

    private void Awake()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain", TagType.tag);
        AddTag(tagsProp, "Cloud", TagType.tag);
        AddTag(tagsProp, "Shore", TagType.tag);

        tagManager.ApplyModifiedProperties();

        SerializedProperty layersProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layersProp, "Terrain", TagType.layer);

        tagManager.ApplyModifiedProperties();

        gameObject.tag = "Terrain";
        if (terrainLayer != -1)
            gameObject.layer = terrainLayer;
    }

    private int AddTag(SerializedProperty tagsProp, string newtag, TagType tagType)
    {
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newtag)) { found = true; return i; }
        }

        if (!found && tagType == TagType.tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newtag;
        }
        else if (!found && tagType == TagType.layer)
        {
            for (int j = 6; j < tagsProp.arraySize; j++)
            {
                SerializedProperty layer = tagsProp.GetArrayElementAtIndex(j);
                if (layer.stringValue == "")
                {
                    layer.stringValue = newtag;
                    return j;
                }
            }
        }

        return -1;
    }

    private void OnEnable()
    {
        terrain = GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    float[,] GetHeightMap()
    {
        if (resetTerrain)
            return new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        return terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
            for (int y = 0; y < terrainData.heightmapResolution; y++)
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void SinglePerlin()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
            for (int y = 0; y < terrainData.heightmapResolution; y++)
                heightMap[x, y] += Utils.FBM((x + perlinXOffset) * perlinXScale, (y + perlinYOffset) * perlinYScale, perlinOctaves, perlinPersistence) * perlinHeightScale;
        //Mathf.PerlinNoise((x + perlinXOffset) * perlinXScale, (y + perlinYOffset) * perlinYScale);

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlin()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
            for (int y = 0; y < terrainData.heightmapResolution; y++)
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.FBM((x + p.perlinXOffset) * p.perlinXScale, (y + p.perlinYOffset) * p.perlinYScale, p.perlinOctaves, p.perlinPersistence) * p.perlinHeightScale;
                }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
                keptPerlinParameters.Add(perlinParameters[i]);
        }
        if (keptPerlinParameters.Count == 0)
            keptPerlinParameters.Add(perlinParameters[0]);

        perlinParameters = keptPerlinParameters;
    }

    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
            for (int z = 0; z < terrainData.heightmapResolution; z++)
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;

        terrainData.SetHeights(0, 0, heightMap);
    }

    public Texture2D GetCurrentHeightMap()
    {
        if (heightMapImage == null)
        {
            float[,] heightMap = GetHeightMap();

            for (int y = 0; y < terrainData.heightmapResolution; y++)
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                    heightMapImage.SetPixel(x, y, new Color(heightMap[x, y], heightMap[x, y], heightMap[x, y], 1));

            heightMapImage.Apply();
        }

        return heightMapImage;
    }

    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();

        for (int i = 0; i < peaksCount; i++)
        {
            Vector3 peak = new Vector3(Random.Range(0, terrainData.heightmapResolution), Random.Range(minHeight, maxHeight), Random.Range(0, terrainData.heightmapResolution));
            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue;

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

            for (int z = 0; z < terrainData.heightmapResolution; z++)
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                    if (!(x == peak.x && z == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, z)) / maxDistance;
                        float height = 0.0f;

                        switch (voronoiType)
                        {
                            case VoronoiType.linear:
                                height = peak.y - distanceToPeak * fallOff;
                                break;
                            case VoronoiType.power:
                                height = peak.y - Mathf.Pow(distanceToPeak, dropOff) * fallOff;
                                break;
                            case VoronoiType.combined:
                                height = peak.y - distanceToPeak * fallOff - Mathf.Pow(distanceToPeak, dropOff);
                                break;
                            case VoronoiType.sin:
                                height = peak.y - Mathf.Sin(distanceToPeak * 100) * 0.1f;
                                break;
                            case VoronoiType.macorine:
                                height = peak.y - Mathf.Pow(distanceToPeak * 3, fallOff) - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / dropOff;
                                break;
                        }

                        if (heightMap[x, z] < height)
                            heightMap[x, z] = height;

                    }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MidpointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapResolution - 1;
        int squareSize = width;
        float minheight = MPD_minHeight;
        float maxheight = MPD_maxHeight;
        //float height = (squareSize) / 2.0f * 0.01f;
        float heightDampener = (float)Mathf.Pow(MPD_heightDampenerPower, -1 * MPD_roughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        //heightMap[0, 0] = Random.Range(0, 0.2f);
        //heightMap[0, terrainData.heightmapResolution - 2] = Random.Range(0, 0.2f);
        //heightMap[terrainData.heightmapResolution - 2, 0] = Random.Range(0, 0.2f);
        //heightMap[terrainData.heightmapResolution - 2, terrainData.heightmapResolution - 2] = Random.Range(0, 0.2f);

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)(heightMap[x, y] + heightMap[x, cornerY] + heightMap[cornerX, y] + heightMap[cornerX, cornerY]) / 4.0f + Random.Range(minheight, maxheight);
                }

            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = x + squareSize;
                    cornerY = y + squareSize;

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidXR >= width - 1 || pmidYD <= 0 || pmidYU >= width - 1) continue;

                    heightMap[midX, y] = (float)(heightMap[midX, midY] + heightMap[cornerX, y] + heightMap[x, y] + heightMap[midX, pmidYD]) / 4.0f + Random.Range(minheight, maxheight);
                    heightMap[midX, cornerY] = (float)(heightMap[midX, midY] + heightMap[x, cornerY] + heightMap[cornerX, cornerY] + heightMap[midX, pmidYU]) / 4.0f + Random.Range(minheight, maxheight);
                    heightMap[x, midY] = (float)(heightMap[midX, midY] + heightMap[x, cornerY] + heightMap[x, y] + heightMap[pmidXL, midY]) / 4.0f + Random.Range(minheight, maxheight);
                    heightMap[cornerX, midY] = (float)(heightMap[midX, midY] + heightMap[cornerX, cornerY] + heightMap[cornerX, y] + heightMap[pmidXR, midY]) / 4.0f + Random.Range(minheight, maxheight);
                }
            }

            squareSize = (int)(squareSize / 2.0f);
            minheight *= heightDampener;
            maxheight *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    private List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 npos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1), Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(npos))
                        neighbours.Add(npos);
                }
            }
        }

        return neighbours;
    }

    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        int smoothProgress = 0;

        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < smoothAmount; i++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    float AvgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y), width, height);
                    foreach (Vector2 n in neighbours)
                        AvgHeight += heightMap[(int)n.x, (int)n.y];

                    heightMap[x, y] = AvgHeight / (neighbours.Count + 1.0f);
                }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / smoothAmount);
        }

        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    private float GetSteepness(float[,] heightMap, int x, int y, int width, int height)
    {
        float h = heightMap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightMap[nx, y] - h;
        float dy = heightMap[x, ny] - h;

        Vector2 gradian = new Vector2(dx, dy);

        float steep = gradian.magnitude;

        return steep;
    }

    public void SplatMaps()
    {
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatMapsParameters.Count];
        int spindex = 0;
        foreach (SplatMapsParameters sp in splatMapsParameters)
        {
            newSplatPrototypes[spindex] = new TerrainLayer();
            newSplatPrototypes[spindex].diffuseTexture = sp.texture;
            newSplatPrototypes[spindex].tileOffset = sp.tileOffset;
            newSplatPrototypes[spindex].tileSize = sp.tileSize;
            newSplatPrototypes[spindex].diffuseTexture.Apply(true);
            spindex++;
        }

        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float[,,] splatData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatMapsParameters.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * splatMapsParameters[i].noiseXScale, y * splatMapsParameters[i].noiseYScale) * splatMapsParameters[i].noiseScaler;
                    float offset = splatMapsParameters[i].splatOffset + noise;
                    float thisHeightStart = splatMapsParameters[i].minHeight - offset;
                    float thisHeightStop = splatMapsParameters[i].maxHeight + offset;
                    float thisSlopeStop = splatMapsParameters[i].minSlope;
                    float thisSlopeEnd = splatMapsParameters[i].maxSlope;
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight, x / (float)terrainData.alphamapWidth);//GetSteepness(heightMap, x, y, terrainData.heightmapResolution, terrainData.heightmapResolution);
                    if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop) && (steepness >= thisSlopeStop && steepness <= thisSlopeEnd))
                        splat[i] = 1;
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatMapsParameters.Count; j++)
                    splatData[x, y, j] = splat[j];
            }
        }

        terrainData.SetAlphamaps(0, 0, splatData);
    }

    private void NormalizeVector(float[] splat)
    {
        float sum = 0;
        for (int i = 0; i < splat.Length; i++)
        {
            sum += splat[i];
        }

        for (int i = 0; i < splat.Length; i++)
        {
            splat[i] /= sum;
        }
    }

    public void AddNewSplatMap()
    {
        splatMapsParameters.Add(new SplatMapsParameters());
    }

    public void RemoveSplatMap()
    {
        List<SplatMapsParameters> keptSplatParameters = new List<SplatMapsParameters>();
        for (int i = 0; i < splatMapsParameters.Count; i++)
        {
            if (!splatMapsParameters[i].remove)
                keptSplatParameters.Add(splatMapsParameters[i]);
        }
        if (keptSplatParameters.Count == 0)
            keptSplatParameters.Add(splatMapsParameters[0]);

        splatMapsParameters = keptSplatParameters;
    }

    public void Vegetation()
    {
        TreePrototype[] newTreePrototype;
        newTreePrototype = new TreePrototype[vegetationParameters.Count];
        int index = 0;
        foreach (VegetationParameters vegetation in vegetationParameters)
        {
            newTreePrototype[index] = new TreePrototype();
            newTreePrototype[index].prefab = vegetation.mesh;
            index++;
        }

        terrainData.treePrototypes = newTreePrototype;
        float randomrange = 0.5f;

        List<TreeInstance> allVegetations = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (Random.Range(0.0f, 1.0f) > vegetationParameters[tp].density) break;

                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x, z / (float)terrainData.size.z);

                    if (thisHeight >= vegetationParameters[tp].minHeight && thisHeight <= vegetationParameters[tp].maxHeight &&
                        steepness >= vegetationParameters[tp].minSloop && steepness <= vegetationParameters[tp].maxSloop)
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + Random.Range(-randomrange, randomrange)) / terrainData.size.x, thisHeight, (z + Random.Range(-randomrange, randomrange)) / terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x, instance.position.y * terrainData.size.y, instance.position.z * terrainData.size.z);
                        RaycastHit hit;
                        int layermask = 1 << terrainLayer;
                        if (Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), -Vector3.up, out hit, 100, layermask) || Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layermask))
                        {
                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(instance.position.x, treeHeight, instance.position.z);

                            instance.rotation = Random.Range(vegetationParameters[tp].minRotation, vegetationParameters[tp].maxRotation);
                            instance.prototypeIndex = tp;
                            instance.color = Color.Lerp(vegetationParameters[tp].color1, vegetationParameters[tp].color2, Random.Range(0.0f, 1.0f));
                            instance.lightmapColor = vegetationParameters[tp].lightmapColor;
                            float scale = Random.Range(vegetationParameters[tp].minScale, vegetationParameters[tp].maxScale);
                            instance.heightScale = scale;
                            instance.widthScale = scale;
                            allVegetations.Add(instance);
                            if (allVegetations.Count >= maxTrees) goto TREESDONE;
                        }
                    }
                }
            }
        }
    TREESDONE:
        terrainData.treeInstances = allVegetations.ToArray();
    }


    public void AddNewVegetation()
    {
        vegetationParameters.Add(new VegetationParameters());
    }

    public void RemoveVegetation()
    {
        List<VegetationParameters> keptVegetationParameters = new List<VegetationParameters>();
        for (int i = 0; i < vegetationParameters.Count; i++)
        {
            if (!vegetationParameters[i].remove)
                keptVegetationParameters.Add(vegetationParameters[i]);
        }
        if (keptVegetationParameters.Count == 0)
            keptVegetationParameters.Add(vegetationParameters[0]);

        vegetationParameters = keptVegetationParameters;
    }


    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        terrainData.SetHeights(0, 0, heightMap);
    }


}
