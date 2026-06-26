using UnityEngine;

namespace Game
{
    public class AIJuiceEffects : MonoBehaviour
    {
        private ParticleSystem _sweatParticles;
        private ParticleSystem _reliefParticles;
        private GameObject _windTrail;
        private UnitView _unitView;
        private bool _isFleeing = false;
        private float _lastDustTime;

        private void Start()
        {
            _unitView = GetComponent<UnitView>();
            CreateSweatParticles();
            CreateReliefParticles();
        }

        private void CreateSweatParticles()
        {
            GameObject go = new GameObject("SweatParticles");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.8f, 0f); // Above the character's head

            _sweatParticles = go.AddComponent<ParticleSystem>();
            _sweatParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _sweatParticles.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 0.6f;
            main.startSpeed = 1.5f;
            main.startSize = 0.15f;
            main.gravityModifier = 1.5f; // sweat drops fall down
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = _sweatParticles.emission;
            emission.rateOverTime = 12f;

            var shape = _sweatParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.radius = 0.2f;
            shape.angle = 25f;
            shape.rotation = new Vector3(90f, 0f, 0f); // Point downwards/outwards

            var colorModule = _sweatParticles.colorOverLifetime;
            colorModule.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.4f, 0.7f, 1.0f), 0.0f), new GradientColorKey(new Color(0.2f, 0.5f, 0.9f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorModule.color = gradient;

            // Apply a default billboard sprite material
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void CreateReliefParticles()
        {
            GameObject go = new GameObject("ReliefParticles");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 2.0f, 0f);

            _reliefParticles = go.AddComponent<ParticleSystem>();
            _reliefParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _reliefParticles.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 2.0f;
            main.startSize = 0.25f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var emission = _reliefParticles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 8) });

            var shape = _reliefParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var colorModule = _reliefParticles.colorOverLifetime;
            colorModule.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.4f, 0.9f, 0.4f), 0.0f), new GradientColorKey(new Color(0.1f, 0.7f, 0.2f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorModule.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        private void Update()
        {
            if (_unitView == null) return;

            // Handle Wind Trail based on movement
            if (_unitView.AnimationType == AnimationType.Walk)
            {
                if (_windTrail == null)
                {
                    var trailPrefab = Resources.Load<GameObject>("Prefabs/CFXR4 Wind Trails");
                    if (trailPrefab != null)
                    {
                        _windTrail = Instantiate(trailPrefab, transform.position, Quaternion.identity, transform);
                        _windTrail.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                    }
                }

                // Adjust scale: larger if fleeing
                float targetScale = _isFleeing ? 0.9f : 0.5f;
                if (_windTrail != null)
                {
                    _windTrail.transform.localScale = Vector3.one * targetScale;
                }

                // If fleeing, spawn running dust puffs!
                if (_isFleeing && Time.time - _lastDustTime > 0.18f)
                {
                    _lastDustTime = Time.time;
                    SpawnRunningDust();
                }
            }
            else
            {
                if (_windTrail != null)
                {
                    Destroy(_windTrail);
                    _windTrail = null;
                }
            }
        }

        private void SpawnRunningDust()
        {
            var poofPrefab = Resources.Load<GameObject>("Prefabs/CFXR Magic Poof");
            if (poofPrefab != null)
            {
                // Spawn slightly behind the feet
                Vector3 spawnPos = transform.position - transform.forward * 0.3f + Vector3.up * 0.05f;
                var dust = Instantiate(poofPrefab, spawnPos, Quaternion.identity);
                dust.transform.localScale = Vector3.one * 0.18f; // very small poof
                Destroy(dust, 0.8f);
            }
        }

        public void StartFleeing()
        {
            if (_isFleeing) return;
            _isFleeing = true;

            // 1. Play alert flash (CFXR Flash) above head
            var flashPrefab = Resources.Load<GameObject>("Prefabs/CFXR Flash");
            if (flashPrefab != null)
            {
                var flash = Instantiate(flashPrefab, transform.position + Vector3.up * 2.0f, Quaternion.identity, transform);
                flash.transform.localScale = Vector3.one * 1.5f;
                Destroy(flash, 1.0f);
            }

            // 2. Play sweat particles
            if (_sweatParticles != null)
            {
                _sweatParticles.Play();
            }
        }

        public void StopFleeing()
        {
            if (!_isFleeing) return;
            _isFleeing = false;

            // 1. Stop sweat particles
            if (_sweatParticles != null)
            {
                _sweatParticles.Stop();
            }

            // 2. Play relief green stars
            if (_reliefParticles != null)
            {
                _reliefParticles.Play();
            }

            // 3. Play cartoon poof to show they are hidden/safe
            var poofPrefab = Resources.Load<GameObject>("Prefabs/CFXR Magic Poof");
            if (poofPrefab != null)
            {
                var poof = Instantiate(poofPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity);
                poof.transform.localScale = Vector3.one * 0.7f;
                Destroy(poof, 1.5f);
            }
        }

        private void OnDestroy()
        {
            if (_windTrail != null)
            {
                Destroy(_windTrail);
            }
        }
    }
}
