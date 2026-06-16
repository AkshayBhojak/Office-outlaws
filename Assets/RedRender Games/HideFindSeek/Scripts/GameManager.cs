using System;
using System.Collections.Generic;
using System.Linq;
using Game.Enemy;
using Game.Player;
using Injection;
using UnityEngine;

namespace Game
{
    public sealed class GameManager : IDisposable
    {
        public Action<UnitView> UNIT_KILLED;
        public Action<UnitView> UNIT_RESCUED;
        public Action<UnitView, int> COINS_COLLECTED;
        public Action<Vector3> UNIT_SOUND;

        [Inject]
        private Injector _injector;
        [Inject]
        private Context _context;

        private PlayerController _player;
        private readonly List<EnemyController> _enemies;

        public PlayerController Player => _player;
        public int EnemiesCount => _enemies.Count;

        public GameManager()
        {
            _enemies = new List<EnemyController>();
        }

        public void Dispose()
        {
            foreach (var enemy in _enemies)
            {
                enemy.Dispose();
            }
            _enemies.Clear();

            _player.Dispose();
            _player = null;
        }

        public void CreatePlayer(UnitView unit, bool isVictim)
        {
            _player = new PlayerController(_context, unit);
            _injector.Inject(_player);

            _player.Live(isVictim);
        }

        public void CreateEnemy(UnitView unit, bool isVictim)
        {
            EnemyController enemyController = new EnemyController(unit, _context);
            _injector.Inject(enemyController);
            _enemies.Add(enemyController);

            enemyController.Live(isVictim);
        }

        public void Kill(UnitView unitView)
        {
            UNIT_KILLED.SafeInvoke(unitView);
            if (Player.View == unitView)
            {
                Player.Kill();
                return;
            }

            var enemy = _enemies.FirstOrDefault(temp => temp.View == unitView);
            _enemies.Remove(enemy);
            enemy.Kill();
        }

        public void Rescue(UnitView unit)
        {
            if (Player.View == unit)
                return;

            UNIT_RESCUED.SafeInvoke(unit);

            bool isVissible = unit.IsVissible;

            CreateEnemy(unit, true);
            
            unit.IsVissible = isVissible;
        }

        public void CollectCoin(UnitView unit, int value)
        {
            COINS_COLLECTED.SafeInvoke(unit, value);
        }

        public void FireSound(Vector3 position)
        {
            UNIT_SOUND.SafeInvoke(position);
        }
    }
}