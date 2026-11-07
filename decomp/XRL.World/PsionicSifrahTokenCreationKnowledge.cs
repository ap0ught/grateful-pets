using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenCreationKnowledge : TinkeringSifrahTokenCreationKnowledge
{
	public PsionicSifrahTokenCreationKnowledge()
	{
	}

	public PsionicSifrahTokenCreationKnowledge(string Blueprint)
		: base(Blueprint)
	{
	}

	public PsionicSifrahTokenCreationKnowledge(GameObject Object)
		: base(Object)
	{
	}
}
