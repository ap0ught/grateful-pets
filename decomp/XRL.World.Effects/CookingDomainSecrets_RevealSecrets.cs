using System;
using Qud.API;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainSecrets_RevealSecrets : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "Reveals a secret to @thisCreature.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			if (Object.IsPlayer())
			{
				JournalAPI.RevealRandomSecret();
			}
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
