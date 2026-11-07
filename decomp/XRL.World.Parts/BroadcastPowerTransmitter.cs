using System;

namespace XRL.World.Parts;

[Serializable]
public class BroadcastPowerTransmitter : IPoweredPart
{
	public int TransmitRate;

	public BroadcastPowerTransmitter()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as BroadcastPowerTransmitter).TransmitRate != TransmitRate)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CollectBroadcastChargeEvent>.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GetBasisZone()?.RequirePart<BroadcastPowerField>().RegisterTransmitter(ParentObject);
		return true;
	}

	public override bool HandleEvent(CollectBroadcastChargeEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = Math.Min(E.Charge, ParentObject.QueryCharge(LiveOnly: false, 0L) - ChargeUse);
			if (TransmitRate != 0 && num > TransmitRate)
			{
				num = TransmitRate;
			}
			if (num > 0)
			{
				ParentObject.UseCharge(num + ChargeUse, LiveOnly: false, 0L);
				E.Charge -= num;
				if (num >= E.Charge)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override int GetDraw(QueryDrawEvent E = null)
	{
		int num = base.GetDraw(E);
		Zone anyBasisZone = GetAnyBasisZone();
		if (anyBasisZone != null && anyBasisZone.IsActive() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			bool flag = false;
			int num2 = 0;
			int num3 = 0;
			if (E != null)
			{
				flag = E.BroadcastDrawDone;
				num2 = (flag ? E.BroadcastDraw : 0);
				num3 = E.HighTransmitRate;
			}
			if (!flag || (num3 > 0 && num3 < TransmitRate))
			{
				if (!flag)
				{
					num2 = QueryBroadcastDrawEvent.GetFor(anyBasisZone);
					if (E != null)
					{
						E.BroadcastDraw = num2;
					}
				}
				int num4 = num2 - num3;
				if (num4 > 0)
				{
					if (TransmitRate > 0 && num4 > TransmitRate)
					{
						num4 = TransmitRate;
					}
					num += num4;
					if (E != null && (TransmitRate <= 0 || TransmitRate > num3))
					{
						E.HighTransmitRate = num3;
					}
				}
			}
		}
		return num;
	}
}
