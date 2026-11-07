using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetContextEvent : PooledEvent<GetContextEvent>
{
	public GameObject Object;

	public GameObject ObjectContext;

	public Cell CellContext;

	public BodyPart BodyPartContext;

	public int Relation;

	public IContextRelationManager RelationManager;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		ObjectContext = null;
		CellContext = null;
		BodyPartContext = null;
		Relation = 0;
		RelationManager = null;
	}

	public static void Get(GameObject Object, out GameObject ObjectContext, out Cell CellContext, out BodyPart BodyPartContext, out int Relation, out IContextRelationManager RelationManager)
	{
		if (GameObject.Validate(ref Object))
		{
			GetContextEvent E = PooledEvent<GetContextEvent>.FromPool();
			E.Object = Object;
			Object.HandleEvent(E);
			ObjectContext = E.ObjectContext;
			CellContext = E.CellContext;
			BodyPartContext = E.BodyPartContext;
			Relation = E.Relation;
			RelationManager = E.RelationManager;
			PooledEvent<GetContextEvent>.ResetTo(ref E);
		}
		else
		{
			ObjectContext = null;
			CellContext = null;
			BodyPartContext = null;
			Relation = 0;
			RelationManager = null;
		}
	}

	public static void Get(GameObject Object, out GameObject ObjectContext, out Cell CellContext, out int Relation, out IContextRelationManager RelationManager)
	{
		Get(Object, out ObjectContext, out CellContext, out var _, out Relation, out RelationManager);
	}

	public static void Get(GameObject Object, out GameObject ObjectContext, out Cell CellContext, out int Relation)
	{
		Get(Object, out ObjectContext, out CellContext, out Relation, out var _);
	}

	public static void Get(GameObject Object, out GameObject ObjectContext, out Cell CellContext)
	{
		Get(Object, out ObjectContext, out CellContext, out var _);
	}

	public static bool HasAny(GameObject Object)
	{
		Get(Object, out var ObjectContext, out var CellContext);
		if (ObjectContext == null)
		{
			return CellContext != null;
		}
		return true;
	}
}
