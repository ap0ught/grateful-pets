using System;
using XRL.Core;
using XRL.Language;

namespace XRL.World.Effects;

[Serializable]
public class Flagging : Effect
{
	public int Bonus;

	public int Counter;

	public Flagging()
	{
		DisplayName = "{{r|flagging}}";
	}

	public Flagging(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override string GetDetails()
	{
		return "Will fall asleep soon.";
	}

	public override bool Apply(GameObject Object)
	{
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		Object.ForceApplyEffect(new Asleep(5, forced: true));
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			Duration--;
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You're going to collapse from exhaustion in " + Grammar.Cardinal(Duration) + " " + ((Duration == 1) ? "round" : "rounds") + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "Z";
			E.ColorString = "&r";
		}
		return true;
	}
}
