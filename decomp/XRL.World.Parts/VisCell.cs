namespace XRL.World.Parts;

public class VisCell
{
	public Cell C;

	public int Turns;

	public VisCell(Cell _C, int _Turns)
	{
		C = _C;
		Turns = _Turns;
	}

	public override string ToString()
	{
		return C.X + "." + C.Y + "." + C.ParentZone.ZoneID;
	}
}
