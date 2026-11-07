using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class PickGameObjectLineData : PooledFrameworkDataElement<PickGameObjectLineData>
{
	public PickGameObjectLineDataType type;

	public PickGameObjectLineDataStyle style;

	public GameObject go;

	public string category;

	public bool collapsed;

	public bool indent;

	public char quickKey;

	public string hotkeyDescription;

	public PickGameObjectLineData set(PickGameObjectLineDataType type = PickGameObjectLineDataType.Item, PickGameObjectLineDataStyle style = PickGameObjectLineDataStyle.Interact, GameObject go = null, string category = null, bool collapsed = false, bool indent = false, char quickKey = '\0', string hotkeyDescription = null)
	{
		this.type = type;
		this.style = style;
		this.go = go;
		this.category = category;
		this.collapsed = collapsed;
		this.indent = indent;
		this.quickKey = quickKey;
		this.hotkeyDescription = hotkeyDescription;
		return this;
	}

	public override void free()
	{
		go = null;
		base.free();
	}
}
