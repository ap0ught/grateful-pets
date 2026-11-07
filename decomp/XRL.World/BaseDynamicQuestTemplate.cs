using System;
using XRL.Core;

namespace XRL.World;

[Serializable]
public abstract class BaseDynamicQuestTemplate
{
	public long id;

	public ZoneManager zoneManager => XRLCore.Core.Game.ZoneManager;

	public abstract void init(DynamicQuestContext context);
}
