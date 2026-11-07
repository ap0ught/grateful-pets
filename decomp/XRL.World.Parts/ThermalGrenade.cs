using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ThermalGrenade : IGrenade
{
	public int Radius = 1;

	public int TemperatureDelta = 1000;

	public override bool SameAs(IPart p)
	{
		if ((p as ThermalGrenade).TemperatureDelta != TemperatureDelta)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID)
		{
			return ID == GetComponentAdjacentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(8);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(8);
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		DidX("explode", null, "!");
		ParentObject.FireEvent(Event.New("Detonated", "Owner", Actor));
		PlayWorldSound(GetPropertyOrTag("DetonatedSound", "Sounds/Grenade/sfx_grenade_highExplosive_explode"), 1f, 0f, Combat: true);
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		C.GetAdjacentCells(Radius, list, LocalOnly: false);
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		bool flag = false;
		int phase = ParentObject.GetPhase();
		foreach (Cell item in list)
		{
			item.TemperatureChange(TemperatureDelta, Actor, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase);
			if (!item.IsVisible())
			{
				continue;
			}
			if (TemperatureDelta > 0)
			{
				scrapBuffer.Goto(item);
				switch (Stat.Random(1, 3))
				{
				case 1:
					scrapBuffer.Write("&R*");
					break;
				case 2:
					scrapBuffer.Write("&W*");
					break;
				default:
					scrapBuffer.Write("&r*");
					break;
				}
			}
			else
			{
				scrapBuffer.Goto(item);
				switch (Stat.Random(1, 3))
				{
				case 1:
					scrapBuffer.Write("&C*");
					break;
				case 2:
					scrapBuffer.Write("&Y*");
					break;
				default:
					scrapBuffer.Write("&c*");
					break;
				}
			}
			GameObject firstObject = item.GetFirstObject();
			if (firstObject != null)
			{
				if (TemperatureDelta > 0)
				{
					for (int i = 0; i < 5; i++)
					{
						firstObject.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int j = 0; j < 5; j++)
					{
						firstObject.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int k = 0; k < 5; k++)
					{
						firstObject.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
				}
				else
				{
					for (int l = 0; l < 5; l++)
					{
						firstObject.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int m = 0; m < 5; m++)
					{
						firstObject.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int n = 0; n < 5; n++)
					{
						firstObject.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
				}
			}
			flag = true;
		}
		if (flag)
		{
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(20);
		}
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
