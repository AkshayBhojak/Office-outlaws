using System;
using Game.Enemy;
using Game.Utilities;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Game
{
    public sealed class UnitView : AnimationUnitView
    {
        public Action<Node> NodeEntered;

        [SerializeField]
        private RadarView _radar;
        [SerializeField]
        private CircleRadarView _circleRadar;
        [SerializeField]
        private Animator _enemyBehaviour;
        [SerializeField]
        private SkinnedMeshRenderer _meshRenderer;
        [SerializeField]
        private CapsuleCollider _capsuleCollider;
        [SerializeField]
        public NavMeshAgent MeshAgent;

        [HideInInspector]
        public Vector3 KnockbackVelocity;

        [HideInInspector]
        public float SpeedMultiplier = 1.0f;

        [HideInInspector]
        public float SpeedBoostExpiryTime = 0f;

        public void SetSpeedMultiplier(float newMultiplier)
        {
            if (Mathf.Approximately(SpeedMultiplier, newMultiplier)) return;

            float oldMultiplier = SpeedMultiplier;
            SpeedMultiplier = newMultiplier;

            if (MeshAgent != null && MeshAgent.enabled)
            {
                MeshAgent.speed = (MeshAgent.speed / oldMultiplier) * newMultiplier;
            }
        }

        private bool _isVictim;
        private bool _isVissible;

        public bool IsVictim
        {
            get
            {
                return _isVictim;
            }
            set
            {
                _isVictim = value;

                gameObject.layer = LayerUtils.GetVictimLayer(value);

                _radar.gameObject.SetActive(!value);
                _circleRadar.gameObject.SetActive(!value);
            }
        }

        public bool IsVissible
        {
            get
            {
                return _isVissible;
            }
            set
            {
                _isVissible = value;
                _meshRenderer.enabled = value;
            }
        }

        public bool IsAI
        {
            set
            {
                _enemyBehaviour.enabled = value;
            }
        }

        public float Radius
        {
            get
            {
                return _capsuleCollider.radius;
            }
        }

        private Material[] _originalMaterials;

        private void Awake()
        {
            _enemyBehaviour.enabled = false;
            if (_meshRenderer != null)
            {
                _originalMaterials = _meshRenderer.sharedMaterials;
            }
        }

        public override void Die()
        {
            base.Die();
            _meshRenderer.enabled = true;
            _enemyBehaviour.enabled = false;
            IsVictim = false;

            _radar.gameObject.SetActive(false);
            _circleRadar.gameObject.SetActive(false);
        }

        public void FireNodeEnter(Node node)
        {
            NodeEntered.SafeInvoke(node);
        }

        public void RestartNodes()
        {
            _enemyBehaviour.Rebind();
            _enemyBehaviour.speed = 0;
        }

        public Collider GetSeenVictim()
        {
            if (_isVictim)
                return null;

            var result = _radar.GetSeenVictim();
            return (null != result) ? result : _circleRadar.GetSeenVictim();
        }

        public bool IsTargetVisible(UnitView target)
        {
            if (target == null || target.IsDied()) return false;

            Vector3 bossPos = transform.position;
            Vector3 targetPos = target.transform.position;
            float distance = Vector3.Distance(bossPos, targetPos);

            // 1. Check Circle Radar (all-around vision with small radius)
            float circleRadius = 3f; // Default circle radar radius
            int wallMask = LayerUtils.GetRadarAllLayerMask();

            if (distance <= circleRadius)
            {
                // Linecast to check for wall blockage (raise by 0.5f to avoid floor intersection)
                if (!Physics.Linecast(bossPos + Vector3.up * 0.5f, targetPos + Vector3.up * 0.5f, wallMask))
                {
                    return true;
                }
            }

            // 2. Check Front Radar Cone
            if (_radar != null && _radar.gameObject.activeSelf)
            {
                Vector3 localPos = _radar.transform.InverseTransformPoint(targetPos);
                // Radar size (default width = 2.5f, length = 4.5f)
                float width = 2.5f;
                float length = 4.5f;

                // Check if inside the bounding box of the cone
                if (Mathf.Abs(localPos.x) <= width && localPos.z >= 0f && localPos.z <= length)
                {
                    // Linecast to check for wall blockage (raise by 0.5f to avoid floor intersection)
                    if (!Physics.Linecast(bossPos + Vector3.up * 0.5f, targetPos + Vector3.up * 0.5f, wallMask))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SetOutline(Material[] materials)
        {
            if (_meshRenderer == null) return;

            var currentMaterials = _meshRenderer.sharedMaterials;
            var cleanOriginals = new System.Collections.Generic.List<Material>();

            if (currentMaterials != null)
            {
                foreach (var mat in currentMaterials)
                {
                    if (mat == null) continue;
                    string matName = mat.name.ToLower();
                    if (!matName.Contains("outline") && !matName.Contains("blur"))
                    {
                        cleanOriginals.Add(mat);
                    }
                }
            }

            // Fallback: if we filtered out everything, restore at least one material
            if (cleanOriginals.Count == 0 && currentMaterials != null && currentMaterials.Length > 0)
            {
                cleanOriginals.Add(currentMaterials[0]);
            }

            var result = new System.Collections.Generic.List<Material>(cleanOriginals);

            if (null != materials)
            {
                foreach (var mat in materials)
                {
                    if (mat == null) continue;
                    string matName = mat.name.ToLower();
                    if (!matName.Contains("blur"))
                    {
                        result.Add(mat);
                    }
                }
            }
            _meshRenderer.materials = result.ToArray();
        }
    }
}