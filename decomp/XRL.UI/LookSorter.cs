using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class LookSorter : Comparer<GameObject>
{
	public override int Compare(GameObject x, GameObject y)
	{
		int num = x.HasPart<TerrainTravel>().CompareTo(y.HasPart<TerrainTravel>());
		if (num != 0)
		{
			return -num;
		}
		int num2 = x.ConsiderSolidInRenderingContext().CompareTo(y.ConsiderSolidInRenderingContext());
		if (num2 != 0)
		{
			return -num2;
		}
		int num3 = x.IsPlayer().CompareTo(y.IsPlayer());
		if (num3 != 0)
		{
			return -num3;
		}
		int num4 = x.IsCombatObject().CompareTo(y.IsCombatObject());
		if (num4 != 0)
		{
			return -num4;
		}
		int num5 = x.HasPart<SultanShrine>().CompareTo(y.HasPart<SultanShrine>());
		if (num5 != 0)
		{
			return -num5;
		}
		int num6 = (x.HasPart<ModPainted>() || x.HasPart<ModEngraved>() || x.HasPart<Tombstone>()).CompareTo(y.HasPart<ModPainted>() || y.HasPart<ModEngraved>() || y.HasPart<Tombstone>());
		if (num6 != 0)
		{
			return -num6;
		}
		int? num7 = x.Render?.RenderLayer.CompareTo(y.Render?.RenderLayer);
		if (num7 != 0 && num7.HasValue)
		{
			return (-num7).Value;
		}
		return 0;
	}
}
