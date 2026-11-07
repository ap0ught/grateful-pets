using System;

namespace XRL.World.Effects;

[Serializable]
public class IrisdualMoltingSound : SoundRendererEffect
{
	[NonSerialized]
	private string SoundID;

	public override string GetDescription()
	{
		return null;
	}

	public override void Render(RenderSoundEvent E)
	{
		if (SoundID == null)
		{
			SoundID = "IrisdualMolting" + "." + Guid.NewGuid().ToString();
		}
		E.Char.soundExtra.Add(SoundID, "Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_molting_lp", 1f, 1f, 0.5f, 20);
	}
}
