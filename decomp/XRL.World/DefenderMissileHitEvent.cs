using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class DefenderMissileHitEvent : PooledEvent<DefenderMissileHitEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Launcher;

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Owner;

	public GameObject Projectile;

	public Projectile ProjectilePart;

	public GameObject AimedAt;

	public GameObject ApparentTarget;

	public MissilePath MissilePath;

	public FireType Type;

	public int AimLevel;

	public int NaturalHitResult;

	public int HitResult;

	public bool PathInvolvesPlayer;

	public GameObject MessageAsFrom;

	public bool Done;

	public bool PenetrateCreatures;

	public bool PenetrateWalls;

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
		Launcher = null;
		Attacker = null;
		Defender = null;
		Owner = null;
		Projectile = null;
		ProjectilePart = null;
		AimedAt = null;
		ApparentTarget = null;
		MissilePath = null;
		Type = FireType.Normal;
		AimLevel = 0;
		NaturalHitResult = 0;
		HitResult = 0;
		PathInvolvesPlayer = false;
		MessageAsFrom = null;
		Done = false;
		PenetrateCreatures = false;
		PenetrateWalls = false;
	}

	public static bool Check(GameObject Launcher, GameObject Attacker, GameObject Defender, GameObject Owner, GameObject Projectile, Projectile ProjectilePart, GameObject AimedAt, GameObject ApparentTarget, MissilePath MissilePath, FireType Type, int AimLevel, int NaturalHitResult, int HitResult, bool PathInvolvesPlayer, GameObject MessageAsFrom, ref bool Done, ref bool PenetrateCreatures, ref bool PenetrateWalls)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Defender) && Defender.HasRegisteredEvent("DefenderMissileHit"))
		{
			Event obj = Event.New("DefenderMissileHit");
			obj.SetParameter("Launcher", Launcher);
			obj.SetParameter("Attacker", Attacker);
			obj.SetParameter("Defender", Defender);
			obj.SetParameter("Owner", Owner);
			obj.SetParameter("Projectile", Projectile);
			obj.SetParameter("ProjectilePart", ProjectilePart);
			obj.SetParameter("AimedAt", AimedAt);
			obj.SetParameter("ApparentTarget", ApparentTarget);
			obj.SetParameter("MissilePath", MissilePath);
			obj.SetParameter("Type", Type);
			obj.SetParameter("AimLevel", AimLevel);
			obj.SetParameter("NaturalHitResult", NaturalHitResult);
			obj.SetParameter("HitResult", HitResult);
			obj.SetFlag("PathInvolvesPlayer", PathInvolvesPlayer);
			obj.SetParameter("MessageAsFrom", MessageAsFrom);
			obj.SetFlag("Done", Done);
			obj.SetFlag("PenetrateCreatures", PenetrateCreatures);
			obj.SetFlag("PenetrateWalls", PenetrateWalls);
			flag = Defender.FireEvent(obj);
			Done = obj.HasFlag("Done");
			PenetrateCreatures = obj.HasFlag("PenetrateCreatures");
			PenetrateWalls = obj.HasFlag("PenetrateWalls");
		}
		if (flag && GameObject.Validate(ref Defender) && Defender.WantEvent(PooledEvent<DefenderMissileHitEvent>.ID, CascadeLevel))
		{
			DefenderMissileHitEvent defenderMissileHitEvent = PooledEvent<DefenderMissileHitEvent>.FromPool();
			defenderMissileHitEvent.Launcher = Launcher;
			defenderMissileHitEvent.Attacker = Attacker;
			defenderMissileHitEvent.Defender = Defender;
			defenderMissileHitEvent.Owner = Owner;
			defenderMissileHitEvent.Projectile = Projectile;
			defenderMissileHitEvent.ProjectilePart = ProjectilePart;
			defenderMissileHitEvent.AimedAt = AimedAt;
			defenderMissileHitEvent.ApparentTarget = ApparentTarget;
			defenderMissileHitEvent.MissilePath = MissilePath;
			defenderMissileHitEvent.Type = Type;
			defenderMissileHitEvent.AimLevel = AimLevel;
			defenderMissileHitEvent.NaturalHitResult = NaturalHitResult;
			defenderMissileHitEvent.HitResult = HitResult;
			defenderMissileHitEvent.PathInvolvesPlayer = PathInvolvesPlayer;
			defenderMissileHitEvent.MessageAsFrom = MessageAsFrom;
			defenderMissileHitEvent.Done = Done;
			defenderMissileHitEvent.PenetrateCreatures = PenetrateCreatures;
			defenderMissileHitEvent.PenetrateWalls = PenetrateWalls;
			flag = Defender.HandleEvent(defenderMissileHitEvent);
			Done = defenderMissileHitEvent.Done;
			PenetrateCreatures = defenderMissileHitEvent.PenetrateCreatures;
			PenetrateWalls = defenderMissileHitEvent.PenetrateWalls;
		}
		return flag;
	}
}
