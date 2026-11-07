using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class TunnelMaker
{
	public int Width;

	public int Height;

	public int StartX;

	public int StartY;

	public int EndX;

	public int EndY;

	public string Directions;

	public string[,] Map;

	public TunnelMaker(int _Width, int _Height, string _StartY, string _EndY, string _Directions)
	{
		Width = _Width;
		Height = _Height;
		StartY = _StartY.RollCached();
		EndY = _EndY.RollCached();
		Directions = _Directions;
		Map = new string[Width, Height];
		try
		{
			CreateTunnel();
		}
		catch
		{
			CreateTunnel();
		}
	}

	public bool CreateTunnel()
	{
		for (int i = 0; i < 100; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				for (int k = 0; k < Height; k++)
				{
					Map[j, k] = "";
				}
			}
			int num = 0;
			StartX = 0;
			EndX = Width - 1;
			if (Directions.Contains("W"))
			{
				num = (StartX = Width - 1);
				EndX = 0;
			}
			int num2 = StartY;
			int num3 = 10;
			while (true)
			{
				num3--;
				if (num3 == 0)
				{
					break;
				}
				string text = Directions.GetRandomElement().ToString();
				int x = num;
				int y = num2;
				XRL.Rules.Directions.ApplyDirection(text, ref x, ref y);
				if (x >= 0 && y >= 0 && x < Width && y < Height)
				{
					if (Map[x, y] == "")
					{
						Map[num, num2] += text;
						num = x;
						num2 = y;
						Map[num, num2] = XRL.Rules.Directions.GetOppositeDirection(text);
						num3 = 10;
					}
					if (num == EndX && num2 == EndY)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
