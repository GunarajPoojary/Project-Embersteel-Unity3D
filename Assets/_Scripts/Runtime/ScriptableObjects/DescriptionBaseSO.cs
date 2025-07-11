﻿using UnityEngine;

namespace ProjectEmbersteel
{
	/// <summary>
	/// Base class for ScriptableObjects that need a public description field.
	/// </summary>
	public class DescriptionBaseSO : ScriptableObject
	{
		[TextArea]
		public string Description;
	}
}