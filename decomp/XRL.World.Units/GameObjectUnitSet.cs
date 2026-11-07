using System;
using System.Collections.Generic;

namespace XRL.World.Units;

[Serializable]
public class GameObjectUnitSet : GameObjectUnit
{
	public List<GameObjectUnit> Units = new List<GameObjectUnit>();
}
