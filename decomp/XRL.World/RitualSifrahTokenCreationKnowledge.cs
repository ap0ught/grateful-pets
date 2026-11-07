using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenCreationKnowledge : TinkeringSifrahTokenCreationKnowledge
{
	public RitualSifrahTokenCreationKnowledge()
	{
	}

	public RitualSifrahTokenCreationKnowledge(string Blueprint)
		: base(Blueprint)
	{
	}

	public RitualSifrahTokenCreationKnowledge(GameObject Object)
		: base(Object)
	{
	}
}
