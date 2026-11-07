using System.Collections.Generic;
using XRL.World;

namespace XRL.Core;

public class JoinPartyLeaderCommand : IActionCommand, IComposite
{
	private static JoinPartyLeaderCommand Instance = new JoinPartyLeaderCommand();

	public static void Issue()
	{
		ActionManager actionManager = The.ActionManager;
		if (!actionManager.HasAction<JoinPartyLeaderCommand>())
		{
			actionManager.EnqueueAction(Instance);
		}
	}

	public void Execute(XRLGame Game, ActionManager Manager)
	{
		int num = 0;
		bool flag = true;
		while (flag && ++num < 100)
		{
			Dictionary<string, Zone> cachedZones = Game.ZoneManager.CachedZones;
			int count = cachedZones.Count;
			flag = false;
			foreach (Zone value in cachedZones.Values)
			{
				for (int i = 0; i < value.Height; i++)
				{
					for (int j = 0; j < value.Width; j++)
					{
						Cell cell = value.GetCell(j, i);
						int k = 0;
						for (int num2 = cell.Objects.Count; k < num2; k++)
						{
							if (cell.Objects[k].GoToPartyLeader())
							{
								flag = true;
								k--;
								num2--;
								if (num2 > cell.Objects.Count)
								{
									num2 = cell.Objects.Count;
								}
							}
						}
					}
				}
				if (cachedZones.Count != count)
				{
					break;
				}
			}
		}
	}
}
