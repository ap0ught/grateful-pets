using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class RegenerateLimbEvent : ILimbRegenerationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(RegenerateLimbEvent), null, CountPool, ResetPool);

	private static List<RegenerateLimbEvent> Pool;

	private static int PoolCounter;

	public RegenerateLimbEvent()
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

	public static void ResetTo(ref RegenerateLimbEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static RegenerateLimbEvent FromPool()
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

	public static bool Send(GameObject Object, GameObject Actor = null, GameObject Source = null, bool Whole = false, bool All = false, bool IncludeMinor = true, bool Voluntary = true, int? ParentID = null, int? Category = null, int[] Categories = null, int? ExceptCategory = null, int[] ExceptCategories = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("RegenerateLimb"))
		{
			Event obj = Event.New("RegenerateLimb");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Source", Source);
			obj.SetParameter("ParentID", ParentID);
			obj.SetParameter("Category", Category);
			obj.SetParameter("Categories", Categories);
			obj.SetParameter("ExceptCategory", ExceptCategory);
			obj.SetParameter("ExceptCategories", ExceptCategories);
			obj.SetFlag("Whole", Whole);
			obj.SetFlag("All", All);
			obj.SetFlag("IncludeMinor", IncludeMinor);
			obj.SetFlag("Voluntary", Voluntary);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, ILimbRegenerationEvent.CascadeLevel))
		{
			RegenerateLimbEvent regenerateLimbEvent = FromPool();
			regenerateLimbEvent.Object = Object;
			regenerateLimbEvent.Actor = Actor;
			regenerateLimbEvent.Source = Source;
			regenerateLimbEvent.ParentID = ParentID;
			regenerateLimbEvent.Category = Category;
			regenerateLimbEvent.Categories = Categories;
			regenerateLimbEvent.ExceptCategory = ExceptCategory;
			regenerateLimbEvent.ExceptCategories = ExceptCategories;
			regenerateLimbEvent.Whole = Whole;
			regenerateLimbEvent.All = All;
			regenerateLimbEvent.IncludeMinor = IncludeMinor;
			regenerateLimbEvent.Voluntary = Voluntary;
			flag = Object.HandleEvent(regenerateLimbEvent);
		}
		return !flag;
	}
}
