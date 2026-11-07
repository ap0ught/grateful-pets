using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class AIUrnDuster : AIBehaviorPart
{
	public const string URN_ZONES = "JoppaWorld.53.3.0.0.9,JoppaWorld.53.3.0.1.9,JoppaWorld.53.3.0.2.9";

	public const string URN_PART = "EaterUrn";

	public int LastUrnSelected = -1;

	[NonSerialized]
	private GlobalLocation LastUrnLocation;

	[NonSerialized]
	private int LastUrnCount;

	[NonSerialized]
	private int DustingIndex = -1;

	[NonSerialized]
	private static List<GlobalLocation> GlobalUrns;

	[NonSerialized]
	private static List<GlobalLocation> LocalUrns;

	[NonSerialized]
	private static string LocalUrnsZoneID;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<GetDebugInternalsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		bool flag = IsInGlobalUrnArea();
		E.AddEntry(this, "Urn dusting mode", flag ? "global" : "local");
		if (ParentObject.HasGoal("DustAnUrnGoal"))
		{
			E.AddEntry(this, "Urn dusting status", "Trying to dust an urn");
		}
		else
		{
			E.AddEntry(this, "Urn dusting status", "Not currently trying to dust an urn");
			if (LastUrnSelected != -1)
			{
				E.AddEntry(this, "Last urn dusted", LastUrnSelected + " of " + LastUrnCount);
			}
		}
		if (LastUrnSelected != -1)
		{
			if (LastUrnLocation == null)
			{
				E.AddEntry(this, "Last urn location", "no longer available");
			}
			else
			{
				E.AddEntry(this, "Last urn location", LastUrnLocation.CellX + ", " + LastUrnLocation.CellY + " in " + LastUrnLocation.ZoneID);
			}
		}
		else
		{
			E.AddEntry(this, "Last urn location", "has not dusted an urn");
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckDustUrn();
	}

	public bool CheckDustUrn()
	{
		if (ParentObject.IsBusy())
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		if (ParentObject.IsPlayerControlled())
		{
			return false;
		}
		List<GlobalLocation> urns = GetUrns(ParentObject.CurrentZone);
		if (urns == null)
		{
			return false;
		}
		bool flag = IsInGlobalUrnArea();
		if ((urns.Count < 3) & flag)
		{
			return false;
		}
		if (DustingIndex == -1)
		{
			DustingIndex = Stat.Random(0, urns.Count - 1);
		}
		int dustingIndex = DustingIndex;
		int num = dustingIndex % urns.Count;
		int num2 = (dustingIndex + 16) % urns.Count;
		int num3 = (dustingIndex + 32) % urns.Count;
		int num4 = num;
		if (LastUrnSelected == num)
		{
			num4 = num2;
		}
		else if (LastUrnSelected == num2)
		{
			num4 = num3;
		}
		GlobalLocation globalLocation = urns[num4];
		ParentObject.Brain.PushGoal(new DustAnUrnGoal(globalLocation));
		LastUrnSelected = num4;
		LastUrnCount = urns.Count;
		LastUrnLocation = globalLocation;
		return true;
	}

	public bool IsInGlobalUrnArea()
	{
		return IsInGlobalUrnArea(ParentObject.CurrentZone);
	}

	public static bool IsInGlobalUrnArea(Zone Z)
	{
		return "JoppaWorld.53.3.0.0.9,JoppaWorld.53.3.0.1.9,JoppaWorld.53.3.0.2.9".CachedCommaExpansion().Contains(Z?.ZoneID);
	}

	public static List<GlobalLocation> GetUrns(Zone Z)
	{
		if (IsInGlobalUrnArea(Z))
		{
			if (GlobalUrns == null)
			{
				GlobalUrns = FindGlobalUrns();
			}
			return GlobalUrns;
		}
		if (Z == null)
		{
			return null;
		}
		if (Z.ZoneID == LocalUrnsZoneID)
		{
			return LocalUrns;
		}
		LocalUrnsZoneID = Z.ZoneID;
		LocalUrns = null;
		Z.ForeachObjectWithPart("EaterUrn", (GameObject o) =>
		{
			if (LocalUrns == null)
			{
				LocalUrns = new List<GlobalLocation>();
			}
			LocalUrns.Add(new GlobalLocation(o));
		});
		return LocalUrns;
	}

	public static void UrnFoundDestroyed(Zone Z)
	{
		if ("JoppaWorld.53.3.0.0.9,JoppaWorld.53.3.0.1.9,JoppaWorld.53.3.0.2.9".CachedCommaExpansion().Contains(Z?.ZoneID))
		{
			GlobalUrns = null;
		}
	}

	private static List<GlobalLocation> FindGlobalUrns()
	{
		List<GlobalLocation> list = new List<GlobalLocation>();
		foreach (string item in "JoppaWorld.53.3.0.0.9,JoppaWorld.53.3.0.1.9,JoppaWorld.53.3.0.2.9".CachedCommaExpansion())
		{
			if (!The.ZoneManager.IsZoneLive(item))
			{
				continue;
			}
			Zone zone = The.ZoneManager.GetZone(item);
			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					Cell cell = zone.GetCell(j, i);
					int k = 0;
					for (int count = cell.Objects.Count; k < count; k++)
					{
						GameObject gameObject = cell.Objects[k];
						if (gameObject.HasPart("EaterUrn"))
						{
							list.Add(new GlobalLocation(gameObject));
						}
					}
				}
			}
		}
		return list;
	}
}
