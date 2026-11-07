using XRL.World.Capabilities;

namespace XRL.World;

/// <summary>
///   Event fired during autoact sequence.  Two major modes <see cref="F:XRL.World.AutoexploreObjectEvent.AutogetOnlyMode" /> 
///   is checked when walking around manually, is false when auto-explore is engaged.
/// </summary>
[GameEvent(Cascade = 256, Cache = Cache.Singleton)]
public class AutoexploreObjectEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AutoexploreObjectEvent));

	public new static readonly int CascadeLevel = 256;

	public static readonly AutoexploreObjectEvent Instance = new AutoexploreObjectEvent();

	/// <summary>The autoact setting we are requesting feedback for.</summary>
	public string Setting;

	/// <summary>The ongoing action attached to the autoact setting, if any</summary>
	public OngoingAction Action;

	/// <summary><see cref="F:XRL.World.InventoryActionEvent.Command" /> we are requesting from the explorer.</summary>
	public string Command;

	/// <summary>Should we be able to re-initiate this action multiple times from autoexplore?</summary>
	public bool AllowRetry;

	/// <summary>If true, we should only respond with the "Autoget" class of actions.</summary>
	public bool AutogetOnlyMode;

	public AutoexploreObjectEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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
		Setting = null;
		Action = null;
		Command = null;
		AllowRetry = false;
		AutogetOnlyMode = false;
	}

	public static bool Check(GameObject Actor, GameObject Object, string Setting, OngoingAction Action)
	{
		if (AutoAct.AutoexploreSuppressed(Object))
		{
			return false;
		}
		Instance.Actor = Actor;
		Instance.Item = Object;
		Instance.Setting = Setting;
		Instance.Action = Action;
		Instance.Command = null;
		Instance.AllowRetry = false;
		Instance.AutogetOnlyMode = false;
		if (Actor.HandleEvent(Instance))
		{
			Object.HandleEvent(Instance);
		}
		if (Instance.Command != null)
		{
			if (!Instance.AllowRetry)
			{
				return AutoAct.GetAutoexploreActionProperty(Object, Instance.Command) <= 0;
			}
			return true;
		}
		return false;
	}

	public static bool CheckForAdjacent(GameObject Actor, GameObject Object, string Setting, OngoingAction Action, bool AutogetOnly = false)
	{
		string Command;
		bool AllowRetry;
		return CheckForAdjacent(out Command, out AllowRetry, Actor, Object, Setting, Action, AutogetOnly);
	}

	public static bool CheckForAdjacent(out string Command, out bool AllowRetry, GameObject Actor, GameObject Object, string Setting, OngoingAction Action, bool AutogetOnly = false)
	{
		if (AutoAct.AutoexploreSuppressed(Object))
		{
			Command = null;
			AllowRetry = false;
			return false;
		}
		Instance.Actor = Actor;
		Instance.Item = Object;
		Instance.Setting = Setting;
		Instance.Action = Action;
		Instance.Command = null;
		Instance.AllowRetry = false;
		Instance.AutogetOnlyMode = AutogetOnly;
		if (Actor.HandleEvent(Instance))
		{
			Object.HandleEvent(Instance);
		}
		Command = Instance.Command;
		AllowRetry = Instance.AllowRetry;
		if (Command != null)
		{
			if (!AllowRetry)
			{
				return AutoAct.GetAutoexploreActionProperty(Object, Command) <= 0;
			}
			return true;
		}
		return false;
	}

	public static string GetAdjacentAction(GameObject Actor, GameObject Object, string Setting, OngoingAction Action)
	{
		if (AutoAct.AutoexploreSuppressed(Object))
		{
			return null;
		}
		Instance.Actor = Actor;
		Instance.Item = Object;
		Instance.Setting = Setting;
		Instance.Action = Action;
		Instance.AllowRetry = false;
		Instance.Command = null;
		if (Actor.HandleEvent(Instance))
		{
			Object.HandleEvent(Instance);
		}
		if (Instance.Command == null || (!Instance.AllowRetry && AutoAct.GetAutoexploreActionProperty(Object, Instance.Command) > 0))
		{
			return null;
		}
		return Instance.Command;
	}
}
