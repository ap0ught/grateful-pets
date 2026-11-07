using System;
using Cysharp.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedWallGeneric : IPart
{
	public int Frames;

	public int nFrameOffset;

	public int RushingBack;

	public int RushingForward;

	public string Color = "Normal";

	public string Tile = "";

	public bool AllowRushing = true;

	public int RushingChance = 64;

	public int FrameMS = 500;

	[NonSerialized]
	private int lastN = 1;

	[NonSerialized]
	private long accumulator;

	[NonSerialized]
	private string postfix;

	[NonSerialized]
	private string lastwall;

	[NonSerialized]
	private string atlas;

	public AnimatedWallGeneric()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
		RushingBack = 0;
		RushingForward = 0;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<RepaintedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(RepaintedEvent E)
	{
		postfix = null;
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (E.Tile != null)
		{
			if (postfix == null)
			{
				atlas = GetPropertyOrTag("PaintedWallAtlas");
				if (E.Tile.Contains('-'))
				{
					postfix = E.Tile.Substring(E.Tile.LastIndexOf('-') + 1);
				}
				else
				{
					postfix = "";
				}
			}
			if (string.IsNullOrEmpty(postfix))
			{
				return true;
			}
			using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
			if (RushingForward > 0)
			{
				RushingForward--;
				string value = E.Tile.Substring(E.Tile.LastIndexOf('-') + 1);
				lastN++;
				if (lastN >= Frames)
				{
					lastN = 0;
				}
				utf16ValueStringBuilder.Clear();
				utf16ValueStringBuilder.Append(atlas);
				utf16ValueStringBuilder.Append(Tile);
				utf16ValueStringBuilder.Append(lastN);
				utf16ValueStringBuilder.Append('-');
				utf16ValueStringBuilder.Append(value);
				ParentObject.Render.Tile = utf16ValueStringBuilder.ToString();
			}
			else if (RushingBack > 0)
			{
				RushingBack--;
				string value2 = E.Tile.Substring(E.Tile.LastIndexOf('-') + 1);
				lastN--;
				if (lastN < 0)
				{
					lastN = Frames - 1;
				}
				utf16ValueStringBuilder.Clear();
				utf16ValueStringBuilder.Append(atlas);
				utf16ValueStringBuilder.Append(Tile);
				utf16ValueStringBuilder.Append(lastN);
				utf16ValueStringBuilder.Append('-');
				utf16ValueStringBuilder.Append(value2);
				ParentObject.Render.Tile = utf16ValueStringBuilder.ToString();
			}
			else if (XRLCore.FrameTimer.ElapsedMilliseconds - accumulator >= FrameMS)
			{
				accumulator = XRLCore.FrameTimer.ElapsedMilliseconds;
				string value3 = E.Tile.Substring(E.Tile.LastIndexOf('-') + 1);
				if (++lastN >= Frames)
				{
					lastN = 0;
				}
				utf16ValueStringBuilder.Clear();
				utf16ValueStringBuilder.Append(atlas);
				utf16ValueStringBuilder.Append(Tile);
				utf16ValueStringBuilder.Append(lastN);
				utf16ValueStringBuilder.Append('-');
				utf16ValueStringBuilder.Append(value3);
				ParentObject.Render.Tile = utf16ValueStringBuilder.ToString();
				if (AllowRushing)
				{
					if (Stat.RandomCosmetic(1, RushingChance) <= 1)
					{
						RushingBack = Stat.Random(15, 120);
					}
					else if (Stat.RandomCosmetic(1, RushingChance) <= 1)
					{
						RushingForward = Stat.Random(15, 120);
					}
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
