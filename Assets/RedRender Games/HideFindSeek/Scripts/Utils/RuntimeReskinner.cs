using UnityEngine;
using System.Collections.Generic;

namespace Game.Utilities
{
    public struct OfficeTheme
    {
        public Color FloorColor;
        public Color WallColor;
        public Color WoodColor; // Cabinets / Buildings
        public Color SteelColor; // Door / Cage / Default
        public Color CoinColor; // Gold / Coins
        public Color SpotlightColor; // Spotlights in Night Mode
    }

    public static class RuntimeReskinner
    {
        public static OfficeTheme GetThemeForLevel(int levelIndex)
        {
            switch ((levelIndex - 1) % 6)
            {
                case 0: // Corporate Blue
                    return new OfficeTheme
                    {
                        FloorColor = new Color(0.75f, 0.78f, 0.82f, 1f),
                        WallColor = new Color(0.9f, 0.88f, 0.85f, 1f),
                        WoodColor = new Color(0.55f, 0.45f, 0.35f, 1f),
                        SteelColor = new Color(0.4f, 0.45f, 0.5f, 1f),
                        CoinColor = new Color(0.2f, 0.75f, 0.3f, 1f),
                        SpotlightColor = new Color(0.8f, 0.9f, 1.0f, 1f)
                    };
                case 1: // Creative Teal
                    return new OfficeTheme
                    {
                        FloorColor = new Color(0.65f, 0.82f, 0.8f, 1f),
                        WallColor = new Color(0.93f, 0.93f, 0.94f, 1f),
                        WoodColor = new Color(0.75f, 0.65f, 0.55f, 1f),
                        SteelColor = new Color(0.25f, 0.27f, 0.3f, 1f),
                        CoinColor = new Color(0.2f, 0.75f, 0.3f, 1f),
                        SpotlightColor = new Color(0.7f, 1.0f, 0.95f, 1f)
                    };
                case 2: // Modern Executive
                    return new OfficeTheme
                    {
                        FloorColor = new Color(0.45f, 0.47f, 0.5f, 1f),
                        WallColor = new Color(0.95f, 0.93f, 0.88f, 1f),
                        WoodColor = new Color(0.3f, 0.22f, 0.15f, 1f),
                        SteelColor = new Color(0.7f, 0.72f, 0.75f, 1f),
                        CoinColor = new Color(0.2f, 0.75f, 0.3f, 1f),
                        SpotlightColor = new Color(1.0f, 1.0f, 1.0f, 1f)
                    };
                case 3: // Tech Startup
                    return new OfficeTheme
                    {
                        FloorColor = new Color(0.6f, 0.62f, 0.65f, 1f),
                        WallColor = new Color(0.85f, 0.92f, 0.7f, 1f),
                        WoodColor = new Color(0.95f, 0.95f, 0.95f, 1f),
                        SteelColor = new Color(0.15f, 0.15f, 0.15f, 1f),
                        CoinColor = new Color(0.2f, 0.75f, 0.3f, 1f),
                        SpotlightColor = new Color(0.9f, 1.0f, 0.85f, 1f)
                    };
                case 4: // Warm Autumn
                    return new OfficeTheme
                    {
                        FloorColor = new Color(0.65f, 0.58f, 0.52f, 1f),
                        WallColor = new Color(0.92f, 0.88f, 0.82f, 1f),
                        WoodColor = new Color(0.6f, 0.35f, 0.25f, 1f),
                        SteelColor = new Color(0.5f, 0.42f, 0.35f, 1f),
                        CoinColor = new Color(0.2f, 0.75f, 0.3f, 1f),
                        SpotlightColor = new Color(1.0f, 0.8f, 0.6f, 1f)
                    };
                case 5: // Minimalist Mint
                default:
                    return new OfficeTheme
                    {
                        FloorColor = new Color(0.7f, 0.85f, 0.78f, 1f),
                        WallColor = new Color(0.97f, 0.97f, 0.97f, 1f),
                        WoodColor = new Color(0.82f, 0.76f, 0.7f, 1f),
                        SteelColor = new Color(0.2f, 0.2f, 0.2f, 1f),
                        CoinColor = new Color(0.2f, 0.75f, 0.3f, 1f),
                        SpotlightColor = new Color(0.8f, 1.0f, 0.9f, 1f)
                    };
            }
        }

