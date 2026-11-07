using System;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Endurance_ShakeItOff : BaseSkill
{
	[NonSerialized]
	public static Event eGetShakeItOffChance = new Event("GetShakeItOffChance", "Chance", 0);

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyStun");
		Registrar.Register("ApplyDazed");
		Registrar.Register("BeforeApplyDamage");
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		TryToShakeItOff();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			if (E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Poison"))
			{
				damage.Amount = damage.Amount * 3 / 4;
				if (damage.Amount <= 0)
				{
					return false;
				}
			}
		}
		else if ((E.ID == "ApplyStun" || E.ID == "ApplyDazed") && GetShakeItOffChance().in100())
		{
			if (ParentObject.IsPlayer())
			{
				if (E.ID == "ApplyStun")
				{
					ParentObject.ParticleText("Shook off stun!", 'G');
					IComponent<GameObject>.AddPlayerMessage("You shook off the stun.", 'g');
				}
				else if (E.ID == "ApplyDazed")
				{
					ParentObject.ParticleText("Shook off daze!", 'G');
					IComponent<GameObject>.AddPlayerMessage("You shook off the dazing.", 'g');
				}
			}
			else if (E.ID == "ApplyStun")
			{
				ParentObject.ParticleText("Shook off stun!", 'R');
				IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + " shook off the stun.");
			}
			else if (E.ID == "ApplyDazed")
			{
				ParentObject.ParticleText("Shook off daze!", 'R');
				IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + " shook off the dazing.");
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return true;
	}

	public void TryToShakeItOff()
	{
		if (ParentObject.HasEffect<Dazed>() && GetShakeItOffChance().in100())
		{
			ParentObject.RemoveEffect<Dazed>();
			char color = ColorCoding.ConsequentialColorChar(ParentObject);
			ParentObject.ParticleText("Shook off daze!", color);
			DidX("shake", "off " + ParentObject.its + " daze", null, null, color.ToString() ?? "");
		}
		if (ParentObject.HasEffect<Stun>() && GetShakeItOffChance().in100())
		{
			ParentObject.RemoveEffect<Stun>();
			char color2 = ColorCoding.ConsequentialColorChar(ParentObject);
			ParentObject.ParticleText("Shook off stun!", color2);
			DidX("shake", "off the stun", null, null, color2.ToString() ?? "");
		}
	}

	public int GetShakeItOffChance()
	{
		int value = ParentObject.Stat("Toughness") - 10;
		eGetShakeItOffChance.SetParameter("Chance", value);
		ParentObject.FireEvent(eGetShakeItOffChance);
		ParentObject.FireEventOnBodyparts(eGetShakeItOffChance);
		return eGetShakeItOffChance.GetIntParameter("Chance");
	}
}
