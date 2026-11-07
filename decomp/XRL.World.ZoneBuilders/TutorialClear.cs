using System;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class TutorialClear
{
	public string ZoneTemplate;

	public bool BuildZone(Zone Z)
	{
		if (The.Game.GetStringGameState("JoppaWorldTutorial") != "Yes")
		{
			Z.GetObjectsWithTagOrProperty("Tutorial").ForEach((GameObject o) =>
			{
				o?.Obliterate(null, Silent: true);
			});
			Z.BuildReachableMap();
			if (!string.IsNullOrEmpty(ZoneTemplate))
			{
				ZoneTemplateManager.Templates[ZoneTemplate].Execute(Z);
			}
		}
		return true;
	}
}
