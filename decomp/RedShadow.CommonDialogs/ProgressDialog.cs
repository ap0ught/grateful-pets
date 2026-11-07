using UnityEngine;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class ProgressDialog : DialogBase
{
	public bool IsIndeterminate;

	private Text _messageText;

	private RectTransform _bar;

	private RectTransform _progress;

	private float _indeterminateValue;

	protected override void Awake()
	{
		base.Awake();
		_messageText = transform.Find("Window/MessagePanel/Text").GetComponent<Text>();
		_bar = transform.Find("Window/ProgressBar").GetComponent<RectTransform>();
		_progress = transform.Find("Window/ProgressBar/Panel").GetComponent<RectTransform>();
	}

	public override void Update()
	{
		base.Update();
		if (IsVisible && IsIndeterminate)
		{
			_indeterminateValue += Time.deltaTime / 5f;
			if (_indeterminateValue > 1f)
			{
				_indeterminateValue = 0f;
			}
			setProgress(_indeterminateValue);
		}
	}

	public void show(string text)
	{
		show(text, indeterminate: true);
	}

	public void show(string text, bool indeterminate)
	{
		setText(text);
		IsIndeterminate = indeterminate;
		_indeterminateValue = 0f;
		StartCoroutine(show_co(0.01f));
	}

	public void close()
	{
		hide();
	}

	public ProgressDialog setText(string text)
	{
		_messageText.text = text;
		return this;
	}

	public ProgressDialog setProgress(float value)
	{
		float x = _bar.sizeDelta.x;
		_progress.sizeDelta = new Vector2(value * x, _progress.sizeDelta.y);
		return this;
	}

	public override void cancel()
	{
	}
}
