using XRL.UI.Framework;

namespace Qud.UI;

public class BookLineData : PooledFrameworkDataElement<BookLineData>
{
	public string text;

	public BookLineData set(string text)
	{
		this.text = text;
		return this;
	}

	public override void free()
	{
		text = null;
		base.free();
	}
}
