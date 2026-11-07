using System;
using ConsoleLib.Console;
using TMPro;
using UnityEngine;
using XRL.World.Text;

namespace XRL.UI;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class UITextSkin : MonoBehaviour
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

	public enum AdjustmentFactor
	{
		none,
		messageLog
	}

	public enum TextWrappingModeType
	{
		LegacyPaddingUpgradeMode
	}

	private static int LastCapacity = 32768;

	private static TextBuilder Buffer1 = new StaticTextBuilder(32768);

	private static TextBuilder Buffer2 = new StaticTextBuilder(32768);

	private RectTransform _rt;

	private TextMeshProUGUI _tmp;

	public Size style;

	public AdjustmentFactor adjustmentFactor;

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

	public TextWrappingModeType textWrappingMode;

	protected bool _StripFormatting;

	private bool autoSizeSet;

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

	private static void SwapBuffers()
	{
		TextBuilder buffer = Buffer1;
		Buffer1 = Buffer2;
		Buffer2 = buffer;
	}

	private void SetTheText(ReadOnlySpan<char> Text)
	{
		if (StripFormatting)
		{
			ConsoleLib.Console.ColorUtility.StripFormatting(Text, Buffer1);
		}
		else
		{
			Buffer1.Rebase(Text);
		}
		ZMarkup.Transform(Buffer1, Buffer2);
		if (useBlockWrap && blockWrap > 0)
		{
			TextBlock.ParseSpan(Buffer2.AsSpan(), Buffer1, blockWrap, 5000);
		}
		else
		{
			SwapBuffers();
		}
		Sidebar.FormatToRTF(Buffer1, Buffer2);
		Buffer2.SetToTMP(tmp);
		if (LastCapacity != Math.Max(Buffer1.Capacity, Buffer2.Capacity))
		{
			LastCapacity = Math.Max(Buffer1.Capacity, Buffer2.Capacity);
			MetricsManager.LogWarning($"Expanded buffer size, now {LastCapacity}");
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
		if (!autoSizeSet)
		{
			autoSizeSet = true;
			tmp.vertexBufferAutoSizeReduction = false;
		}
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
		if (adjustmentFactor == AdjustmentFactor.messageLog)
		{
			num += Options.MessageLogLineSizeAdjustment;
		}
		SetTheText(text);
		if (style != Size.unset && tmp.fontSize != (float)num)
		{
			tmp.fontSize = num;
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
		if (textWrappingMode == TextWrappingModeType.LegacyPaddingUpgradeMode && tmp.textWrappingMode == TextWrappingModes.Normal)
		{
			tmp.textWrappingMode = TextWrappingModes.PreserveWhitespace;
		}
	}

	public void Start()
	{
		Apply();
	}
}
