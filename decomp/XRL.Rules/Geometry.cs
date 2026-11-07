using System;
using System.Collections.Generic;
using Genkit;
using UnityEngine;
using XRL.World;

namespace XRL.Rules;

public class Geometry
{
	public static int Distance(Point P1, Point P2)
	{
		int num = P1.X - P2.X;
		int num2 = P1.Y - P2.Y;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public static int Distance(int X1, int Y1, int X2, int Y2)
	{
		int num = X1 - X2;
		int num2 = Y1 - Y2;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public static int Distance(int X1, int Y1, XRL.World.GameObject obj)
	{
		int num = X1 - obj.Physics.CurrentCell.X;
		int num2 = Y1 - obj.Physics.CurrentCell.Y;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public static bool TestAngleTo(Location2D fulcrum, Location2D from, Location2D to, float MaxAngle)
	{
		Vector2 vector = new Vector2(from.X - fulcrum.X, from.Y - fulcrum.Y);
		Vector2 to2 = new Vector2(to.X - fulcrum.X, to.Y - fulcrum.Y);
		if (Vector2.Angle(vector, to2) <= MaxAngle)
		{
			return true;
		}
		float num = 0.25f;
		Vector2 vector2 = new Vector2(from.X - fulcrum.X, from.Y - fulcrum.Y);
		to2 = new Vector2(to.X - fulcrum.X, to.Y - fulcrum.Y);
		if (Vector2.Angle(vector2, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector3 = new Vector2((float)(from.X - fulcrum.X) - num, (float)(from.Y - fulcrum.Y) - num);
		to2 = new Vector2((float)(to.X - fulcrum.X) - num, (float)(to.Y - fulcrum.Y) - num);
		if (Vector2.Angle(vector3, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector4 = new Vector2((float)(from.X - fulcrum.X) - num, (float)(from.Y - fulcrum.Y) - num);
		to2 = new Vector2((float)(to.X - fulcrum.X) + num, (float)(to.Y - fulcrum.Y) + num);
		if (Vector2.Angle(vector4, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector5 = new Vector2((float)(from.X - fulcrum.X) + num, (float)(from.Y - fulcrum.Y) + num);
		to2 = new Vector2((float)(to.X - fulcrum.X) - num, (float)(to.Y - fulcrum.Y) - num);
		if (Vector2.Angle(vector5, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector6 = new Vector2((float)(from.X - fulcrum.X) - num, (float)(from.Y - fulcrum.Y) + num);
		to2 = new Vector2((float)(to.X - fulcrum.X) + num, (float)(to.Y - fulcrum.Y) - num);
		if (Vector2.Angle(vector6, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector7 = new Vector2((float)(from.X - fulcrum.X) + num, (float)(from.Y - fulcrum.Y) - num);
		to2 = new Vector2((float)(to.X - fulcrum.X) - num, (float)(to.Y - fulcrum.Y) + num);
		if (Vector2.Angle(vector7, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector8 = new Vector2((float)(from.X - fulcrum.X) + num, (float)(from.Y - fulcrum.Y) - num);
		to2 = new Vector2((float)(to.X - fulcrum.X) - num, (float)(to.Y - fulcrum.Y) - num);
		if (Vector2.Angle(vector8, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector9 = new Vector2((float)(from.X - fulcrum.X) - num, (float)(from.Y - fulcrum.Y) + num);
		to2 = new Vector2((float)(to.X - fulcrum.X) - num, (float)(to.Y - fulcrum.Y) - num);
		if (Vector2.Angle(vector9, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector10 = new Vector2((float)(from.X - fulcrum.X) - num, (float)(from.Y - fulcrum.Y) - num);
		to2 = new Vector2((float)(to.X - fulcrum.X) - num, (float)(to.Y - fulcrum.Y) + num);
		if (Vector2.Angle(vector10, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector11 = new Vector2((float)(from.X - fulcrum.X) - num, (float)(from.Y - fulcrum.Y) - num);
		to2 = new Vector2((float)(to.X - fulcrum.X) + num, (float)(to.Y - fulcrum.Y) - num);
		if (Vector2.Angle(vector11, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector12 = new Vector2((float)(from.X - fulcrum.X) + num, (float)(from.Y - fulcrum.Y) - num);
		to2 = new Vector2((float)(to.X - fulcrum.X) + num, (float)(to.Y - fulcrum.Y) - num);
		if (Vector2.Angle(vector12, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector13 = new Vector2((float)(from.X - fulcrum.X) - num, (float)(from.Y - fulcrum.Y) + num);
		to2 = new Vector2((float)(to.X - fulcrum.X) - num, (float)(to.Y - fulcrum.Y) + num);
		if (Vector2.Angle(vector13, to2) <= MaxAngle)
		{
			return true;
		}
		Vector2 vector14 = new Vector2((float)(from.X - fulcrum.X) + num, (float)(from.Y - fulcrum.Y) + num);
		to2 = new Vector2((float)(to.X - fulcrum.X) + num, (float)(to.Y - fulcrum.Y) + num);
		if (Vector2.Angle(vector14, to2) <= MaxAngle)
		{
			return true;
		}
		return false;
	}

	public static List<Location2D> GetCone(Location2D source, Location2D target, int Length, int Angle, List<Location2D> result = null)
	{
		if (result == null)
		{
			result = new List<Location2D>();
		}
		else
		{
			result.Clear();
		}
		if (source.Distance(target) == 0)
		{
			result.Add(source);
			return result;
		}
		foreach (Location2D item in source.YieldAdjacent(Length + 1))
		{
			if (item.X < 80 && item.Y < 25 && item.Distance(source) <= Length && TestAngleTo(source, item, target, (float)Angle / 2f))
			{
				result.Add(item);
			}
		}
		return result;
	}
}
