using XRL.UI;

namespace XRL.World.Effects;

public class BasicCookingEffect_ToHit : BasicCookingEffect
{
	public BasicCookingEffect_ToHit()
	{
	}

	public BasicCookingEffect_ToHit(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		return "+1 to hit";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetToHitModifierEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Actor == Object && E.Checking == "Actor")
		{
			E.Modifier++;
		}
		return base.HandleEvent(E);
	}

	public override void ApplyEffect(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|+1 to hit for the rest of the day}}");
		}
		base.ApplyEffect(Object);
	}
}
