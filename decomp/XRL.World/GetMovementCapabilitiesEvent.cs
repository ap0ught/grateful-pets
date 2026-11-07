using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 273)]
public class GetMovementCapabilitiesEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetMovementCapabilitiesEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 273;

	private static List<GetMovementCapabilitiesEvent> Pool;

	private static int PoolCounter;

	public GameObject Actor;

	public List<string> Descriptions = new List<string>();

	public List<string> Commands = new List<string>();

	public List<ActivatedAbilityEntry> Abilities = new List<ActivatedAbilityEntry>();

	public List<int> Order = new List<int>();

	public bool IncludeAttacks;

	public GetMovementCapabilitiesEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref GetMovementCapabilitiesEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetMovementCapabilitiesEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Descriptions.Clear();
		Commands.Clear();
		Abilities.Clear();
		Order.Clear();
		IncludeAttacks = false;
	}

	public void Add(string Description, string Command, int Order, ActivatedAbilityEntry Ability = null, bool IsAttack = false)
	{
		if (IsAttack && !IncludeAttacks)
		{
			return;
		}
		int num = -1;
		int i = 0;
		for (int count = this.Order.Count; i < count; i++)
		{
			if (this.Order[i] > Order)
			{
				num = i;
				break;
			}
			if (this.Order[i] == Order && Description.CompareTo(Descriptions[i]) <= 0)
			{
				num = i;
				break;
			}
		}
		if (Ability != null && !Ability.IsUsable)
		{
			if (IsAttack)
			{
				Description += " [attack]";
			}
			if (Ability.Toggleable)
			{
				Description = ((!Ability.ToggleState) ? (Description + " [toggled off]") : (Description + " [toggled on]"));
			}
			Description = Description.Color("K");
		}
		else
		{
			if (IsAttack)
			{
				Description += " {{W|[attack]}}";
			}
			if (Ability != null && Ability.Toggleable)
			{
				Description = ((!Ability.ToggleState) ? (Description + " [toggled off]") : (Description + " {{g|[toggled on]}}"));
			}
		}
		if (num == -1)
		{
			Descriptions.Add(Description);
			Commands.Add(Command);
			Abilities.Add(Ability);
			this.Order.Add(Order);
		}
		else
		{
			Descriptions.Insert(num, Description);
			Commands.Insert(num, Command);
			Abilities.Insert(num, Ability);
			this.Order.Insert(num, Order);
		}
	}

	public static GetMovementCapabilitiesEvent GetFor(GameObject Actor, bool IncludeAttacks = true)
	{
		GetMovementCapabilitiesEvent getMovementCapabilitiesEvent = null;
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			if (getMovementCapabilitiesEvent == null)
			{
				getMovementCapabilitiesEvent = FromPool();
			}
			getMovementCapabilitiesEvent.Actor = Actor;
			getMovementCapabilitiesEvent.IncludeAttacks = IncludeAttacks;
			Actor.HandleEvent(getMovementCapabilitiesEvent);
		}
		return getMovementCapabilitiesEvent;
	}
}
