using System.Collections.Generic;
using ConsoleLib.Console;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class ProjectileMovingEvent : PooledEvent<ProjectileMovingEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Attacker;

	public GameObject Launcher;

	public GameObject Projectile;

	public GameObject Defender;

	public GameObject ApparentTarget;

	public GameObject HitOverride;

	public Cell Cell;

	public Cell TargetCell;

	public List<Point> Path;

	public int PathIndex = -1;

	public ScreenBuffer ScreenBuffer;

	public bool Throw;

	public bool ActivateShowUninvolved;

	public bool RecheckPhase;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Attacker = null;
		Launcher = null;
		Projectile = null;
		Defender = null;
		ApparentTarget = null;
		HitOverride = null;
		Cell = null;
		TargetCell = null;
		Path = null;
		PathIndex = -1;
		ScreenBuffer = null;
		Throw = false;
		ActivateShowUninvolved = false;
		RecheckPhase = false;
	}
}
