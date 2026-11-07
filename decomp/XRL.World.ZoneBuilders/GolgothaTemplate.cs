using System;
using System.Collections.Generic;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class GolgothaTemplate : IComposite
{
	public Box MainBuilding;

	public List<Box> Chutes;
}
