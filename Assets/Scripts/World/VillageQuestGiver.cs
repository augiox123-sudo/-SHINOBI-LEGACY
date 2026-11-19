using UnityEngine;
using ShinobiLegacy.Player;

namespace ShinobiLegacy.World
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class VillageQuestGiver : MonoBehaviour
    {
        private void Awake()
        {
            SphereCollider area = GetComponent<SphereCollider>();
            area.isTrigger = true;
            area.radius = 5.5f;
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerProgression progression = other.GetComponent<PlayerProgression>();
            if (progression == null) progression = other.GetComponentInParent<PlayerProgression>();
            if (progression == null) return;
            VillageQuestSystem quests = progression.GetComponent<VillageQuestSystem>();
            if (quests != null)
            {
                quests.SetNearby(true);
                quests.SetBoardVisible(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerProgression progression = other.GetComponent<PlayerProgression>();
            if (progression == null) progression = other.GetComponentInParent<PlayerProgression>();
            if (progression == null) return;
            VillageQuestSystem quests = progression.GetComponent<VillageQuestSystem>();
            if (quests != null) quests.SetNearby(false);
        }
    }
}
