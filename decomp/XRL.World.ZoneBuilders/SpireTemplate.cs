using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class SpireTemplate
{
	public int MaxDepth;

	[NonSerialized]
	private static Point P;

	[NonSerialized]
	private static bool bOK;

	public int Width;

	public int Height;

	public int[,] Rooms;

	public SpireTemplateTile[,] Map;

	public List<SpireRoomData> RoomList = new List<SpireRoomData>();

	public SpireTemplate(SpireTemplate Source)
	{
		Width = Source.Width;
		Height = Source.Height;
		Map = new SpireTemplateTile[Width, Height];
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j, i] = Source.Map[j, i];
			}
		}
		Rooms = new int[Width, Height];
	}

	public SpireTemplate(int Width, int Height)
	{
		this.Width = Width;
		this.Height = Height;
		Map = new SpireTemplateTile[Width, Height];
		Rooms = new int[Width, Height];
	}

	public SpireTemplate(int _Width, int _Height, int nSquares)
	{
		Width = _Width;
		Height = _Height;
		Map = new SpireTemplateTile[Width, Height];
		Rooms = new int[Width, Height];
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j, i] = SpireTemplateTile.Void;
				Rooms[j, i] = 0;
			}
		}
		for (int k = 0; k < nSquares; k++)
		{
			int num = 0;
			int num2 = Stat.Random(9, _Width / 2);
			int num3 = Stat.Random(num2 + 3, _Width - 10);
			int num4 = Stat.Random(4, _Height / 2);
			num = Stat.Random(num4 + 3, _Height - 5);
			for (int l = num2; l <= num3; l++)
			{
				for (int m = num4; m <= num; m++)
				{
					Map[l, m] = SpireTemplateTile.Wall;
				}
			}
		}
		for (int n = 0; n < Width; n++)
		{
			FloodOutsideWall(n, 0);
			FloodOutsideWall(n, Height - 1);
		}
		for (int num5 = 0; num5 < Height; num5++)
		{
			FloodOutsideWall(0, num5);
			FloodOutsideWall(Width - 1, num5);
		}
		for (int num6 = 1; num6 < Width - 1; num6++)
		{
			for (int num7 = 1; num7 < Height - 1; num7++)
			{
				if (Map[num6, num7] == SpireTemplateTile.Wall && Map[num6 - 1, num7] == SpireTemplateTile.Wall && Map[num6 + 1, num7] == SpireTemplateTile.Wall && Map[num6, num7 - 1] == SpireTemplateTile.Wall && Map[num6, num7 + 1] == SpireTemplateTile.Wall)
				{
					FloodInsideWall(num6, num7);
				}
			}
		}
		int num8 = Stat.Random(1, 2);
		if (10.in100())
		{
			num8 += Stat.Random(1, 2);
		}
		int num9 = Width * Height / num8;
		int num10 = Stat.Random(0, num9);
		bool flag = false;
		for (int num11 = 0; num11 < Width; num11++)
		{
			for (int num12 = 0; num12 < Height; num12++)
			{
				num10++;
				if (num10 >= num9)
				{
					flag = true;
					num10 = 0;
				}
				if (Map[num11, num12] == SpireTemplateTile.OutsideWall && flag)
				{
					Map[num11, num12] = SpireTemplateTile.Door;
					flag = false;
				}
			}
		}
	}

	public void AddMap(int StartX, int StartY, SpireTemplate Source)
	{
		for (int i = 0; i < Source.Height; i++)
		{
			for (int j = 0; j < Source.Width; j++)
			{
				Map[j + StartX, i + StartY] = Source.Map[j, i];
				Rooms[j + StartX, i + StartY] = Source.Rooms[j, i];
			}
		}
	}

	public void GenerateRooms(int Depth)
	{
		MaxDepth = "2d4".RollCached();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j, i] == SpireTemplateTile.OutsideWall)
				{
					Map[j, i] = SpireTemplateTile.Wall;
				}
			}
		}
		for (int k = 0; k < Height; k++)
		{
			for (int l = 0; l < Width; l++)
			{
				if (Map[l, k] == SpireTemplateTile.Inside)
				{
					FillRoom(l, k, 0);
					break;
				}
			}
		}
		for (int m = 0; m < Width; m++)
		{
			for (int n = 0; n < Height; n++)
			{
				Rooms[m, n] = 0;
			}
		}
		int num = 0;
		for (short num2 = 0; num2 < Width; num2++)
		{
			for (short num3 = 0; num3 < Height; num3++)
			{
				if (Rooms[num2, num3] == 0 && Map[num2, num3] == SpireTemplateTile.Inside)
				{
					SpireRoomData spireRoomData = new SpireRoomData(Width, Height);
					FloodRoom(num2, num3, spireRoomData, (short)(num + 1));
					RoomList.Add(spireRoomData);
					num++;
				}
			}
		}
		int num4 = 20;
		int chance = 20 + 3 * Depth;
		for (int num5 = 0; num5 < num4; num5++)
		{
			for (int num6 = 100; num6 > 0; num6--)
			{
				int num7 = Stat.Random(0, Width - 1);
				int num8 = Stat.Random(0, Height - 1);
				if (DoorPositionOk(num7, num8))
				{
					if (chance.in100())
					{
						Map[num7, num8] = SpireTemplateTile.SecurityDoor;
					}
					else
					{
						Map[num7, num8] = SpireTemplateTile.Door;
					}
					break;
				}
			}
		}
		foreach (SpireRoomData room in RoomList)
		{
			if (room.Doors.Count != 1)
			{
				continue;
			}
			for (int num9 = 0; num9 < Width; num9++)
			{
				for (int num10 = 0; num10 < Height; num10++)
				{
					if (room.Room[num9, num10] != 0)
					{
						Map[num9, num10] = SpireTemplateTile.InsideSealed;
					}
				}
			}
			Map[room.Doors[0].X, room.Doors[0].Y] = SpireTemplateTile.SecurityDoor;
		}
	}

	private bool DoorPositionOk(int x, int y)
	{
		if (x < 1)
		{
			return false;
		}
		if (x > Width - 2)
		{
			return false;
		}
		if (y < 1)
		{
			return false;
		}
		if (y > Height - 2)
		{
			return false;
		}
		if (Map[x, y] != SpireTemplateTile.Wall)
		{
			return false;
		}
		if (Map[x + 1, y] == SpireTemplateTile.Wall)
		{
			if (Map[x, y + 1] == SpireTemplateTile.Wall)
			{
				return false;
			}
			if (Map[x, y - 1] == SpireTemplateTile.Wall)
			{
				return false;
			}
		}
		if (Map[x - 1, y] == SpireTemplateTile.Wall)
		{
			if (Map[x, y + 1] == SpireTemplateTile.Wall)
			{
				return false;
			}
			if (Map[x, y - 1] == SpireTemplateTile.Wall)
			{
				return false;
			}
		}
		if (Map[x, y - 1] == SpireTemplateTile.Wall)
		{
			if (Map[x - 1, y] == SpireTemplateTile.Wall)
			{
				return false;
			}
			if (Map[x + 1, y] == SpireTemplateTile.Wall)
			{
				return false;
			}
		}
		if (Map[x, y + 1] == SpireTemplateTile.Wall)
		{
			if (Map[x - 1, y] == SpireTemplateTile.Wall)
			{
				return false;
			}
			if (Map[x + 1, y] == SpireTemplateTile.Wall)
			{
				return false;
			}
		}
		if (Map[x + 1, y] == SpireTemplateTile.Door)
		{
			return false;
		}
		if (Map[x - 1, y] == SpireTemplateTile.Door)
		{
			return false;
		}
		if (Map[x, y + 1] == SpireTemplateTile.Door)
		{
			return false;
		}
		if (Map[x, y - 1] == SpireTemplateTile.Door)
		{
			return false;
		}
		if (Map[x + 1, y] == SpireTemplateTile.SecurityDoor)
		{
			return false;
		}
		if (Map[x - 1, y] == SpireTemplateTile.SecurityDoor)
		{
			return false;
		}
		if (Map[x, y + 1] == SpireTemplateTile.SecurityDoor)
		{
			return false;
		}
		if (Map[x, y - 1] == SpireTemplateTile.SecurityDoor)
		{
			return false;
		}
		return true;
	}

	private void FillRoom(int x, int y, int Depth)
	{
		if (Depth >= MaxDepth)
		{
			return;
		}
		SpireRoomData spireRoomData = new SpireRoomData(Width, Height);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				spireRoomData.Room[i, j] = 0;
			}
		}
		FloodRoom1((short)x, (short)y, spireRoomData);
		spireRoomData.Width = spireRoomData.Right - spireRoomData.Left + 1;
		spireRoomData.Height = spireRoomData.Bottom - spireRoomData.Top + 1;
		if (spireRoomData.Size <= 24 || spireRoomData.Width <= 4 || spireRoomData.Height <= 4 || spireRoomData.RightAt[y] - spireRoomData.LeftAt[y] <= 2 || spireRoomData.BottomAt[x] - spireRoomData.TopAt[x] <= 2)
		{
			return;
		}
		int num = ((spireRoomData.Width <= spireRoomData.Height) ? 1 : 0);
		int num3;
		if (num == 0)
		{
			int num2 = 0;
			num3 = Stat.Random(spireRoomData.LeftAt[y] + 1, spireRoomData.RightAt[y] - 1);
			while (num2 < 10 && (Map[num3, spireRoomData.TopAt[num3] - 1] != SpireTemplateTile.Wall || Map[num3, spireRoomData.BottomAt[num3] + 1] != SpireTemplateTile.Wall))
			{
				num2++;
				num3 = Stat.Random(spireRoomData.LeftAt[y] + 1, spireRoomData.RightAt[y] - 1);
			}
			if (num2 == 10 || Math.Abs(spireRoomData.TopAt[num3] - spireRoomData.BottomAt[num3]) < 3)
			{
				return;
			}
			for (int k = spireRoomData.TopAt[num3]; k <= spireRoomData.BottomAt[num3]; k++)
			{
				if (spireRoomData.Room[num3, k] == 1)
				{
					Map[num3, k] = SpireTemplateTile.Wall;
					Rooms[num3, k] = 0;
				}
			}
			num2 = 0;
			int num4 = Stat.Random(spireRoomData.TopAt[num3], spireRoomData.BottomAt[num3]);
			while (num2 < 10 && !DoorPositionOk(num3, num4))
			{
				num2++;
				num4 = Stat.Random(spireRoomData.TopAt[num3], spireRoomData.BottomAt[num3]);
			}
			Map[num3, num4] = SpireTemplateTile.Door;
			Rooms[num3, num4] = 0;
			FillRoom(num3 - 1, num4, Depth + 1);
			FillRoom(num3 + 1, num4, Depth + 1);
		}
		if (num != 1)
		{
			return;
		}
		int num5 = 0;
		num3 = Stat.Random(spireRoomData.TopAt[x] + 1, spireRoomData.BottomAt[x] - 1);
		while (num5 < 10 && (Map[spireRoomData.LeftAt[num3] - 1, num3] != SpireTemplateTile.Wall || Map[spireRoomData.RightAt[num3] + 1, num3] != SpireTemplateTile.Wall))
		{
			num5++;
			num3 = Stat.Random(spireRoomData.TopAt[x] + 1, spireRoomData.BottomAt[x] - 1);
		}
		if (num5 == 10 || Math.Abs(spireRoomData.LeftAt[num3] - spireRoomData.RightAt[num3]) < 3)
		{
			return;
		}
		for (int l = spireRoomData.LeftAt[num3]; l <= spireRoomData.RightAt[num3]; l++)
		{
			if (spireRoomData.Room[l, num3] == 1)
			{
				Map[l, num3] = SpireTemplateTile.Wall;
				Rooms[l, num3] = 0;
			}
		}
		num5 = 0;
		int num6 = Stat.Random(spireRoomData.LeftAt[num3], spireRoomData.RightAt[num3]);
		while (num5 < 10 && !DoorPositionOk(num6, num3))
		{
			num5++;
			num6 = Stat.Random(spireRoomData.LeftAt[num3], spireRoomData.RightAt[num3]);
		}
		Map[num6, num3] = SpireTemplateTile.Door;
		Rooms[num6, num3] = 0;
		FillRoom(num6, num3 - 1, Depth + 1);
		FillRoom(num6, num3 + 1, Depth + 1);
	}

	private void FloodRoom1(short x, short y, SpireRoomData Data)
	{
		if (x >= 0 && x < Width && y >= 0 && y < Height && Data.Room[x, y] == 0 && Map[x, y] == SpireTemplateTile.Inside)
		{
			if (x < Data.Left)
			{
				Data.Left = x;
			}
			if (x > Data.Right)
			{
				Data.Right = x;
			}
			if (y < Data.Top)
			{
				Data.Top = y;
			}
			if (y > Data.Bottom)
			{
				Data.Bottom = y;
			}
			if (x < Data.LeftAt[y])
			{
				Data.LeftAt[y] = x;
			}
			if (x > Data.RightAt[y])
			{
				Data.RightAt[y] = x;
			}
			if (y < Data.TopAt[x])
			{
				Data.TopAt[x] = y;
			}
			if (y > Data.BottomAt[x])
			{
				Data.BottomAt[x] = y;
			}
			Rooms[x, y] = 1;
			Data.Room[x, y] = 1;
			Data.Size++;
			FloodRoom1((short)(x - 1), y, Data);
			FloodRoom1((short)(x + 1), y, Data);
			FloodRoom1(x, (short)(y - 1), Data);
			FloodRoom1(x, (short)(y + 1), Data);
		}
	}

	private void FloodRoom(short x, short y, SpireRoomData Data, short n)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height || Data.Room[x, y] != 0)
		{
			return;
		}
		if (Map[x, y] == SpireTemplateTile.Door || Map[x, y] == SpireTemplateTile.SecurityDoor)
		{
			P = new Point(x, y);
			bOK = true;
			foreach (Point door in Data.Doors)
			{
				if (door.X == x && door.Y == y)
				{
					bOK = false;
					break;
				}
			}
			if (bOK)
			{
				Data.Doors.Add(P);
			}
		}
		if (Map[x, y] == SpireTemplateTile.Inside)
		{
			if (x < Data.Left)
			{
				Data.Left = x;
			}
			if (x > Data.Right)
			{
				Data.Right = x;
			}
			if (y < Data.Top)
			{
				Data.Top = y;
			}
			if (y > Data.Bottom)
			{
				Data.Bottom = y;
			}
			if (x < Data.LeftAt[y])
			{
				Data.LeftAt[y] = x;
			}
			if (x > Data.RightAt[y])
			{
				Data.RightAt[y] = x;
			}
			if (y < Data.TopAt[x])
			{
				Data.TopAt[x] = y;
			}
			if (y > Data.BottomAt[x])
			{
				Data.BottomAt[x] = y;
			}
			Rooms[x, y] = n;
			Data.Room[x, y] = n;
			Data.Size++;
			FloodRoom((short)(x - 1), y, Data, n);
			FloodRoom((short)(x + 1), y, Data, n);
			FloodRoom(x, (short)(y - 1), Data, n);
			FloodRoom(x, (short)(y + 1), Data, n);
		}
	}

	private void FloodInsideWall(int xp, int yp)
	{
		if (xp >= 0 && xp < Width && yp >= 0 && yp < Height && Rooms[xp, yp] != 2 && Rooms[xp, yp] != 3 && Map[xp, yp] != SpireTemplateTile.OutsideWall)
		{
			if (Rooms[xp, yp] > 0 && Map[xp, yp] <= SpireTemplateTile.Void)
			{
				Rooms[xp, yp] = 2;
				return;
			}
			Rooms[xp, yp] = 3;
			Map[xp, yp] = SpireTemplateTile.Inside;
			FloodInsideWall(xp - 1, yp - 1);
			FloodInsideWall(xp - 1, yp);
			FloodInsideWall(xp - 1, yp + 1);
			FloodInsideWall(xp, yp - 1);
			FloodInsideWall(xp, yp + 1);
			FloodInsideWall(xp + 1, yp - 1);
			FloodInsideWall(xp + 1, yp);
			FloodInsideWall(xp + 1, yp + 1);
		}
	}

	private void FloodOutsideWall(int xp, int yp)
	{
		if (xp >= 0 && xp < Width && yp >= 0 && yp < Height && Map[xp, yp] != SpireTemplateTile.OutsideWall && Map[xp, yp] != SpireTemplateTile.Outside)
		{
			if (Map[xp, yp] == SpireTemplateTile.Wall)
			{
				Map[xp, yp] = SpireTemplateTile.OutsideWall;
				return;
			}
			Map[xp, yp] = SpireTemplateTile.Outside;
			FloodOutsideWall(xp - 1, yp - 1);
			FloodOutsideWall(xp - 1, yp);
			FloodOutsideWall(xp - 1, yp + 1);
			FloodOutsideWall(xp, yp - 1);
			FloodOutsideWall(xp, yp + 1);
			FloodOutsideWall(xp + 1, yp - 1);
			FloodOutsideWall(xp + 1, yp);
			FloodOutsideWall(xp + 1, yp + 1);
		}
	}

	public void Draw(SpireRoomData Data)
	{
		Popup._ScreenBuffer.Clear();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j, i] == SpireTemplateTile.Inside)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(58);
				}
				if (Map[j, i] == SpireTemplateTile.Void)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(88);
				}
				if (Map[j, i] == SpireTemplateTile.Outside)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(44);
				}
				if (Map[j, i] == SpireTemplateTile.OutsideWall)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(36);
				}
				if (Data != null && Data.Room[j, i] > 0)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write((short)(65 + Data.Room[j, i]));
				}
				if (Map[j, i] == SpireTemplateTile.Wall)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(42);
				}
				if (Map[j, i] == SpireTemplateTile.Door)
				{
					Popup._ScreenBuffer.Goto(j, i);
					Popup._ScreenBuffer.Write(43);
				}
			}
		}
		Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
		Popup._ScreenBuffer.Goto(0, 0);
		Popup._ScreenBuffer.Write(Data.Width + "," + Data.Height);
		Keyboard.getch();
	}
}
