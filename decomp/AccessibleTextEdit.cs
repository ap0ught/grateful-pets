using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Text Edit")]
public class AccessibleTextEdit : UAP_BaseElement
{
	private string prevText = "";

	private string deltaText = "";

	private AccessibleTextEdit()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ETextEdit;
	}

	public override bool IsElementActive()
	{
		if (!base.IsElementActive())
		{
			return false;
		}
		if (m_ReferenceElement != null && !m_ReferenceElement.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (!UAP_AccessibilityManager.GetSpeakDisabledInteractables() && !IsInteractable())
		{
			return false;
		}
		return true;
	}

	private InputField GetInputField()
	{
		InputField inputField = null;
		if (m_ReferenceElement != null)
		{
			inputField = m_ReferenceElement.GetComponent<InputField>();
		}
		if (inputField == null)
		{
			inputField = GetComponent<InputField>();
		}
		return inputField;
	}

	private Component GetTMPInputField()
	{
		Component component = null;
		if (m_ReferenceElement != null)
		{
			component = m_ReferenceElement.GetComponent("TMP_InputField");
		}
		if (component == null)
		{
			component = GetComponent("TMP_InputField");
		}
		return component;
	}

	public override string GetCurrentValueAsText()
	{
		if (IsPassword())
		{
			return UAP_AccessibilityManager.Localize_Internal("Keyboard_PasswordHidden");
		}
		return GetValueFromEditBox();
	}

	private string GetValueFromEditBox()
	{
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			return inputField.text;
		}
		Component tMPInputField = GetTMPInputField();
		if (tMPInputField != null)
		{
			PropertyInfo property = tMPInputField.GetType().GetProperty("text");
			if (property != null)
			{
				return property.GetValue(tMPInputField, null) as string;
			}
		}
		return "";
	}

	private bool IsPassword()
	{
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			return inputField.contentType == InputField.ContentType.Password;
		}
		Component tMPInputField = GetTMPInputField();
		if (tMPInputField != null)
		{
			PropertyInfo property = tMPInputField.GetType().GetProperty("contentType");
			if (property != null)
			{
				return (int)property.GetValue(tMPInputField, null) == 7;
			}
		}
		return false;
	}

	public override bool IsInteractable()
	{
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			if (!inputField.enabled || !inputField.interactable)
			{
				return false;
			}
			return true;
		}
		Behaviour behaviour = (Behaviour)GetTMPInputField();
		if (behaviour != null)
		{
			if (!behaviour.enabled)
			{
				return false;
			}
			PropertyInfo property = behaviour.GetType().GetProperty("interactable");
			if (property != null)
			{
				return (bool)property.GetValue(behaviour, null);
			}
			return true;
		}
		return false;
	}

	protected override void OnInteract()
	{
		bool flag = UAP_AccessibilityManager.ShouldUseBuiltInKeyboard();
		bool flag2 = IsPassword();
		bool alllowMultiline = false;
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			prevText = inputField.text;
			deltaText = prevText;
			if (!flag)
			{
				inputField.onValueChanged.AddListener(delegate
				{
					ValueChangeCheck();
				});
				EventSystem.current.SetSelectedGameObject(inputField.gameObject);
			}
			else
			{
				alllowMultiline = inputField.multiLine;
			}
		}
		Behaviour behaviour = (Behaviour)GetTMPInputField();
		if (behaviour != null)
		{
			PropertyInfo property = behaviour.GetType().GetProperty("text");
			prevText = property.GetValue(behaviour, null) as string;
			deltaText = prevText;
			if (!flag)
			{
				PropertyInfo property2 = behaviour.GetType().GetProperty("onValueChanged");
				if (property2 != null)
				{
					((UnityEvent<string>)property2.GetValue(behaviour, null)).AddListener(delegate
					{
						ValueChangeCheck();
					});
					EventSystem.current.SetSelectedGameObject(behaviour.gameObject);
				}
			}
			else
			{
				PropertyInfo property3 = behaviour.GetType().GetProperty("multiLine");
				if (property3 != null)
				{
					alllowMultiline = (bool)property3.GetValue(behaviour, null);
				}
			}
		}
		if (flag)
		{
			UAP_VirtualKeyboard.ShowOnscreenKeyboard(prevText, flag2 ? UAP_VirtualKeyboard.EKeyboardMode.Password : UAP_VirtualKeyboard.EKeyboardMode.Default, !flag2, alllowMultiline);
			UAP_VirtualKeyboard.SetOnFinishListener(OnInputFinished);
		}
	}

	private void OnInputFinished(string editedText, bool wasConfirmed)
	{
		if (wasConfirmed)
		{
			InputField inputField = GetInputField();
			if (inputField != null)
			{
				inputField.text = editedText;
				inputField.onEndEdit.Invoke(editedText);
			}
			Component tMPInputField = GetTMPInputField();
			if (tMPInputField != null)
			{
				PropertyInfo property = tMPInputField.GetType().GetProperty("text");
				if (property != null)
				{
					property.SetValue(tMPInputField, editedText, null);
					PropertyInfo property2 = tMPInputField.GetType().GetProperty("onEndEdit");
					if (property2 != null)
					{
						((UnityEvent<string>)property2.GetValue(tMPInputField, null)).Invoke(editedText);
					}
				}
			}
		}
		UAP_AccessibilityManager.FinishCurrentInteraction();
	}

	public void ValueChangeCheck()
	{
		string text = "";
		string text2 = "";
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			text = inputField.text;
		}
		Component tMPInputField = GetTMPInputField();
		if (tMPInputField != null)
		{
			PropertyInfo property = tMPInputField.GetType().GetProperty("text");
			if (property != null)
			{
				text = property.GetValue(tMPInputField, null) as string;
			}
		}
		text2 = text;
		if (text.StartsWith(deltaText))
		{
			text = text.Substring(deltaText.Length);
		}
		if (text.Length > 0)
		{
			UAP_AccessibilityManager.Say(text);
		}
		deltaText = text2;
	}

	protected override void OnInteractAbort()
	{
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			inputField.onValueChanged.RemoveListener(delegate
			{
				ValueChangeCheck();
			});
			inputField.text = prevText;
		}
		Component tMPInputField = GetTMPInputField();
		if (tMPInputField != null)
		{
			PropertyInfo property = tMPInputField.GetType().GetProperty("onValueChanged");
			if (property != null)
			{
				((UnityEvent<string>)property.GetValue(tMPInputField, null)).AddListener(delegate
				{
					ValueChangeCheck();
				});
				PropertyInfo property2 = tMPInputField.GetType().GetProperty("text");
				if (property2 != null)
				{
					property2.SetValue(tMPInputField, prevText, null);
				}
			}
		}
		prevText = "";
	}

	protected override void OnInteractEnd()
	{
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			inputField.onValueChanged.RemoveListener(delegate
			{
				ValueChangeCheck();
			});
		}
		Component tMPInputField = GetTMPInputField();
		if (!(tMPInputField != null))
		{
			return;
		}
		PropertyInfo property = tMPInputField.GetType().GetProperty("onValueChanged");
		if (property != null)
		{
			((UnityEvent<string>)property.GetValue(tMPInputField, null)).RemoveListener(delegate
			{
				ValueChangeCheck();
			});
		}
	}

	protected override void OnHoverHighlight(bool enable)
	{
		InputField inputField = GetInputField();
		if (inputField != null)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			if (enable)
			{
				inputField.OnPointerEnter(eventData);
			}
			else
			{
				inputField.OnPointerExit(eventData);
			}
		}
	}

	public override bool AutoFillTextLabel()
	{
		if (!base.AutoFillTextLabel())
		{
			m_Text = "";
		}
		return false;
	}
}
