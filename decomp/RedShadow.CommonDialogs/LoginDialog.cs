using System;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class LoginDialog : DialogBase
{
	private InputField _userNameInputField;

	private InputField _passwordInputField;

	private Text _messageText;

	private Action<string, string> _callback;

	public Buttons Result { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Result = Buttons.Cancel;
		_messageText = transform.Find("Window/MessagePanel/Text").GetComponent<Text>();
		_userNameInputField = transform.Find("Window/InputPanel/UserName").GetComponent<InputField>();
		_passwordInputField = transform.Find("Window/InputPanel/Password").GetComponent<InputField>();
	}

	public void getLogin(string text, string defaultUserName, string defaultPassword, Action<string, string> callback)
	{
		Result = Buttons.Cancel;
		_callback = callback;
		setText(text);
		setUserName(defaultUserName);
		setPassword(defaultPassword);
		StartCoroutine(show_co(0.01f));
	}

	public void setText(string text)
	{
		_messageText.text = text;
	}

	public void setUserName(string value)
	{
		_userNameInputField.text = value;
	}

	public void setPassword(string value)
	{
		_passwordInputField.text = value;
	}

	public string getUserName()
	{
		return _userNameInputField.text;
	}

	public string getPassword()
	{
		return _passwordInputField.text;
	}

	public void onOk()
	{
		Result = Buttons.Ok;
		hide();
	}

	protected override void hide()
	{
		base.hide();
		_callback((Result != Buttons.Ok) ? null : getUserName(), (Result != Buttons.Ok) ? null : getPassword());
	}
}
