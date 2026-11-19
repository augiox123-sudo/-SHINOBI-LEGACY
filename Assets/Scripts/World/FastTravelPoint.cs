using UnityEngine;

namespace ShinobiLegacy.World
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class FastTravelPoint : MonoBehaviour
    {
        public string pointId = "village_gate";
        public string displayName = "Village Gate";
        [Tooltip("Optional region ID. A point remains hidden until its region is unlocked.")]
        public string regionId = "";
        [Min(1f)] public float unlockRadius = 7f;

        private string PreferenceKey => "SL_FAST_TRAVEL_UNLOCKED_" + pointId;

        public bool IsUnlocked
        {
            get
            {
                if (string.IsNullOrEmpty(regionId))
                    return PlayerPrefs.GetInt(PreferenceKey, 0) == 1;

                WorldRegionProgression regions = FindObjectOfType<WorldRegionProgression>();
                if (regions == null || !regions.IsUnlocked(regionId)) return false;

                // Regional destinations become available as soon as their region unlocks.
                return true;
            }
        }

        private void Awake()
        {
            SphereCollider trigger = GetComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = unlockRadius;
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            FastTravelMapUI map = other.GetComponent<FastTravelMapUI>();
            if (map == null) map = other.GetComponentInParent<FastTravelMapUI>();
            if (map != null) Unlock();
        }

        public void Unlock()
        {
            if (IsUnlocked) return;
            PlayerPrefs.SetInt(PreferenceKey, 1);
            PlayerPrefs.Save();
            Debug.Log("Fast travel unlocked: " + displayName);
        }
    }
}
