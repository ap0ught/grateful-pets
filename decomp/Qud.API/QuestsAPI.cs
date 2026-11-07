using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World;

namespace Qud.API;

public static class QuestsAPI
{
	public static Quest fabricateEmptyQuest()
	{
		return new Quest
		{
			ID = Guid.NewGuid().ToString()
		};
	}

	public static IEnumerable<Quest> allQuests()
	{
		foreach (Quest value in XRLCore.Core.Game.Quests.Values)
		{
			yield return value;
		}
	}
}
