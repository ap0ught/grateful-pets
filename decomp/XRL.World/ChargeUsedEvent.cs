using System.Collections.Generic;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ChargeUsedEvent : IChargeConsumptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ChargeUsedEvent), null, CountPool, ResetPool);

	private static List<ChargeUsedEvent> Pool;

	private static int PoolCounter;

	public int DesiredAmount;

	public ChargeUsedEvent()
	{
		base.ID = ID;
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

	public static void ResetTo(ref ChargeUsedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ChargeUsedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		DesiredAmount = 0;
	}

	public static void Send(GameObject Object, GameObject Source, int Amount, int DesiredAmount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true, int PowerLoadLevel = 100)
	{
		if (Wanted(Object))
		{
			ChargeUsedEvent chargeUsedEvent = FromPool();
			chargeUsedEvent.Source = Source;
			chargeUsedEvent.Amount = Amount;
			chargeUsedEvent.DesiredAmount = DesiredAmount;
			chargeUsedEvent.Multiple = Multiple;
			chargeUsedEvent.GridMask = GridMask;
			chargeUsedEvent.Forced = Forced;
			chargeUsedEvent.LiveOnly = LiveOnly;
			chargeUsedEvent.IncludeTransient = IncludeTransient;
			chargeUsedEvent.IncludeBiological = IncludeBiological;
			chargeUsedEvent.PowerLoadLevel = PowerLoadLevel;
			Process(Object, chargeUsedEvent);
		}
		if (PowerLoadLevel <= 100)
		{
			return;
		}
		int num = GetOverloadChargeEvent.GetFor(Object, Amount);
		if (num > 0)
		{
			GameObject holder = Object.Holder;
			Object.TemperatureChange(1 + num / 100, holder);
			holder?.TemperatureChange(1 + num / 100, holder);
			if ((1 + num / 10).in10000() && Object.ApplyEffect(new Broken(FromDamage: false, FromExamine: false, FromOverload: true)))
			{
				Messaging.XDidY(Object, "overheat", null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, holder, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: false, holder);
			}
		}
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("ChargeUsed");
		}
		return true;
	}

	public static bool Process(GameObject Object, ChargeUsedEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "ChargeUsed"))
		{
			return false;
		}
		if (!Object.HandleEvent(E))
		{
			return false;
		}
		return true;
	}
}
