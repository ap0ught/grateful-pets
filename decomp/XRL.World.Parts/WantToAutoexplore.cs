using System;
using System.Reflection;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <summary>
///     Will respond to <see cref="T:XRL.World.AutoexploreObjectEvent" /> with a request to be visited.
///     <para>
///     Set the <see cref="F:XRL.World.Parts.WantToAutoexplore.AdjacentAction" /> to control the <see cref="F:XRL.World.AutoexploreObjectEvent.Command" />
///     which becomes a <see cref="F:XRL.World.InventoryActionEvent.Command" /> dispatched to your part when visited.
///     </para>
/// </summary>
[Serializable]
public class WantToAutoexplore : IPart
{
	/// <summary>
	///     Controls the <see cref="F:XRL.World.AutoexploreObjectEvent.Command" /> setting.
	/// </summary>
	public string AdjacentAction;

	/// <summary>
	///     Sets <see cref="F:XRL.World.AutoexploreObjectEvent.AllowRetry" />, which if true means that the command
	///     is allowed to be re-executed on the object; otherwise, autoexplore will only try a given command
	///     once per object.
	/// </summary>
	public bool AllowRetry;

	/// <summary>
	///     If true, we will configure the <see cref="T:XRL.World.AutoexploreObjectEvent" /> to our values any time
	///     its <see cref="F:XRL.World.AutoexploreObjectEvent.Command" /> is not already the same as our
	///     <see cref="F:XRL.World.Parts.WantToAutoexplore.AdjacentAction" />, not just when it is null.
	/// </summary>
	public bool Override;

	/// <summary>
	///     A text event name, or minevent class name, that when triggered should count as having
	///     already done the AdjacentAction.   Example if AdjacentAction is "Look" TriggeredEvent should be
	///     "AfterLookedAt".  Then if someone looks before autoexploring, this part will be removed.
	/// </summary>
	public string TriggeredEvent;

	[NonSerialized]
	private int? TriggeredMinEvent;

	public override bool WantEvent(int ID, int cascade)
	{
		if (ID == TriggeredMinEvent)
		{
			ParentObject.RemovePart(this);
		}
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override void Register(GameObject GO, IEventRegistrar Registrar)
	{
		Type type = ModManager.ResolveType("XRL.World", TriggeredEvent);
		if ((object)type != null)
		{
			PropertyInfo property = type.GetProperty("ID", BindingFlags.Static | BindingFlags.Public);
			if ((object)property != null)
			{
				TriggeredMinEvent = (int)property.GetValue(null);
			}
		}
		if (!TriggeredMinEvent.HasValue && !TriggeredEvent.IsNullOrEmpty())
		{
			GO.RegisterPartEvent(this, TriggeredEvent);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == TriggeredEvent)
		{
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}

	/// <summary>
	///     If the <see cref="F:XRL.World.AutoexploreObjectEvent.Command" /> of the event isn't already set,
	///     set it = <see cref="F:XRL.World.Parts.WantToAutoexplore.AdjacentAction" /> and set <see cref="F:XRL.World.AutoexploreObjectEvent.AllowRetry" /> to
	///     <see cref="F:XRL.World.Parts.WantToAutoexplore.AllowRetry" />.
	///     <para>
	///     When <see cref="F:XRL.World.AutoexploreObjectEvent.AutogetOnlyMode" /> is enabled, only works for "Autoget" and "CollectLiquid" actions.
	///     In order to prevent constantly doing the requested <see cref="F:XRL.World.Parts.WantToAutoexplore.AdjacentAction" />,
	///     checks <see cref="M:XRL.World.Capabilities.AutoAct.GetAutoexploreActionProperty(XRL.World.GameObject,System.String)" /> is less than or equal to 0.
	///     </para>
	///     This check has no general-exploration equivalent property to check so the part would need to be removed if AdjacentAction is null.
	/// </summary>
	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!AdjacentAction.IsNullOrEmpty() && (E.Command == null || (Override && E.Command != AdjacentAction)) && (!E.AutogetOnlyMode || AdjacentAction == "Autoget" || AdjacentAction == "CollectLiquid") && (AllowRetry || AutoAct.GetAutoexploreActionProperty(ParentObject, AdjacentAction) <= 0))
		{
			E.Command = AdjacentAction;
		}
		return base.HandleEvent(E);
	}
}
