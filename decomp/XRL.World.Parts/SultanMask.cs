using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class SultanMask : IPart
{
	public int Period = 1;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == EquippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Period <= 5)
		{
			foreach (string item in HistoryAPI.GetLikedFactionsForSultan(Period))
			{
				AddsRep.AddModifier(ParentObject, item, 200);
			}
			foreach (string item2 in HistoryAPI.GetDislikedFactionsForSultan(Period))
			{
				AddsRep.AddModifier(ParentObject, item2, -200);
			}
			foreach (string item3 in HistoryAPI.GetDomainsForSultan(Period).ShuffleInPlace())
			{
				if (RelicGenerator.ApplyElementBestowal(ParentObject, item3, "Face", ParentObject.GetIntProperty("RelicTemplateTier"), "armor"))
				{
					break;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		AchievementManager.IncrementStat("STAT_WEAR_FACE_" + Period);
		return base.HandleEvent(E);
	}
}
