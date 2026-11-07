using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class BasiliskPoison : Effect, ITierInitialized
{
	public int TotalDebuff;

	public BasiliskPoison()
	{
		DisplayName = "{{K|stony}}";
	}

	public BasiliskPoison(int Duration, GameObject Owner)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override string GetDetails()
	{
		return "-1d6 Move Speed per turn for 14-18 turns.\n[total:-{{C|" + TotalDebuff + "}} Move Speed]";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("ApplyPoison"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Poison", this))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "BasiliskPoison", this))
		{
			return false;
		}
		if (Object.HasEffect<BasiliskPoison>())
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_poisoned");
		Duration = Stat.Roll("1d5+13");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.HasStat("MoveSpeed") && TotalDebuff != 0)
		{
			Object.GetStat("MoveSpeed").Bonus -= TotalDebuff;
			TotalDebuff = 0;
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		Registrar.Register("Recuperating");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 160;
		if (num > 0 && num < 10)
		{
			E.RenderString = "R";
			E.ColorString = "&K";
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Duration--;
			if (Duration > 0 && Object.HasStat("MoveSpeed"))
			{
				int num = Stat.Random(1, 6);
				TotalDebuff += num;
				Object.GetStat("MoveSpeed").Bonus += num;
				if (Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You feel stiff as a stone.");
				}
			}
		}
		else if (E.ID == "Recuperating")
		{
			Duration = 0;
			DidX("feel", "less stiff", null, null, null, Object);
		}
		return base.FireEvent(E);
	}
}
