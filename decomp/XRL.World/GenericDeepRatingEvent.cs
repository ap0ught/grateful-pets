using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 15)]
public class GenericDeepRatingEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GenericDeepRatingEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 15;

	private static List<GenericDeepRatingEvent> Pool;

	private static int PoolCounter;

	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Type;

	public int Level;

	public int BaseRating;

	public int Rating;

	public GenericDeepRatingEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref GenericDeepRatingEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GenericDeepRatingEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Subject = null;
		Source = null;
		Type = null;
		Level = 0;
		BaseRating = 0;
		Rating = 0;
	}

	public static int GetFor(GameObject Object, string Type, GameObject Subject = null, GameObject Source = null, int Level = 0, int BaseRating = 0)
	{
		bool flag = true;
		int num = BaseRating;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GenericDeepRating"))
		{
			Event obj = Event.New("GenericDeepRating");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Subject", Subject);
			obj.SetParameter("Source", Source);
			obj.SetParameter("Type", Type);
			obj.SetParameter("Level", Level);
			obj.SetParameter("BaseRating", BaseRating);
			obj.SetParameter("Rating", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Rating");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			GenericDeepRatingEvent genericDeepRatingEvent = FromPool();
			genericDeepRatingEvent.Object = Object;
			genericDeepRatingEvent.Subject = Subject;
			genericDeepRatingEvent.Source = Source;
			genericDeepRatingEvent.Type = Type;
			genericDeepRatingEvent.Level = Level;
			genericDeepRatingEvent.BaseRating = BaseRating;
			genericDeepRatingEvent.Rating = num;
			flag = Object.HandleEvent(genericDeepRatingEvent);
			num = genericDeepRatingEvent.Rating;
		}
		return num;
	}
}
