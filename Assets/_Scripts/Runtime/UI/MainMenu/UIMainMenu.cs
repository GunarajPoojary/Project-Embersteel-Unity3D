using PrimeTween;
using ProjectEmbersteel.Events.EventChannel;
using ProjectEmbersteel.SceneManagement;
using UnityEngine;

namespace ProjectEmbersteel.UI
{
    public class UIMainMenu : MonoBehaviour
    {
        [SerializeField] private GameSceneSO _locationToLoad;

		[Header("UI Animation")]
		[SerializeField] private RectTransform _popupPanel;
        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private float _visibleOffset = 0f;
        [SerializeField] private float _hiddenOffset = -120f;

        [Header("Publisher")]
        [SerializeField] private LoadSceneEventChannelSO _loadLocationEvent;

        private bool _isAnimating = false;

        private void Start() => SetPopupDefaultPosition();

        public void StartNewGame() => _loadLocationEvent.RaiseEvent(_locationToLoad, true, false);

        public void QuitGame() => Application.Quit();

		public void ShowComingSoonPopup()
		{
			if (_isAnimating) return;

			_popupPanel.gameObject.SetActive(true);

			_isAnimating = true;

			Tween.LocalPositionY(_popupPanel, _visibleOffset, _animationDuration, Ease.OutBack)
				 .OnComplete(OnShowPopup);
		}

        private void SetPopupDefaultPosition()
        {
            _popupPanel.localPosition = new Vector2(0, _hiddenOffset);

            _popupPanel.gameObject.SetActive(false);
        }

		private void OnShowPopup() => Tween.Delay(_displayDuration)
										   .OnComplete(OnReachDisplayDuration);

		private void OnReachDisplayDuration() => Tween.LocalPositionY(_popupPanel, _hiddenOffset, _animationDuration, Ease.InBack)
				   .OnComplete(OnHidePopup);

		private void OnHidePopup()
		{
			_popupPanel.gameObject.SetActive(false);
			_isAnimating = false;
		}
    }
}