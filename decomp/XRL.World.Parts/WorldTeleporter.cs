using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class WorldTeleporter : IPart
{
	public string TargetZone = "";

	public string TargetObject = "";

	public int MaxLevel = -1;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckAgainstPlayer();
		base.TurnTick(TimeTick, Amount);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectEnteredCell");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	private void CheckAgainstPlayer()
	{
		if (The.Player.Stat("Level") > MaxLevel && ParentObject.CurrentCell != null)
		{
			ParentObject.CurrentCell.AddObject("Shale");
			ParentObject.Destroy();
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			CheckAgainstPlayer();
		}
		else if (E.ID == "ObjectEnteredCell")
		{
			if (ParentObject.IsNowhere())
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter == ParentObject)
			{
				return true;
			}
			if (gameObjectParameter == null)
			{
				return true;
			}
			if (gameObjectParameter.Stat("Level") > MaxLevel)
			{
				return true;
			}
			Render render = gameObjectParameter.Render;
			if (render == null || render.RenderLayer == 0)
			{
				return true;
			}
			if (TargetZone[0] == '$')
			{
				TargetZone = The.Game.GetStringGameState(TargetZone);
			}
			gameObjectParameter.ApplyEffect(new Lost(9999, TargetZone, null, 10, DisableUnlost: true));
			Zone zone = The.ZoneManager.GetZone(TargetZone);
			Cell cell = null;
			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					Cell cell2 = zone.GetCell(j, i);
					foreach (GameObject item in cell2.GetObjectsInCell())
					{
						if (!(item.Blueprint == TargetObject))
						{
							continue;
						}
						foreach (Cell adjacentCell in cell2.GetAdjacentCells())
						{
							if (adjacentCell.IsEmpty())
							{
								cell = adjacentCell;
								goto end_IL_0199;
							}
						}
					}
				}
				continue;
				end_IL_0199:
				break;
			}
			if (cell == null)
			{
				return true;
			}
			The.ZoneManager.SetActiveZone(TargetZone);
			gameObjectParameter.TeleportTo(cell, 0);
			The.ZoneManager.ProcessGoToPartyLeader();
			if (gameObjectParameter.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You are sucked through the surface of the sphere!", 'C');
			}
		}
		return base.FireEvent(E);
	}
}
