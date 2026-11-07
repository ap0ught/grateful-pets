using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFear_Intimidate_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they intimidate@s creatures around @them.";
	}

	public override string GetNotification()
	{
		return "@they intimidate@s everyone around @them.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer() && go.GetCurrentCell() != null)
		{
			Persuasion_Intimidate.ApplyIntimidate(go.GetCurrentCell(), go, FreeAction: true);
		}
	}
}
