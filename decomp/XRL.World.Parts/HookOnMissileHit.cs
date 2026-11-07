using System;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class HookOnMissileHit : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileEntering");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileEntering")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			Cell cell = E.GetParameter("Cell") as Cell;
			if (!(E.GetParameter("Path") is MissilePath missilePath))
			{
				return true;
			}
			foreach (GameObject item in cell.GetObjectsWithPart("Physics"))
			{
				if (item.Weight <= 0 || !item.Physics.IsReal || !item.CanBeInvoluntarilyMoved() || item.HasTagOrProperty("ExcavatoryTerrainFeature"))
				{
					continue;
				}
				int i = 1;
				for (int count = missilePath.Path.Count; i < count; i++)
				{
					if (!missilePath.Path[i].IsEmpty() || !item.CurrentCell.RemoveObject(item))
					{
						continue;
					}
					try
					{
						if (item.IsCombat)
						{
							TextConsole textConsole = Look._TextConsole;
							ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
							TextConsole.ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
							XRLCore.Core.RenderMapToBuffer(scrapBuffer);
							for (int num = missilePath.Path.Count - 1; num >= i; num--)
							{
								scrapBuffer.Copy(TextConsole.ScrapBuffer2);
								scrapBuffer.Goto(cell.X, cell.Y);
								scrapBuffer.Write("&b^b ");
								for (int j = 1; j < missilePath.Path.Count - 1; j++)
								{
									scrapBuffer.Goto(missilePath.Path[j].X, missilePath.Path[j].Y);
									if (50.in100())
									{
										scrapBuffer.Write("&b^b ");
									}
									else
									{
										scrapBuffer.Write("&b^r ");
									}
								}
								if (!string.IsNullOrEmpty(item.Render.TileColor))
								{
									if (string.IsNullOrEmpty(item.Render.DetailColor))
									{
										scrapBuffer[missilePath.Path[num].X, missilePath.Path[num].Y].Detail = The.Color.DarkBlack;
									}
									else
									{
										scrapBuffer[missilePath.Path[num].X, missilePath.Path[num].Y].Detail = ConsoleLib.Console.ColorUtility.ColorMap[item.Render.DetailColor[0]];
									}
								}
								else
								{
									scrapBuffer.Goto(missilePath.Path[num].X, missilePath.Path[num].Y);
									scrapBuffer.Write(item.Render.ColorString + item.Render.RenderString);
								}
								if (!string.IsNullOrEmpty(item.Render.Tile))
								{
									scrapBuffer[missilePath.Path[num].X, missilePath.Path[num].Y].Tile = item.Render.Tile;
								}
								textConsole.DrawBuffer(scrapBuffer);
								textConsole.WaitFrame();
							}
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("Exception during hookonhit: " + ex.ToString());
					}
					missilePath.Path[i].AddObject(item);
					IComponent<GameObject>.XDidYToZ(item, "are", "dragged toward", gameObjectParameter, null, null, null, null, null, item);
					break;
				}
			}
		}
		return base.FireEvent(E);
	}
}
