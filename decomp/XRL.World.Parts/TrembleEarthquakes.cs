using System;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class TrembleEarthquakes : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckQuake();
		return base.HandleEvent(E);
	}

	public void CheckQuake()
	{
		if (Stat.Random(1, 200) == 1)
		{
			Quake();
		}
	}

	public void Quake()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone.IsActive())
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
			The.Core.RenderBaseToBuffer(scrapBuffer);
			scrapBuffer.Shake(500, 25, Popup._TextConsole);
			if (currentZone.IsOutside())
			{
				IComponent<GameObject>.AddPlayerMessage("The ground shakes violently!");
				return;
			}
			IComponent<GameObject>.AddPlayerMessage("The ground shakes violently and loose rock falls from the ceiling!");
			RocksFall(currentZone);
		}
	}

	public void RocksFall(Zone Zone)
	{
		for (int i = 0; i < Zone.Height; i++)
		{
			for (int j = 0; j < Zone.Width; j++)
			{
				Cell cell = Zone.GetCell(j, i);
				if (cell == null)
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (!gameObject.IsCreature || !gameObject.IsPlayerControlled() || !gameObject.PhaseMatches(1))
					{
						continue;
					}
					int num = Stat.RollDamagePenetrations(Stats.GetCombatAV(gameObject), 0, 0);
					if (num > 0)
					{
						string resultColor = Stat.GetResultColor(num);
						int num2 = 0;
						for (int l = 0; l < num; l++)
						{
							num2 += Stat.Random(1, 3);
						}
						gameObject.TakeDamage(num2, "from falling rocks! {{" + resultColor + "|(x" + num + ")}}", "Crushing Cudgel", "You were crushed by falling rocks.", gameObject.Does("were", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " @@crushed by falling rocks.");
					}
				}
			}
		}
	}
}
