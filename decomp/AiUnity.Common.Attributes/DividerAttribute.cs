using UnityEngine;

namespace AiUnity.Common.Attributes;

public class DividerAttribute : PropertyAttribute
{
	public readonly string col = "grey";

	public readonly float space;

	public readonly float thickness = 1f;

	public float widthPct = 1f;

	public DividerAttribute()
	{
	}

	public DividerAttribute(string col)
	{
		this.col = col;
	}

	public DividerAttribute(float widthPct)
	{
		this.widthPct = widthPct;
	}

	public DividerAttribute(string col, float thickness)
	{
		this.col = col;
		this.thickness = thickness;
	}

	public DividerAttribute(float widthPct, float thickness)
	{
		this.widthPct = widthPct;
		this.thickness = thickness;
	}

	public DividerAttribute(string col, float thickness, float widthPct)
	{
		this.col = col;
		this.thickness = thickness;
		this.widthPct = widthPct;
	}

	public DividerAttribute(float widthPct, float thickness, float space)
	{
		this.widthPct = widthPct;
		this.thickness = thickness;
		this.space = space;
	}

	public DividerAttribute(string col, float thickness, float widthPct, float space)
	{
		this.col = col;
		this.thickness = thickness;
		this.widthPct = widthPct;
		this.space = space;
	}
}
