using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class FeralLahPod : IPart
{
	public string damage = "1d10";

	public bool exploding;

	public int countdown = 2;

	public bool blowingup;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != BeforeRenderEvent.ID || !exploding))
		{
			return ID == GetDifficultyEvaluationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (exploding)
		{
			int num = XRLCore.CurrentFrame % 18 / 6;
			if (num == 0)
			{
				ParentObject.Render.TileColor = "&R^r";
				ParentObject.Render.DetailColor = "W";
				ParentObject.Render.ColorString = "&R^r";
			}
			if (num == 1)
			{
				ParentObject.Render.TileColor = "&W^R";
				ParentObject.Render.DetailColor = "r";
				ParentObject.Render.ColorString = "&W^R";
			}
			if (num == 2)
			{
				ParentObject.Render.TileColor = "&r^W";
				ParentObject.Render.DetailColor = "R";
				ParentObject.Render.ColorString = "&r^W";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDifficultyEvaluationEvent E)
	{
		if (GameObject.Validate(E.Actor))
		{
			E.MinimumRating(5);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDie");
		Registrar.Register("BeginTakeAction");
		Registrar.Register("EndAction");
		base.Register(Object, Registrar);
	}

	public void Explode()
	{
		if (blowingup)
		{
			return;
		}
		if (ParentObject.HasStat("XP") && !ParentObject.IsHero())
		{
			ParentObject.GetStat("XPValue").BaseValue = 0;
		}
		blowingup = true;
		Cell cell = ParentObject.GetCurrentCell();
		if (cell != null)
		{
			List<Cell> list = new List<Cell>();
			cell.GetAdjacentCells(2, list);
			List<Cell> list2 = new List<Cell>();
			foreach (Cell item in list)
			{
				if (ParentObject.HasUnobstructedLineTo(item))
				{
					list2.Add(item);
				}
			}
			if (cell.ParentZone.IsActive())
			{
				for (int i = 0; i < 3; i++)
				{
					TextConsole textConsole = Look._TextConsole;
					ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
					XRLCore.Core.RenderMapToBuffer(scrapBuffer);
					foreach (Cell item2 in list2)
					{
						if (item2.PathDistanceTo(cell) == i && item2.IsVisible())
						{
							scrapBuffer.Goto(item2.X, item2.Y);
							if (Stat.RandomCosmetic(1, 2) == 1)
							{
								scrapBuffer.Write("&R*");
							}
							else
							{
								scrapBuffer.Write("&W*");
							}
						}
					}
					textConsole.DrawBuffer(scrapBuffer);
					Thread.Sleep(10);
				}
			}
			foreach (Cell item3 in list2)
			{
				foreach (GameObject item4 in item3.GetObjectsInCell())
				{
					if (item4.PhaseMatches(ParentObject))
					{
						item4.TakeDamage(damage.RollCached(), "from %t explosion!", "Explosion", null, null, null, ParentObject);
					}
				}
			}
		}
		ParentObject.Destroy();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie")
		{
			Explode();
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (XRLCore.Core.Calm)
			{
				return true;
			}
			if (ParentObject.Brain.Target != null && ParentObject.Brain.Target.DistanceTo(ParentObject) <= 1)
			{
				exploding = true;
			}
			if (exploding)
			{
				countdown--;
				if (countdown <= 0)
				{
					Explode();
				}
				return false;
			}
		}
		else if (E.ID == "EndAction")
		{
			if (XRLCore.Core.Calm)
			{
				return true;
			}
			if (ParentObject.Brain.Target != null && ParentObject.Brain.Target.DistanceTo(ParentObject) <= 1)
			{
				exploding = true;
			}
		}
		return base.FireEvent(E);
	}
}
