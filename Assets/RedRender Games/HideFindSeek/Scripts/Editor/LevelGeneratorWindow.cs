using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace Game.LevelGenerator
{
    public class LevelGeneratorWindow : EditorWindow
    {
        private int _startLevel = 4;
        private int _endLevel = 10;
        private string _templateScenePath = "Assets/RedRender Games/HideFindSeek/Scenes/Level1.unity";
        private string _scenesFolder = "Assets/RedRender Games/HideFindSeek/Scenes";

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

        private void ReskinProjectMaterials()
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

        private void GenerateLevels()
        {
            if (!File.Exists(_templateScenePath))
            {
                EditorUtility.DisplayDialog("Error", $"Template scene not found at: {_templateScenePath}", "OK");
                return;
            }

            // Save current scene first
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

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

            EditorUtility.DisplayDialog("Success", $"Generated levels {_startLevel} to {_endLevel} successfully!", "OK");
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
                spotGO.transform.localPosition = positions[i];
                spotGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                Light lightComp = spotGO.AddComponent<Light>();
                lightComp.type = LightType.Spot;
                lightComp.range = 18f;
                lightComp.spotAngle = 105f;
                lightComp.color = spotlightColor;
                lightComp.intensity = 8f;
                lightComp.shadows = LightShadows.Hard; // Shadows cast behind office objects
            }
        }

        private void RandomizeLevelLayout(LevelView levelView)
        {
            // Collect only inner walls to randomise
            List<Transform> obstacles = new List<Transform>();

            // Find all Walls inside Level0/Walls / Level3/Walls
            var walls = levelView.transform.Find("Level0/Walls") ?? levelView.transform.Find("Level3/Walls");
            if (walls != null)
            {
                foreach (Transform child in walls)
                {
                    // Only collect inner walls (whose X and Z local positions are within playable bounds, not on the outer border at +/- 10)
                    if (Mathf.Abs(child.localPosition.x) < 9.5f && Mathf.Abs(child.localPosition.z) < 9.5f)
                    {
                        obstacles.Add(child);
                    }
                }
            }

            // Area boundary constraints (strictly inside the outer walls at 10)
            float minRadius = 2.0f;
            float maxRadius = 8.5f;

            // Pick random locations for inner obstacles
            List<Vector3> placedPositions = new List<Vector3>();

            foreach (var obstacle in obstacles)
            {
                Vector3 pos = Vector3.zero;
                bool valid = false;

                // Try to find a non-overlapping random spot
                for (int attempts = 0; attempts < 40; attempts++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float r = Random.Range(minRadius, maxRadius);
                    pos = new Vector3(Mathf.Cos(angle) * r, obstacle.position.y, Mathf.Sin(angle) * r);

                    // Check overlaps with previously placed obstacles
                    bool overlaps = false;
                    foreach (var placed in placedPositions)
                    {
                        if (Vector3.Distance(pos, placed) < 1.8f)
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
                    obstacle.position = pos;
                    // Snap rotation to 0 or 90 degrees for a clean office cubicle look!
                    obstacle.rotation = Quaternion.Euler(0f, Random.Range(0, 2) * 90f, 0f);
                    placedPositions.Add(pos);
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
                            if (Vector3.Distance(pos, obstaclePos) < 1.0f)
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
                    }
                }
            }

            // Position Boss (Suit/Seeker model) in the center (0, y, 0) so it becomes Units[0]
            Transform bossTransform = levelView.transform.Find("UnitF") ?? levelView.transform.Find("EnemyE") ?? levelView.transform.Find("EnemyF");
            if (bossTransform != null)
            {
                bossTransform.position = new Vector3(0f, bossTransform.position.y, 0f);
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
                    }
                }
            }
        }

        private void SetNavigationStatic(LevelView levelView)
        {
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
