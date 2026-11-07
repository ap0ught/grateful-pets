using System;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialMainframeTapeDrive : IPart
{
	public int nFrameOffset;

	public int RushingBack;

	public int RushingForward;

	public string Color = "Normal";

	[NonSerialized]
	private StringBuilder tileBuilder = new StringBuilder();

	[NonSerialized]
	private int lastN = 1;

	[NonSerialized]
	private long accumulator;

	public string Tile = "Assets_Content_Textures_Tiles2_mainframe_tape_drive_";

	public AnimatedMaterialMainframeTapeDrive()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
		RushingBack = 0;
		RushingForward = 0;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (E.Tile != null)
		{
			if (RushingForward > 0)
			{
				RushingForward--;
				string value = E.Tile.Substring(E.Tile.LastIndexOf('-') + 1);
				lastN++;
				if (lastN > 4)
				{
					lastN = 1;
				}
				tileBuilder.Length = 0;
				tileBuilder.Append(Tile).Append(lastN).Append('-')
					.Append(value);
				ParentObject.Render.Tile = tileBuilder.ToString();
			}
			else if (RushingBack > 0)
			{
				RushingBack--;
				string value2 = E.Tile.Substring(E.Tile.LastIndexOf('-') + 1);
				lastN--;
				if (lastN < 1)
				{
					lastN = 4;
				}
				tileBuilder.Length = 0;
				tileBuilder.Append(Tile).Append(lastN).Append('-')
					.Append(value2);
				ParentObject.Render.Tile = tileBuilder.ToString();
			}
			else if (XRLCore.FrameTimer.ElapsedMilliseconds - accumulator > 500)
			{
				accumulator = XRLCore.FrameTimer.ElapsedMilliseconds;
				string value3 = E.Tile.Substring(E.Tile.LastIndexOf('-') + 1);
				if (++lastN > 4)
				{
					lastN = 1;
				}
				tileBuilder.Length = 0;
				tileBuilder.Append(Tile).Append(lastN).Append('-')
					.Append(value3);
				ParentObject.Render.Tile = tileBuilder.ToString();
				if (Stat.RandomCosmetic(1, 64) <= 1)
				{
					RushingBack = Stat.Random(15, 120);
				}
				else if (Stat.RandomCosmetic(1, 64) <= 1)
				{
					RushingForward = Stat.Random(15, 120);
				}
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
