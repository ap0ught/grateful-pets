using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class SkillsAndPowersLineData : PooledFrameworkDataElement<SkillsAndPowersLineData>
{
	public GameObject go;

	public SPNode entry;

	public SkillsAndPowersStatusScreen screen;

	public SkillsAndPowersLineData set(SPNode entry, SkillsAndPowersStatusScreen screen, GameObject go)
	{
		this.entry = entry;
		this.screen = screen;
		this.go = go;
		return this;
	}

	public override void free()
	{
		go = null;
		entry = null;
		screen = null;
		base.free();
	}
}
