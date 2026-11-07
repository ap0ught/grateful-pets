using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

public class CombatSorter : Comparer<GameObject>
{
	private GameObject POV;

	public CombatSorter(GameObject POV)
	{
		this.POV = POV;
	}

	public override int Compare(GameObject x, GameObject y)
	{
		int num = (x == POV).CompareTo(y == POV);
		if (num != 0)
		{
			return num;
		}
		int num2 = x.HasEffect<Engulfed>().CompareTo(y.HasEffect<Engulfed>());
		if (num2 != 0)
		{
			return num2;
		}
		if (POV != null && !POV.IsPlayerControlled())
		{
			int num3 = x.IsPlayer().CompareTo(y.IsPlayer());
			if (num3 != 0)
			{
				return -num3;
			}
		}
		int num4 = x.IsHostileTowards(POV).CompareTo(y.IsHostileTowards(POV));
		if (num4 != 0)
		{
			return -num4;
		}
		int num5 = x.IsCombat.CompareTo(y.IsCombat);
		if (num5 != 0)
		{
			return -num5;
		}
		int num6 = x.HasPart<NoDamage>().CompareTo(y.HasPart<NoDamage>());
		if (num6 != 0)
		{
			return num6;
		}
		int num7 = x.HasStat("Hitpoints").CompareTo(y.HasStat("Hitpoints"));
		if (num7 != 0)
		{
			return -num7;
		}
		int num8 = x.PhaseAndFlightMatches(POV).CompareTo(y.PhaseAndFlightMatches(POV));
		if (num8 != 0)
		{
			return -num8;
		}
		int num9 = (x.Physics != null && x.Physics.IsReal).CompareTo(y.Physics != null && y.Physics.IsReal);
		if (num9 != 0)
		{
			return -num9;
		}
		int num10 = ((x.Render != null) ? x.Render.RenderLayer : 0).CompareTo((y.Render != null) ? y.Render.RenderLayer : 0);
		if (num10 != 0)
		{
			return -num10;
		}
		return 0;
	}
}
