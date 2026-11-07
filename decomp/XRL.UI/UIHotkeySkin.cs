using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.UI;
using TMPro;
using UnityEngine;

namespace XRL.UI;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class UIHotkeySkin : MonoBehaviour
{
	public enum Size
	{
		normal,
		header,
		subscript,
		topstatusbar,
		bottomstatusbar,
		xsmall,
		small,
		unset
	}

	private RectTransform _rt;

	private TextMeshProUGUI _tmp;

	public Size style;

	/// <summary>
	/// Call Apply() after setting this.
	/// </summary>
	[Multiline]
	public string text = "";

	public bool useBlockWrap = true;

	public int blockWrap = 72;

	protected string formattedText;

	public bool bold;

	public Color color = new Color(0.69f, 0.78f, 0.76f);

	protected bool _StripFormatting;

	private static List<string> keysByLength;

	private StringBuilder textProcess = new StringBuilder();

	private string lasttext;

	private int lastRefreshIndex = -1;

	private ControlManager.InputDeviceType lastDeviceType = ControlManager.InputDeviceType.Unknown;

	public RectTransform rectTransform => _rt ?? (_rt = GetComponent<RectTransform>());

	private TextMeshProUGUI tmp
	{
		get
		{
			if (_tmp == null)
			{
				_tmp = GetComponent<TextMeshProUGUI>();
			}
			return _tmp;
		}
	}

	public float preferredHeight => Updated().preferredHeight;

	public float preferredWidth => Updated().preferredWidth;

	public bool StripFormatting
	{
		get
		{
			return _StripFormatting;
		}
		set
		{
			_StripFormatting = value;
			formattedText = null;
		}
	}

	private TextMeshProUGUI Updated()
	{
		Apply();
		return tmp;
	}

	public Vector2 GetPreferredValues(float MaxWidth, float MaxHeight)
	{
		return Updated().GetPreferredValues(MaxWidth, MaxHeight);
	}

	public Vector2 GetPreferredValues(string Text, float MaxWidth, float MaxHeight)
	{
		return Updated().GetPreferredValues(Text, MaxWidth, MaxHeight);
	}

	/// <summary>
	/// shortcut to set .<paramref name="text" /> then call .Apply()
	/// </summary>
	/// <param name="text" />
	public bool SetText(string text)
	{
		if (this.text == text)
		{
			return false;
		}
		this.text = text;
		formattedText = null;
		Apply();
		return true;
	}

	public void Apply()
	{
		int num = 16;
		if (style == Size.xsmall)
		{
			num = 12;
		}
		if (style == Size.small)
		{
			num = 14;
		}
		if (style == Size.normal)
		{
			num = 16;
		}
		if (style == Size.header)
		{
			num = 24;
		}
		if (style == Size.subscript)
		{
			num = 8;
		}
		if (style == Size.topstatusbar)
		{
			num = 16;
		}
		if (style == Size.bottomstatusbar)
		{
			num = 14;
		}
		lasttext = this.text;
		textProcess.Length = 0;
		textProcess.Append(this.text);
		if (keysByLength == null)
		{
			keysByLength = CommandBindingManager.CommandBindings?.Keys?.ToList();
			keysByLength?.Sort((string a, string b) => b.Length - a.Length);
		}
		if (keysByLength != null && textProcess.Contains("~"))
		{
			for (int num2 = 0; num2 < keysByLength.Count; num2++)
			{
				string text = keysByLength[num2];
				if (textProcess.Contains("~" + text))
				{
					textProcess.Replace("~" + text, ControlManager.getCommandInputDescription(text));
					if (!textProcess.Contains("~"))
					{
						break;
					}
				}
			}
		}
		formattedText = RTF.FormatToRTF(GameText.VariableReplace(textProcess), "FF", useBlockWrap ? blockWrap : (-1), StripFormatting);
		if (style != Size.unset && tmp.fontSize != (float)num)
		{
			tmp.fontSize = num;
		}
		if (tmp.text != formattedText)
		{
			tmp.text = formattedText;
		}
		if (tmp.color != color)
		{
			tmp.color = color;
		}
		if (bold)
		{
			if ((tmp.fontStyle & FontStyles.Bold) == 0)
			{
				tmp.fontStyle += 1;
			}
		}
		else if ((tmp.fontStyle & FontStyles.Bold) != 0)
		{
			tmp.fontStyle -= 1;
		}
	}

	public void Start()
	{
		Apply();
	}

	public void Update()
	{
		if (lastDeviceType != ControlManager.activeControllerType || lasttext != text || lastRefreshIndex != CommandBindingManager.BindingRefreshIndex)
		{
			Apply();
			lastDeviceType = ControlManager.activeControllerType;
			lastRefreshIndex = CommandBindingManager.BindingRefreshIndex;
		}
	}
}
