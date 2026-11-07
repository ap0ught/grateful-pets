using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Metamorphosis : BaseMutation
{
	public GameObject Target;

	public Brain _pBrain;

	public Brain pBrain
	{
		get
		{
			if (_pBrain == null)
			{
				_pBrain = ParentObject.Brain;
			}
			return _pBrain;
		}
	}

	public static void ClearInventory(GameObject go)
	{
		go.Inventory?.Clear();
		Body body = go.Body;
		if (body == null)
		{
			return;
		}
		foreach (BodyPart part in body.GetParts())
		{
			try
			{
				if (part.Equipped != null)
				{
					_ = part.Equipped;
					if (!part.Equipped.IsNatural())
					{
						part.Equipped.Obliterate();
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("metamorphosis0", x);
			}
		}
	}

	public static void TransferInventory(GameObject source, GameObject dest, bool bTagLastEquipped = true)
	{
		Inventory inventory = source.Inventory;
		Inventory inventory2 = dest.Inventory;
		while (inventory.Objects.Count > 0)
		{
			GameObject gameObject = inventory.Objects[0];
			inventory.RemoveObject(gameObject);
			inventory2.AddObject(gameObject);
		}
		Body body = source.Body;
		Body body2 = dest.Body;
		List<GameObject> list = new List<GameObject>();
		List<BodyPart> list2 = new List<BodyPart>();
		foreach (BodyPart part in body.GetParts())
		{
			try
			{
				if (part.Equipped == null)
				{
					continue;
				}
				GameObject equipped = part.Equipped;
				if (part.Equipped.IsNatural())
				{
					continue;
				}
				if (bTagLastEquipped)
				{
					part.Equipped.SetStringProperty("MetamorphLastEquipped", part.Name);
				}
				source.FireEvent(Event.New("CommandUnequipObject", "BodyPart", part, "SemiForced", 1));
				if (part.Equipped == null)
				{
					if (!list.Contains(equipped))
					{
						list.Add(equipped);
						list2.Add(part);
					}
				}
				else if (!part.Equipped.IsNatural())
				{
					list.Add(part.Equipped);
					list2.Add(part);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("metamorphosis0", x);
			}
		}
		inventory.Objects.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			try
			{
				if (!list[i].HasStringProperty("MetamorphLastEquipped"))
				{
					goto IL_023d;
				}
				BodyPart partByName = body2.GetPartByName(list[i].GetStringProperty("MetamorphLastEquipped"));
				if (partByName != null && partByName.Equipped != null && partByName.Equipped.DisplayName == list[i].DisplayName)
				{
					partByName._Equipped = list[i];
					list[i].Physics._Equipped = list[i];
					continue;
				}
				if (partByName == null || partByName.Equipped != null)
				{
					goto IL_023d;
				}
				dest.FireEvent(Event.New("CommandEquipObject", "Object", list[i], "BodyPart", partByName));
				if (partByName.Equipped != list[i])
				{
					goto IL_023d;
				}
				continue;
				IL_023d:
				List<BodyPart> unequippedPart = body2.GetUnequippedPart(list2[i].Type);
				if ((unequippedPart.Count == 0 || !dest.FireEvent(Event.New("CommandEquipObject", "Object", list[i], "BodyPart", unequippedPart[0])) || unequippedPart[0].Equipped != list[i]) && !inventory2.Objects.Contains(list[i]))
				{
					inventory2.AddObject(list[i]);
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogException("metamorphosis1", x2);
			}
		}
		List<GameObject> list3 = new List<GameObject>(inventory2.Objects);
		for (int j = 0; j < list3.Count; j++)
		{
			try
			{
				if (list3[j].HasStringProperty("MetamorphLastEquipped"))
				{
					BodyPart partByName2 = body2.GetPartByName(list3[j].GetStringProperty("MetamorphLastEquipped"));
					if (partByName2 != null && partByName2.Equipped == null)
					{
						dest.FireEvent(Event.New("CommandEquipObject", "Object", list3[j], "BodyPart", partByName2));
					}
					if (!bTagLastEquipped)
					{
						list3[j].Property.Remove("MetamorphLastEquipped");
					}
				}
			}
			catch (Exception x3)
			{
				MetricsManager.LogException("metamorphosis2", x3);
			}
		}
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandMetamorphosis");
	}

	public override string GetDescription()
	{
		return "You assume the form of any creature you touch.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("May only assume the form of creatures level " + GetMaxLevel(Level) + " or lower.\n", "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public int GetCooldown(int Level)
	{
		return 525 - 25 * Level;
	}

	public int GetMaxLevel(int Level)
	{
		return (1 + Level) * 5;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("MaxLevel", GetMaxLevel(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static void TransferMental(GameObject Source, GameObject Target)
	{
		if (Source.IsPlayer())
		{
			Target.RemovePart<ConversationScript>();
		}
		Target.Statistics["Intelligence"] = new Statistic(Source.Statistics["Intelligence"]);
		Target.Statistics["Willpower"] = new Statistic(Source.Statistics["Willpower"]);
		Target.Statistics["Ego"] = new Statistic(Source.Statistics["Ego"]);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandMetamorphosis")
		{
			Cell cell = PickDirection(ForAttack: true, "Metamorphosis");
			if (cell != null)
			{
				foreach (GameObject item in cell.GetObjectsWithPart("Combat"))
				{
					if (item != ParentObject)
					{
						GameObject gameObject = item.DeepCopy();
						gameObject.RemovePart<GivesRep>();
						gameObject.Inventory.Objects.Clear();
						if (gameObject.ApplyEffect(new Metamorphed(ParentObject, 1)))
						{
							GameObject player = The.Player;
							ClearInventory(gameObject);
							TransferMental(player, gameObject);
							TransferInventory(player, gameObject);
							DidX("assume", "the form of " + item.a + item.ShortDisplayName, null, null, null, ParentObject);
							XRLCore.Core.Game.ActionManager.RemoveActiveObject(player);
							XRLCore.Core.Game.ActionManager.AddActiveObject(gameObject);
							Cell cell2 = player.Physics.CurrentCell;
							cell2.RemoveObject(player);
							cell2.AddObject(gameObject);
							player.MakeInactive();
							gameObject.MakeActive();
							XRLCore.Core.Game.Player.Body = gameObject;
						}
						gameObject.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
						UseEnergy(1000, "Physical Mutation");
						CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
						return true;
					}
					if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("That creature is of too high a level to duplicate!", 'K');
					}
				}
			}
			return true;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Metamorphosis", "CommandMetamorphosis", "Physical Mutations", null, "\u0001");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
