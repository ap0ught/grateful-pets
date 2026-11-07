using System;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsBaseItem : IPart, IContextRelationManager
{
	public GameObject ImplantedOn;

	public string Slots;

	public int Cost;

	public string BehaviorDescription;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeginBeingUnequippedEvent>.ID && ID != PooledEvent<CanBeUnequippedEvent>.ID && ID != EnteredCellEvent.ID && ID != EquippedEvent.ID && ID != PooledEvent<GetContextEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != RemoveFromContextEvent.ID && ID != PooledEvent<ReplaceInContextEvent>.ID && ID != OnDestroyObjectEvent.ID && ID != TakenEvent.ID && ID != TryRemoveFromContextEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeUnequippedEvent E)
	{
		if (!E.Forced && ImplantedOn != null)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginBeingUnequippedEvent E)
	{
		if (!E.Forced && ImplantedOn != null)
		{
			E.AddFailureMessage("You can't remove " + ParentObject.t() + ".");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ImplantedOn = E.Implantee;
		ParentObject.SetIntProperty("CannotEquip", 1);
		ParentObject.SetIntProperty("CannotDrop", 1);
		ParentObject.SetIntProperty("NoRemoveOptionInInventory", 1);
		int cyberneticRejectionSyndromeChance = GetCyberneticRejectionSyndromeChance(E.Implantee);
		if (cyberneticRejectionSyndromeChance > 0)
		{
			string cyberneticRejectionSyndromeKey = GetCyberneticRejectionSyndromeKey(E.Implantee);
			if (!ParentObject.HasIntProperty(cyberneticRejectionSyndromeKey))
			{
				ParentObject.SetIntProperty(cyberneticRejectionSyndromeKey, cyberneticRejectionSyndromeChance.in100() ? 1 : 0);
			}
			if (ParentObject.GetIntProperty(cyberneticRejectionSyndromeKey) > 0)
			{
				E.Implantee.ForceApplyEffect(new CyberneticRejectionSyndrome(Cost));
			}
		}
		if (Sidebar.CurrentTarget == ParentObject)
		{
			Sidebar.CurrentTarget = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		ClearImplantConfiguration();
		if (E.Implantee.IsMutant())
		{
			string cyberneticRejectionSyndromeKey = GetCyberneticRejectionSyndromeKey(E.Implantee);
			if (ParentObject.GetIntProperty(cyberneticRejectionSyndromeKey) > 0 && E.Implantee.TryGetEffect<CyberneticRejectionSyndrome>(out var Effect))
			{
				Effect.Reduce(Cost);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.Validate(ref ImplantedOn))
		{
			E.ObjectContext = ImplantedOn;
			E.BodyPartContext = ImplantedOn.FindCybernetics(ParentObject);
			E.Relation = 4;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		if (GameObject.Validate(ref ImplantedOn))
		{
			BodyPart bodyPart = ImplantedOn.FindCybernetics(ParentObject);
			if (bodyPart != null)
			{
				ParentObject.Unimplant();
				bodyPart.Implant(E.Replacement);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		ParentObject.Unimplant();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		if (GameObject.Validate(ref ImplantedOn))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.IncludeImplantPrefix && !E.UsingAdjunctNoun && !ParentObject.HasProperName && E.Understood())
		{
			E.AddAdjective("[{{W|Implant}}] -", 40);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		string text = GetCyberneticsBehaviorDescriptionEvent.GetFor(ParentObject, BehaviorDescription);
		if (!text.IsNullOrEmpty())
		{
			E.Postfix.AppendRules(text).Append('\n');
		}
		if (ParentObject != null && ParentObject.HasTag("CyberneticsDestroyOnRemoval"))
		{
			E.Postfix.AppendRules("Destroyed when uninstalled.");
		}
		if (!Slots.IsNullOrEmpty())
		{
			E.Postfix.AppendRules("Target body parts: " + Slots.Replace(",", ", "));
		}
		E.Postfix.AppendRules("License points: " + Cost).AppendRules("Only compatible with True Kin genotypes");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		ClearImplantConfiguration();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		ClearImplantConfiguration();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ClearImplantConfiguration();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		ParentObject.Unimplant();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanBeTaken");
		base.Register(Object, Registrar);
	}

	private void ClearImplantConfiguration()
	{
		ImplantedOn = null;
		ParentObject.RemoveIntProperty("CannotEquip");
		ParentObject.RemoveIntProperty("CannotDrop");
		ParentObject.RemoveIntProperty("NoRemoveOptionInInventory");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanBeTaken" && !E.HasFlag("Forced") && ImplantedOn != null)
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public static string GetCyberneticRejectionSyndromeKey(GameObject Implantee)
	{
		return "CyberneticRejection" + Implantee.GeneID;
	}

	public int GetCyberneticRejectionSyndromeChance(GameObject Implantee)
	{
		if (!Implantee.IsMutant())
		{
			return 0;
		}
		int num = 5 + Cost + ParentObject.GetIntProperty("CyberneticRejectionSyndromeModifier");
		if (Implantee.TryGetPart<Mutations>(out var Part) && Part.MutationList != null)
		{
			foreach (BaseMutation mutation in Part.MutationList)
			{
				num = ((!mutation.IsPhysical() && !(Slots == "Head")) ? (num + 1) : (num + mutation.Level));
			}
		}
		return 0;
	}

	public bool RestoreContextRelation(GameObject Object, GameObject ObjectContext, Cell CellContext, BodyPart BodyPartContext, int Relation, bool Silent = true)
	{
		if (Relation == 4 && BodyPartContext != null)
		{
			if (BodyPartContext.Cybernetics == Object && Object.Implantee == ObjectContext)
			{
				return true;
			}
			BodyPartContext.Implant(Object, ForDeepCopy: false, Silent);
			return true;
		}
		return false;
	}
}
