using ProjectEmbersteel.StatSystem;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectEmbersteel.Events.EventChannel
{
    [CreateAssetMenu(fileName = "RuntimeStatUpdateEvent", menuName = "Custom/Events/Runtime Stat Update Event Channel")]
    public class RuntimeStatUpdateEventChannel : DescriptionBaseSO
    {
        public event UnityAction<StatType, float, float> OnEventRaised;

        public void RaiseEvent(StatType type, float currentValue, float maxValue) => OnEventRaised?.Invoke(type, currentValue, maxValue);
    }
}