using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class CharacterEffectLineData : PooledFrameworkDataElement<CharacterEffectLineData>
{
	public Effect effect;

	public CharacterEffectLineData set(Effect effect)
	{
		this.effect = effect;
		return this;
	}

	public override void free()
	{
		effect = null;
		base.free();
	}
}
