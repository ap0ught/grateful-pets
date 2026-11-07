using System.Collections.Generic;
using TMPro;
using XRL.UI;

public class MessageLogElement : PooledScrollRectElement<string>
{
	public TextMeshProUGUI text;

	public int lastSize = 16;

	public override void Setup(int placement, List<string> allData)
	{
		if (text.text != allData[placement])
		{
			text.vertexBufferAutoSizeReduction = false;
			text.text = allData[placement];
		}
	}

	public void Awake()
	{
		Update();
	}

	public void Update()
	{
		int num = 16 + Options.MessageLogLineSizeAdjustment;
		if ((float)num != text.fontSize)
		{
			text.fontSize = num;
		}
	}
}
