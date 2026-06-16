using System;
using System.Collections.Generic;
using System.IO;
using Game.Config;
using Game.States;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Game
{
    [CustomEditor(typeof(GameStartBehaviour), false)]
    public sealed class GameStartBehaviourEditor : Editor
    {
        private Vector2 _scrollPosition = Vector2.zero;
        private Dictionary<GameParam, string> _texts = new Dictionary<GameParam, string>();
        private GameConfig _config;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var config = GetConfig();

            var styleTextField = new GUIStyle(GUI.skin.textField);
            var styleLabel = new GUIStyle(GUI.skin.label);

            if (GUILayout.Button("Save"))
            {
                string data = JsonConvert.SerializeObject(config);
                var asset = Resources.Load<TextAsset>(GameConfig.Name);

                var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, AssetDatabase.GetAssetPath(asset));

                if (null == asset)
                {
                    path = Path.Combine(Application.dataPath, "Resources");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path = Path.Combine(path, GameConfig.Name + ".txt");
                }

                File.WriteAllText(path, data);
                AssetDatabase.Refresh();

                Debug.Log("Success saved " + DateTime.Now.ToLongTimeString());
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));

            var keys = EnumUtils.GetValues<GameParam>();
            foreach (var key in keys)
            {
                var value = config.ParamsMap.ContainsKey(key) ? config.ParamsMap[key] : string.Empty;

                if (!_texts.ContainsKey(key))
                {
                    _texts[key] = value.ToString();
                }

                bool isParsed = false;

                if (value is Vector3)
                {
                    if (config.TryParseVector3(_texts[key], out Vector3 vector))
                    {
                        config.ParamsMap[key] = vector;
                        isParsed = true;
                    }
                }

                if (float.TryParse(_texts[key], out float floatResult))
                {
                    config.ParamsMap[key] = floatResult;
                    isParsed = true;
                }

                if (!isParsed && config.ParamsMap.ContainsKey(key) && string.IsNullOrEmpty(_texts[key]))
                {
                    _texts[key] = config.ParamsMap[key].ToString();
                }

                var color = (isParsed) ? Color.white : Color.red;

                styleLabel.normal.textColor = color;
                styleTextField.normal.textColor = color;

                GUILayout.Label(key.ToString(), styleLabel);
                _texts[key] = GUILayout.TextField(_texts[key], styleTextField);
            }

            GUILayout.EndScrollView();
        }

        private GameConfig GetConfig()
        {
            if (!Application.isPlaying)
            {
                if (null == _config)
                {
                    _config = GameInitializeState.LoadConfig();
                }
                return _config;
            }

            var behaviour = GameObject.FindObjectOfType<GameStartBehaviour>();

            if (null == behaviour)
                return null;

            var context = behaviour.Context;

            if (null == context)
                return null;

            return context.Get<GameConfig>();
        }
    }
}