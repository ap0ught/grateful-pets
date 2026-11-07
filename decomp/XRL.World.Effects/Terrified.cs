using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Effects;

[Serializable]
public class Terrified : Effect, ITierInitialized
{
	public string TerrifiedOfID;

	public GlobalLocation TerrifiedOfLocation;

	public bool Silent;

	public bool Psionic;

	[NonSerialized]
	private IMovementGoal Goal;

	[NonSerialized]
	private GameObject TerrifiedOfObject;

	public Terrified()
	{
		DisplayName = "{{W|terrified}}";
	}

	public Terrified(int Duration)
		: this()
	{
		base.Duration = Duration + 1;
	}

	public Terrified(int Duration, GameObject TerrifiedOf, bool Psionic = false, bool Silent = false)
		: this(Duration)
	{
		TerrifiedOfID = TerrifiedOf.ID;
		TerrifiedOfObject = TerrifiedOf;
		this.Psionic = Psionic;
		this.Silent = Silent;
	}

	public Terrified(int Duration, GlobalLocation TerrifiedOfLocation, bool Psionic = false, bool Silent = false)
		: this(Duration)
	{
		this.TerrifiedOfLocation = TerrifiedOfLocation;
		this.Psionic = Psionic;
		this.Silent = Silent;
	}

	public Terrified(int Duration, Cell TerrifiedOfCell)
		: this(Duration, new GlobalLocation(TerrifiedOfCell))
	{
	}

	public Terrified(int Duration, Cell TerrifiedOfCell, bool Psionic = false, bool Silent = false)
		: this(Duration, TerrifiedOfCell)
	{
		this.Psionic = Psionic;
		this.Silent = Silent;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(5, 10);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		int num = 117440514;
		if (Psionic)
		{
			num |= 0x8000;
		}
		return num;
	}

	public override string GetDescription()
	{
		return "{{W|terrified}}";
	}

	public override string GetDetails()
	{
		return "Fleeing in panic.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyFear") || !Object.FireEvent("ApplyFear"))
		{
			return false;
		}
		Object.RemoveEffect<Terrified>();
		if (!CheckGoal())
		{
			return false;
		}
		Object.ParticleText("&W!");
		if (!Silent)
		{
			DidX("are", "overwhelmed with terror", "!", null, null, null, Object);
		}
		Object.FireEvent("FearApplied");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Goal?.FailToParent();
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			CheckGoal();
		}
		return base.HandleEvent(E);
	}

	private bool CheckGoal()
	{
		if (Goal != null)
		{
			return true;
		}
		return SetUpGoal();
	}

	private bool SetUpGoal()
	{
		if (!SetUpObjectGoal())
		{
			return SetUpLocationGoal();
		}
		return true;
	}

	private bool SetUpObjectGoal()
	{
		if (TerrifiedOfID.IsNullOrEmpty())
		{
			Cell cell = Object.CurrentCell;
			List<GameObject> list = cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 6, "Physics", Object);
			if (list.Count <= 0)
			{
				return false;
			}
			TerrifiedOfID = list.GetRandomElement().ID;
		}
		if (!GameObject.Validate(ref TerrifiedOfObject))
		{
			TerrifiedOfObject = GameObject.FindByID(TerrifiedOfID);
		}
		if (TerrifiedOfObject == null)
		{
			return false;
		}
		if (Object.Brain == null)
		{
			return false;
		}
		Goal = new Flee(TerrifiedOfObject, -1, Panicked: true);
		Object.Brain.PushGoal(Goal);
		return true;
	}

	private bool SetUpLocationGoal()
	{
		if (TerrifiedOfLocation == null)
		{
			return false;
		}
		if (Object.Brain == null)
		{
			return false;
		}
		Cell cell = TerrifiedOfLocation.ResolveCell();
		if (cell == null)
		{
			return false;
		}
		Goal = new FleeLocation(cell, -1, Panicked: true);
		Object.Brain.PushGoal(Goal);
		return true;
	}

	public static bool OfAttacker(MentalAttackEvent E)
	{
		return Attack(E, E.Attacker);
	}

	public static bool OfCell(MentalAttackEvent E)
	{
		return Attack(E, null, E.Attacker.CurrentCell);
	}

	public static bool Attack(MentalAttackEvent E, GameObject Object = null, Cell Cell = null, bool Psionic = false, bool Silent = false)
	{
		if (E.Penetrations > 0)
		{
			if (Object != null)
			{
				Terrified e = new Terrified(E.Magnitude, Object, Psionic, Silent);
				if (E.Defender.ApplyEffect(e))
				{
					return true;
				}
			}
			else if (Cell != null)
			{
				Terrified e2 = new Terrified(E.Magnitude, Cell, Psionic, Silent);
				if (E.Defender.ApplyEffect(e2))
				{
					return true;
				}
			}
		}
		IComponent<GameObject>.XDidY(E.Defender, "resist", "becoming afraid", null, null, null, E.Defender);
		return false;
	}
}
