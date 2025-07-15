using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmbersteel.DialogueSystem
{
	public enum ActorType
	{
		Player,
		VillageChief
	}
	
	[Serializable]
	public class Actor
	{
		[field: SerializeField] public ActorType ActorType { get; private set; } = default;
		[field: SerializeField] public string ActorName { get; private set; } = default;
	}
	
	[Serializable]
	public class Line
	{
		[field: SerializeField] public Actor Actor { get; private set; } = default;
		[field: SerializeField] public string SpeechText { get; private set; } = default;
	}

	/// <summary>
	/// A Dialogue is a list of consecutive DialogueLines. They play in sequence using the input of the player to skip forward.
	/// In future versions it might contain support for branching conversations.
	/// </summary>
	[CreateAssetMenu(fileName = "NewDialogue", menuName = "Custom/Dialogues/Dialogue Data")]
	public class DialogueSO : ScriptableObject
	{
		[field: SerializeField] public List<Line> Lines { get; private set; } = default;
	}
}