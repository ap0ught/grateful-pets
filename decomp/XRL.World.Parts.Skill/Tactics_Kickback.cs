using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Kickback : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<BeforeFireMissileWeaponsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeFireMissileWeaponsEvent E)
	{
		if (E.Actor == ParentObject && GameObject.Validate(E.ApparentTarget))
		{
			Cell cell = E.Actor.CurrentCell;
			if (cell != null && cell.IsAdjacentTo(E.ApparentTarget.CurrentCell) && (E.Actor.HasBodyPart("Feet") || E.Actor.GetBodyPartCount("Foot") >= 2) && E.ApparentTarget.IsCombatObject() && E.Actor.FlightCanReach(E.ApparentTarget))
			{
				if (!E.Actor.PhaseMatches(E.ApparentTarget) || E.ApparentTarget.GetMatterPhase() != 1)
				{
					if (E.Actor.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You kick at " + E.ApparentTarget.t() + ", but the kick passes through " + E.ApparentTarget.them + ".");
					}
					else if (E.ApparentTarget.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("kick") + " at you, but the kick passes through you.");
					}
					else if (E.ApparentTarget.IsVisible() && E.Actor.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("kick") + " at " + E.ApparentTarget.t() + ", but the kick passes through " + E.ApparentTarget.them + ".");
					}
				}
				else if (!E.ApparentTarget.CanBeInvoluntarilyMoved() || E.ApparentTarget.MakeSave("Strength", 15, E.Actor, null, "Kickback Knockback") || !E.ApparentTarget.Push(E.Actor.CurrentCell.GetDirectionFromCell(E.ApparentTarget.CurrentCell), E.Actor.Stat("Strength") * 50, 4))
				{
					E.ApparentTarget.PlayWorldSound("sfx_characterTrigger_kickback");
					if (E.Actor.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You kick at " + E.ApparentTarget.t() + ", but " + E.ApparentTarget.does("hold", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " " + E.ApparentTarget.its + " ground.");
					}
					else if (E.ApparentTarget.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("kick") + " at you, but you hold your ground.");
					}
					if (E.ApparentTarget.IsVisible() && E.Actor.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("kick") + " at " + E.ApparentTarget.t() + ", but " + E.ApparentTarget.does("hold", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " " + E.ApparentTarget.its + " ground.");
					}
				}
				else
				{
					E.ApparentTarget.PlayWorldSound("sfx_characterTrigger_kickback");
					E.ApparentTarget.DustPuff();
					if (E.Actor.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You kick " + E.ApparentTarget.t() + " backwards.");
					}
					else if (E.ApparentTarget.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("kick") + " you backwards.");
					}
					else if (E.ApparentTarget.IsVisible() && E.Actor.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("kick") + " " + E.ApparentTarget.t() + " backwards.");
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool AddSkill(GameObject Object)
	{
		bool? flag = Object.Brain?.PointBlankRange;
		if (flag.HasValue)
		{
			if (flag == true)
			{
				Object.ModIntProperty("HadPointBlankRangeBeforeKickback", 1);
			}
			else
			{
				Object.Brain.PointBlankRange = true;
			}
		}
		return base.AddSkill(Object);
	}

	public override bool RemoveSkill(GameObject Object)
	{
		if (!Object.HasProperty("HadPointBlankRangeBeforeKickback") && Object.Brain != null)
		{
			Object.Brain.PointBlankRange = false;
		}
		return base.RemoveSkill(Object);
	}
}