        public static void Reskin(LevelView levelView, GameView gameView)
        {
            if (levelView == null || gameView == null) return;

            // 1. Get level index to determine the theme and Day/Night mode
            int levelIndex = 1;
            string sceneName = levelView.gameObject.scene.name;
            if (!string.IsNullOrEmpty(sceneName))
            {
                string numStr = "";
                foreach (char c in sceneName)
                {
                    if (char.IsDigit(c)) numStr += c;
                }
                if (!string.IsNullOrEmpty(numStr) && int.TryParse(numStr, out int parsed))
                {
                    levelIndex = parsed;
                }
            }

            bool isNight = (levelIndex % 2 == 0); // Even levels = Night Mode, Odd levels = Day Mode
            OfficeTheme theme = GetThemeForLevel(levelIndex);

            Debug.Log($"[RuntimeReskinner] Reskinning Level {levelIndex} ({(isNight ? "NIGHT" : "DAY")} mode)...");

            // Define dimming multiplier for Night Mode environment colors
            Color nightMult = isNight ? new Color(0.4f, 0.45f, 0.65f, 1f) : Color.white;

            // 2. Reskin Environment to look like an Office
            var allRenderers = levelView.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in allRenderers)
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

                if (isBackground)
                {
                    // Exterior city buildings
                    foreach (var mat in renderer.materials)
                    {
                        if (isNight)
                        {
                            mat.color = new Color(0.12f, 0.15f, 0.28f, 1f); // Dark blue silhouette
                        }
                        else
                        {
                            mat.color = new Color(0.5f, 0.52f, 0.58f, 1f); // Steel grey city buildings
                        }
                    }
                    continue;
                }

                // Office Floor (light grey carpet / office tiles)
                if (name.Contains("floor") || name.Contains("ground"))
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.color = theme.FloorColor * nightMult;
                    }
                }
                // Office Cubicle Walls (light beige or clean white/grey)
                else if (name.Contains("wall"))
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.color = theme.WallColor * nightMult;
                    }
                }
                // Office Buildings/Large partitions (Office Cabinets/Desks)
                else if (name.Contains("building") || name.Contains("plinth"))
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.color = theme.WoodColor * nightMult;
                    }
                }
                // Gold / Coins (representing Office cash / bonuses)
                else if (name.Contains("coin") || name.Contains("gold"))
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (isNight)
                        {
                            // Glowing lime green cash coins
                            mat.color = new Color(0.3f, 0.9f, 0.4f, 1f);
                            mat.EnableKeyword("_EMISSION");
                            mat.SetColor("_EmissionColor", new Color(0.1f, 0.7f, 0.2f) * 3f); // Higher glow intensity at night!
                        }
                        else
                        {
                            mat.color = theme.CoinColor;
                            mat.EnableKeyword("_EMISSION");
                            mat.SetColor("_EmissionColor", new Color(theme.CoinColor.r * 0.5f, theme.CoinColor.g * 0.5f, theme.CoinColor.b * 0.5f) * 1.2f);
                        }
                    }
                }
                // Cage / Door / Default
                else if (name.Contains("cage") || name.Contains("door") || name.Contains("default"))
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.color = theme.SteelColor * nightMult;
                    }
                }
            }

            // 3. Adjust Lighting (Directional Lights)
            var lights = levelView.GetComponentsInChildren<Light>(true);
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
                }
                else if (light.type == LightType.Spot)
                {
                    // Apply theme-specific spotlight color and intensity
                    light.color = theme.SpotlightColor;
                    light.intensity = 5.0f;
                }
            }

            // 4. Adjust Ambient Light Settings
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

            // 5. Adjust Main Camera Background Color to fit Day/Night
            var mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = isNight ? new Color(0.04f, 0.05f, 0.1f, 1f) : new Color(0.7f, 0.75f, 0.8f, 1f);
            }

            Debug.Log("[RuntimeReskinner] Office Outlaws reskin applied successfully!");
        }
    }
}
