using ProjectEmbersteel.Events.EventChannel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ProjectEmbersteel.SceneManagement
{
    /// <summary>
    /// Handles the loading and unloading of scenes using Unity Addressables.
    /// Supports both menu and location scene types, and manages optional loading screen display.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private GameSceneSO _gameplayScene = default;

        [Header("Publisher")]
        [SerializeField] private SceneLoadProgressEventChannelSO _sceneLoadingProgressEvent = default;

        [Header("Listeners")]
        [SerializeField] private LoadSceneEventChannelSO _loadLocationEvent = default;
        [SerializeField] private LoadSceneEventChannelSO _loadMainMenuSceneEvent = default;

        private bool _isLoading = false;
        private GameSceneSO _sceneToLoad;
        private GameSceneSO _currentlyLoadedScene;
        private bool _showLoadingScreen;
        private SceneInstance _gameplayManagerSceneInstance;
        private AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
        private AsyncOperationHandle<SceneInstance> _gameplayManagerLoadingOpHandle;

        private void OnEnable() => SubscribeToEventChannels(true);
        private void OnDisable() => SubscribeToEventChannels(false);

        private void SubscribeToEventChannels(bool subscribe)
        {
            if (subscribe)
            {
                _loadMainMenuSceneEvent.OnLoadingRequested += LoadMainMenu;
                _loadLocationEvent.OnLoadingRequested += LoadLocation;
            }
            else
            {
                _loadMainMenuSceneEvent.OnLoadingRequested -= LoadMainMenu;
                _loadLocationEvent.OnLoadingRequested -= LoadLocation;
            }
        }

        /// <summary>
        /// Called when a location scene is requested to load.
        /// </summary>
        private void LoadLocation(GameSceneSO locationToLoad, bool showLoadingScreen, bool fadeScreen)
        {
            // Prevent double-loading if already in a loading state
            if (_isLoading)
                return;

            _sceneToLoad = locationToLoad; // Store scene to load
            _showLoadingScreen = showLoadingScreen; // Store loading screen preference
            _isLoading = true; // Set loading state

            // If gameplay manager scene is not yet loaded, load it first
            if (_gameplayManagerSceneInstance.Scene == null || !_gameplayManagerSceneInstance.Scene.isLoaded)
            {
                // Load gameplay scene additively and wait for completion
                _gameplayManagerLoadingOpHandle = _gameplayScene.SceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
                _gameplayManagerLoadingOpHandle.Completed += OnGameplayManagerSceneLoaded;
            }
            else
            {
                // Otherwise, unload previous scene and proceed
                UnloadPreviousScene();
            }
        }

        /// <summary>
        /// Unloads the previously loaded non-persistent scene, if any.
        /// </summary>
        private void UnloadPreviousScene()
        {
            if (_currentlyLoadedScene != null) // Skip if starting from Bootstrapper (no previous scene)
            {
                if (_currentlyLoadedScene.SceneReference.OperationHandle.IsValid())
                {
                    // Unload scene via Addressables if it's still valid
                    _currentlyLoadedScene.SceneReference.UnLoadScene();
                }
            }

            // Start loading the new scene
            LoadNewScene();
        }

        /// <summary>
        /// Called when the persistent Gameplay Manager scene has finished loading.
        /// </summary>
        private void OnGameplayManagerSceneLoaded(AsyncOperationHandle<SceneInstance> handle)
        {
            _gameplayManagerSceneInstance = handle.Result; // Save loaded gameplay manager scene
            UnloadPreviousScene(); // Proceed to unload current scene and load new one
        }

        /// <summary>
        /// Loads the menu scene and handles cleanup of persistent gameplay manager if needed.
        /// </summary>
        private void LoadMainMenu(GameSceneSO menuToLoad, bool showLoadingScreen, bool fadeScreen)
        {
            // Prevent double-loading
            if (_isLoading)
                return;

            _sceneToLoad = menuToLoad;
            _showLoadingScreen = showLoadingScreen;
            _isLoading = true;

            // If the persistent gameplay scene is still loaded, unload it
            if (_gameplayManagerSceneInstance.Scene != null && _gameplayManagerSceneInstance.Scene.isLoaded)
                Addressables.UnloadSceneAsync(_gameplayManagerLoadingOpHandle, true);

            // Proceed to unload and load menu
            UnloadPreviousScene();
        }

        /// <summary>
        /// Initiates async scene loading using Addressables and optionally shows the loading UI.
        /// </summary>
        private void LoadNewScene()
        {
            // Start loading the target scene additively in background
            _loadingOperationHandle = _sceneToLoad.SceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);

            // Raise event to show loading UI and pass loading progress handle
            if (_showLoadingScreen)
                _sceneLoadingProgressEvent.RaiseEvent(true, _loadingOperationHandle);

            // Register callback when loading completes
            _loadingOperationHandle.Completed += OnNewSceneLoaded;
        }

        /// <summary>
        /// Called when the new scene has been successfully loaded.
        /// </summary>
        private void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> handle)
        {
            _currentlyLoadedScene = _sceneToLoad; // Update current scene reference
            _isLoading = false; // Reset loading state

            // Raise event to hide loading screen
            if (_showLoadingScreen)
                _sceneLoadingProgressEvent.RaiseEvent(false, handle);
        }
    }
}