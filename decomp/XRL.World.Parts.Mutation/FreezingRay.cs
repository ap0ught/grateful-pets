using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FreezingRay : BaseDefaultEquipmentMutation
{
	public static readonly string COMMAND_NAME = "CommandFreezingRay";

	public static readonly string PROJECTILE_BLUEPRINT = "ProjectileFreezingRay";

	public static readonly int RANGE = 9;

	public string BodyPartType;

	public bool CreateObject = true;

	public int OldFreeze = -1;

	public int OldBrittle = -1;

	public string Sound = "Sounds/Abilities/sfx_ability_mutation_freezingRay_attack";

	[NonSerialized]
	private static GameObject _Projectile;

	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.Validate(ref _Projectile))
			{
				_Projectile = GameObject.CreateUnmodified(PROJECTILE_BLUEPRINT);
			}
			return _Projectile;
		}
	}

	public override bool UseVariantName => false;

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override string GetCreateCharacterDisplayName()
	{
		if (BodyPartType != null)
		{
			return GetDisplayName() + " (" + BodyPartType + ")";
		}
		return GetDisplayName();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (CheckObjectProperlyEquipped() && E.Distance <= RANGE && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (OnWorldMap)
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			if (!CheckObjectProperlyEquipped())
			{
				BodyPart firstBodyPart = ParentObject.GetFirstBodyPart(BodyPartType);
				if (firstBodyPart != null)
				{
					return ParentObject.Fail("Your " + firstBodyPart.GetOrdinalName() + " " + (firstBodyPart.Plural ? "are" : "is") + " too damaged to do that!");
				}
				return ParentObject.Fail("Your " + BodyPartType + " is too damaged to do that!");
			}
			if (!Cast(this))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("EmitText", GetDescription());
		stats.Set("Range", RANGE);
		stats.Set("Damage", ComputeDamage(), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 20);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("ice", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		BodyPart registeredSlot = GetRegisteredSlot(BodyPartType, evenIfDismembered: true);
		if (registeredSlot != null)
		{
			return "You emit a ray of frost from your " + registeredSlot.GetOrdinalName() + ".";
		}
		return "You emit a ray of frost.";
	}

	public override string GetLevelText(int Level)
	{
		int rANGE = RANGE;
		return string.Concat(string.Concat(string.Concat("Emits a " + rANGE + "-square ray of frost in the direction of your choice.\n", "Damage: {{rules|", ComputeDamage(Level), "}}\n"), "Cooldown: 20 rounds\n"), "Melee attacks cool opponents by {{rules|", GetCoolOnHitAmount(Level), "}} degrees");
	}

	public string GetCoolOnHitAmount(int Level)
	{
		return "-" + Level + "d4";
	}

	public string ComputeDamage(int UseLevel)
	{
		string text = UseLevel + "d3";
		if (ParentObject != null)
		{
			int partCount = ParentObject.Body.GetPartCount(BodyPartType);
			if (partCount > 0)
			{
				text += partCount.Signed();
			}
		}
		else
		{
			text += "+1";
		}
		return text;
	}

	public string ComputeDamage()
	{
		return ComputeDamage(Level);
	}

	public void Freeze(Cell C, ref ScreenBuffer Buffer)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (item.PhaseMatches(ParentObject) && item.TemperatureChange(-120 - 7 * Level, ParentObject))
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
				}
			}
			foreach (GameObject item2 in C.GetObjectsWithPart("Combat"))
			{
				if (item2.PhaseMatches(ParentObject))
				{
					item2.TakeDamage(dice.RollCached(), "from %t freezing effect!", "Cold", null, null, null, ParentObject);
				}
			}
		}
		if (C.IsVisible())
		{
			if (Buffer == null)
			{
				Buffer = ScreenBuffer.GetScrapBuffer1();
				XRLCore.Core.RenderMapToBuffer(Buffer);
			}
			Buffer.Goto(C.X, C.Y);
			string text = "&C";
			int num = Stat.Random(1, 3);
			if (num == 1)
			{
				text = "&C";
			}
			if (num == 2)
			{
				text = "&B";
			}
			if (num == 3)
			{
				text = "&Y";
			}
			int num2 = Stat.Random(1, 3);
			if (num2 == 1)
			{
				text += "^C";
			}
			if (num2 == 2)
			{
				text += "^B";
			}
			if (num2 == 3)
			{
				text += "^Y";
			}
			Buffer.Write(text + (char)(219 + Stat.Random(0, 4)));
			Popup._TextConsole.DrawBuffer(Buffer);
			Thread.Sleep(10);
		}
	}

	public static bool Cast(FreezingRay mutation = null, string level = "5-6")
	{
		if (mutation == null)
		{
			mutation = new FreezingRay();
			mutation.Level = level.RollCached();
			mutation.ParentObject = The.Player;
		}
		ScreenBuffer Buffer = null;
		List<Cell> list = mutation.PickLine(RANGE, AllowVis.Any, null, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, BlackoutStops: false, null, null, "Freezing Ray", Snap: true);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		if (list.Count == 1 && mutation.ParentObject.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target " + mutation.ParentObject.itself + "?") != DialogResult.Yes)
		{
			return false;
		}
		if (!TutorialManager.AllowTargetPick(mutation?.ParentObject, typeof(FreezingRay), list))
		{
			return false;
		}
		mutation.ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_freezingRay_attack");
		mutation.UseEnergy(1000, "Physical Mutation Freezing Hands");
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, 20);
		mutation.PlayWorldSound(mutation.Sound, 0.5f, 0f, Combat: true);
		int i = 0;
		for (int num = Math.Min(list.Count, 10); i < num; i++)
		{
			if (list.Count == 1 || list[i] != mutation.ParentObject.CurrentCell)
			{
				mutation.Freeze(list[i], ref Buffer);
			}
			if (i < num - 1 && list[i].IsSolidForProjectile(Projectile, mutation.ParentObject, null, mutation.ParentObject.Target))
			{
				break;
			}
		}
		BodyPart registeredSlot = mutation.GetRegisteredSlot(mutation.BodyPartType, evenIfDismembered: false);
		IComponent<GameObject>.XDidY(mutation.ParentObject, "emit", "a freezing ray" + ((registeredSlot != null) ? (" from " + mutation.ParentObject.its + " " + registeredSlot.GetOrdinalName()) : ""), "!", null, null, mutation.ParentObject);
		return true;
	}

	public bool CheckObjectProperlyEquipped()
	{
		if (!CreateObject)
		{
			return true;
		}
		if (HasRegisteredSlot(BodyPartType))
		{
			return GetRegisteredSlot(BodyPartType, evenIfDismembered: false) != null;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit")
		{
			if (!CheckObjectProperlyEquipped())
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null)
			{
				string coolOnHitAmount = GetCoolOnHitAmount(Level);
				int num = -10 * Level;
				int num2 = coolOnHitAmount.RollMaxCached();
				if ((num2 > 0 && gameObjectParameter.Physics.Temperature < num) || (num2 < 0 && gameObjectParameter.Physics.Temperature > num))
				{
					gameObjectParameter.TemperatureChange(coolOnHitAmount.RollCached(), E.GetGameObjectParameter("Attacker"), Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, ParentObject.GetPhase());
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.Physics.BrittleTemperature = -600 + -300 * Level;
		return base.ChangeLevel(NewLevel);
	}

	private void AddAbility()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Freezing Ray", COMMAND_NAME, "Physical Mutations", null, "*");
	}

	public override void SetVariant(string Variant)
	{
		base.SetVariant(Variant);
		BodyPartType = GameObjectFactory.Factory.Blueprints.GetValue(Variant)?.GetPartParameter("Armor", "WornOn", "Hands");
	}

	public override string GetVariantName(GameObjectBlueprint Blueprint)
	{
		return Blueprint?.GetPartParameter("Armor", "WornOn", "Hands");
	}

	public override IRenderable GetIcon()
	{
		if (!MutationFactory.TryGetMutationEntry(this, out var Entry))
		{
			return null;
		}
		return Entry.GetRenderable();
	}

	public override bool TryGetVariantValidity(GameObject Object, string Variant, out string Message)
	{
		string text = GameObjectFactory.Factory.Blueprints.GetValue(Variant)?.GetPartParameter("Armor", "WornOn", "Hands");
		if (!Object.Body.HasPart(text, EvenIfDismembered: true))
		{
			BodyPartType bodyPartTypeOrFail = Anatomies.GetBodyPartTypeOrFail(text);
			Message = ("Missing " + bodyPartTypeOrFail.Name).WithColor("R");
			return false;
		}
		return base.TryGetVariantValidity(Object, Variant, out Message);
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		base.OnRegenerateDefaultEquipment(body);
	}

	public void MakeFreezing(BodyPart part)
	{
		if (part == null)
		{
			return;
		}
		if (part.DefaultBehavior != null && part.DefaultBehavior.Blueprint != Variant)
		{
			part.DefaultBehavior.RequirePart<Icy>();
		}
		if (part.Parts != null)
		{
			for (int i = 0; i < part.Parts.Count; i++)
			{
				MakeFreezing(part.Parts[i]);
			}
		}
	}

	public override void OnDecorateDefaultEquipment(Body Body)
	{
		if (CreateObject)
		{
			GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(Variant);
			BodyPartType = blueprint.GetPartParameter("Armor", "WornOn", "Hands");
			if (!TryGetRegisteredSlot(Body, BodyPartType, out var Part, EvenIfDismembered: true))
			{
				Part = Body.GetFirstPart(BodyPartType);
				if (Part != null)
				{
					RegisterSlot(BodyPartType, Part);
				}
			}
			if (Part != null && Part.DefaultBehavior == null)
			{
				GameObject gameObject = GameObject.Create(blueprint);
				gameObject.GetPart<Armor>().WornOn = BodyPartType;
				Part.DefaultBehavior = gameObject;
				Part.DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "FreezingRay");
			}
			MakeFreezing(Part);
			if (BodyPartType == "Hands")
			{
				foreach (BodyPart item in Body.GetPart("Hand"))
				{
					MakeFreezing(item);
				}
			}
		}
		base.OnDecorateDefaultEquipment(Body);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (Variant.IsNullOrEmpty())
		{
			List<string> variants = GetVariants();
			Variant = variants.GetRandomElement();
			if (!BodyPartType.IsNullOrEmpty())
			{
				foreach (string item in variants)
				{
					if (GameObjectFactory.Factory.GetBlueprint(item).GetPartParameter("Armor", "WornOn", "") == BodyPartType)
					{
						Variant = item;
						break;
					}
				}
			}
		}
		if (GO.Physics != null)
		{
			OldFreeze = GO.Physics.FreezeTemperature;
			OldBrittle = GO.Physics.BrittleTemperature;
		}
		AddAbility();
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (GO.Physics != null)
		{
			if (OldFreeze != -1)
			{
				GO.Physics.FreezeTemperature = OldFreeze;
			}
			if (OldBrittle != -1)
			{
				GO.Physics.BrittleTemperature = OldBrittle;
			}
			OldFreeze = -1;
			OldBrittle = -1;
			GO.Physics.Temperature = 25;
		}
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
