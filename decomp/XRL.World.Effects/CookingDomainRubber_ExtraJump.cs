using System;
using XRL.UI;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainRubber_ExtraJump : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "Whenever @thisCreature jump@s, @they may immediately jump again.";
	}

	public override string GetTemplatedDescription()
	{
		return "Whenever @thisCreature jump@s, @they may immediately jump again.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "Jumped");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "Jumped");
	}

	public override void FireEvent(Event E)
	{
		if (!(E.ID == "Jumped") || E.GetIntParameter("Pass") != JumpedEvent.PASSES)
		{
			return;
		}
		string stringParameter = E.GetStringParameter("SourceKey");
		if (stringParameter != null && stringParameter.Contains("CookingDomainRubber"))
		{
			return;
		}
		GameObject gameObject = parent?.Object;
		if (gameObject != null)
		{
			The.Core.RenderBase();
			if (gameObject.IsPlayer())
			{
				Popup.Show("You bounce.");
			}
			Acrobatics_Jump.Jump(gameObject, 0, null, GetType().Name);
		}
	}
}
