using System.Collections.Generic;

namespace XRL.World.Parts;

public class MissilePath
{
	public List<Cell> Path = new List<Cell>();

	public List<float> Cover;

	public float Angle;

	public float X0;

	public float Y0;

	public float X1;

	public float Y1;

	public void Reset()
	{
		Path.Clear();
		Cover?.Clear();
		Angle = 0f;
		X0 = 0f;
		Y0 = 0f;
		X1 = 0f;
		Y1 = 0f;
	}
}
