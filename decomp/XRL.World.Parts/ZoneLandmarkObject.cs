using System;
using UnityEngine;

namespace XRL.World.Parts;

[Serializable]
public class ZoneLandmarkObject : IZoneLandmark
{
	public int Radius = 1;

	public bool Manhattan = true;

	public bool Visual = true;

	public override string GetDisplayName()
	{
		return ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true, Short: true);
	}

	public override bool IsApplicable(int X, int Y)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (Visual)
		{
			Y -= Mathf.RoundToInt((float)(cell.Y - Y) * 24f / 32f);
		}
		return (Manhattan ? cell.ManhattanDistanceTo(X, Y) : cell.PathDistanceTo(X, Y)) <= Radius;
	}
}
