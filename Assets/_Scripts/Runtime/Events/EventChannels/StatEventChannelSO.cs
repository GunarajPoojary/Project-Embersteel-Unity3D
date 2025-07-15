using ProjectEmbersteel.StatSystem;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectEmbersteel.Events.EventChannel
{
    [CreateAssetMenu(menuName = "Custom/Events/Stat Update Event Channel")]
    public class StatEventChannelSO : DescriptionBaseSO
    {
        public event UnityAction<StatType, Stat> OnEventRaised;

        public void RaiseEvent(StatType type, Stat stat) => OnEventRaised?.Invoke(type, stat);
    }
}