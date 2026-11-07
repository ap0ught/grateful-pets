using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Ironshank : Effect, ITierInitialized
{
	public int Count;

	public int Penalty;

	public int AVBonus;

	public bool DrankCure;

	public Ironshank()
	{
		DisplayName = "ironshank";
	}

	public override string GetDetails()
	{
		return "Leg bones are fusing at the joints.\n{{C|-" + Penalty + "}} Move Speed\n{{C|+" + AVBonus + "}} AV\nWill continue to lose Move Speed and gain AV until Move Speed penalty reaches -80.";
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public void SetPenalty(int Amount)
	{
		if (Amount < 0)
		{
			Amount = 0;
		}
		StatShifter.SetStatShift("MoveSpeed", Amount);
		AVBonus = Amount / 15;
		StatShifter.SetStatShift("AV", AVBonus);
		Penalty = Amount;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Ironshank>())
		{
			return false;
		}
		if (!IsInfectable(Object))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyDisease"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Disease", this))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyIronshank"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Ironshank", this))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_diseased");
		Duration = 1;
		SetPenalty(Penalty + Stat.Random(6, 10));
		if (Object.IsPlayer())
		{
			int num = Stat.Random(2, 6);
			Achievement.GET_IRONSHANK.Unlock();
			Popup.Show("You have contracted Ironshank! You feel the cartilage stretch as your leg bones grind together at the joints.");
			JournalAPI.AddAccomplishment("You contracted ironshank.", "Woe to the scroundrels and dastards who conspired to have =name= contract Stiff Leg!", $"Near the location of Golgotha, =name= was captured by bandits. {The.Player.GetPronounProvider().CapitalizedSubjective} languished in captivity for {num} years, eventually contracting ironshank before escaping to {The.Player.CurrentZone.GetTerrainDisplayName()}.", null, "general", MuralCategory.BodyExperienceBad, MuralWeight.Medium, null, -1L);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		base.Remove(Object);
	}

	public override string GetDescription()
	{
		return "{{ironshank|ironshank}}";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DrinkingFrom");
		Registrar.Register("Eating");
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (Duration > 0 && !IsInfectable(Object))
			{
				Duration = 0;
			}
			if (Duration > 0)
			{
				Count++;
				if (Count >= 1200 && DrankCure)
				{
					Count = 0;
					DrankCure = false;
					if (Object.IsPlayer())
					{
						Popup.Show("The pain in your joints subsides.");
					}
					SetPenalty(Penalty - Stat.Random(6, 10));
					if (Penalty <= 0)
					{
						Duration = 0;
						if (Object.IsPlayer())
						{
							Achievement.CURE_IRONSHANK.Unlock();
							Popup.Show("You are cured of ironshank.");
							JournalAPI.AddAccomplishment("You were cured of ironshank and your leg bones softened.", "Blessed was the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", in the year of " + Calendar.GetYear() + " AR, when =name= was cured of ironing leg!", "Some time in =year=, =name= came upon the bandits who had captured " + The.Player.GetPronounProvider().Objective + " and let " + The.Player.GetPronounProvider().Objective + " languish in captivity until " + The.Player.GetPronounProvider().Subjective + " contracted ironshank. =name= murdered their leader <spice.elements." + The.Player.GetMythicDomain() + ".murdermethods.!random> and cured " + The.Player.GetPronounProvider().Reflexive + " of the disease.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.Medium, null, -1L);
						}
					}
				}
				else if (Count >= 4800)
				{
					Count = 0;
					if (Penalty < 80)
					{
						int num = Stat.Random(6, 10);
						if (Penalty + num >= 80)
						{
							SetPenalty(80);
							if (Object.IsPlayer())
							{
								Popup.Show("Your legs bones are nearly fused at the joints.");
							}
						}
						else
						{
							if (Object.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You feel the cartilage stretch as your leg bones grind together at the joints.", 'R');
							}
							SetPenalty(Penalty + num);
						}
					}
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			LiquidVolume liquidVolume = E.GetGameObjectParameter("Container").LiquidVolume;
			if (liquidVolume.ContainsLiquid(The.Game.GetStringGameState("IronshankCure")) && liquidVolume.ContainsLiquid("gel"))
			{
				DrankCure = true;
			}
		}
		return base.FireEvent(E);
	}

	public static bool IsInfectable(GameObject who)
	{
		return who.HasBodyPart(IsInfectableLimb);
	}

	public static bool IsInfectableLimb(BodyPart Part)
	{
		if (Part.Type != "Feet")
		{
			return false;
		}
		if (!Part.IsCategoryLive())
		{
			return false;
		}
		if (Part.Mobility <= 0)
		{
			return false;
		}
		return true;
	}
}
