using System.Linq;
using System.Text;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public abstract class IConnectionBuilder
{
	public int X = -1;

	public int Y = -1;

	public int Range = 5;

	public virtual int GetMouthX(Zone Z)
	{
		return Stat.Rnd.Next(Range, Z.Width - Range);
	}

	public virtual int GetMouthY(Zone Z)
	{
		return Stat.Rnd.Next(Range, Z.Height - Range);
	}

	public bool ConnectionMouth(Zone Z, string Type, string Direction, string Suffix = "Mouth")
	{
		StringBuilder stringBuilder = Event.NewStringBuilder(Type).Append(Direction).Append(Suffix);
		string full = stringBuilder.ToString();
		if (The.ZoneManager.GetZoneConnections(Z.ZoneID).Any((ZoneConnection ZC) => ZC.Type == full))
		{
			return true;
		}
		string type;
		string targetZoneDirection;
		int num;
		int num2;
		int x;
		int y;
		switch (Direction)
		{
		case "North":
			type = stringBuilder.Clear().Append(Type).Append("South")
				.Append(Suffix)
				.ToString();
			if (MatchConnection(Z, type, targetZoneDirection = "n", full, -1, 0))
			{
				return true;
			}
			num = ((X < 0) ? GetMouthX(Z) : X);
			num2 = 0;
			x = num;
			y = Z.Height - 1;
			break;
		case "East":
			type = stringBuilder.Clear().Append(Type).Append("West")
				.Append(Suffix)
				.ToString();
			if (MatchConnection(Z, type, targetZoneDirection = "e", full, Z.Width - 1, -1))
			{
				return true;
			}
			num = Z.Width - 1;
			num2 = ((Y < 0) ? GetMouthY(Z) : Y);
			x = 0;
			y = num2;
			break;
		case "South":
			type = stringBuilder.Clear().Append(Type).Append("North")
				.Append(Suffix)
				.ToString();
			if (MatchConnection(Z, type, targetZoneDirection = "s", full, -1, Z.Height - 1))
			{
				return true;
			}
			num = ((X < 0) ? GetMouthX(Z) : X);
			num2 = Z.Height - 1;
			x = num;
			y = 0;
			break;
		case "West":
			type = stringBuilder.Clear().Append(Type).Append("East")
				.Append(Suffix)
				.ToString();
			if (MatchConnection(Z, type, targetZoneDirection = "w", full, 0, -1))
			{
				return true;
			}
			num = 0;
			num2 = ((Y < 0) ? GetMouthY(Z) : Y);
			x = Z.Width - 1;
			y = num2;
			break;
		default:
			MetricsManager.LogError("Invalid connection direction: " + Direction);
			return true;
		}
		Z.GetCell(num, num2).Clear();
		Z.CacheZoneConnection("-", num, num2, full, null);
		Z.CacheZoneConnection(targetZoneDirection, x, y, type, null);
		return true;
	}

	public bool MatchConnection(Zone Z, string Type, string Direction, string Origin, int X, int Y)
	{
		string zoneFromIDAndDirection = The.ZoneManager.GetZoneFromIDAndDirection(Z.ZoneID, Direction);
		if (!The.ZoneManager.IsZoneBuilt(zoneFromIDAndDirection))
		{
			return false;
		}
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(zoneFromIDAndDirection))
		{
			if (zoneConnection.Type == Type)
			{
				X = ((X == -1) ? zoneConnection.X : X);
				Y = ((Y == -1) ? zoneConnection.Y : Y);
				Z.GetCell(X, Y).Clear();
				Z.CacheZoneConnection("-", X, Y, Origin, null);
				return true;
			}
		}
		return false;
	}

	public virtual void GetStartPosition(Zone Z, out int X, out int Y)
	{
		int num = 0;
		do
		{
			X = Stat.Random(5, 75);
			Y = Stat.Random(5, 15);
		}
		while (!Z.GetCell(X, Y).IsEmpty() && num++ < 1000);
	}

	public bool ConnectionStart(Zone Z, string Type, string Suffix = "Mouth")
	{
		string full = Event.NewStringBuilder(Type).Append("Start").Append(Suffix)
			.ToString();
		if (The.ZoneManager.GetZoneConnections(Z.ZoneID).Any((ZoneConnection ZC) => ZC.Type == full))
		{
			return true;
		}
		if (X < 0 || Y < 0)
		{
			GetStartPosition(Z, out X, out Y);
		}
		Z.CacheZoneConnection("-", X, Y, full, null);
		return true;
	}
}
