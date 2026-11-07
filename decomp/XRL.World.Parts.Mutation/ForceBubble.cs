using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ForceBubble : BaseMutation
{
	public static readonly string COMMAND_NAME = "CommandForceBubble";

	public string Blueprint = "Forcefield";

	public bool IsRealityDistortionBased = true;

	public int Duration;

	[NonSerialized]
	public Dictionary<string, GameObject> CurrentField = new Dictionary<string, GameObject>(8);

	[NonSerialized]
	public static List<string> RemovalList = new List<string>();

	public ForceBubble()
	{
		Type = "Mental";
	}

	public void Validate()
	{
		if (CurrentField.Count <= 0)
		{
			return;
		}
		RemovalList.Clear();
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			if (!GameObject.Validate(item.Value))
			{
				RemovalList.Add(item.Key);
			}
		}
		foreach (string removal in RemovalList)
		{
			CurrentField.Remove(removal);
		}
		if (CurrentField.Count == 0)
		{
			DestroyBubble(Validated: true);
		}
	}

	public bool IsActive()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			return CurrentField.Count > 0;
		}
		return false;
	}

	public bool IsSuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count <= 0)
			{
				return false;
			}
			foreach (GameObject value in CurrentField.Values)
			{
				if (value.CurrentCell != null)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool IsAnySuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count <= 0)
			{
				return false;
			}
			foreach (GameObject value in CurrentField.Values)
			{
				if (value.CurrentCell == null)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void DestroyBubble(bool Validated = false)
	{
		if (!Validated)
		{
			Validate();
		}
		MyActivatedAbility(ActivatedAbilityID).ToggleState = false;
		foreach (GameObject value in CurrentField.Values)
		{
			value.Obliterate();
		}
		CurrentField.Clear();
		if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("The {{B|force bubble}} snaps off.");
		}
		else if (IComponent<GameObject>.Visible(ParentObject))
		{
			IComponent<GameObject>.AddPlayerMessage("The {{B|force bubble}} around " + ParentObject.t() + " snaps off.");
		}
	}

	public int GetPushForce()
	{
		return 5000 + Level * 500;
	}

	public int CreateBubble()
	{
		Validate();
		int num = 0;
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return num;
		}
		if (cell.ParentZone.IsWorldMap())
		{
			return num;
		}
		string[] directionList = Directions.DirectionList;
		foreach (string text in directionList)
		{
			Cell cellFromDirection = cell.GetCellFromDirection(text, BuiltOnly: false);
			if (CurrentField.ContainsKey(text))
			{
				GameObject gameObject = CurrentField[text];
				if (gameObject.CurrentCell == cellFromDirection)
				{
					continue;
				}
				gameObject.Obliterate();
				CurrentField.Remove(text);
			}
			if (cellFromDirection != null && IsRealityDistortionBased && !IComponent<GameObject>.CheckRealityDistortionAccessibility(null, cellFromDirection, ParentObject, null, this))
			{
				continue;
			}
			GameObject gameObject2 = GameObject.Create(Blueprint);
			Forcefield part = gameObject2.GetPart<Forcefield>();
			if (part != null)
			{
				part.Creator = ParentObject;
				part.MovesWithOwner = true;
				part.RejectOwner = false;
			}
			ExistenceSupport existenceSupport = gameObject2.RequirePart<ExistenceSupport>();
			existenceSupport.SupportedBy = ParentObject;
			existenceSupport.ValidateEveryTurn = true;
			Phase.carryOver(ParentObject, gameObject2);
			CurrentField.Add(text, gameObject2);
			cellFromDirection?.AddObject(gameObject2);
			if (cellFromDirection == null || gameObject2.CurrentCell != cellFromDirection)
			{
				continue;
			}
			num++;
			if (The.Game.InForceFieldPush)
			{
				continue;
			}
			The.Game.InForceFieldPush = true;
			try
			{
				foreach (GameObject item in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
				{
					if (item != gameObject2 && !item.HasPart<Forcefield>() && !item.HasPart<HologramMaterial>() && item.ConsiderSolidFor(gameObject2) && gameObject2.ConsiderSolidFor(item))
					{
						item.Push(text, GetPushForce(), 4);
					}
				}
				foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
				{
					if (item2 != gameObject2 && !item2.HasPart<Forcefield>() && !item2.HasPart<HologramMaterial>() && (part == null || !part.CanPass(item2)) && item2.PhaseMatches(gameObject2))
					{
						item2.Push(text, GetPushForce(), 4);
					}
				}
			}
			finally
			{
				The.Game.InForceFieldPush = false;
			}
		}
		IComponent<GameObject>.EmitMessage(ParentObject, "A {{B|force bubble}} pops into being around " + ParentObject.t() + ".", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, ParentObject);
		return num;
	}

	public void SuspendBubble()
	{
		Validate();
		foreach (GameObject value in CurrentField.Values)
		{
			value.RemoveFromContext();
		}
	}

	public void DesuspendBubble(bool Validated = false)
	{
		if (!Validated)
		{
			Validate();
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			DestroyBubble();
		}
		else
		{
			if (cell.ParentZone != null && cell.ParentZone.IsWorldMap())
			{
				return;
			}
			RemovalList.Clear();
			foreach (KeyValuePair<string, GameObject> item in CurrentField)
			{
				string key = item.Key;
				GameObject value = item.Value;
				if (value.CurrentCell != null)
				{
					continue;
				}
				Cell cellFromDirection = cell.GetCellFromDirection(key, BuiltOnly: false);
				if (cellFromDirection == null)
				{
					continue;
				}
				cellFromDirection.AddObject(value);
				Forcefield part = value.GetPart<Forcefield>();
				if (value.CurrentCell == cellFromDirection)
				{
					if (The.Game.InForceFieldPush)
					{
						continue;
					}
					The.Game.InForceFieldPush = true;
					try
					{
						foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
						{
							if (item2 != value && !item2.HasPart<Forcefield>() && !item2.HasPart<HologramMaterial>() && item2.ConsiderSolidFor(value) && value.ConsiderSolidFor(item2))
							{
								item2.Push(key, GetPushForce(), 4);
							}
						}
						foreach (GameObject item3 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
						{
							if (item3 != value && !item3.HasPart<Forcefield>() && !item3.HasPart<HologramMaterial>() && (part == null || !part.CanPass(item3)) && item3.PhaseMatches(value))
							{
								item3.Push(key, GetPushForce(), 4);
							}
						}
					}
					finally
					{
						The.Game.InForceFieldPush = false;
					}
				}
				else
				{
					value.Obliterate();
					RemovalList.Add(key);
				}
			}
			foreach (string removal in RemovalList)
			{
				CurrentField.Remove(removal);
			}
		}
	}

	public void MaintainBubble()
	{
		Phase.syncPrep(ParentObject, out var FX, out var FX2);
		foreach (GameObject value in CurrentField.Values)
		{
			Phase.sync(ParentObject, value, FX, FX2);
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(CurrentField.Count);
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			Writer.Write(item.Key);
			Writer.WriteGameObject(item.Value);
		}
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		CurrentField.Clear();
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			GameObject value = Reader.ReadGameObject("forcebubble");
			CurrentField.Add(key, value);
		}
		base.Read(Basis, Reader);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EnteredCellEvent.ID && ID != PooledEvent<GetPartyLeaderFollowDistanceEvent>.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !IsMyActivatedAbilityToggledOn(ActivatedAbilityID) && (!IsRealityDistortionBased || CheckMyRealityDistortionAdvisability()) && !ParentObject.CurrentCell.AnyAdjacentCell((Cell c) => ParentObject.IsAlliedTowards(c.GetCombatObject())))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (IsActive())
			{
				DestroyBubble();
			}
			else
			{
				if (IsRealityDistortionBased && !ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
				{
					return false;
				}
				CooldownMyActivatedAbility(ActivatedAbilityID, 100);
				MyActivatedAbility(ActivatedAbilityID).ToggleState = true;
				Duration = 9 + Level * 3 + 1;
				CreateBubble();
				UseEnergy(1000, "Mental Mutation ForceBubble");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		DesuspendBubble();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPartyLeaderFollowDistanceEvent E)
	{
		if (IsActive())
		{
			E.MinDistance(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (CurrentField.ContainsValue(E.Object))
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && cell.DistanceTo(E.Object) <= 1)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (IsActive())
		{
			DesuspendBubble(Validated: true);
			Duration--;
			if (Duration <= 0)
			{
				DestroyBubble();
			}
			else
			{
				MaintainBubble();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		DestroyBubble();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginMove");
		Registrar.Register("MoveFailed");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You generate a forcefield around yourself.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Creates a 3x3 forcefield centered on yourself\n" + "Duration: {{rules|" + GetDuration(Level) + "}} rounds\n", "Cooldown: 100 rounds\n"), "You may fire missile weapons through the forcefield.");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Duration", GetDuration(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			SuspendBubble();
		}
		else if (E.ID == "MoveFailed")
		{
			DesuspendBubble();
		}
		return base.FireEvent(E);
	}

	public int GetCooldown(int Level)
	{
		return 100;
	}

	public int GetDuration(int Level)
	{
		return 9 + Level * 3 + 1;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Force Bubble", COMMAND_NAME, "Mental Mutations", null, "\t", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
