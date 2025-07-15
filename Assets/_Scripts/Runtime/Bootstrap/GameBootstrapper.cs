using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ProjectEmbersteel.Bootstrap
{
	/// <summary>
	/// This class is responsible for starting the game by loading the persistent managers scene 
	/// and raising the event to load the Main Menu
	/// </summary>
	public class GameBootstrapper : MonoBehaviour
	{
		[SerializeField] private GameSceneSO _persistentManagersScene = default;
		[SerializeField] private GameSceneSO _mainMenuScene = default;

		[Header("Publisher")]
		[SerializeField] private AssetReference _mainMenuSceneLoadEvent = default;
		private const int BOOTSTRAPPER_SCENE_INDEX = 0;

        private void Start() => LoadPersistentManagers();

        private void LoadPersistentManagers() => _persistentManagersScene.SceneReference.LoadSceneAsync(
            LoadSceneMode.Additive,
            true).Completed += LoadEventChannel;

        // Now the persistent Managers scene is active, let's load the event channel from addressable
        private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj) => _mainMenuSceneLoadEvent.LoadAssetAsync<LoadSceneEventChannelSO>().Completed += LoadMainMenu;

        // Once the eventchannel has been loaded from addressable raise the event which will be listened by SceneLoader
        private void LoadMainMenu(AsyncOperationHandle<LoadSceneEventChannelSO> eventChannel)
		{
			eventChannel.Result.RaiseEvent(_mainMenuScene, true);

			SceneManager.UnloadSceneAsync(BOOTSTRAPPER_SCENE_INDEX); // Bootstrapper scene is the only scene in BuildSettings, thus it has index 0
		}
	}
}