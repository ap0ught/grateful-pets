using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainRubber_JumpThrice_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they jump@s three times in a row, each time at +2 range.";
	}

	public override string GetNotification()
	{
		return "@they feel@s a overwhelming springiness inside.";
	}

	public override void Apply(GameObject go)
	{
		for (int i = 0; i < 3; i++)
		{
			if (!Acrobatics_Jump.Jump(go, 2, null, GetType().Name))
			{
				break;
			}
		}
	}
}
