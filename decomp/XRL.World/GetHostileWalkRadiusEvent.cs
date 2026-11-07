using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetHostileWalkRadiusEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetHostileWalkRadiusEvent), null, CountPool, ResetPool);

	private static List<GetHostileWalkRadiusEvent> Pool;

	private static int PoolCounter;

	public int Radius;

	public GetHostileWalkRadiusEvent()
	{
		base.ID = ID;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref GetHostileWalkRadiusEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetHostileWalkRadiusEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Radius = 0;
	}

	public static GetHostileWalkRadiusEvent FromPool(GameObject Actor, GameObject Item, int Radius)
	{
		GetHostileWalkRadiusEvent getHostileWalkRadiusEvent = FromPool();
		getHostileWalkRadiusEvent.Actor = Actor;
		getHostileWalkRadiusEvent.Item = Item;
		getHostileWalkRadiusEvent.Radius = Radius;
		return getHostileWalkRadiusEvent;
	}

	public void MaxRadius(int Radius)
	{
		if (this.Radius < Radius)
		{
			this.Radius = Radius;
		}
	}

	public static int GetFor(GameObject Actor, GameObject Item)
	{
		int num = Brain.DEFAULT_HOSTILE_WALK_RADIUS;
		if (Item.Brain != null)
		{
			Item.Brain.CheckMobility(out var Immobile, out var Waterbound, out var WallWalker);
			if ((Immobile | Waterbound | WallWalker) && !Item.HasReadyMissileWeapon())
			{
				if (Immobile)
				{
					num = ((!Item.IsCombat) ? 1 : 2);
				}
				else if (Waterbound & WallWalker)
				{
					num = 1;
					if (Actor != null)
					{
						List<Tuple<Cell, char>> lineTo = Item.GetLineTo(Actor);
						if (lineTo != null)
						{
							int i = 1;
							for (int num2 = Math.Min(lineTo.Count, Item.Brain.HostileWalkRadius); i < num2 && (lineTo[i].Item1.HasAquaticSupportFor(Item) || lineTo[i].Item1.HasWalkableWallFor(Item)); i++)
							{
								num++;
							}
						}
						if (num < Item.Brain.HostileWalkRadius)
						{
							num++;
						}
					}
				}
				else if (Waterbound)
				{
					num = 1;
					if (Actor != null)
					{
						List<Tuple<Cell, char>> lineTo2 = Item.GetLineTo(Actor);
						if (lineTo2 != null)
						{
							int j = 1;
							for (int num3 = Math.Min(lineTo2.Count, Item.Brain.HostileWalkRadius); j < num3 && lineTo2[j].Item1.HasAquaticSupportFor(Item); j++)
							{
								num++;
							}
						}
						if (num < Item.Brain.HostileWalkRadius)
						{
							num++;
						}
					}
				}
				else if (WallWalker)
				{
					num = 1;
					if (Actor != null)
					{
						List<Tuple<Cell, char>> lineTo3 = Item.GetLineTo(Actor);
						if (lineTo3 != null)
						{
							int k = 1;
							for (int num4 = Math.Min(lineTo3.Count, Item.Brain.HostileWalkRadius); k < num4 && lineTo3[k].Item1.HasWalkableWallFor(Item); k++)
							{
								num++;
							}
						}
						if (num < Item.Brain.HostileWalkRadius)
						{
							num++;
						}
					}
				}
				else
				{
					num = Item.Brain.HostileWalkRadius;
				}
			}
		}
		if (Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetHostileWalkRadiusEvent getHostileWalkRadiusEvent = FromPool(Actor, Item, num);
			Item.HandleEvent(getHostileWalkRadiusEvent);
			num = getHostileWalkRadiusEvent.Radius;
		}
		return num;
	}
}
