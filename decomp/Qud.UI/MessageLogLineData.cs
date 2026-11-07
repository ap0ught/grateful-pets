using XRL.UI.Framework;

namespace Qud.UI;

public class MessageLogLineData : PooledFrameworkDataElement<MessageLogLineData>
{
	public string text;

	public string sortText;

	public MessageLogLineData set(string text)
	{
		this.text = text;
		sortText = text.ToLower();
		return this;
	}

	public override void free()
	{
		text = null;
		sortText = null;
		base.free();
	}
}
