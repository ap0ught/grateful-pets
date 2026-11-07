using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainRubber_JumpTwice_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they jump twice in a row, each time at +1 range.";
	}

	public override string GetNotification()
	{
		return "@they feel a springiness inside.";
	}

	public override void Apply(GameObject go)
	{
		for (int i = 0; i < 2; i++)
		{
			if (!Acrobatics_Jump.Jump(go, 1, null, GetType().Name))
			{
				break;
			}
		}
	}
}
