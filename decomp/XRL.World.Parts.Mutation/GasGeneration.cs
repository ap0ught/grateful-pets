using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class GasGeneration : BaseMutation
{
	public string GasObject = "AcidGas";

	public int BillowsTimer = 20;

	public int GasObjectDensity = 800;

	[NonSerialized]
	private bool AddSeeping;

	[NonSerialized]
	private bool AlreadySeeping;

	[NonSerialized]
	private string Description;

	[NonSerialized]
	private string GasType;

	[NonSerialized]
	private string ReleaseAbilityCommand;

	public GasGeneration()
	{
		SyncFromBlueprint();
	}

	public GasGeneration(string GasObject)
	{
		this.GasObject = GasObject;
		SyncFromBlueprint();
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		GasGeneration obj = base.DeepCopy(Parent, MapInv) as GasGeneration;
		obj.SyncFromBlueprint();
		return obj;
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		SyncFromBlueprint();
	}

	private GameObjectBlueprint GeneratedGasBlueprint()
	{
		return GameObjectFactory.Factory.GetBlueprint(GasObject);
	}

	protected void SyncFromBlueprint()
	{
		ReleaseAbilityCommand = "GasGenerationCommand" + GasObject;
		GameObjectBlueprint gameObjectBlueprint = GeneratedGasBlueprint();
		SetDisplayName(gameObjectBlueprint.GetTag("GasGenerationName", null) ?? (GasObject + " Generation"));
		GasType = gameObjectBlueprint.GetPartParameter<string>("Gas", "GasType");
		AddSeeping = gameObjectBlueprint.GetTag("GasGenerationAddSeeping").EqualsNoCase("true");
		AlreadySeeping = gameObjectBlueprint.GetPartParameter("Gas", "Seeping", Default: false);
		string partParameter = gameObjectBlueprint.GetPartParameter<string>("Render", "DisplayName");
		if (!string.IsNullOrEmpty(partParameter))
		{
			Description = "You release a burst of " + partParameter + " around yourself.";
		}
		else
		{
			Description = "You release a gaseous burst around yourself.";
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CheckGasCanAffectEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add(GetReleaseAbilityCommand());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckGasCanAffectEvent E)
	{
		if (E.Gas.GasType == GasType)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(GetReleaseAbilityCommand());
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		stats.Set("Duration", GetReleaseDuration(Level), !flag);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetReleaseCooldown(Level));
	}

	public override string GetDescription()
	{
		return Description;
	}

	public virtual int GetReleaseDuration(int Level)
	{
		return 1 + Level / 2;
	}

	public virtual int GetReleaseCooldown(int Level)
	{
		return 40;
	}

	public virtual string GetReleaseAbilityName()
	{
		return "Gas Generation";
	}

	public virtual string GetReleaseAbilityCommand()
	{
		return ReleaseAbilityCommand;
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text = text + "Releases gas for {{rules|" + GetReleaseDuration(Level) + "}} rounds";
		if (Level != base.Level)
		{
			string tag = GeneratedGasBlueprint().GetTag("LevelEffectDescription");
			if (tag != null)
			{
				text = ((Level <= base.Level) ? (text + "\n{{rules|Decreased " + tag + "}}") : (text + "\n{{rules|Increased " + tag + "}}"));
			}
		}
		return text + "\nCooldown: " + GetReleaseCooldown(Level) + " rounds";
	}

	public override bool Render(RenderEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			string tag = GeneratedGasBlueprint().GetTag("ActivationColorString");
			if (tag != null)
			{
				E.ColorString = tag;
			}
		}
		return true;
	}

	public virtual int GetGasDensityForLevel(int Level)
	{
		return GasObjectDensity;
	}

	public virtual void PumpGas()
	{
		ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_gasMutation_passiveRelease");
		List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
		List<Cell> list = new List<Cell>(8);
		foreach (Cell item in adjacentCells)
		{
			if (AddSeeping || AlreadySeeping || !item.IsOccluding())
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			list.Add(ParentObject.CurrentCell);
		}
		Phase.carryOverPrep(ParentObject, out var FX, out var FX2);
		Event obj = Event.New("CreatorModifyGas", "Gas", (object)null);
		foreach (Cell item2 in list)
		{
			GameObject gameObject = GameObject.Create(GasObject);
			Gas part = gameObject.GetPart<Gas>();
			part.Creator = ParentObject;
			part.Density = GetGasDensityForLevel(Level) / list.Count;
			if (AddSeeping)
			{
				part.Seeping = true;
			}
			part.Level = Level;
			Phase.carryOver(ParentObject, gameObject, FX, FX2);
			obj.SetParameter("Gas", part);
			ParentObject.FireEvent(obj);
			item2.AddObject(gameObject);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				if (ParentObject.OnWorldMap())
				{
					BillowsTimer = -1;
				}
				else
				{
					BillowsTimer--;
				}
				if (BillowsTimer < 0)
				{
					ToggleMyActivatedAbility(ActivatedAbilityID);
					DidX("stop", "releasing " + GeneratedGasBlueprint().DisplayName());
				}
				else
				{
					PumpGas();
				}
			}
		}
		else if (E.ID == GetReleaseAbilityCommand())
		{
			if (ParentObject.OnWorldMap())
			{
				ParentObject.Fail("You cannot do that on the world map.");
				return false;
			}
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_gasMutation_activeRelease");
			UseEnergy(1000, "Physical Mutation Gas Generation");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetReleaseCooldown(Level));
			ToggleMyActivatedAbility(ActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				BillowsTimer = GetReleaseDuration(Level);
				PumpGas();
				DidX("start", "releasing " + GeneratedGasBlueprint().DisplayName());
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility(GetReleaseAbilityName(), GetReleaseAbilityCommand(), "Physical Mutations", null, "á", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
