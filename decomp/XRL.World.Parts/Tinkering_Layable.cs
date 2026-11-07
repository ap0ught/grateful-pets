using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class Tinkering_Layable : IPart, IContextRelationManager
{
	public string DetonationMessage = "AfterThrown";

	public GameObject ComponentOf;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetContextEvent>.ID && ID != RemoveFromContextEvent.ID && ID != PooledEvent<ReplaceInContextEvent>.ID)
		{
			return ID == TryRemoveFromContextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.Validate(ref ComponentOf))
		{
			E.ObjectContext = ComponentOf;
			E.Relation = 6;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		if (GameObject.Validate(ref ComponentOf))
		{
			Tinkering_Mine part = ComponentOf.GetPart<Tinkering_Mine>();
			if (part != null && part.Explosive == ParentObject)
			{
				part.SetExplosive(E.Replacement);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		if (GameObject.Validate(ref ComponentOf))
		{
			Tinkering_Mine part = ComponentOf.GetPart<Tinkering_Mine>();
			if (part != null && part.Explosive == ParentObject)
			{
				part.SetExplosive(null);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		if (GameObject.Validate(ref ComponentOf))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool RestoreContextRelation(GameObject Object, GameObject ObjectContext, Cell CellContext, BodyPart BodyPartContext, int Relation, bool Silent = true)
	{
		if (Relation == 6 && ObjectContext != null)
		{
			Tinkering_Mine part = ObjectContext.GetPart<Tinkering_Mine>();
			if (part != null)
			{
				if (part.Explosive != Object || ComponentOf != ObjectContext)
				{
					part.SetExplosive(Object);
				}
				return true;
			}
		}
		return false;
	}
}
