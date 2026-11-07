using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class InputDialog : DialogBase
{
	private InputField _inputField;

	private Slider _slider;

	private InputField _sliderInput;

	private Dropdown _dropdown;

	private Text _messageText;

	private Action<string> _stringCallback;

	private Action<float> _floatCallback;

	private Action<int> _intCallback;

	private bool _isUpdating;

	public Buttons Result { get; private set; }

	public InputType InputType { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Result = Buttons.Cancel;
		_messageText = transform.Find("Window/MessagePanel/Text").GetComponent<Text>();
		_inputField = transform.Find("Window/InputPanel/InputField").GetComponent<InputField>();
		_slider = transform.Find("Window/InputPanel/Slider").GetComponent<Slider>();
		_sliderInput = transform.Find("Window/InputPanel/Slider/InputField").GetComponent<InputField>();
		_dropdown = transform.Find("Window/InputPanel/Dropdown").GetComponent<Dropdown>();
	}

	public void getString(string text, string defaultValue, Action<string> callback, InputType type = InputType.String)
	{
		switch (type)
		{
		case InputType.UserName:
			_inputField.contentType = InputField.ContentType.Alphanumeric;
			break;
		case InputType.Password:
			_inputField.contentType = InputField.ContentType.Password;
			break;
		case InputType.Email:
			_inputField.contentType = InputField.ContentType.EmailAddress;
			break;
		default:
			type = InputType.String;
			break;
		case InputType.String:
			break;
		}
		InputType = type;
		_inputField.gameObject.SetActive(value: true);
		_slider.gameObject.SetActive(value: false);
		_dropdown.gameObject.SetActive(value: false);
		Result = Buttons.Cancel;
		_stringCallback = callback;
		_floatCallback = null;
		_intCallback = null;
		setText(text);
		setValue(defaultValue);
		StartCoroutine(show_co(0.01f));
	}

	public void getUserName(string text, string defaultValue, Action<string> callback)
	{
		getString(text, defaultValue, callback, InputType.UserName);
	}

	public void getPassword(string text, string defaultValue, Action<string> callback)
	{
		getString(text, defaultValue, callback, InputType.Password);
	}

	public void getEmail(string text, string defaultValue, Action<string> callback)
	{
		getString(text, defaultValue, callback, InputType.Email);
	}

	public void getFloat(string text, float defaultValue, float minValue, float maxValue, Action<float> callback)
	{
		InputType = InputType.Float;
		_inputField.gameObject.SetActive(value: false);
		_slider.gameObject.SetActive(value: true);
		_dropdown.gameObject.SetActive(value: false);
		Result = Buttons.Cancel;
		_stringCallback = null;
		_floatCallback = callback;
		_intCallback = null;
		setText(text);
		_sliderInput.contentType = InputField.ContentType.DecimalNumber;
		_slider.minValue = minValue;
		_slider.maxValue = maxValue;
		_slider.wholeNumbers = false;
		setValue(defaultValue);
		StartCoroutine(show_co(0.01f));
	}

	public void getInt(string text, int defaultValue, int minValue, int maxValue, Action<int> callback)
	{
		InputType = InputType.Int;
		_inputField.gameObject.SetActive(value: false);
		_slider.gameObject.SetActive(value: true);
		_dropdown.gameObject.SetActive(value: false);
		Result = Buttons.Cancel;
		_stringCallback = null;
		_floatCallback = null;
		_intCallback = callback;
		setText(text);
		_sliderInput.contentType = InputField.ContentType.IntegerNumber;
		_slider.minValue = minValue;
		_slider.maxValue = maxValue;
		_slider.wholeNumbers = true;
		setValue(defaultValue);
		StartCoroutine(show_co(0.01f));
	}

	public void getChoice(string text, int defaultChoice, List<string> choices, Action<int> callback)
	{
		InputType = InputType.Choice;
		_inputField.gameObject.SetActive(value: false);
		_slider.gameObject.SetActive(value: false);
		_dropdown.gameObject.SetActive(value: true);
		Result = Buttons.Cancel;
		_stringCallback = null;
		_floatCallback = null;
		_intCallback = callback;
		setText(text);
		_dropdown.AddOptions(choices);
		setValue(defaultChoice);
		StartCoroutine(show_co(0.01f));
	}

	public void setText(string text)
	{
		_messageText.text = text;
	}

	public void setValue(string value)
	{
		if (InputType == InputType.String || InputType == InputType.UserName || InputType == InputType.Password || InputType == InputType.Email)
		{
			_inputField.text = value;
		}
	}

	public void setValue(float value)
	{
		if (InputType == InputType.Float)
		{
			_slider.value = value;
		}
	}

	public void setValue(int value)
	{
		if (InputType == InputType.Choice)
		{
			_dropdown.value = value;
		}
		else if (InputType == InputType.Int || InputType == InputType.Float)
		{
			_slider.value = value;
		}
	}

	public string getStringValue()
	{
		if (InputType == InputType.String || InputType == InputType.UserName || InputType == InputType.Password || InputType == InputType.Email)
		{
			return _inputField.text;
		}
		if (InputType == InputType.Float)
		{
			return _slider.value.ToString();
		}
		if (InputType == InputType.Int)
		{
			return Mathf.RoundToInt(_slider.value).ToString();
		}
		if (InputType == InputType.Choice)
		{
			return _dropdown.captionText.text;
		}
		return null;
	}

	public float getFloatValue()
	{
		if (InputType == InputType.Float || InputType == InputType.Int)
		{
			return _slider.value;
		}
		return -1f;
	}

	public int getIntValue()
	{
		if (InputType == InputType.Choice)
		{
			return _dropdown.value;
		}
		if (InputType == InputType.Int || InputType == InputType.Float)
		{
			return Mathf.RoundToInt(_slider.value);
		}
		return -1;
	}

	public void onSliderChanged()
	{
		if (!_isUpdating)
		{
			_isUpdating = true;
			_sliderInput.text = _slider.value.ToString();
			_isUpdating = false;
		}
	}

	public void onSliderInputChanged()
	{
		if (!_isUpdating)
		{
			_isUpdating = true;
			try
			{
				_slider.value = float.Parse(_sliderInput.text);
			}
			catch (FormatException)
			{
				_slider.value = _slider.minValue;
			}
			_isUpdating = false;
		}
	}

	public void onOk()
	{
		Result = Buttons.Ok;
		hide();
	}

	protected override void hide()
	{
		base.hide();
		switch (InputType)
		{
		case InputType.String:
		case InputType.UserName:
		case InputType.Password:
		case InputType.Email:
			_stringCallback((Result != Buttons.Ok) ? null : getStringValue());
			break;
		case InputType.Float:
			_floatCallback((Result != Buttons.Ok) ? (-1f) : getFloatValue());
			break;
		case InputType.Int:
			_intCallback((Result != Buttons.Ok) ? (-1) : getIntValue());
			break;
		case InputType.Choice:
			_intCallback((Result != Buttons.Ok) ? (-1) : getIntValue());
			break;
		}
	}
}
