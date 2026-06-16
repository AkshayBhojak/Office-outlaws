using System;
using Game.Config;
using Game.States;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public sealed class LevelView : MonoBehaviour
    {
        [NonSerialized]
        public UnitView[] Units;

        [NonSerialized]
        public CoinView[] Coins;

        [NonSerialized]
        public DoorView[] Doors;

        [SerializeField] 
        public float Duration;

        private void Awake()
        {
            Units = GameObject.FindObjectsOfType<UnitView>();
            Coins = GameObject.FindObjectsOfType<CoinView>();
            Doors = GameObject.FindObjectsOfType<DoorView>();

            float minDistance = float.MaxValue;
            int index = 0;

            for (int i = 0; i < Units.Length; i++)
            {
                var unit = Units[i];
                if (unit.Position.sqrMagnitude < minDistance)
                {
                    minDistance = unit.Position.sqrMagnitude;
                    index = i;
                }
            }

            var tempUnit = Units[0];
            Units[0] = Units[index];
            Units[index] = tempUnit;

            var buildIndex = SceneManager.GetActiveScene().buildIndex;
            if (buildIndex != 0 && SceneManager.sceneCount <= 1)
            {
                GameConfig config = GameInitializeState.LoadConfig();
                GameModel model = GameModel.Load(config);

                model.Level = buildIndex;
                model.Save();

                SceneManager.LoadScene(0);
            }
        }
    }
}