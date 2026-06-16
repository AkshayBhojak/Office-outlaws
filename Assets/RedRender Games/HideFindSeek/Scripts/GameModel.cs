using System;
using Game.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace Game
{
    public sealed class GameModel   // ✅ SerializeField removed
    {
        public static GameModel Load(GameConfig config)
        {
            try
            {
                var data = PlayerPrefs.GetString("model");
                if (string.IsNullOrEmpty(data))
                    return new GameModel(config);

                return JsonConvert.DeserializeObject<GameModel>(data);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return new GameModel(config);
            }
        }

        [NonSerialized]
        public Action Changed;

        public int Level;

        [NonSerialized]
        public bool IsTimeOut;

        [NonSerialized]
        public Vector4 LevelBounds;

        public GameModel()
        {
            IsTimeOut = false;
        }

        public GameModel(GameConfig config) : this()
        {
            Level = 1;
            IsTimeOut = false;
        }

        public void SetChanged()
        {
            Changed?.Invoke();
        }

        public void Save()
        {
            var data = JsonConvert.SerializeObject(this);
            PlayerPrefs.SetString("model", data);
            PlayerPrefs.Save();
        }

        public void Remove()
        {
            PlayerPrefs.DeleteKey("model");
            PlayerPrefs.Save();
        }
    }
}
