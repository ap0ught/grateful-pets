using ConsoleLib.Console;
using UnityEngine;

namespace XRL.World.Effects;

public abstract class IEffectWithPrefabImposter : Effect
{
	public string PrefabID;

	public bool ImposterActive = true;

	public bool VisibleOnly = true;

	public int X;

	public int Y;

	public IEffectWithPrefabImposter()
	{
	}

	public void SetActive(bool State)
	{
		ImposterActive = State;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<GetDebugInternalsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "VisibleOnly", VisibleOnly);
		E.AddEntry(this, "PrefabID", PrefabID);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Object.SetIntProperty("HasImposter", 1);
		base.Register(Object, Registrar);
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (ImposterActive && !VisibleOnly)
		{
			E.Imposters.Add(new ImposterExtra.ImposterInfo(PrefabID, new Vector2(X, Y)));
		}
		return base.FinalRender(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (ImposterActive && VisibleOnly && E.Visible && (int)E.Lit > 1)
		{
			E.Imposters.Add(new ImposterExtra.ImposterInfo(PrefabID, new Vector2(X, Y)));
		}
		return true;
	}
}
