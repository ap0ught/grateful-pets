using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PickRandomTile : IPart
{
	public string Tile = "";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == SingletonEvent<RefreshTileEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RefreshTileEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public void RandomizeTile()
	{
		ParentObject.Render.Tile = RandomVariant(Tile);
	}

	public static string RandomVariant(string Source, Random R = null)
	{
		string text = Source;
		for (int num = Source.IndexOf('~'); num != -1; num = text.IndexOf('~', num))
		{
			int num2 = num + ((Source[num + 1] != '#') ? 1 : 2);
			int num3 = text.IndexOf('~', num2);
			if (num3 == -1)
			{
				break;
			}
			int num4 = text.IndexOf('-', num2, num3 - num2);
			if (num4 == -1 || !int.TryParse(text.Substring(num2, num4 - num2), out var result) || !int.TryParse(text.Substring(num4 + 1, num3 - num4 - 1), out var result2))
			{
				break;
			}
			if (R == null)
			{
				R = Stat.Rnd;
			}
			int num5 = R.Next(result, result2 + 1);
			text = text.Remove(num, num3 - num + 1);
			text = text.Insert(num, num5.ToString());
		}
		return text;
	}
}
