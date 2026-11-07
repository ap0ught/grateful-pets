using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class CharacterAttributeLineData : PooledFrameworkDataElement<CharacterAttributeLineData>
{
	public enum Category
	{
		primary,
		secondary,
		resistance
	}

	public Category category;

	public Statistic data;

	public GameObject go;

	public string stat;

	public CharacterAttributeLineData set(Statistic data, GameObject go, string stat)
	{
		this.data = data;
		this.go = go;
		this.stat = stat;
		return this;
	}

	public override void free()
	{
		data = null;
		go = null;
		stat = null;
		base.free();
	}
}
