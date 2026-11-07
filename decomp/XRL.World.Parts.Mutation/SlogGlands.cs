using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SlogGlands : BaseMutation
{
	public const string EQUIPMENT_BLUEPRINT = "Bilge Sphincter";

	public const string MANAGER_ID = "Mutation::SlogGlands";

	public static readonly int COOLDOWN = 10;

	public static readonly int RANGE = 10;

	public SlogGlands()
	{
		Type = "Physical";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", RANGE);
		stats.Set("Area", "3x3");
		stats.Set("KnockdownChance", "Strength / Agility vs. character level");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 10 && FindSpitVolume() != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && HasBilgeSphincter() && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add("CommandSlog");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeApplyDamage");
		Registrar.Register("CommandSlog");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You bear a sphincter-choked bilge hose that you use to slurp up nearby liquids and spew them at enemies, occasionally knocking them down.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "+6 Strength\n", "+1 AV\n"), "+100 Acid Resistance\n"), "+300 reputation with mollusks\n"), "Bilge sphincter acts as a melee weapon.\n"), "+50 move speed when moving through tiles with 200+ drams of liquid\n"), "You can spew liquid from your tile into a nearby area.\n"), "Spew range: 10\n"), "Spew area: 3x3\n"), "Spew chance to knock the targets down: Strength/Agility save vs. character level\n"), "Spew cooldown: 10 rounds\n");
	}

	private LiquidVolume FindSpitVolume()
	{
		GameObject gameObject = ParentObject.CurrentCell?.GetFirstObjectWithPart("LiquidVolume");
		if (gameObject != null)
		{
			LiquidVolume liquidVolume = gameObject.LiquidVolume;
			if (liquidVolume.MaxVolume == -1 && liquidVolume.Volume > 0)
			{
				return liquidVolume;
			}
		}
		return null;
	}

	private GameObject FindBilgeSphincter()
	{
		return ParentObject.Body?.FindEquipmentOrDefaultByBlueprint("Bilge Sphincter");
	}

	private bool HasBilgeSphincter()
	{
		return FindBilgeSphincter() != null;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSlog")
		{
			if (!HasBilgeSphincter())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("Your bilge sphincter is missing.");
				}
				return false;
			}
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			LiquidVolume liquidVolume = FindSpitVolume();
			if (liquidVolume == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("There is no liquid here for you to spew.");
				}
				return false;
			}
			int statValue = ParentObject.GetStatValue("Level", 15);
			List<Cell> list = PickBurst(1, 10, Locked: false, AllowVis.OnlyVisible, "Spew");
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(ParentObject) > 10)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("That is out of range! (10 squares)");
					}
					return false;
				}
			}
			UseEnergy(1000, "Physical Mutation Bilge Sphincter");
			CooldownMyActivatedAbility(ActivatedAbilityID, 10);
			SlimeGlands.SlimeAnimation("&w", ParentObject.CurrentCell, list[0]);
			List<LiquidVolume> list2 = new List<LiquidVolume>();
			int num = 0;
			foreach (Cell item2 in list)
			{
				if (num != 0 && !80.in100())
				{
					continue;
				}
				GameObject gameObject = GameObject.Create("Water");
				LiquidVolume liquidVolume2 = gameObject.LiquidVolume;
				liquidVolume2.ComponentLiquids.Clear();
				foreach (KeyValuePair<string, int> componentLiquid in liquidVolume.ComponentLiquids)
				{
					liquidVolume2.ComponentLiquids.Add(componentLiquid.Key, componentLiquid.Value);
				}
				list2.Add(liquidVolume2);
				item2.AddObject(gameObject);
				num++;
			}
			if (liquidVolume.Volume < list2.Count)
			{
				liquidVolume.MixWith(new LiquidVolume("slime", list2.Count - liquidVolume.Volume));
			}
			foreach (LiquidVolume item3 in list2)
			{
				item3.Volume = Math.Max(Math.Min(1000, liquidVolume.Volume) / num, 1);
				item3.Update();
			}
			liquidVolume.UseDrams(1000);
			foreach (Cell item4 in list)
			{
				foreach (GameObject item5 in item4.GetObjectsWithPart("Combat"))
				{
					if (item5 != ParentObject && !item5.MakeSave("Agility,Strength", statValue, null, null, "SlogGlands Knockdown"))
					{
						item5.ApplyEffect(new Prone());
					}
				}
			}
			DidX("spew", "a pool of stinking liquid", "!", null, null, ParentObject);
		}
		return base.FireEvent(E);
	}

	public void AddSphincterTo(BodyPart Part)
	{
		GameObject obj = Part.FindEquipmentOrDefaultByBlueprint("Bilge Sphincter");
		if (obj == null)
		{
			obj = GameObject.Create("Bilge Sphincter");
			obj.GetPart<Armor>().WornOn = Part.Type;
			obj.RequirePart<SlogGladsItem>();
		}
		bool flag = obj.EquipAsDefaultBehavior();
		if (flag && Part.DefaultBehavior != null)
		{
			if (Part.DefaultBehavior == obj)
			{
				return;
			}
			Part.DefaultBehavior = null;
		}
		if (!flag && Part.Equipped != null)
		{
			if (Part.Equipped == obj)
			{
				return;
			}
			if (Part.Equipped.CanBeUnequipped(null, null, Forced: false, SemiForced: true))
			{
				Part.ForceUnequip(Silent: true);
			}
		}
		if (!Part.Equip(obj, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true))
		{
			CleanUpMutationEquipment(ParentObject, ref obj);
		}
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (ParentObject.Body != null)
		{
			BodyPart bodyPart = Stinger.AddTail(GO, "Mutation::SlogGlands", ParentObject.Body.Anatomy.StartsWith("Slug"));
			if (bodyPart != null)
			{
				AddSphincterTo(bodyPart);
				ActivatedAbilityID = AddMyActivatedAbility("Spew", "CommandSlog", "Physical Mutations", "You slurp up nearby liquids and spew them with your bilge sphincter, occasionally knocking enemies prone.", "\u00ad");
			}
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		CleanUpMutationEquipment(GO, FindBilgeSphincter());
		Stinger.RemoveTail(GO, "Mutation::SlogGlands");
		return base.Unmutate(GO);
	}
}
