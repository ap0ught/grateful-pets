using System;
using UnityEngine;
using UnityEngine.UI;

namespace AiUnity.Common.LogUI.Scripts;

public class InputFieldValidate : MonoBehaviour
{
	public bool validateNumber = true;

	public char ValidateInput(string text, int charIndex, char addedChar)
	{
		if (validateNumber && !char.IsNumber(addedChar))
		{
			addedChar = '\0';
		}
		return addedChar;
	}

	private void Start()
	{
		InputField component = gameObject.GetComponent<InputField>();
		component.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(component.onValidateInput, new InputField.OnValidateInput(ValidateInput));
	}
}
