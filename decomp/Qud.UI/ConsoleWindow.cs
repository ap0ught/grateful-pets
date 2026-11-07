using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

public class ConsoleWindow : SingletonWindowBase<ConsoleWindow>
{
	public ScrollRect _scrollRect;

	public InputField _inputField;

	public Text _textField;

	public RectTransform panel;

	private bool enterAllowed;

	public static Text textField => SingletonWindowBase<ConsoleWindow>.instance._textField;

	public void ScrollToTop()
	{
		_scrollRect.normalizedPosition = new Vector2(0f, 1f);
	}

	public void ScrollToBottom()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.gameObject.GetComponent<RectTransform>());
		_scrollRect.normalizedPosition = new Vector2(0f, 0f);
	}

	public void OnServerInitialized()
	{
	}

	public void Update()
	{
		if (DebugConsole.dirty)
		{
			ScrollToBottom();
			DebugConsole.dirty = false;
		}
		if (enterAllowed && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
		{
			DebugConsole.Execute(_inputField.text);
			_inputField.text = "";
			enterAllowed = false;
			_inputField.ActivateInputField();
		}
		else
		{
			enterAllowed = _inputField.isFocused;
		}
	}

	public override void Show()
	{
		base.Show();
		_inputField.ActivateInputField();
	}
}
