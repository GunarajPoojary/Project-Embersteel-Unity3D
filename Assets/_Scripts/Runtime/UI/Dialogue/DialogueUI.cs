using ProjectEmbersteel.Events.EventChannel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEmbersteel.DialogueSystem.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private GameObject _playerNameObj;
        [SerializeField] private GameObject _nPCNameObj;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeDialogueButton;

        [Header("Listener")]
        [SerializeField] private DialogueSOEventChannelSO _startDialogueEventChannel;

		[Header("Publisher")]
        [SerializeField] private VoidEventChannelSO _endDialogueEventChannel;

        private DialogueSO _currentDialogue;
        private int _currentLineIndex = 0;
        private bool _isDialogueActive = false;

        private void OnEnable() => SubscribeToEvents(true);

        private void OnDisable() => SubscribeToEvents(false);

        private void Start()
        {
            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(false);
        }

        private void SubscribeToEvents(bool subscribe)
        {
            if (subscribe)
            {
                _nextButton.onClick.AddListener(NextLine);
                _closeDialogueButton.onClick.AddListener(EndDialogue);
                _startDialogueEventChannel.OnEventRaised += StartDialogue;
            }
            else
            {
                _nextButton.onClick.RemoveListener(NextLine);
                _closeDialogueButton.onClick.RemoveListener(EndDialogue);
                _startDialogueEventChannel.OnEventRaised -= StartDialogue;
            }
        }

        private void StartDialogue(DialogueSO dialogue)
        {
            if (dialogue == null || dialogue.Lines.Count == 0)
                return;

            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            _isDialogueActive = true;

            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(true);

            DisplayCurrentLine();
        }

        private void NextLine()
        {
            if (!_isDialogueActive || _currentDialogue == null)
                return;

            _currentLineIndex++;

            if (_currentLineIndex >= _currentDialogue.Lines.Count - 1)
            {
                _nextButton.gameObject.SetActive(false);
                return;
            }

            DisplayCurrentLine();
        }

        private void DisplayCurrentLine()
        {
            if (_currentDialogue == null || _currentLineIndex >= _currentDialogue.Lines.Count)
                return;

            Line currentLine = _currentDialogue.Lines[_currentLineIndex];

            if (_speakerNameText != null)
                _speakerNameText.text = currentLine.Actor.ActorName;

            if (_dialogueText != null)
                _dialogueText.text = currentLine.SpeechText;

            if (currentLine.Actor.ActorType == ActorType.Player)
            {
                _playerNameObj.SetActive(true);
                _nPCNameObj.SetActive(false);
            }
            else
            {
                _playerNameObj.SetActive(false);
                _nPCNameObj.SetActive(true);
            }
        }

        private void EndDialogue()
        {
            _isDialogueActive = false;
            _currentDialogue = null;
            _currentLineIndex = 0;

            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(false);

            _endDialogueEventChannel.RaiseEvent();
        }
    }
}