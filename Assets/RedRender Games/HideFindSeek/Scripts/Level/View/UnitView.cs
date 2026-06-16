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