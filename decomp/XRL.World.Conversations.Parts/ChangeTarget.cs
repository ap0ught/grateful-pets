namespace XRL.World.Conversations.Parts;

/// <summary>Change target element if all/any predicates are satisfied.</summary>
public class ChangeTarget : IPredicatePart
{
	/// <summary>The alternate navigation target if predicates match.</summary>
	public string Target;

	/// <summary>Require one predicate to match rather than all.</summary>
	public new bool Any;

	public bool Not;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == GetTargetElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTargetElementEvent E)
	{
		if (Check(Any) != Not)
		{
			E.Target = Target;
		}
		return base.HandleEvent(E);
	}
}
