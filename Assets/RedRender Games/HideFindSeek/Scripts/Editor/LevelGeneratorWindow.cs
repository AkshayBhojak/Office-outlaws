using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace Game.LevelGenerator
{
    public class LevelGeneratorWindow : EditorWindow
    {
        private int _startLevel = 1;
        private int _endLevel = 10;
        private string _templateScenePath = "Assets/RedRender Games/HideFindSeek/Scenes/Level1.unity";
        private string _scenesFolder = "Assets/RedRender Games/HideFindSeek/Scenes";
        private float _levelScale = 1.5f;
        private float _minWallDistance = 3.0f;
        private float _innerWallLengthMultiplier = 1.4f;
        public bool silentMode = false;

        [MenuItem("Window/Hide Find Seek/Level Generator")]
        public static void ShowWindow()
        {
            GetWindow<LevelGeneratorWindow>("Level Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Procedural Level Generator (Office Outlaws)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _startLevel = EditorGUILayout.IntField("Start Level Index", _startLevel);
            _endLevel = EditorGUILayout.IntField("End Level Index", _endLevel);
            _templateScenePath = EditorGUILayout.TextField("Template Scene Path", _templateScenePath);
            _scenesFolder = EditorGUILayout.TextField("Output Scenes Folder", _scenesFolder);
            _levelScale = EditorGUILayout.FloatField("Level Size Multiplier", _levelScale);
            _minWallDistance = EditorGUILayout.FloatField("Min Wall Distance", _minWallDistance);
            _innerWallLengthMultiplier = EditorGUILayout.FloatField("Inner Wall Length Multiplier", _innerWallLengthMultiplier);

            EditorGUILayout.Space();

            if (GUILayout.Button("Reskin Project Materials (Office)"))
            {
                ReskinProjectMaterials();
                EditorUtility.DisplayDialog("Success", "Office Outlaws materials applied to the project!", "OK");
            }

            if (GUILayout.Button("Generate Levels"))
            {
                // Ensure materials are reskinned first
                ReskinProjectMaterials();
                GenerateLevels();
            }
        }

        public void ReskinProjectMaterials()
        {
            Debug.Log("[LevelGenerator] Applying Office Outlaws material reskin to project assets...");

            string materialsFolder = "Assets/RedRender Games/HideFindSeek/ResourcesStatic/Materials";

            // 1. Office Floor (Blue-grey office carpet tint)
            Color floorColor = new Color(0.75f, 0.78f, 0.82f, 1f);
            string[] floorMats = { "GroundLevel1.mat", "GroundLevel2.mat", "GroundLevel3.mat" };
            foreach (var matName in floorMats)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsFolder}/{matName}");
                if (mat != null)
                {
                    mat.color = floorColor;
                    EditorUtility.SetDirty(mat);
                }
            }

            // 2. Office Cubicle Walls (Light beige office cubicle walls)
            Color wallColor = new Color(0.9f, 0.88f, 0.85f, 1f);
            string[] wallMats = { "WallLevel1.mat", "WallLevel2.mat", "WallLevel3.mat" };
            foreach (var matName in wallMats)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsFolder}/{matName}");
                if (mat != null)
                {
                    mat.color = wallColor;
                    EditorUtility.SetDirty(mat);
                }
            }

            // 3. Office Buildings/Cabinet blocks (Wooden desk/cabinet color)
            Color woodenColor = new Color(0.55f, 0.45f, 0.35f, 1f);
            string[] woodenMats = { "BuildingLevel1.mat", "BuildingLevel2.mat", "BuildingLevel3.mat", "WallPlinthLevel1.mat", "WallPlinthLevel3.mat" };
            foreach (var matName in woodenMats)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsFolder}/{matName}");
                if (mat != null)
                {
                    mat.color = woodenColor;
                    EditorUtility.SetDirty(mat);
                }
            }

            // 4. Gold / Coins (Cash Green)
            var goldMat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsFolder}/Gold.mat");
            if (goldMat != null)
            {
                goldMat.color = new Color(0.2f, 0.75f, 0.3f, 1f);
                goldMat.EnableKeyword("_EMISSION");
                goldMat.SetColor("_EmissionColor", new Color(0.1f, 0.5f, 0.15f) * 1.2f);
                EditorUtility.SetDirty(goldMat);
            }

            // 5. Door / Cage / Default (Steel grey / corporate borders)
            Color steelColor = new Color(0.4f, 0.45f, 0.5f, 1f);
            string[] steelMats = { "Door.mat", "Default.mat" };
            foreach (var matName in steelMats)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>($"{materialsFolder}/{matName}");
                if (mat != null)
                {
                    mat.color = steelColor;
                    EditorUtility.SetDirty(mat);
                }
            }

            // Save all material modifications
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LevelGenerator] Material reskin saved successfully.");
        }

        public void GenerateLevels()
        {
            // Self-healing template backup if template is Level1.unity
            if (_templateScenePath == "Assets/RedRender Games/HideFindSeek/Scenes/Level1.unity")
            {
                string backupPath = "Assets/RedRender Games/HideFindSeek/Scenes/TemplateLevel.unity";
                if (!System.IO.File.Exists(backupPath))
                {
                    AssetDatabase.CopyAsset(_templateScenePath, backupPath);
                    AssetDatabase.Refresh();
                    Debug.Log($"[LevelGenerator] Backed up Level1.unity to {backupPath}");
                }
                _templateScenePath = backupPath;
            }

            if (!File.Exists(_templateScenePath))
            {
                if (!silentMode)
                {
                    EditorUtility.DisplayDialog("Error", $"Template scene not found at: {_templateScenePath}", "OK");
                }
                else
                {
                    Debug.LogError($"[LevelGenerator] Template scene not found at: {_templateScenePath}");
                }
                return;
            }

            // Save current scene first
            if (silentMode)
            {
                EditorSceneManager.SaveOpenScenes();
            }
            else
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            for (int levelIndex = _startLevel; levelIndex <= _endLevel; levelIndex++)
            {
                string newScenePath = $"{_scenesFolder}/Level{levelIndex}.unity";

                // 1. Copy template scene
                if (File.Exists(newScenePath))
                {
                    AssetDatabase.DeleteAsset(newScenePath);
                }

                if (!AssetDatabase.CopyAsset(_templateScenePath, newScenePath))
                {
                    Debug.LogError($"[LevelGenerator] Failed to copy scene to {newScenePath}");
                    continue;
                }

                AssetDatabase.Refresh();

                // 2. Open the newly copied scene
                var scene = EditorSceneManager.OpenScene(newScenePath, OpenSceneMode.Single);
                Debug.Log($"[LevelGenerator] Opened scene: {scene.name}");

                // 3. Find and randomise level layout
                var levelView = FindObjectOfType<LevelView>();
                if (levelView == null)
                {
                    Debug.LogError("[LevelGenerator] LevelView component not found in scene!");
                    continue;
                }

                // Apply Level-Specific Reskin Materials (Edit Mode)
                ReskinLevelMaterials(levelIndex, levelView);

                // Configure Lighting and Spotlights (Edit Mode)
                ConfigureLevelLighting(levelIndex, levelView);

                RandomizeLevelLayout(levelView);

                // 4. Mark all environment obstacles as Navigation Static
                SetNavigationStatic(levelView);

                // 5. Bake the NavMesh
                Debug.Log($"[LevelGenerator] Baking NavMesh for Level {levelIndex}...");
                UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

                // 6. Save and close
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[LevelGenerator] Successfully generated Level {levelIndex}!");
            }

            // 7. Add generated levels to Build Settings
            AddLevelsToBuildSettings();

            if (!silentMode)
            {
                EditorUtility.DisplayDialog("Success", $"Generated levels {_startLevel} to {_endLevel} successfully!", "OK");
            }
            else
            {
                Debug.Log($"[LevelGenerator] Generated levels {_startLevel} to {_endLevel} successfully!");
            }
        }

        private void ReskinLevelMaterials(int levelIndex, LevelView levelView)
        {
            var theme = Game.Utilities.RuntimeReskinner.GetThemeForLevel(levelIndex);
            bool isNight = (levelIndex % 2 == 0);

            // Create a directory for this level's materials if it doesn't exist
            string levelFolder = $"{_scenesFolder}/Level{levelIndex}";
            if (!AssetDatabase.IsValidFolder(levelFolder))
            {
                AssetDatabase.CreateFolder(_scenesFolder, $"Level{levelIndex}");
            }

            var renderers = levelView.GetComponentsInChildren<MeshRenderer>(true);
            Dictionary<Material, Material> matCache = new Dictionary<Material, Material>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                string name = renderer.name.ToLower();
                if (renderer.transform.parent != null)
                {
                    name += " " + renderer.transform.parent.name.ToLower();
                    if (renderer.transform.parent.parent != null)
                    {
                        name += " " + renderer.transform.parent.parent.name.ToLower();
                    }
                }

                // Check if background skyscraper
                bool isBackground = (renderer.transform.parent != null && 
                                     (renderer.transform.parent.name.Contains("EnvironmentLevel1") || 
                                      renderer.transform.parent.name.Contains("EnvironmentLevel3")));

                var materials = renderer.sharedMaterials;
                bool changed = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    var originalMat = materials[i];
                    if (originalMat == null) continue;

                    string matName = originalMat.name.ToLower();

                    // Only reskin standard environment materials, do not touch character textures!
                    if (matName.Contains("ground") || matName.Contains("floor") || matName.Contains("wall") || 
                        matName.Contains("building") || matName.Contains("plinth") || matName.Contains("gold") || 
                        matName.Contains("coin") || matName.Contains("door") || matName.Contains("default") ||
                        matName.Contains("cage"))
                    {
                        if (!matCache.TryGetValue(originalMat, out Material newMat))
                        {
                            // Create a copy of the material in the Level folder
                            string baseMatName = originalMat.name.Replace($"_L{levelIndex}", ""); // Clean up potential double names
                            string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{levelFolder}/{baseMatName}_L{levelIndex}.mat");
                            newMat = new Material(originalMat);
                            AssetDatabase.CreateAsset(newMat, uniquePath);

                            // Apply Theme Colors
                            Color nightMult = isNight ? new Color(0.4f, 0.45f, 0.65f, 1f) : Color.white;

                            if (isBackground)
                            {
                                if (isNight)
                                {
                                    newMat.color = new Color(0.12f, 0.15f, 0.28f, 1f); // Dark blue silhouette
                                }
                                else
                                {
                                    newMat.color = new Color(0.5f, 0.52f, 0.58f, 1f); // Steel grey city buildings
                                }
                            }
                            else if (matName.Contains("floor") || matName.Contains("ground"))
                            {
                                newMat.color = theme.FloorColor * nightMult;
                            }
                            else if (matName.Contains("wall"))
                            {
                                newMat.color = theme.WallColor * nightMult;
                            }
                            else if (matName.Contains("building") || matName.Contains("plinth"))
                            {
                                newMat.color = theme.WoodColor * nightMult;
                            }
                            else if (matName.Contains("gold") || matName.Contains("coin"))
                            {
                                if (isNight)
                                {
                                    newMat.color = new Color(0.3f, 0.9f, 0.4f, 1f);
                                    newMat.EnableKeyword("_EMISSION");
                                    newMat.SetColor("_EmissionColor", new Color(0.1f, 0.7f, 0.2f) * 3f);
                                }
                                else
                                {
                                    newMat.color = theme.CoinColor;
                                    newMat.EnableKeyword("_EMISSION");
                                    newMat.SetColor("_EmissionColor", new Color(theme.CoinColor.r * 0.5f, theme.CoinColor.g * 0.5f, theme.CoinColor.b * 0.5f) * 1.2f);
                                }
                            }
                            else if (matName.Contains("cage") || matName.Contains("door") || matName.Contains("default"))
                            {
                                newMat.color = theme.SteelColor * nightMult;
                            }

                            EditorUtility.SetDirty(newMat);
                            matCache[originalMat] = newMat;
                        }

                        materials[i] = newMat;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(renderer);
                }
            }

            AssetDatabase.SaveAssets();
        }

        private void ConfigureLevelLighting(int levelIndex, LevelView levelView)
        {
            bool isNight = (levelIndex % 2 == 0);
            var theme = Game.Utilities.RuntimeReskinner.GetThemeForLevel(levelIndex);

            // Find Light container under the levelView root
            Transform lightContainer = levelView.transform.Find("Light");
            if (lightContainer == null)
            {
                // Fallback to searching all children for a gameobject named "Light"
                foreach (Transform child in levelView.transform)
                {
                    if (child.name == "Light")
                    {
                        lightContainer = child;
                        break;
                    }
                }
            }

            if (lightContainer == null)
            {
                Debug.LogWarning("[LevelGenerator] 'Light' container not found under level view root!");
                return;
            }

            // 1. Adjust Directional Lights
            var lights = lightContainer.GetComponentsInChildren<Light>(true);
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    if (isNight)
                    {
                        // Dim cool moonlight
                        light.color = new Color(0.2f, 0.3f, 0.55f, 1f);
                        light.intensity = light.name.Contains("1") ? 0.02f : 0.08f;
                    }
                    else
                    {
                        // Bright warm office light
                        light.color = new Color(1.0f, 0.98f, 0.9f, 1f);
                        light.intensity = light.name.Contains("1") ? 0.1f : 0.8f;
                    }
                    EditorUtility.SetDirty(light);
                }
            }

            // 2. Adjust / Create Spotlights
            CreateOfficeSpotlights(lightContainer, isNight, theme.SpotlightColor);

            // 3. Set Ambient Light Settings in RenderSettings for the Scene
            if (isNight)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.12f, 0.15f, 0.25f, 1f);
            }
            else
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.72f, 1f);
            }
        }

        private void CreateOfficeSpotlights(Transform lightContainer, bool isNight, Color spotlightColor)
        {
            // First, destroy any existing Spotlights container to prevent duplicates
            var existingSpotlights = lightContainer.Find("Spotlights");
            if (existingSpotlights != null)
            {
                DestroyImmediate(existingSpotlights.gameObject);
            }

            if (!isNight) return;

            // Create new container
            GameObject spotlightsGO = new GameObject("Spotlights");
            spotlightsGO.transform.SetParent(lightContainer, false);

            // Define grid positions for 5 spotlights (spread further out and raised slightly)
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0f, 6.5f, 0f),       // Center
                new Vector3(-6.8f, 6.5f, -6.8f), // Bottom-Left
                new Vector3(-6.8f, 6.5f, 6.8f),  // Top-Left
                new Vector3(6.8f, 6.5f, -6.8f),  // Bottom-Right
                new Vector3(6.8f, 6.5f, 6.8f)    // Top-Right
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject spotGO = new GameObject($"Spotlight_{i + 1}");
                spotGO.transform.SetParent(spotlightsGO.transform, false);
                spotGO.transform.localPosition = positions[i] * _levelScale;
                spotGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                Light lightComp = spotGO.AddComponent<Light>();
                lightComp.type = LightType.Spot;
                lightComp.range = 18f * _levelScale;
                lightComp.spotAngle = 105f;
                lightComp.color = spotlightColor;
                lightComp.intensity = 8f;
                lightComp.shadows = LightShadows.Hard; // Shadows cast behind office objects
            }
        }

        private void RandomizeLevelLayout(LevelView levelView)
        {
            // 1. Scale FloorMesh
            Transform floorMesh = levelView.transform.Find("Level0/FloorMesh") ?? levelView.transform.Find("Level3/FloorMesh");
            if (floorMesh != null)
            {
                floorMesh.localScale = new Vector3(20f * _levelScale, 20f * _levelScale, 1f);
                EditorUtility.SetDirty(floorMesh.gameObject);
            }

            // 2. Scale Doors
            var doorsContainer = levelView.transform.Find("Level0/Doors") ?? levelView.transform.Find("Level3/Doors");
            if (doorsContainer != null)
            {
                foreach (Transform door in doorsContainer)
                {
                    door.localPosition = new Vector3(door.localPosition.x * _levelScale, door.localPosition.y, door.localPosition.z * _levelScale);
                    EditorUtility.SetDirty(door.gameObject);
                }
            }

            // 3. Find and scale outer walls, destroy inner walls
            var walls = levelView.transform.Find("Level0/Walls") ?? levelView.transform.Find("Level3/Walls");
            if (walls != null)
            {
                List<GameObject> toDestroy = new List<GameObject>();
                foreach (Transform child in walls)
                {
                    // If it is an outer wall (on the boundary)
                    if (Mathf.Abs(child.localPosition.x) >= 9.5f || Mathf.Abs(child.localPosition.z) >= 9.5f)
                    {
                        child.localPosition = new Vector3(child.localPosition.x * _levelScale, child.localPosition.y, child.localPosition.z * _levelScale);
                        child.localScale = new Vector3(child.localScale.x * _levelScale, child.localScale.y, child.localScale.z);
                        EditorUtility.SetDirty(child.gameObject);
                    }
                    else
                    {
                        toDestroy.Add(child.gameObject);
                    }
                }

                foreach (var go in toDestroy)
                {
                    DestroyImmediate(go);
                }
            }

            // --- Spawn Procedural Partition Walls & Internal Room Doors ---
            if (walls != null && doorsContainer != null)
            {
                float vWall1X = -4.0f * _levelScale;
                float vWall2X = 4.0f * _levelScale;
                float zRange = 8.0f * _levelScale;

                // Vertical Wall 1 (Left Room Divider)
                SpawnPartitionWallLine(walls, doorsContainer, new Vector3(vWall1X, 0f, -zRange), new Vector3(vWall1X, 0f, zRange), 0.5f);

                // Vertical Wall 2 (Right Room Divider)
                SpawnPartitionWallLine(walls, doorsContainer, new Vector3(vWall2X, 0f, -zRange), new Vector3(vWall2X, 0f, zRange), 0.5f);

                // Horizontal Wall 1 (Left Inner Room Splitter)
                SpawnPartitionWallLine(walls, doorsContainer, new Vector3(-8.0f * _levelScale, 0f, 0f), new Vector3(vWall1X, 0f, 0f), 0.5f);

                // Horizontal Wall 2 (Right Inner Room Splitter)
                SpawnPartitionWallLine(walls, doorsContainer, new Vector3(vWall2X, 0f, 0f), new Vector3(8.0f * _levelScale, 0f, 0f), 0.5f);
            }

            // Define modular office furniture prefabs from VNB Office Set
            string[] furniturePaths = new string[]
            {
                "Assets/VNB - Office Set/Prefabs/Office/desk_1.prefab",
                "Assets/VNB - Office Set/Prefabs/Office/desk_2.prefab",
                "Assets/VNB - Office Set/Prefabs/Office/desk_3.prefab",
                "Assets/VNB - Office Set/Prefabs/Office/desk_4_2_people.prefab",
                "Assets/VNB - Office Set/Prefabs/Office/desk_5.prefab",
                "Assets/VNB - Office Set/Prefabs/Furniture/bookshelf.prefab",
                "Assets/VNB - Office Set/Prefabs/Furniture/file_cabinet_medium.prefab",
                "Assets/VNB - Office Set/Prefabs/Furniture/file_cabinet_small.prefab",
                "Assets/VNB - Office Set/Prefabs/Furniture/counter_a_1.prefab",
                "Assets/VNB - Office Set/Prefabs/Furniture/counter_a_2.prefab",
                "Assets/VNB - Office Set/Prefabs/Furniture/table.prefab",
                "Assets/VNB - Office Set/Prefabs/Furniture/whiteboard_wall.prefab",
                "Assets/VNB - Office Set/Prefabs/Office/printer.prefab"
            };

            // Area boundary constraints (scaled according to level size)
            float minRadius = 2.0f * _levelScale;
            float maxRadius = 8.5f * _levelScale;
            List<Vector3> placedPositions = new List<Vector3>();

            // Spawn random modular office furniture items
            int numObstacles = Random.Range(11, 16); // 11 to 15 furniture items per level

            for (int i = 0; i < numObstacles; i++)
            {
                Vector3 pos = Vector3.zero;
                bool valid = false;

                // Try to find a non-overlapping spot
                for (int attempts = 0; attempts < 40; attempts++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float r = Random.Range(minRadius, maxRadius);
                    pos = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);

                    bool overlaps = false;
                    foreach (var placed in placedPositions)
                    {
                        if (Vector3.Distance(pos, placed) < _minWallDistance)
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    // Check distance to vertical walls to avoid overlapping partition walls
                    float vWall1X = -4.0f * _levelScale;
                    float vWall2X = 4.0f * _levelScale;
                    if (Mathf.Abs(pos.x - vWall1X) < 1.8f || Mathf.Abs(pos.x - vWall2X) < 1.8f)
                    {
                        overlaps = true;
                    }
                    // Check distance to horizontal walls to avoid overlapping partition walls
                    if (Mathf.Abs(pos.z) < 1.8f && (pos.x < vWall1X || pos.x > vWall2X))
                    {
                        overlaps = true;
                    }

                    if (!overlaps)
                    {
                        valid = true;
                        break;
                    }
                }

                if (valid && walls != null)
                {
                    string prefabPath = furniturePaths[Random.Range(0, furniturePaths.Length)];
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        GameObject spawned = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                        if (spawned != null)
                        {
                            spawned.transform.SetParent(walls);
                            spawned.transform.localPosition = pos;
                            spawned.transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f);
                            spawned.transform.localScale = Vector3.one * 1.5f; // scale matching characters

                            // Ensure it has a box collider or mesh collider
                            if (spawned.GetComponentInChildren<Collider>() == null)
                            {
                                var renderers = spawned.GetComponentsInChildren<MeshRenderer>();
                                Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
                                bool hasBounds = false;
                                foreach (var rdr in renderers)
                                {
                                    if (!hasBounds)
                                    {
                                        combinedBounds = rdr.bounds;
                                        hasBounds = true;
                                    }
                                    else
                                    {
                                        combinedBounds.Encapsulate(rdr.bounds);
                                    }
                                }

                                if (hasBounds)
                                {
                                    BoxCollider col = spawned.AddComponent<BoxCollider>();
                                    col.center = spawned.transform.InverseTransformPoint(combinedBounds.center);
                                    col.size = spawned.transform.InverseTransformVector(combinedBounds.size);
                                }
                            }

                            placedPositions.Add(pos);
                            EditorUtility.SetDirty(spawned);
                        }
                    }
                }
            }

            // Reposition Coins and Gold Bars
            var coinsContainer = levelView.transform.Find("Level0/Coins") ?? levelView.transform.Find("Level3/Coins");
            if (coinsContainer != null)
            {
                foreach (Transform coin in coinsContainer)
                {
                    Vector3 pos = Vector3.zero;
                    bool valid = false;

                    for (int attempts = 0; attempts < 30; attempts++)
                    {
                        float angle = Random.Range(0f, Mathf.PI * 2f);
                        float r = Random.Range(minRadius, maxRadius);
                        pos = new Vector3(Mathf.Cos(angle) * r, coin.position.y, Mathf.Sin(angle) * r);

                        // Ensure it's not too close to obstacles
                        bool tooClose = false;
                        foreach (var obstaclePos in placedPositions)
                        {
                            if (Vector3.Distance(pos, obstaclePos) < 1.0f * Mathf.Min(_levelScale, 1.5f))
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (!tooClose)
                        {
                            valid = true;
                            break;
                        }
                    }

                    if (valid)
                    {
                        coin.position = pos;
                        EditorUtility.SetDirty(coin.gameObject);
                    }
                }
            }

            // Position Boss (Suit/Seeker model) in the center (0, y, 0) so it becomes Units[0]
            Transform bossTransform = levelView.transform.Find("UnitF") ?? levelView.transform.Find("EnemyE") ?? levelView.transform.Find("EnemyF");
            if (bossTransform != null)
            {
                bossTransform.position = new Vector3(0f, bossTransform.position.y, 0f);
                EditorUtility.SetDirty(bossTransform.gameObject);
            }

            // Reposition other units at random open spots (including UnitA/PlayerA which are the Employee/Hider players)
            string[] allUnitNames = { "UnitA", "UnitB", "UnitC", "UnitD", "UnitE", "UnitF", "PlayerA", "EnemyB", "EnemyC", "EnemyD", "EnemyE", "EnemyF" };
            foreach (var enemyName in allUnitNames)
            {
                var enemy = levelView.transform.Find(enemyName);
                if (enemy != null && enemy != bossTransform)
                {
                    Vector3 pos = Vector3.zero;
                    bool valid = false;

                    for (int attempts = 0; attempts < 40; attempts++)
                    {
                        float angle = Random.Range(0f, Mathf.PI * 2f);
                        float r = Random.Range(minRadius + 1.5f, maxRadius - 0.5f);
                        pos = new Vector3(Mathf.Cos(angle) * r, enemy.position.y, Mathf.Sin(angle) * r);

                        // Ensure they don't overlap with obstacles
                        bool overlaps = false;
                        foreach (var obstaclePos in placedPositions)
                        {
                            if (Vector3.Distance(pos, obstaclePos) < 1.4f)
                            {
                                overlaps = true;
                                break;
                            }
                        }

                        if (!overlaps)
                        {
                            valid = true;
                            break;
                        }
                    }

                    if (valid)
                    {
                        enemy.position = pos;
                        enemy.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                        EditorUtility.SetDirty(enemy.gameObject);
                    }
                }
            }

            // --- Spawn Coffee Speed Power-Ups ---
            Transform levelRoot = levelView.transform.Find("Level0") ?? levelView.transform.Find("Level3");
            Transform powerUpsContainer = null;
            if (levelRoot != null)
            {
                powerUpsContainer = levelRoot.Find("PowerUps");
                if (powerUpsContainer != null)
                {
                    DestroyImmediate(powerUpsContainer.gameObject);
                }
                GameObject powerUpsGO = new GameObject("PowerUps");
                powerUpsGO.transform.SetParent(levelRoot, false);
                powerUpsContainer = powerUpsGO.transform;
            }

            if (powerUpsContainer != null)
            {
                string cupPrefabPath = "Assets/VNB - Office Set/Prefabs/Food/coffee_cup.prefab";
                string glowPrefabPath = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR3 Ambient Glows.prefab";
                GameObject cupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cupPrefabPath);
                GameObject glowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glowPrefabPath);

                if (cupPrefab != null && glowPrefab != null)
                {
                    List<Vector3> powerUpPositions = new List<Vector3>();
                    for (int p = 0; p < 2; p++)
                    {
                        Vector3 pos = Vector3.zero;
                        bool valid = false;

                        for (int attempts = 0; attempts < 50; attempts++)
                        {
                            float angle = Random.Range(0f, Mathf.PI * 2f);
                            float r = Random.Range(minRadius, maxRadius);
                            pos = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);

                            // Check distance to furniture obstacles
                            bool overlaps = false;
                            foreach (var obstaclePos in placedPositions)
                            {
                                if (Vector3.Distance(pos, obstaclePos) < 1.8f)
                                {
                                    overlaps = true;
                                    break;
                                }
                            }

                            // Check distance to other powerups
                            foreach (var puPos in powerUpPositions)
                            {
                                if (Vector3.Distance(pos, puPos) < 4.0f)
                                {
                                    overlaps = true;
                                    break;
                                }
                            }

                            // Check distance to vertical partition walls
                            float vWall1X = -4.0f * _levelScale;
                            float vWall2X = 4.0f * _levelScale;
                            if (Mathf.Abs(pos.x - vWall1X) < 1.8f || Mathf.Abs(pos.x - vWall2X) < 1.8f)
                            {
                                overlaps = true;
                            }
                            // Check distance to horizontal partition walls
                            if (Mathf.Abs(pos.z) < 1.8f && (pos.x < vWall1X || pos.x > vWall2X))
                            {
                                overlaps = true;
                            }

                            // Ensure it's not too close to the center (boss spawning point)
                            if (Vector3.Distance(pos, Vector3.zero) < 3.0f)
                            {
                                overlaps = true;
                            }

                            if (!overlaps)
                            {
                                valid = true;
                                break;
                            }
                        }

                        if (valid)
                        {
                            powerUpPositions.Add(pos);

                            // Create the parent CoffeePowerUp GameObject
                            GameObject powerUpGO = new GameObject($"CoffeePowerUp_{p + 1}");
                            powerUpGO.transform.SetParent(powerUpsContainer, false);
                            powerUpGO.transform.localPosition = pos;

                            // Add collider (used only for editor gizmo visualization; detection uses OverlapSphere at runtime)
                            SphereCollider triggerCol = powerUpGO.AddComponent<SphereCollider>();
                            triggerCol.isTrigger = true;
                            triggerCol.radius = 1.5f;

                            // Add CoffeePowerUp component
                            powerUpGO.AddComponent<CoffeePowerUp>();

                            // Create the "Visuals" child
                            GameObject visualsGO = new GameObject("Visuals");
                            visualsGO.transform.SetParent(powerUpGO.transform, false);
                            visualsGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);

                            // Instantiate the coffee cup mesh as child of Visuals
                            GameObject cupInstance = PrefabUtility.InstantiatePrefab(cupPrefab) as GameObject;
                            if (cupInstance != null)
                            {
                                cupInstance.transform.SetParent(visualsGO.transform, false);
                                cupInstance.transform.localPosition = Vector3.zero;
                                cupInstance.transform.localRotation = Quaternion.identity;
                                cupInstance.transform.localScale = Vector3.one * 15.0f; // Large enough to be clearly visible in the level
                            }

                            // Instantiate the glowing particle effect as child of Visuals so it hides when cup is collected
                            GameObject glowInstance = PrefabUtility.InstantiatePrefab(glowPrefab) as GameObject;
                            if (glowInstance != null)
                            {
                                glowInstance.transform.SetParent(visualsGO.transform, false);
                                glowInstance.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                                glowInstance.transform.localRotation = Quaternion.identity;
                                glowInstance.transform.localScale = Vector3.one * 3.5f; // Glow around the large cup
                            }

                            EditorUtility.SetDirty(powerUpGO);
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[LevelGenerator] Prefabs not found at paths. Cup: {cupPrefabPath}, Glow: {glowPrefabPath}");
                }
            }
        }

        private void SetNavigationStatic(LevelView levelView)
        {
            var walls = levelView.transform.Find("Level0/Walls") ?? levelView.transform.Find("Level3/Walls");
            if (walls != null)
            {
                var transforms = walls.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.NavigationStatic);
                }
            }

            var renderers = levelView.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                string name = renderer.name.ToLower();
                if (name.Contains("floor") || name.Contains("ground") || name.Contains("wall") || name.Contains("building"))
                {
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.NavigationStatic);
                }
            }
        }

        private void SpawnPartitionWallLine(Transform wallsContainer, Transform doorsContainer, Vector3 start, Vector3 end, float doorwayT)
        {
            string wallPath = "Assets/RedRender Games/HideFindSeek/ResourcesStatic/Prefabs/Walls/WallLevel1.prefab";
            string doorPath = "Assets/RedRender Games/HideFindSeek/ResourcesStatic/Prefabs/Environment/Door.prefab";

            GameObject wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wallPath);
            GameObject doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(doorPath);

            if (wallPrefab == null || doorPrefab == null) return;

            float wallLength = 3.0f; // Standard wall length (scaled)
            Vector3 direction = end - start;
            float totalLength = direction.magnitude;
            direction.Normalize();

            int numSegments = Mathf.RoundToInt(totalLength / wallLength);
            float step = totalLength / numSegments;

            // Determine which segment index gets the door
            int doorSegmentIndex = Mathf.Clamp(Mathf.RoundToInt(doorwayT * (numSegments - 1)), 1, numSegments - 2);

            for (int i = 0; i < numSegments; i++)
            {
                Vector3 pos = start + direction * (i * step + step * 0.5f);
                Quaternion rot = Quaternion.LookRotation(new Vector3(-direction.z, 0f, direction.x), Vector3.up);

                if (i == doorSegmentIndex)
                {
                    // Spawn Door
                    GameObject spawnedDoor = PrefabUtility.InstantiatePrefab(doorPrefab) as GameObject;
                    if (spawnedDoor != null)
                    {
                        spawnedDoor.transform.SetParent(doorsContainer);
                        spawnedDoor.transform.position = pos;
                        spawnedDoor.transform.rotation = rot;
                        spawnedDoor.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Match WallLevel1 height
                        EditorUtility.SetDirty(spawnedDoor);
                    }
                }
                else
                {
                    // Spawn Wall
                    GameObject spawnedWall = PrefabUtility.InstantiatePrefab(wallPrefab) as GameObject;
                    if (spawnedWall != null)
                    {
                        spawnedWall.transform.SetParent(wallsContainer);
                        spawnedWall.transform.position = pos;
                        spawnedWall.transform.rotation = rot;
                        spawnedWall.transform.localScale = new Vector3(2.0f * wallLength, 1.0f, 1.0f); // Scale to match WallLevel1 dimensions
                        EditorUtility.SetDirty(spawnedWall);
                    }
                }
            }
        }

        private void AddLevelsToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenesList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            for (int i = _startLevel; i <= _endLevel; i++)
            {
                string path = $"{_scenesFolder}/Level{i}.unity";
                
                bool exists = false;
                foreach (var s in scenesList)
                {
                    if (s.path == path)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists && File.Exists(path))
                {
                    scenesList.Add(new EditorBuildSettingsScene(path, true));
                    Debug.Log($"[LevelGenerator] Added {path} to Build Settings.");
                }
            }

            EditorBuildSettings.scenes = scenesList.ToArray();
        }
    }
}
