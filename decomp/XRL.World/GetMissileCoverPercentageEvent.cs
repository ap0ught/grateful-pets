using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetMissileCoverPercentageEvent : PooledEvent<GetMissileCoverPercentageEvent>
{
	public GameObject Object;

	public GameObject Actor;

	public GameObject Projectile;

	public bool PenetrateCreatures;

	public bool PenetrateWalls;

	public int Percentage;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
		Projectile = null;
		PenetrateCreatures = false;
		PenetrateWalls = false;
		Percentage = 0;
	}

	public void MinPercentage(int Percentage)
	{
		if (this.Percentage < Percentage)
		{
			this.Percentage = Percentage;
		}
	}

	public static int GetFor(GameObject Object, GameObject Actor = null, GameObject Projectile = null, bool PenetrateCreatures = false, bool PenetrateWalls = false)
	{
		int num = 0;
		if (GameObject.Validate(ref Object))
		{
			if (!PenetrateCreatures && !PenetrateWalls)
			{
				Render render = Object.Render;
				if (render != null && render.Occluding && Object.IsReal)
				{
					num = RuleSettings.DEFAULT_OCCLUDING_COVER_PERCENTAGE;
				}
			}
			if (Object.WantEvent(PooledEvent<GetMissileCoverPercentageEvent>.ID, MinEvent.CascadeLevel))
			{
				GetMissileCoverPercentageEvent getMissileCoverPercentageEvent = PooledEvent<GetMissileCoverPercentageEvent>.FromPool();
				getMissileCoverPercentageEvent.Object = Object;
				getMissileCoverPercentageEvent.Actor = Actor;
				getMissileCoverPercentageEvent.Projectile = Projectile;
				getMissileCoverPercentageEvent.PenetrateCreatures = PenetrateCreatures;
				getMissileCoverPercentageEvent.PenetrateWalls = PenetrateWalls;
				getMissileCoverPercentageEvent.Percentage = num;
				Object.HandleEvent(getMissileCoverPercentageEvent);
				num = getMissileCoverPercentageEvent.Percentage;
			}
		}
		return num;
	}
}
