using UnityEngine;
using UnityEngine.Events;

namespace ArrowOperator.Jeff
{
    [RequireComponent(typeof(Collider))]
    public sealed class ArrowHazard : MonoBehaviour
    {
        public GameObject gameOverPanel;
        public UnityEvent onCrash = new UnityEvent();
        bool ended;

        void OnEnable() => ended = false;
        void OnCollisionEnter(Collision hit) => Contact(hit);
        void OnCollisionStay(Collision hit) => Contact(hit);

        void Contact(Collision hit)
        {
            // Unity reports the normal toward this hazard; the arrow needs the opposite normal.
            Vector3 normal = hit.contactCount > 0 ? -hit.GetContact(0).normal : Vector3.zero;
            Handle(hit.collider, normal);
        }

        void OnTriggerEnter(Collider other) => Handle(other, Vector3.zero);
        void OnTriggerStay(Collider other) => Handle(other, Vector3.zero);

        void Handle(Collider other, Vector3 normal)
        {
            if (ended || Time.timeScale <= 0f) return;
            ArrowController arrow = other.GetComponentInParent<ArrowController>();
            if (arrow == null || !arrow.isActiveAndEnabled) return;
            ArrowPowerups powerups = arrow.GetComponent<ArrowPowerups>();
            if (powerups != null && powerups.TryProtect(normal)) return;
            ended = true;
            arrow.enabled = false;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            onCrash.Invoke();
        }
    }
}
