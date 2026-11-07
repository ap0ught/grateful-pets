using System;

namespace XRL.World.AI;

[Serializable]
public class ObjectOpinion
{
	public int Disposition;

	public ObjectOpinion(int Feeling)
	{
		Disposition = Feeling;
	}

	public ObjectOpinion(ObjectOpinion src)
	{
		Disposition = src.Disposition;
	}
}
