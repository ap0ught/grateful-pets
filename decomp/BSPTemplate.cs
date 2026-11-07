using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.ZoneBuilders;

[Serializable]
public class BSPTemplate
{
	public int Width;

	public int Height;

	public int[,] Rooms;

	public BuildingTemplateTile[,] Map;

	public List<RoomData> RoomList = new List<RoomData>();

	public BSPTemplate(int _Width, int _Height)
	{
		Width = _Width;
		Height = _Height;
		Map = new BuildingTemplateTile[Width, Height];
		Rooms = new int[Width, Height];
	}

	public void AddMap(int StartX, int StartY, BSPTemplate Source)
	{
		for (int i = 0; i < Source.Width; i++)
		{
			for (int j = 0; j < Source.Height; j++)
			{
				Map[i + StartX, j + StartY] = Source.Map[i, j];
				Rooms[i + StartX, j + StartY] = Source.Rooms[i, j];
			}
		}
	}

	public BSPTemplate(int _Width, int _Height, int nSquares, bool FullSquare)
	{
		Width = _Width;
		Height = _Height;
		Map = new BuildingTemplateTile[Width, Height];
		Rooms = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Map[i, j] = BuildingTemplateTile.Void;
				Rooms[i, j] = 0;
			}
		}
		if (FullSquare)
		{
			for (int k = 0; k < Width; k++)
			{
				for (int l = 0; l < Height; l++)
				{
					Map[k, l] = BuildingTemplateTile.Wall;
				}
			}
		}
		else
		{
			for (int m = 0; m < nSquares; m++)
			{
				int num = 0;
				int num2 = Stat.Random((int)(1f + (float)(_Width / 2) * 0.3f), _Width / 2);
				int num3 = Stat.Random(num2 + 3, _Width - (int)((float)(_Width / 2) * 0.3f));
				int num4 = Stat.Random((int)(1f + (float)(_Height / 2) * 0.3f), _Height / 2);
				num = Stat.Random(num4 + 3, _Height - (int)((float)(_Height / 2) * 0.3f));
				for (int n = num2; n <= num3; n++)
				{
					for (int num5 = num4; num5 <= num; num5++)
					{
						Map[n, num5] = BuildingTemplateTile.Wall;
					}
				}
			}
		}
		for (int num6 = 0; num6 < Width; num6++)
		{
			FloodOutsideWall(num6, 0);
			FloodOutsideWall(num6, Height - 1);
		}
		for (int num7 = 0; num7 < Height; num7++)
		{
			FloodOutsideWall(0, num7);
			FloodOutsideWall(Width - 1, num7);
		}
		for (int num8 = 1; num8 < Width - 1; num8++)
		{
			for (int num9 = 1; num9 < Height - 1; num9++)
			{
				if (Map[num8, num9] == BuildingTemplateTile.Wall && Map[num8 - 1, num9] == BuildingTemplateTile.Wall && Map[num8 + 1, num9] == BuildingTemplateTile.Wall && Map[num8, num9 - 1] == BuildingTemplateTile.Wall && Map[num8, num9 + 1] == BuildingTemplateTile.Wall)
				{
					FloodInsideWall(num8, num9);
				}
			}
		}
		int num10 = Stat.Random(1, 2);
		if (Stat.Random(0, 100) > 90)
		{
			num10 += Stat.Random(1, 2);
		}
		int num11 = Width * Height / num10;
		int num12 = Stat.Random(0, num11);
		bool flag = false;
		for (int num13 = 0; num13 < Width; num13++)
		{
			for (int num14 = 0; num14 < Height; num14++)
			{
				num12++;
				if (num12 >= num11)
				{
					flag = true;
					num12 = 0;
				}
				if (Map[num13, num14] == BuildingTemplateTile.OutsideWall && flag)
				{
					Map[num13, num14] = BuildingTemplateTile.Door;
					flag = false;
				}
			}
		}
		for (int num15 = 0; num15 < Width; num15++)
		{
			for (int num16 = 0; num16 < Height; num16++)
			{
				if (Map[num15, num16] == BuildingTemplateTile.Inside)
				{
					FillRoom(num15, num16);
					break;
				}
			}
		}
		for (int num17 = 0; num17 < Width; num17++)
		{
			for (int num18 = 0; num18 < Height; num18++)
			{
				Rooms[num17, num18] = 0;
			}
		}
		int num19 = 0;
		for (int num20 = 0; num20 < Width; num20++)
		{
			for (int num21 = 0; num21 < Height; num21++)
			{
				if (Rooms[num20, num21] == 0 && Map[num20, num21] == BuildingTemplateTile.Inside)
				{
					RoomData roomData = new RoomData
					{
						Room = new int[Width, Height]
					};
					FloodRoom(num20, num21, roomData, num19 + 1);
					RoomList.Add(roomData);
					num19++;
				}
			}
		}
	}

	private void FillRoom(int x, int y)
	{
		RoomData roomData = new RoomData();
		roomData.Room = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				roomData.Room[i, j] = 0;
			}
		}
		FloodRoom(x, y, roomData, 1);
		roomData.Width = roomData.Right - roomData.Left;
		roomData.Height = roomData.Bottom - roomData.Top;
		if (roomData.Size <= 24 || roomData.Width <= 3 || roomData.Height <= 3)
		{
			return;
		}
		int num = ((roomData.Width < roomData.Height) ? 1 : 0);
		if (Stat.Random(1, 4) == 4)
		{
			num = 1 - num;
		}
		int num3;
		if (num == 0)
		{
			int num2 = 0;
			num3 = Stat.Random(roomData.Left + 2, roomData.Right - 2);
			while (num2 < 10 && (Map[num3 - 1, roomData.Top] != BuildingTemplateTile.Inside || Map[num3 + 1, roomData.Top] != BuildingTemplateTile.Inside || Map[num3 - 1, roomData.Bottom] != BuildingTemplateTile.Inside || Map[num3 + 1, roomData.Bottom] != BuildingTemplateTile.Inside))
			{
				num2++;
				num3 = Stat.Random(roomData.Left + 2, roomData.Right - 2);
			}
			if (num2 == 10)
			{
				return;
			}
			for (int k = roomData.Top; k <= roomData.Bottom; k++)
			{
				if (roomData.Room[num3, k] == 1)
				{
					Map[num3, k] = BuildingTemplateTile.Wall;
				}
			}
			num2 = 0;
			int num4 = Stat.Random(roomData.Top + 2, roomData.Bottom - 2);
			while (num2 < 10 && Map[num3, num4] != BuildingTemplateTile.Wall)
			{
				num2++;
				num4 = Stat.Random(roomData.Top + 2, roomData.Bottom - 2);
			}
			if (num2 == 10)
			{
				return;
			}
			Map[num3, num4] = BuildingTemplateTile.Door;
			FillRoom(num3 - 1, num4);
			FillRoom(num3 + 1, num4);
		}
		if (num != 1)
		{
			return;
		}
		int num5 = 0;
		num3 = Stat.Random(roomData.Top + 2, roomData.Bottom - 2);
		while (num5 < 10 && (Map[roomData.Left, num3 - 1] != BuildingTemplateTile.Inside || Map[roomData.Left, num3 + 1] != BuildingTemplateTile.Inside || Map[roomData.Right, num3 - 1] != BuildingTemplateTile.Inside || Map[roomData.Right, num3 + 1] != BuildingTemplateTile.Inside))
		{
			num5++;
			num3 = Stat.Random(roomData.Top + 2, roomData.Bottom - 2);
		}
		if (num5 == 10)
		{
			return;
		}
		for (int l = roomData.Left; l <= roomData.Right; l++)
		{
			if (roomData.Room[l, num3] == 1)
			{
				Map[l, num3] = BuildingTemplateTile.Wall;
			}
		}
		num5 = 0;
		int num6 = Stat.Random(roomData.Left + 2, roomData.Right - 2);
		while (num5 < 10 && Map[num6, num3] != BuildingTemplateTile.Wall)
		{
			num5++;
			num6 = Stat.Random(roomData.Left + 2, roomData.Right - 2);
		}
		if (num5 != 10)
		{
			Map[num6, num3] = BuildingTemplateTile.Door;
			FillRoom(num6, num3 - 1);
			FillRoom(num6, num3 + 1);
		}
	}

	private void FloodRoom(int x, int y, RoomData Data, int n)
	{
		if (x >= 0 && x < Width && y >= 0 && y < Height && Data.Room[x, y] == 0 && Map[x, y] == BuildingTemplateTile.Inside)
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
			Rooms[x, y] = n;
			Data.Room[x, y] = n;
			Data.Size++;
			FloodRoom(x - 1, y, Data, n);
			FloodRoom(x + 1, y, Data, n);
			FloodRoom(x, y - 1, Data, n);
			FloodRoom(x, y + 1, Data, n);
		}
	}

	private void FloodInsideWall(int xp, int yp)
	{
		if (xp >= 0 && xp < Width && yp >= 0 && yp < Height && Rooms[xp, yp] != 2 && Rooms[xp, yp] != 3 && Map[xp, yp] != BuildingTemplateTile.OutsideWall)
		{
			if (Rooms[xp, yp] > 0 && Map[xp, yp] <= BuildingTemplateTile.Void)
			{
				Rooms[xp, yp] = 2;
				return;
			}
			Rooms[xp, yp] = 3;
			Map[xp, yp] = BuildingTemplateTile.Inside;
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
		if (xp >= 0 && xp < Width && yp >= 0 && yp < Height && Map[xp, yp] != BuildingTemplateTile.OutsideWall && Map[xp, yp] != BuildingTemplateTile.Outside)
		{
			if (Map[xp, yp] == BuildingTemplateTile.Wall)
			{
				Map[xp, yp] = BuildingTemplateTile.OutsideWall;
				return;
			}
			Map[xp, yp] = BuildingTemplateTile.Outside;
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

	public void Draw(RoomData Data)
	{
		Popup._ScreenBuffer.Clear();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (Data != null && Data.Room[i, j] > 0)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write((short)(49 + Data.Room[i, j]));
				}
				if (Map[i, j] == BuildingTemplateTile.Inside)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(58);
				}
				if (Map[i, j] == BuildingTemplateTile.Void)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(88);
				}
				if (Map[i, j] == BuildingTemplateTile.Outside)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(44);
				}
				if (Map[i, j] == BuildingTemplateTile.OutsideWall)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(35);
				}
				if (Map[i, j] == BuildingTemplateTile.Wall)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(42);
				}
				if (Map[i, j] == BuildingTemplateTile.Door)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write(43);
				}
				if (Rooms[i, j] > 0)
				{
					Popup._ScreenBuffer.Goto(i, j);
					Popup._ScreenBuffer.Write((short)(65 + Rooms[i, j]));
				}
			}
			Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
		}
		Keyboard.getch();
	}
}
