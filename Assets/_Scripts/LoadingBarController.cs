using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.UI;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace ProjectEmbersteel
{
    public class LoadingBarController : MonoBehaviour
    {
        [SerializeField] private UILoadingBar _loadingInterface = default;

        //[SerializeField] private BoolEventChannelSO _toggleLoadingScreenEvent = default;
        [Header("Listener")]
        [SerializeField] private SceneLoadProgressEventChannelSO _sceneLoadingProgressEvent = default;

        private void OnEnable() => SubscribeToLoadingScreenEvent(true);
        private void OnDisable() => SubscribeToLoadingScreenEvent(false);

        private void SubscribeToLoadingScreenEvent(bool subscribe)
        {
            if (subscribe)
                _sceneLoadingProgressEvent.OnEventRaised += HandleSceneLoadingProgress;
            else
                _sceneLoadingProgressEvent.OnEventRaised -= HandleSceneLoadingProgress;
        }


        private void HandleSceneLoadingProgress(bool state, AsyncOperationHandle<SceneInstance> opHandle)
        {
            _loadingInterface.gameObject.SetActive(state);

            if (!state) return;

            _loadingInterface.ResetBar();
            _loadingInterface.UpdateBar(opHandle.PercentComplete);
        }

    }
}