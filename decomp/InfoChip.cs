using UnityEngine;
using XRL.UI;

[ExecuteAlways]
public class InfoChip : MonoBehaviour
{
	public string title;

	public Color titleColor;

	public string value;

	public Color valueColor;

	public UITextSkin titleText;

	public UITextSkin valueText;

	private string _lastTitle;

	private string _lastValue = "not it";

	private Color _lastTitleColor;

	private Color _lastValueColor;

	private void Start()
	{
	}

	private void Update()
	{
		bool flag = false;
		bool flag2 = false;
		if (_lastTitleColor != titleColor && titleText != null)
		{
			titleText.color = (_lastTitleColor = titleColor);
			flag = true;
		}
		if (_lastTitle != title && titleText != null)
		{
			_lastTitle = title;
			titleText.text = title + ":";
			flag = true;
		}
		if (_lastValueColor != valueColor && valueText != null)
		{
			valueText.color = (_lastValueColor = valueColor);
			flag2 = true;
		}
		if (_lastValue != value && valueText != null)
		{
			valueText.text = (_lastValue = value);
			flag2 = true;
		}
		if (flag)
		{
			titleText.Apply();
		}
		if (flag2)
		{
			valueText.Apply();
		}
	}
}
