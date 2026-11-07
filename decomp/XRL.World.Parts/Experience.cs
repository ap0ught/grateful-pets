using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class Experience : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AwardXPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AwardXPEvent E)
	{
		try
		{
			if (ParentObject.HasTagOrProperty("NoXPGain"))
			{
				return true;
			}
			if (AwardingXPEvent.Check(E, E.Actor))
			{
				int num = E.Amount;
				if (E.TierScaling && E.Tier >= 0)
				{
					int num2 = ((!ParentObject.HasStat("Level")) ? 1 : (ParentObject.Stat("Level") / 5)) - E.Tier;
					if (num2 > 2)
					{
						num = 0;
					}
					else if (num2 > 1)
					{
						num /= 10;
					}
					else if (num2 > 0)
					{
						num /= 2;
					}
				}
				if (num < E.Minimum)
				{
					num = E.Minimum;
				}
				if (num > E.Maximum)
				{
					num = E.Maximum;
				}
				if (XRLCore.Core.XPMul != 1f)
				{
					num = (int)((float)num * XRLCore.Core.XPMul);
				}
				if (num > 0)
				{
					Statistic stat = ParentObject.GetStat("XP");
					if (stat != null)
					{
						stat.BaseValue += num;
						if (IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You gain {{C|" + num + "}} XP!");
						}
						AwardedXPEvent.Send(E, num);
					}
				}
				if (E.PassedDownFrom == null && E.PassedUpFrom != ParentObject && ParentObject.Brain != null)
				{
					GameObject partyLeader = ParentObject.PartyLeader;
					if (partyLeader != null)
					{
						partyLeader.AwardXP(E.Amount, E.Tier, 0, int.MaxValue, E.Kill, E.InfluencedBy, E.PassedUpFrom ?? ParentObject, null, null, E.Deed);
					}
					else if (E.PassedUpFrom == null && ParentObject.IsPlayer())
					{
						Cell cell = ParentObject.GetCurrentCell();
						if (cell != null && cell.ParentZone != null)
						{
							foreach (GameObject item in cell.ParentZone.FastSquareSearch(cell.X, cell.Y, 10, "Brain"))
							{
								if (item != E.Kill && item.IsPlayerLed())
								{
									item.AwardXP(E.Amount, E.Tier, 0, int.MaxValue, E.Kill, E.InfluencedBy, null, ParentObject, null, E.Deed);
								}
							}
						}
					}
				}
				E.Amount = num;
			}
			else
			{
				E.Amount = 0;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Exception awarding XP", x);
		}
		return base.HandleEvent(E);
	}
}
