using XRL.UI;

namespace XRL.World.Effects;

public class BasicCookingEffect_XP : BasicCookingEffect
{
	public BasicCookingEffect_XP()
	{
	}

	public BasicCookingEffect_XP(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		return "+5% XP gained";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AwardingXPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AwardingXPEvent E)
	{
		E.Amount += E.Amount / 20;
		return base.HandleEvent(E);
	}

	public override void ApplyEffect(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|+5% XP gained for the rest of the day}}");
		}
	}
}
