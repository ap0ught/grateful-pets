using System;
using XRL.Collections;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
[HasModSensitiveStaticCache]
public class PluckablePolyp : IPart
{
	public static readonly string COMMAND_NAME = "PluckPolyp";

	[ModSensitiveStaticCache(true)]
	private static StringMap<string> _pluckableTileCache = new StringMap<string>();

	public bool Plucked;

	public long PluckTime;

	public long RegrowTime = 3600L;

	public int CacheChance = 500;

	public string RevealObject = "PolypCache";

	[Obsolete]
	public string OldTile;

	[Obsolete]
	public string OldDisplayName;

	[Obsolete]
	public string PluckSounds;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void FinalizeRead(SerializationReader Reader)
	{
		base.FinalizeRead(Reader);
		if (Reader.FileVersion < 409 && !Plucked)
		{
			Render render = ParentObject.Render;
			render.ColorString = "&C";
			render.Tile = render.Tile?.Replace("_both_", "_metal_");
			render.DisplayName = render.DisplayName.Replace(" with coral growth", "");
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!Plucked && CanBePlucked(E.Actor))
		{
			E.AddAction("Pluck", "pluck", COMMAND_NAME, null, 'p', FireOnActor: false, 50);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			Pluck(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!Plucked && E.Object?.Brain != null && CanBePlucked(E.Object) && !E.Object.HasPropertyOrTag("Polypwalking"))
		{
			Pluck(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command == null && !Plucked && CanBePlucked(E.Actor))
		{
			E.Command = COMMAND_NAME;
			E.AllowRetry = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!Plucked)
		{
			E.AddWithClause("coral growth");
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (!Plucked)
		{
			if (!E.Tile.IsNullOrEmpty())
			{
				string tile = ParentObject.Render.Tile;
				if (E.Tile == tile)
				{
					if (!_pluckableTileCache.TryGetValue(tile, out var Value))
					{
						if (tile.Contains("_metal_"))
						{
							Value = tile.Replace("_metal_", "_both_");
						}
						_pluckableTileCache.Add(tile, Value);
					}
					if (Value != null)
					{
						E.Tile = Value;
					}
				}
			}
			else if (E.ColorsVisible)
			{
				E.ColorString = "&r";
			}
		}
		return base.Render(E);
	}

	public override bool WantTurnTick()
	{
		return Plucked;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckUnpluck();
	}

	public void Pluck(GameObject Actor)
	{
		if (Plucked || !CanBePlucked(Actor))
		{
			return;
		}
		Cell cell = ParentObject.GetCurrentCell();
		Cell cell2 = Actor.GetCurrentCell();
		PlayWorldSound("Sounds/Interact/sfx_interact_coralpolyp_pluck");
		IComponent<GameObject>.XDidY(Actor, "pluck", (The.Player.IsConfused ? ParentObject.an() : "a coral polyp") + " off the strut" + ((cell == cell2) ? "" : (" " + Actor.DescribeDirectionToward(cell))) + " and" + Actor.GetVerb("toss") + " it aside");
		Plucked = true;
		PluckTime = The.Game.Turns;
		if (Stat.Random(1, CacheChance) <= 1)
		{
			if (IComponent<GameObject>.Visible(Actor) && AutoAct.IsInterruptable())
			{
				AutoAct.Interrupt();
			}
			if (IComponent<GameObject>.Visible(Actor))
			{
				CombatJuice.playPrefabAnimation(Actor, "Particles/CoralCachePluck");
			}
			GameObject gameObject = ParentObject.CurrentCell.AddObject(RevealObject);
			IComponent<GameObject>.XDidYToZ(Actor, "reveal", gameObject, null, null, null, null, Actor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, Actor.IsPlayer());
		}
		else if (IComponent<GameObject>.Visible(Actor))
		{
			CombatJuice.playPrefabAnimation(Actor, "Particles/CoralPluck");
		}
	}

	public void Unpluck()
	{
		Plucked = false;
		ParentObject.CurrentCell.GetObjects("PolypCache").ForEach((GameObject o) =>
		{
			o.Obliterate();
		});
	}

	public void CheckUnpluck()
	{
		if (Plucked && The.Game.Turns - PluckTime >= RegrowTime)
		{
			Unpluck();
		}
	}

	public bool CanBePlucked(GameObject Actor = null)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (currentZone.Z > 10)
		{
			return false;
		}
		return AllowPolypPluckingEvent.Check(currentZone, ParentObject, Actor);
	}
}
