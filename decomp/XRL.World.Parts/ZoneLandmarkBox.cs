using System;

namespace XRL.World.Parts;

[Serializable]
public class ZoneLandmarkBox : IZoneLandmark
{
	public string DisplayName;

	public string RequireBlueprint;

	public int RequireCount;

	public int Width;

	public int Height;

	public override string GetDisplayName()
	{
		return DisplayName;
	}

	public override bool IsApplicable(int X, int Y)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		int x = cell.X;
		int y = cell.Y;
		int num = x + Width - 1;
		int num2 = y + Height - 1;
		if (X < x)
		{
			return false;
		}
		if (X > num)
		{
			return false;
		}
		if (Y < y)
		{
			return false;
		}
		if (Y > num2)
		{
			return false;
		}
		if (!RequireBlueprint.IsNullOrEmpty() && RequireCount > 0)
		{
			Zone parentZone = cell.ParentZone;
			int num3 = 0;
			for (int i = y; i <= num2; i++)
			{
				for (int j = x; j <= num; j++)
				{
					Cell cell2 = parentZone.GetCell(j, i);
					if (cell2 == null)
					{
						continue;
					}
					foreach (GameObject @object in cell2.Objects)
					{
						if (!(@object.Blueprint != RequireBlueprint) && ++num3 >= RequireCount)
						{
							goto end_IL_00f5;
						}
					}
				}
				continue;
				end_IL_00f5:
				break;
			}
			if (num3 < RequireCount)
			{
				return false;
			}
		}
		return true;
	}
}
