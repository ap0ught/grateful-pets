using System;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class SensePsychicEffect : Effect
{
	public bool Identified;

	public int Level = 1;

	public GameObject Listener;

	private int LastDistance;

	public SensePsychicEffect()
	{
		Duration = 1;
	}

	public SensePsychicEffect(int Level, GameObject Listener)
		: this()
	{
		this.Level = Level;
		this.Listener = Listener;
	}

	public override int GetEffectType()
	{
		return 32770;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return null;
	}

	private bool BadListener()
	{
		Listener = null;
		Object?.RemoveEffect(this);
		return true;
	}

	public bool CheckListen()
	{
		if (!GameObject.Validate(Object) || !GameObject.Validate(ref Listener) || !Listener.IsPlayer())
		{
			return BadListener();
		}
		SensePsychic part = Listener.GetPart<SensePsychic>();
		if (part == null || part.Level <= 0)
		{
			return BadListener();
		}
		int num = Object.DistanceTo(Listener);
		if (num > part.Radius)
		{
			return BadListener();
		}
		if (Identified)
		{
			return true;
		}
		if (Object.CurrentCell == null)
		{
			return true;
		}
		if (LastDistance > 0 && num >= LastDistance)
		{
			return true;
		}
		LastDistance = num;
		if (((int)((double)(100 + 10 * Level) / Math.Pow(num + 9, 2.0) * 100.0)).in100())
		{
			Identified = true;
			if (Listener.IsPlayer())
			{
				AutoAct.CheckHostileInterrupt();
			}
		}
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		CheckListen();
		return true;
	}

	public override void RegisterActive(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(The.Game, this, PooledEvent<AfterPlayerBodyChangeEvent>.ID);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == EnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		CheckListen();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckListen();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteringCellEvent E)
	{
		CheckListen();
		return base.HandleEvent(E);
	}

	public bool HeardAndNotSeen(GameObject obj)
	{
		if (obj == null || obj.IsPlayer())
		{
			return false;
		}
		if (!obj.IsVisible())
		{
			return true;
		}
		Cell cell = obj.CurrentCell;
		if (cell != null && (!cell.IsLit() || !cell.IsExplored()))
		{
			return true;
		}
		return false;
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (HeardAndNotSeen(Object) && Object.CanHypersensesDetect())
		{
			if (Identified)
			{
				E.HighestLayer = 0;
				E.NoWake = true;
				Object.ComponentRender(E);
				E.RenderString = Object.Render.RenderString;
				if (Options.UseTiles)
				{
					E.HFlip = Object.Render.getHFlip();
					E.VFlip = Object.Render.getVFlip();
					E.Tile = Object.Render.Tile;
				}
				else
				{
					E.Tile = null;
				}
			}
			else
			{
				E.Tile = null;
				E.RenderString = "?";
			}
			E.ColorString = "&B";
			E.DetailColor = "B";
			E.CustomDraw = true;
			return false;
		}
		return true;
	}
}
