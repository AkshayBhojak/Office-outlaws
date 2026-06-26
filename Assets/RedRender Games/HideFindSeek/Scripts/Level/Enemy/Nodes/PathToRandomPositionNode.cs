using Game.Config;
using Injection;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Utils;

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
        private float _lastFleeRecalculateTime;
        private Vector3 _fleeDestination;
        private bool _isCurrentlyFleeing;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _meshAgent = _enemyController.View.MeshAgent;

            // Reset to normal speed when entering state
            _meshAgent.speed = _enemyController.Speed;
            _isCurrentlyFleeing = false;

            var juice = _enemyController.View.GetComponent<AIJuiceEffects>();
            if (juice != null)
            {
                juice.StopFleeing();
            }

            Vector3 targetPosition = Vector3.zero;

            // If it is an Employee (Victim)
            if (_enemyController.View.IsVictim)
            {
                Vector3 employeePos = _enemyController.View.transform.position;
                Vector3 bossPos = _levelView.Units[0].transform.position;
                int wallMask = LayerUtils.GetRadarAllLayerMask();

                // Check if they are currently hidden from the boss
                bool isCurrentlyHidden = Physics.Linecast(bossPos + Vector3.up * 0.5f, employeePos + Vector3.up * 0.5f, wallMask);

                if (isCurrentlyHidden)
                {
                    // 70% chance to just stay put in cover (don't move and expose yourself!)
                    if (Random.value < 0.7f)
                    {
                        targetPosition = employeePos;
                    }
                    else
                    {
                        // 30% chance to pace around nearby within cover
                        targetPosition = GetNearbyCoverPosition(employeePos, bossPos, wallMask);
                    }
                }
                else
                {
                    // Not currently hidden! Immediately seek cover!
                    int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
                    targetPosition = GetSmartFleePosition(difficulty);
                }
            }
            else
            {
                // Seeker (Boss) AI movement
                targetPosition = GetSeekerPosition();
            }

            _meshAgent.SetDestination(targetPosition);
            _meshAgent.isStopped = false;

            _enemyController.View.Walk();
        }

        public override void Dispose()
        {
            if (_meshAgent != null)
            {
                _meshAgent.isStopped = true;
                _meshAgent = null;
            }

            if (_enemyController != null && _enemyController.View != null)
            {
                _enemyController.View.Idle();
                var juice = _enemyController.View.GetComponent<AIJuiceEffects>();
                if (juice != null)
                {
                    juice.StopFleeing();
                }
            }
        }

        public override void Process()
        {
            var distanceToEnemy = Vector2.Distance(_enemyController.Position, _levelView.Units[0].Position);
            
            float minDistance = _config.GetValue(GameParam.MinDistanceToEnemy);
            int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

            float reactionMultiplier = 1f;
            if (difficulty == 0) reactionMultiplier = 0.7f;  // Easy: notices later
            else if (difficulty == 2) reactionMultiplier = 1.4f; // Hard: notices earlier

            bool bossSeesMe = _levelView.Units[0].IsTargetVisible(_enemyController.View);
            bool isTooClose = distanceToEnemy < minDistance * reactionMultiplier;
            bool isBossApproaching = distanceToEnemy < _previousDistanceToEnemy;

            _previousDistanceToEnemy = distanceToEnemy;

            if (_enemyController.View.IsVictim && (bossSeesMe || (isTooClose && isBossApproaching)))
            {
                // Speed boost during flee: 1.3x for Easy, 1.8x for Medium, 2.3x for Hard mode!
                float speedMultiplier = 1.8f;
                if (difficulty == 0) speedMultiplier = 1.3f;
                else if (difficulty == 2) speedMultiplier = 2.3f;

                _meshAgent.speed = _enemyController.Speed * speedMultiplier;

                // Trigger flee visual effects
                var juice = _enemyController.View.GetComponent<AIJuiceEffects>();
                if (juice != null)
                {
                    juice.StartFleeing();
                }

                // Recalculate cover periodically (every 0.5s) to adapt to Boss's movement
                if (!_isCurrentlyFleeing || Time.time - _lastFleeRecalculateTime > 0.5f)
                {
                    _lastFleeRecalculateTime = Time.time;
                    _isCurrentlyFleeing = true;
                    _fleeDestination = GetSmartFleePosition(difficulty);
                    _meshAgent.SetDestination(_fleeDestination);
                }
            }
            else
            {
                // If they are fleeing but the boss is no longer near/seeing them, reset to normal speed
                if (_isCurrentlyFleeing)
                {
                    _isCurrentlyFleeing = false;
                    _meshAgent.speed = _enemyController.Speed;
                    
                    var juice = _enemyController.View.GetComponent<AIJuiceEffects>();
                    if (juice != null)
                    {
                        juice.StopFleeing();
                    }
                }
            }

            if (_meshAgent.remainingDistance <= 0 && !_meshAgent.pathPending)
            {
                NextNode();
            }
        }

        private Vector3 GetNearbyCoverPosition(Vector3 employeePos, Vector3 bossPos, int wallMask)
        {
            // Find a random position close to employee that is also behind cover
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 4.0f;
                Vector3 candidate = employeePos + new Vector3(randomOffset.x, 0f, randomOffset.y);
                
                candidate.x = Mathf.Clamp(candidate.x, _model.LevelBounds.x, _model.LevelBounds.z);
                candidate.z = Mathf.Clamp(candidate.z, _model.LevelBounds.y, _model.LevelBounds.w);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(candidate, out hit, 2.0f, NavMesh.AllAreas))
                {
                    Vector3 testPos = hit.position;
                    bool isBehindCover = Physics.Linecast(bossPos + Vector3.up * 0.5f, testPos + Vector3.up * 0.5f, wallMask);
                    if (isBehindCover)
                    {
                        return testPos;
                    }
                }
            }
            return employeePos; // Fallback to current position
        }

        private Vector3 GetSeekerPosition()
        {
            var position = new Vector3(_enemyController.LastHearVictim.x, 0f, _enemyController.LastHearVictim.y);

            if (position == Vector3.zero)
            {
                position.x = MathUtil.RandomSystem(_model.LevelBounds.x, _model.LevelBounds.z);
                position.z = MathUtil.RandomSystem(_model.LevelBounds.y, _model.LevelBounds.w);
            }

            _enemyController.LastHearVictim = Vector2.zero;
            return position;
        }

        private Vector3 GetSmartFleePosition(int difficulty)
        {
            Vector3 employeePos = _enemyController.View.transform.position;
            Vector3 bossPos = _levelView.Units[0].transform.position;
            
            float fleeDistance = (difficulty == 2) ? 16f : 11f;
            Vector3 bossToEmployee = (employeePos - bossPos).normalized;
            if (bossToEmployee == Vector3.zero)
            {
                bossToEmployee = Random.insideUnitSphere;
                bossToEmployee.y = 0f;
                bossToEmployee.Normalize();
            }

            int wallMask = LayerUtils.GetRadarAllLayerMask();
            Vector3 bestCoverPos = Vector3.zero;
            float bestWeight = -1f;

            // --- 1. Find nearby physical obstacles (desks, cabinets, walls) to hide behind ---
            Collider[] obstacles = Physics.OverlapSphere(employeePos, 8.0f, wallMask);
            foreach (var obs in obstacles)
            {
                if (obs == null) continue;

                // Exclude non-solid obstacles (floor, coins, players, etc.)
                string obsName = obs.name.ToLower();
                if (obsName.Contains("floor") || obsName.Contains("ground") || obsName.Contains("door") || 
                    obsName.Contains("coin") || obsName.Contains("goldbar") || obsName.Contains("radar") ||
                    obsName.Contains("light") || obsName.Contains("camera") || obsName.Contains("joystick"))
                    continue;

                // Find closest point on the obstacle to the employee
                Vector3 closestPoint;
                if (obs is MeshCollider meshCol && !meshCol.convex)
                {
                    closestPoint = obs.bounds.ClosestPoint(employeePos);
                }
                else
                {
                    closestPoint = obs.ClosestPoint(employeePos);
                }

                // Direction from boss to closest point
                Vector3 bossToObsPoint = (closestPoint - bossPos).normalized;
                if (bossToObsPoint == Vector3.zero)
                {
                    bossToObsPoint = bossToEmployee;
                }

                Vector3 rightDir = Vector3.Cross(bossToObsPoint, Vector3.up).normalized;
                Vector3[] offsets = {
                    Vector3.zero,
                    rightDir * 1.0f,
                    -rightDir * 1.0f
                };

                foreach (var offset in offsets)
                {
                    Vector3 candidate = closestPoint + bossToObsPoint * 1.5f + offset;
                    candidate.y = employeePos.y; // keep same height
                    
                    // Clamp candidate to level bounds
                    candidate.x = Mathf.Clamp(candidate.x, _model.LevelBounds.x, _model.LevelBounds.z);
                    candidate.z = Mathf.Clamp(candidate.z, _model.LevelBounds.y, _model.LevelBounds.w);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(candidate, out hit, 3.0f, NavMesh.AllAreas))
                    {
                        Vector3 testPos = hit.position;
                        float distToBoss = Vector3.Distance(testPos, bossPos);
                        
                        // Check if this position is actually hidden from the boss
                        bool isBehindCover = Physics.Linecast(bossPos + Vector3.up * 0.5f, testPos + Vector3.up * 0.5f, wallMask);
                        
                        if (isBehindCover)
                        {
                            // Score: closer to employee (faster hiding) + further from boss
                            float distToEmployee = Vector3.Distance(testPos, employeePos);
                            float score = 100f - distToEmployee + distToBoss * 0.5f;

                            if (score > bestWeight)
                            {
                                bestWeight = score;
                                bestCoverPos = testPos;
                            }
                        }
                    }
                }
            }

            // --- 2. Directional fallback (if no physical obstacle cover was found) ---
            if (bestWeight < 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = -60f + (i * 120f / 7f);
                    Vector3 dir = Quaternion.Euler(0f, angle, 0f) * bossToEmployee;
                    
                    for (float dist = fleeDistance * 0.6f; dist <= fleeDistance; dist += fleeDistance * 0.4f)
                    {
                        Vector3 candidate = employeePos + dir * dist;
                        candidate.x = Mathf.Clamp(candidate.x, _model.LevelBounds.x, _model.LevelBounds.z);
                        candidate.z = Mathf.Clamp(candidate.z, _model.LevelBounds.y, _model.LevelBounds.w);

                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(candidate, out hit, 4.0f, NavMesh.AllAreas))
                        {
                            Vector3 testPos = hit.position;
                            float distToBoss = Vector3.Distance(testPos, bossPos);
                            
                            bool isBehindCover = Physics.Linecast(bossPos + Vector3.up * 0.5f, testPos + Vector3.up * 0.5f, wallMask);
                            
                            float score = distToBoss;
                            if (isBehindCover) score += 20f;

                            if (score > bestWeight)
                            {
                                bestWeight = score;
                                bestCoverPos = testPos;
                            }
                        }
                    }
                }
            }

            if (bestWeight > 0)
            {
                return bestCoverPos;
            }

            // Fallback: run straight away clamped to level bounds
            Vector3 target = employeePos + bossToEmployee * fleeDistance;
            target.x = Mathf.Clamp(target.x, _model.LevelBounds.x, _model.LevelBounds.z);
            target.z = Mathf.Clamp(target.z, _model.LevelBounds.y, _model.LevelBounds.w);
            
            NavMeshHit fallbackHit;
            if (NavMesh.SamplePosition(target, out fallbackHit, 6.0f, NavMesh.AllAreas))
            {
                return fallbackHit.position;
            }
            return target;
        }
    }
}