using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World.AI.GoalHandlers;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class PaxInfection : IPart
{
	public string ColorString = "&O";

	public Guid ActivatedAbilityID = Guid.Empty;

	public int GetCooldown()
	{
		return 50;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID, ParentObject.Equipped), GetCooldown());
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<AfterGameLoadedEvent>.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		UpdateAbility(ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "BeforeApplyDamage");
		E.Actor.RegisterPartEvent(this, "ActivatePaxInfection");
		UpdateAbility(E.Actor);
		if (!E.Actor.IsPlayer() && E.Actor.Brain != null)
		{
			E.Actor.Brain.PushGoal(new PaxKlanqMadness());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "BeforeApplyDamage");
		E.Actor.UnregisterPartEvent(this, "ActivatePaxInfection");
		UpdateAbility(E.Actor, ForceRemove: true);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public void UpdateAbility(GameObject GO, bool ForceRemove = false)
	{
		if (GO == null)
		{
			return;
		}
		bool flag = false;
		if (!ForceRemove)
		{
			Body body = GO.Body;
			if (body != null)
			{
				foreach (BodyPart part in body.GetParts())
				{
					if (part.Equipped != null && part.Equipped.Blueprint == ParentObject.Blueprint)
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			if (ActivatedAbilityID == Guid.Empty || GO.GetActivatedAbility(ActivatedAbilityID) == null)
			{
				ActivatedAbilityID = GO.AddActivatedAbility("Puff Spores", "ActivatePaxInfection", "Items");
			}
		}
		else
		{
			GO.RemoveActivatedAbility(ref ActivatedAbilityID);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ActivatePaxInfection")
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null && equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
			{
				Puff(OnlyIfCreatureNearby: false);
				ParentObject.UseEnergy(1000);
				equipped.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown());
				if (equipped.IsPlayer())
				{
					The.Game.Systems.ForEach((IGameSystem system) =>
					{
						system.FireEvent(E);
					});
				}
				IComponent<GameObject>.XDidY(equipped, "spew", "a cloud of spores", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			}
		}
		else if (E.ID == "BeforeApplyDamage" && 25.in100())
		{
			Puff();
			GameObject equipped2 = ParentObject.Equipped;
			if (equipped2 != null)
			{
				if (equipped2.IsPlayer())
				{
					BodyPart bodyPart = ParentObject.EquippedOn();
					IComponent<GameObject>.AddPlayerMessage("Your " + bodyPart.GetOrdinalName() + " " + (bodyPart.Plural ? "spew" : "spews") + " a cloud of spores.");
				}
				else if (IComponent<GameObject>.Visible(equipped2))
				{
					BodyPart bodyPart2 = ParentObject.EquippedOn();
					IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(equipped2.The + equipped2.ShortDisplayName) + " " + bodyPart2.GetOrdinalName() + " " + (bodyPart2.Plural ? "spew" : "spews") + " a cloud of spores.");
				}
			}
		}
		return base.FireEvent(E);
	}

	public void Puff(bool OnlyIfCreatureNearby = true)
	{
		List<Cell> list = new List<Cell>();
		GameObject equipped = ParentObject.Equipped;
		if (ParentObject.CurrentCell == null)
		{
			if (equipped == null)
			{
				return;
			}
			list = equipped.CurrentCell.GetLocalAdjacentCells();
		}
		else
		{
			list = ParentObject.CurrentCell.GetLocalAdjacentCells();
		}
		bool flag = !OnlyIfCreatureNearby;
		if (!flag && list != null)
		{
			foreach (Cell item in list)
			{
				if (item.HasObjectWithPart("Brain"))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag || list.IsNullOrEmpty())
		{
			return;
		}
		ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_gasMutation_activeRelease");
		if (ParentObject.CurrentCell != null)
		{
			ParentObject.ParticleBlip("&O*", 10, 0L);
		}
		if (equipped != null && equipped.IsPlayer() && list[0].ParentZone.ResolveZoneWorld() == "NorthSheva")
		{
			Achievement.KLANQ_SUN.Unlock();
		}
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = list[i].AddObject("FungalSporeGasPuff");
			gameObject.GetPart<Gas>().ColorString = ColorString;
			gameObject.GetPart<GasFungalSpores>().Infection = "PaxInfection";
			if (ParentObject.Equipped != null)
			{
				gameObject.GetPart<Gas>().Creator = ParentObject.Equipped;
			}
			else
			{
				gameObject.GetPart<Gas>().Creator = ParentObject;
			}
		}
	}
}
