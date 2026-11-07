using System;

namespace XRL.World.AI.Pathfinding;

[Serializable]
public class CellNavigationValue : IComparable
{
	public Cell C;

	public Cell Parent;

	public string Direction;

	public double Cost;

	public double Estimate;

	public double Total;

	public CellNavigationValue(double Cost, double Estimate, Cell C, Cell Parent, string Direction)
	{
		this.Cost = Cost;
		this.Estimate = Estimate;
		Total = this.Cost + this.Estimate;
		this.C = C;
		this.Parent = Parent;
		this.Direction = Direction;
	}

	public CellNavigationValue Set(double Cost = 0.0, double Estimate = 0.0, Cell C = null, Cell Parent = null, string Direction = null)
	{
		this.Cost = Cost;
		this.Estimate = Estimate;
		Total = this.Cost + this.Estimate;
		this.C = C;
		this.Parent = Parent;
		this.Direction = Direction;
		return this;
	}

	public int CompareTo(object Nav2)
	{
		return Total.CompareTo((Nav2 as CellNavigationValue).Total);
	}

	public int CompareTo(CellNavigationValue Nav2)
	{
		return Total.CompareTo(Nav2.Total);
	}
}
