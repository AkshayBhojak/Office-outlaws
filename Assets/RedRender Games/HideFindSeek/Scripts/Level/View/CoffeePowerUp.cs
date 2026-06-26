using System.Collections;
using Game.Audio;
using UnityEngine;

namespace Game
{
    public sealed class CoffeePowerUp : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 90f;
        [SerializeField] private float _floatAmplitude = 0.2f;
        [SerializeField] private float _floatFrequency = 2f;
        [SerializeField] private float _boostDuration = 6.0f;
        [SerializeField] private float _respawnTime = 15.0f;
        [SerializeField] private float _pickupRadius = 2.0f;

        private GameObject _visuals;
        private Collider _collider;
        private bool _isCollected;
        private Vector3 _baseVisualScale;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            
            Transform visualsTransform = transform.Find("Visuals");
            if (visualsTransform != null)
            {
                _visuals = visualsTransform.gameObject;
                _baseVisualScale = _visuals.transform.localScale;
            }
        }

        private void Update()
        {
            if (_isCollected) return;

            if (_visuals != null)
            {
                // Rotate
                _visuals.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
                
                // Float up and down
                Vector3 pos = _visuals.transform.localPosition;
                pos.y = 0.5f + Mathf.Sin(Time.time * _floatFrequency) * _floatAmplitude;
                _visuals.transform.localPosition = pos;

                // Pulse scale for visual feedback
                float pulse = 1.0f + 0.08f * Mathf.Sin(Time.time * 3.0f);
                _visuals.transform.localScale = _baseVisualScale * pulse;
            }

            // Check proximity - use a generous radius around character height
            Collider[] colliders = Physics.OverlapSphere(
                transform.position + Vector3.up * 0.8f,
                _pickupRadius,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore   // Ignore triggers, only hit solid colliders
            );

            foreach (var col in colliders)
            {
                if (col == null || col.gameObject == gameObject) continue;

                UnitView unit = col.GetComponent<UnitView>();
                if (unit == null)
                {
                    unit = col.GetComponentInParent<UnitView>();
                }

                if (unit != null && !unit.IsDied())
                {
                    StartCoroutine(ApplyPowerUp(unit));
                    return;
                }
            }
        }

        private IEnumerator ApplyPowerUp(UnitView unit)
        {
            _isCollected = true;
            if (_collider != null) _collider.enabled = false;
            if (_visuals != null) _visuals.SetActive(false);

            Debug.Log("[CoffeePowerUp] Collected by: " + (unit != null ? unit.gameObject.name : "null"));

            // Play collection sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayPushSound();
            }

            // Spawn "WOW!" text popup
            GameObject wowPrefab = Resources.Load<GameObject>("Prefabs/CFXR3 _WOW_");
            if (wowPrefab != null)
            {
                GameObject wowInstance = Instantiate(wowPrefab, unit.transform.position + Vector3.up * 2.0f, Quaternion.identity);
                Destroy(wowInstance, 2.0f);
            }

            // Spawn wind trails effect if not already active
            GameObject trailPrefab = Resources.Load<GameObject>("Prefabs/CFXR4 Wind Trails");
            if (trailPrefab != null)
            {
                Transform existingTrail = unit.transform.Find("CFXR4 Wind Trails(Clone)");
                if (existingTrail == null)
                {
                    GameObject trailInstance = Instantiate(trailPrefab, unit.transform);
                    trailInstance.transform.localPosition = Vector3.zero;
                    trailInstance.transform.localRotation = Quaternion.identity;
                }
            }

            // Apply speed boost: 1.6x for hiders/employees, 1.4x for boss
            float multiplier = unit.IsVictim ? 1.6f : 1.4f;
            unit.SetSpeedMultiplier(multiplier);

            // Set or extend expiry time
            float expiryTime = Time.time + _boostDuration;
            if (expiryTime > unit.SpeedBoostExpiryTime)
            {
                unit.SpeedBoostExpiryTime = expiryTime;
            }

            Debug.Log("[CoffeePowerUp] Speed boost applied! Multiplier: " + multiplier + " Duration: " + _boostDuration + "s");

            // Wait until boost expires
            while (unit != null && !unit.IsDied() && Time.time < unit.SpeedBoostExpiryTime)
            {
                yield return null;
            }

            // Reset speed multiplier and remove wind trail
            if (unit != null && !unit.IsDied())
            {
                unit.SetSpeedMultiplier(1.0f);
                Transform trail = unit.transform.Find("CFXR4 Wind Trails(Clone)");
                if (trail != null)
                {
                    Destroy(trail.gameObject);
                }
                Debug.Log("[CoffeePowerUp] Speed boost expired for: " + unit.gameObject.name);
            }

            // Wait remaining respawn time
            float remainingRespawn = _respawnTime - _boostDuration;
            if (remainingRespawn > 0)
            {
                yield return new WaitForSeconds(remainingRespawn);
            }

            // Respawn
            _isCollected = false;
            if (_collider != null) _collider.enabled = true;
            if (_visuals != null)
            {
                _visuals.SetActive(true);
                _visuals.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                _visuals.transform.localScale = _baseVisualScale;
            }

            Debug.Log("[CoffeePowerUp] Respawned: " + gameObject.name);
        }
    }
}
