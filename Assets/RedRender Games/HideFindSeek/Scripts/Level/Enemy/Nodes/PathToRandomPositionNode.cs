using Game.Config;
using Injection;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Game.Enemy
{
    public sealed class PathToRandomPositionNode : Node
    {
        [Inject] private GameModel _model;
        [Inject] private EnemyController _enemyController;
        [Inject] private LevelView _levelView;
        [Inject] private GameConfig _config;

        private NavMeshAgent _meshAgent;
        private float _previousDistanceToEnemy;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _meshAgent = _enemyController.View.MeshAgent;

            var position = new Vector3(_enemyController.LastHearVictim.x, 0, _enemyController.LastHearVictim.y);

            if (position == Vector3.zero)
            {
                position.x = MathUtil.RandomSystem(_model.LevelBounds.x, _model.LevelBounds.z);
                position.z = MathUtil.RandomSystem(_model.LevelBounds.y, _model.LevelBounds.w);
            }

            _enemyController.LastHearVictim = Vector2.zero;

            _meshAgent.SetDestination(position);
            _meshAgent.isStopped = false;

            _enemyController.View.Walk();
        }

        public override void Dispose()
        {
            _meshAgent.isStopped = true;
            _meshAgent = null;
            _enemyController.View.Idle();
        }

        public override void Process()
        {
            var distanceToEnemy = Vector2.Distance(_enemyController.Position, _levelView.Units[0].Position);
            
            float minDistance = _config.GetValue(GameParam.MinDistanceToEnemy);
            int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

            float reactionMultiplier = 1f;
            if (difficulty == 0) reactionMultiplier = 0.7f;  // Easy: notices later
            else if (difficulty == 2) reactionMultiplier = 1.4f; // Hard: notices earlier

            if (_enemyController.View.IsVictim && 
                distanceToEnemy < minDistance * reactionMultiplier && distanceToEnemy < _previousDistanceToEnemy)
            {
                Vector3 targetPosition;
                if (difficulty == 0)
                {
                    // Easy mode: dumb random movement
                    targetPosition = GetRandomPositionInBounds();
                }
                else
                {
                    // Medium and Hard modes: smart flee
                    targetPosition = GetSmartFleePosition(difficulty);
                }
                _meshAgent.SetDestination(targetPosition);
            }

            _previousDistanceToEnemy = distanceToEnemy;

            if (_meshAgent.remainingDistance <= 0 && !_meshAgent.pathPending)
            {
                NextNode();
            }
        }

        private Vector3 GetRandomPositionInBounds()
        {
            Vector3 pos = Vector3.zero;
            pos.x = MathUtil.RandomSystem(_model.LevelBounds.x, _model.LevelBounds.z);
            pos.z = MathUtil.RandomSystem(_model.LevelBounds.y, _model.LevelBounds.w);
            return pos;
        }

        private Vector3 GetSmartFleePosition(int difficulty)
        {
            Vector3 employeePos = _enemyController.View.transform.position;
            Vector3 bossPos = _levelView.Units[0].transform.position;
            Vector3 bossToEmployee = (employeePos - bossPos).normalized;

            if (bossToEmployee == Vector3.zero)
            {
                bossToEmployee = Random.insideUnitSphere;
                bossToEmployee.y = 0;
                bossToEmployee.Normalize();
            }

            float fleeDistance = (difficulty == 2) ? 15f : 10f;

            if (difficulty == 1)
            {
                // Medium: run straight away clamped to level bounds
                Vector3 target = employeePos + bossToEmployee * fleeDistance;
                target.x = Mathf.Clamp(target.x, _model.LevelBounds.x, _model.LevelBounds.z);
                target.z = Mathf.Clamp(target.z, _model.LevelBounds.y, _model.LevelBounds.w);
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(target, out hit, 6.0f, NavMesh.AllAreas))
                {
                    return hit.position;
                }
                return target;
            }
            else
            {
                // Hard: try straight, left, and right directions, pick the one farthest from the boss
                Vector3[] directions = new Vector3[3];
                directions[0] = bossToEmployee; // Straight
                directions[1] = Quaternion.Euler(0, 35, 0) * bossToEmployee; // Left
                directions[2] = Quaternion.Euler(0, -35, 0) * bossToEmployee; // Right

                Vector3 bestTarget = employeePos + bossToEmployee * fleeDistance;
                float maxDistance = 0f;

                foreach (var dir in directions)
                {
                    Vector3 candidate = employeePos + dir * fleeDistance;
                    candidate.x = Mathf.Clamp(candidate.x, _model.LevelBounds.x, _model.LevelBounds.z);
                    candidate.z = Mathf.Clamp(candidate.z, _model.LevelBounds.y, _model.LevelBounds.w);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(candidate, out hit, 6.0f, NavMesh.AllAreas))
                    {
                        candidate = hit.position;
                    }

                    float dist = Vector3.Distance(candidate, bossPos);
                    if (dist > maxDistance)
                    {
                        maxDistance = dist;
                        bestTarget = candidate;
                    }
                }
                return bestTarget;
            }
        }
    }
}