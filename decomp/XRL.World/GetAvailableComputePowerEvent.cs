using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Singleton)]
public class GetAvailableComputePowerEvent : SingletonEvent<GetAvailableComputePowerEvent>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Actor;

	public int Amount;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Amount = 0;
	}

	public static int GetFor(GameObject Actor)
	{
		if (GameObject.Validate(ref Actor))
		{
			SingletonEvent<GetAvailableComputePowerEvent>.Instance.Actor = Actor;
			SingletonEvent<GetAvailableComputePowerEvent>.Instance.Amount = 0;
			Actor.HandleEvent(SingletonEvent<GetAvailableComputePowerEvent>.Instance);
			return SingletonEvent<GetAvailableComputePowerEvent>.Instance.Amount;
		}
		return 0;
	}

	public static int GetFor(IActivePart Part)
	{
		int num = 0;
		if (Part.ActivePartHasMultipleSubjects())
		{
			List<GameObject> activePartSubjects = Part.GetActivePartSubjects();
			int i = 0;
			for (int count = activePartSubjects.Count; i < count; i++)
			{
				num += GetFor(activePartSubjects[i]);
			}
		}
		else
		{
			GameObject activePartFirstSubject = Part.GetActivePartFirstSubject();
			if (activePartFirstSubject != null)
			{
				num += GetFor(activePartFirstSubject);
			}
		}
		return num;
	}

	public static int AdjustUp(GameObject Actor, int Amount)
	{
		if (Amount != 0)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = Amount * (100 + num) / 100;
			}
		}
		return Amount;
	}

	public static int AdjustUp(GameObject Actor, int Amount, float Factor)
	{
		if (Amount != 0 && Factor != 0f)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = (int)((float)Amount * (100f + (float)num * Factor) / 100f);
			}
		}
		return Amount;
	}

	public static float AdjustUp(GameObject Actor, float Amount)
	{
		if (Amount != 0f)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = Amount * (float)(100 + num) / 100f;
			}
		}
		return Amount;
	}

	public static float AdjustUp(GameObject Actor, float Amount, float Factor)
	{
		if (Amount != 0f && Factor != 0f)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = Amount * (100f + (float)num * Factor) / 100f;
			}
		}
		return Amount;
	}

	public static int AdjustDown(GameObject Actor, int Amount, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = Math.Max(Amount * (100 - num) / 100, Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static int AdjustDown(GameObject Actor, int Amount, float Factor, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = Math.Max((int)((float)Amount * (100f - (float)num * Factor) / 100f), Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(GameObject Actor, float Amount, int FloorDivisor = 2)
	{
		if (Amount != 0f)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = Math.Max(Amount * (float)(100 - num) / 100f, Amount / (float)FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(GameObject Actor, float Amount, float Factor, int FloorDivisor = 2)
	{
		if (Amount != 0f)
		{
			int num = GetFor(Actor);
			if (num != 0)
			{
				Amount = Math.Max(Amount * (100f - (float)num * Factor) / 100f, Amount / (float)FloorDivisor);
			}
		}
		return Amount;
	}

	public static int AdjustUp(IActivePart Part, int Amount)
	{
		if (Amount != 0)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = Amount * (100 + num) / 100;
			}
		}
		return Amount;
	}

	public static int AdjustUp(IActivePart Part, int Amount, float Factor)
	{
		if (Amount != 0 && Factor != 0f)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = (int)((float)Amount * (100f + (float)num * Factor) / 100f);
			}
		}
		return Amount;
	}

	public static float AdjustUp(IActivePart Part, float Amount)
	{
		if (Amount != 0f)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = Amount * (float)(100 + num) / 100f;
			}
		}
		return Amount;
	}

	public static float AdjustUp(IActivePart Part, float Amount, float Factor)
	{
		if (Amount != 0f && Factor != 0f)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = Amount * (100f + (float)num * Factor) / 100f;
			}
		}
		return Amount;
	}

	public static int AdjustDown(IActivePart Part, int Amount, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = Math.Max(Amount * (100 - num) / 100, Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static int AdjustDown(IActivePart Part, int Amount, float Factor, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = Math.Max((int)((float)Amount * (100f - (float)num * Factor) / 100f), Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(IActivePart Part, float Amount, float FloorDivisor = 2f)
	{
		if (Amount != 0f)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = Math.Max(Amount * (float)(100 - num) / 100f, Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(IActivePart Part, float Amount, float Factor, float FloorDivisor = 2f)
	{
		if (Amount != 0f)
		{
			int num = GetFor(Part);
			if (num != 0)
			{
				Amount = Math.Max(Amount * (100f - (float)num * Factor) / 100f, Amount / FloorDivisor);
			}
		}
		return Amount;
	}
}
