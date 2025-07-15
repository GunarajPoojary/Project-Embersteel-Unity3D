using ProjectEmbersteel.Events.EventChannel;
using UnityEngine;

namespace ProjectEmbersteel.InteractionSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private LayerMask _interactableLayer;

        [Header("Publisher")]
        [SerializeField] private IInteractableEventChannelSO _triggerInteractableEvent;

        private BoxCollider _interactableCollider;

        private void OnValidate()
        {

        }

        private void OnTriggerEnter(Collider collider) => HandleTrigger(collider, true);
        private void OnTriggerExit(Collider collider) => HandleTrigger(collider, false);

        private void HandleTrigger(Collider collider, bool isEntering)
        {
            if (((1 << collider.gameObject.layer) & _interactableLayer) == 0)
                return;

            if (!collider.TryGetComponent(out IInteractable interactable))
                return;

            _triggerInteractableEvent.RaiseEvent(interactable.Interactable && isEntering, interactable);
        }
    }
}