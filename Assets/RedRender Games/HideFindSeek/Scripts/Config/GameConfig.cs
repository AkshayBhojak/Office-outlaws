using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Game.Config
{
    public enum GameParam
    {
        Speed,
        AngleLerpFactor,
        UnitRotateSpeed,
        EnemySpeed,
        GameStartDelayDuration,
        CoinRadius,
        CameraGameZoom,
        CameraMenuZoom,
        CameraSeekZoom,
        CoinsByRescue,
        AudibilityRadius,
        MinDistanceToEnemy,
        RescueReloadDuration
    }

    [Serializable]
    public sealed class GameConfig
    {
        public const string Name = "config";

        public readonly Dictionary<GameParam, object> ParamsMap;

        public GameConfig()
        {
            ParamsMap = new Dictionary<GameParam, object>();
            ParamsMap[GameParam.Speed] = 1;
            ParamsMap[GameParam.AngleLerpFactor] = 10;
            ParamsMap[GameParam.UnitRotateSpeed] = 300;
            ParamsMap[GameParam.EnemySpeed] = 5f;
            ParamsMap[GameParam.CoinRadius] = 1f;
            ParamsMap[GameParam.CoinsByRescue] = 10f;
            ParamsMap[GameParam.AudibilityRadius] = 10f;
            ParamsMap[GameParam.MinDistanceToEnemy] = 5;
            ParamsMap[GameParam.RescueReloadDuration] = 3;

            ParamsMap[GameParam.GameStartDelayDuration] = 3;
            ParamsMap[GameParam.CameraGameZoom] = 60;
            ParamsMap[GameParam.CameraMenuZoom] = 88;
            ParamsMap[GameParam.CameraSeekZoom] = 15;
        }

        public float GetValue(GameParam param)
        {
            if (ParamsMap.TryGetValue(param, out object value))
            {
                return Convert.ToSingle(value);
            }

            throw new KeyNotFoundException(param.ToString());
        }

        public Vector3 GetV3Value(GameParam param)
        {
            if (ParamsMap.TryGetValue(param, out object value))
            {
                if (!(value is Vector3))
                {
                    if (TryParseVector3(value.ToString(), out Vector3 vector))
                    {
                        value = vector;
                        ParamsMap[param] = value;
                    }
                }

                return (Vector3)value;
            }

            throw new KeyNotFoundException(param.ToString());
        }

        public bool TryParseVector3(string value, out Vector3 vector)
        {
            if (value.Contains("("))
            {
                value = value.Replace("(", ":");
                value = value.Replace(",", ",:");
                value = value.Replace(")", string.Empty);
                value = value.Replace(" ", string.Empty);
            }
            value = value.Replace("\r", string.Empty);
            value = value.Replace("\n", string.Empty);
            value = value.Replace("}", string.Empty);
            value = value.Replace("\"", string.Empty);

            var arg = StringUtil.Split(value, ",");
            float x = 0;
            float y = 0;
            float z = 0;

            bool result = float.TryParse(StringUtil.Split(arg[0], ":")[1], out x);
            result = result && float.TryParse(StringUtil.Split(arg[1], ":")[1], out y);
            result = result && float.TryParse(StringUtil.Split(arg[2], ":")[1], out z);

            vector = new Vector3(x, y, z);
            return result;
        }
    }
}