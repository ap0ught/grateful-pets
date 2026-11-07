using System;

namespace XRL.World.Units;

[Serializable]
public class GameObjectPlaceholderUnit : GameObjectUnit
{
	public string Description;

	public override bool CanInscribe()
	{
		return false;
	}

	public override string GetDescription(bool Inscription = false)
	{
		return Description;
	}
}
