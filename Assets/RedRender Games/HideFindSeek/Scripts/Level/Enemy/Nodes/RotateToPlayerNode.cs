using Injection;
using UnityEngine;
using Utilities;

namespace Game.Enemy
{
    public sealed class RotateToPlayerNode : TimeNode
    {
        [Inject] private GameManager _gameManager;
        [Inject] private EnemyController _enemyController;

        public override void Process()
        {
            base.Process();
            float angle = 270 - MathUtil.GetAngle(_enemyController.Position, _enemyController.LastHearVictim);
            float angleSpeed = Time.deltaTime * _enemyController.RotationSpeed;

            angle = Mathf.Clamp(Mathf.DeltaAngle(_enemyController.View.Rotation, angle),
                -angleSpeed, angleSpeed);

            _enemyController.View.Rotation += angle;
        }
    }
}