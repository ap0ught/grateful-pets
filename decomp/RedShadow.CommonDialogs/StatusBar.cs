using UnityEngine;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class StatusBar : MonoBehaviour
{
	private Text _text;

	private float _startTime;

	private float _duration;

	protected void Awake()
	{
		_text = transform.Find("Panel/Text").GetComponent<Text>();
	}

	public void Update()
	{
		if (_startTime > 0f && Time.timeSinceLevelLoad > _startTime + _duration)
		{
			_startTime = 0f;
			_text.text = "";
		}
	}

	public StatusBar showText(string text, float duration = 5f)
	{
		_text.text = text;
		_duration = duration;
		_startTime = Time.timeSinceLevelLoad;
		return this;
	}
}
