using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IExamineEvent : IActOnItemEvent
{
	public int Pass = 1;

	public bool ConfusionBased;

	public bool Identify;

	public bool IdentifyIfDestroyed;

	public string TinkeringBlueprint;

	public string PriorDesc;

	public string PriorPastVerbToBe;

	public string KnownDesc;

	public int Complexity;

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
		Pass = 1;
		ConfusionBased = false;
		Identify = false;
		IdentifyIfDestroyed = false;
		TinkeringBlueprint = null;
		PriorDesc = null;
		PriorPastVerbToBe = null;
		KnownDesc = null;
		Complexity = 0;
	}

	protected void Setup()
	{
		Identify = false;
		IdentifyIfDestroyed = false;
		if (GameObject.Validate(ref Item))
		{
			TinkeringBlueprint = Item.GetTinkeringBlueprint();
			PriorDesc = Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
			PriorPastVerbToBe = Item.GetVerb("were");
			KnownDesc = Item.an(int.MaxValue, null, null, AsIfKnown: true, Single: true);
			Complexity = Item.GetPart<Examiner>()?.Complexity ?? 0;
		}
	}

	protected bool ProcessIdentify()
	{
		if (Identify && GameObject.Validate(ref Item) && !Item.Understood() && GameObject.Validate(ref Actor) && Actor.IsVisible())
		{
			Popup.Show("You realize " + Item.does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: true) + " " + Item.an(int.MaxValue, null, null, AsIfKnown: true, Single: true) + "!");
			Item.MakeUnderstood();
			Identify = false;
			return true;
		}
		if (IdentifyIfDestroyed && !GameObject.Validate(ref Item) && !TinkeringBlueprint.IsNullOrEmpty() && !PriorDesc.IsNullOrEmpty() && !PriorPastVerbToBe.IsNullOrEmpty() && !KnownDesc.IsNullOrEmpty() && Complexity > 0)
		{
			if (Examiner.MakeBlueprintUnderstood(TinkeringBlueprint, Complexity))
			{
				Popup.Show("You realize " + PriorDesc + PriorPastVerbToBe + " " + KnownDesc + "!");
			}
			IdentifyIfDestroyed = false;
			return true;
		}
		return false;
	}

	public void IdentifyImmediately()
	{
		ProcessIdentify();
	}
}
