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
public class FreezeBreath : BaseMutation
{
	public const string MANAGER_ID = "Mutation::FreezeBreath";

	public const string ATTACH_PART = "Head";

	public string BodyPartType = "Face";

	public bool CreateObject = true;

	public int Range = 30;

	public GameObject VaporObject;

	public int OldFreeze = -1;

	public int OldBrittle = -1;

	public string Sound = "hiss_high";

	public Guid FreezeBreathActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private static GameObject _Projectile;

	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.Validate(ref _Projectile))
			{
				_Projectile = GameObject.CreateUnmodified("ProjectileFreezingRay");
			}
			return _Projectile;
		}
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(FreezeBreathActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= Range && IsMyActivatedAbilityAIUsable(FreezeBreathActivatedAbilityID) && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add("CommandFreezeBreath");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("ice", 3);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandFreezeBreath");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You emit jets of frost from your mouth.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("Emits a " + Range + "-square ray of frost in the direction of your choice\n", "Cooldown: 30 rounds\n"), "Damage: ", ComputeDamage(Level), "\n"), "Cannot wear face accessories");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("EmitText", GetDescription());
		stats.Set("Range", Range);
		stats.Set("Damage", ComputeDamage(), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 30);
	}

	public string ComputeDamage(int UseLevel)
	{
		string text = UseLevel + "d4";
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

	public void Freeze(Cell C, ScreenBuffer Buffer)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (item.PhaseMatches(ParentObject) && item.TemperatureChange(-20 - 60 * Level, ParentObject))
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
					item2.TakeDamage(dice.RollCached(), "from %t freeze!", "Cold", null, null, null, ParentObject);
				}
			}
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
		if (C.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone)
		{
			Stat.Random(1, 3);
			Buffer.Write(text + (char)(219 + Stat.Random(0, 4)));
			Popup._TextConsole.DrawBuffer(Buffer);
			Thread.Sleep(10);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFreezeBreath")
		{
			if (OnWorldMap)
			{
				ParentObject.ShowFailure("You cannot do that on the world map.");
				return false;
			}
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			List<Cell> list = PickLine(Range, AllowVis.Any, null, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, BlackoutStops: false, null, null, "Freezing Ultraray", Snap: true);
			if (list == null || list.Count <= 0)
			{
				return false;
			}
			if (list.Count == 1 && ParentObject.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target " + ParentObject.itself + "?") != DialogResult.Yes)
			{
				return false;
			}
			UseEnergy(1000, "Physical Mutation Freeze Breath");
			CooldownMyActivatedAbility(FreezeBreathActivatedAbilityID, 30);
			PlayWorldSound(Sound, 0.5f, 0f, Combat: true);
			int i = 0;
			for (int num = Math.Min(list.Count, Range); i < num; i++)
			{
				if (list.Count == 1 || list[i] != ParentObject.CurrentCell)
				{
					Freeze(list[i], scrapBuffer);
				}
				if (i < num - 1 && list[i].IsSolidForProjectile(Projectile, ParentObject, null, ParentObject.Target))
				{
					break;
				}
			}
		}
		IComponent<GameObject>.XDidY(ParentObject, "emit", "a vast freezing ray from " + ParentObject.its + " mouth", "!", null, null, ParentObject);
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.Physics.BrittleTemperature = -600 + -300 * Level;
		if (GameObject.Validate(ref VaporObject))
		{
			TemperatureOnHit temperatureOnHit = VaporObject.RequirePart<TemperatureOnHit>();
			if (temperatureOnHit != null)
			{
				temperatureOnHit.Amount = "-" + Level + "d4";
			}
		}
		return base.ChangeLevel(NewLevel);
	}

	private void AddAbility()
	{
		FreezeBreathActivatedAbilityID = AddMyActivatedAbility("Freezing Ultraray", "CommandFreezeBreath", "Physical Mutations", null, "*");
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.Physics != null)
		{
			OldFreeze = GO.Physics.FreezeTemperature;
			OldBrittle = GO.Physics.BrittleTemperature;
		}
		if (CreateObject)
		{
			Body body = GO.Body;
			if (body != null)
			{
				BodyPart bodyPart = body.GetFirstPart(BodyPartType);
				if (bodyPart == null || (bodyPart.Equipped != null && !bodyPart.Equipped.CanBeUnequipped(null, null, Forced: false, SemiForced: true)))
				{
					BodyPart bodyPart2 = body.GetFirstPart("Head") ?? body.GetBody();
					string bodyPartType = BodyPartType;
					int? category = bodyPart2.Category;
					string[] orInsertBefore = new string[8] { "Back", "Arm", "Leg", "Foot", "Hands", "Feet", "Roots", "Thrown Weapon" };
					bodyPart = bodyPart2.AddPartAt(bodyPartType, 0, null, null, null, null, "Mutation::FreezeBreath", category, null, null, null, null, null, null, null, null, null, null, null, null, "Head", orInsertBefore);
				}
				if (bodyPart == null)
				{
					return false;
				}
				bodyPart.ForceUnequip(Silent: true);
				VaporObject = GameObjectFactory.Factory.CreateObject("Icy Vapor");
				VaporObject.GetPart<Armor>().WornOn = bodyPart.Type;
				GO.ForceEquipObject(VaporObject, bodyPart, Silent: true, 0);
				AddAbility();
			}
		}
		else
		{
			AddAbility();
		}
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
		CleanUpMutationEquipment(GO, ref VaporObject);
		RemoveMyActivatedAbility(ref FreezeBreathActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
