using System;
using ConsoleLib.Console;
using Genkit;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ExplodeAfterTurns : IPart
{
	public int Turns = 2;

	public int Force = 10000;

	public string Damage = "0";

	public string VFX = "MissileWeaponsEffects/vls_impact";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool FinalRender(RenderEvent E)
	{
		E.WantsToPaint = true;
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		int num = XRLCore.CurrentFrame % 60;
		Location2D location2D = ParentObject?.CurrentCell?.Location;
		ConsoleChar consoleChar;
		if (num < 30)
		{
			int num2 = num / 10;
			if (location2D != null)
			{
				for (int i = -num2; i <= num2; i++)
				{
					for (int j = -num2; j <= num2; j++)
					{
						Location2D location2D2 = Location2D.Get(location2D.X + i, location2D.Y + j);
						if ((object)location2D2 != null && location2D2.Distance(location2D) == num2)
						{
							consoleChar = buffer.get(location2D2.X, location2D2.Y);
							if (consoleChar != null)
							{
								consoleChar.Tile = null;
								consoleChar.Char = '!';
								consoleChar.Foreground = The.Color.R;
							}
						}
					}
				}
			}
		}
		consoleChar = buffer.get(location2D.X, location2D.Y);
		if (consoleChar != null)
		{
			consoleChar.Tile = null;
			consoleChar.Char = 'X';
			consoleChar.Foreground = The.Color.R;
		}
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Turns--;
		if (Turns <= 0)
		{
			Detonate(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public void Detonate(GameObject Owner = null)
	{
		if (!VFX.IsNullOrEmpty() && Options.UseParticleVFX && ParentObject.Physics?.CurrentZone?.IsActive() == true)
		{
			CombatJuice.playPrefabAnimation(ParentObject.Physics.CurrentCell.Location, VFX);
			CombatJuiceWait(0.5f);
		}
		DidX("explode", null, "!");
		Owner = ParentObject.CurrentZone?.FindObject((GameObject o) => o.HasTagOrProperty("HasVLS"));
		ParentObject.Explode(Force, Owner, Damage);
	}
}
