using System;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public abstract class IAmmo : IPart, IContextRelationManager
{
	public GameObject LoadedIn;

	public int ScatterOnDeathThreshold = 5;

	public string ScatterOnDeathPercentage = "8-10";

	public override bool SameAs(IPart p)
	{
		if ((p as IAmmo).LoadedIn != LoadedIn)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != DropOnDeathEvent.ID && ID != PooledEvent<GetContextEvent>.ID && ID != RemoveFromContextEvent.ID && ID != PooledEvent<ReplaceInContextEvent>.ID && ID != TryRemoveFromContextEvent.ID)
		{
			if (Options.AutogetAmmo)
			{
				return ID == AutoexploreObjectEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (E.Command == null && Options.AutogetAmmo)
		{
			bool num;
			if (LoadedIn != null)
			{
				if (!LoadedIn.Understood())
				{
					goto IL_005d;
				}
				num = LoadedIn.CanAutoget();
			}
			else
			{
				num = ParentObject.CanAutoget();
			}
			if (num)
			{
				E.Command = ((LoadedIn != null) ? "UnloadMagazineAmmo" : "Autoget");
			}
		}
		goto IL_005d;
		IL_005d:
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DropOnDeathEvent E)
	{
		if (ScatterOnDeathThreshold > 0 && !ScatterOnDeathPercentage.IsNullOrEmpty())
		{
			int count = ParentObject.Count;
			if (count >= ScatterOnDeathThreshold)
			{
				ParentObject.Count = Math.Max(count * ScatterOnDeathPercentage.RollCached() / 100, ScatterOnDeathThreshold);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.Validate(ref LoadedIn))
		{
			E.ObjectContext = LoadedIn;
			E.Relation = 5;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		ReplaceAmmo(E.Replacement);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		ReplaceAmmo(null);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		ReplaceAmmo(null);
		return base.HandleEvent(E);
	}

	private void ReplaceAmmo(GameObject Replacement)
	{
		if (GameObject.Validate(ref LoadedIn))
		{
			MagazineAmmoLoader part = LoadedIn.GetPart<MagazineAmmoLoader>();
			if (part != null && part.Ammo == ParentObject)
			{
				part.SetAmmo(Replacement);
			}
		}
	}

	public GameObject GetLoadedIn()
	{
		GameObject.Validate(ref LoadedIn);
		return LoadedIn;
	}

	public bool RestoreContextRelation(GameObject Object, GameObject ObjectContext, Cell CellContext, BodyPart BodyPartContext, int Relation, bool Silent = true)
	{
		if (Relation == 5 && ObjectContext != null)
		{
			MagazineAmmoLoader part = ObjectContext.GetPart<MagazineAmmoLoader>();
			if (part != null)
			{
				if (part.Ammo != Object || LoadedIn != ObjectContext)
				{
					part.SetAmmo(Object);
				}
				return true;
			}
		}
		return false;
	}
}
