using System;

namespace XRL.World.Parts;

[Serializable]
public class ActionForwarder : IPart
{
	public string Direction = "N";

	public string Part;

	[NonSerialized]
	private Type _PartType;

	[NonSerialized]
	private IPart _Instance;

	public Type PartType
	{
		get
		{
			if ((object)_PartType == null)
			{
				_PartType = ModManager.ResolveType("XRL.World.Parts." + Part, IgnoreCase: false, ThrowOnError: true);
			}
			return _PartType;
		}
	}

	public IPart Instance
	{
		get
		{
			if (_Instance == null || !_Instance.IsValid)
			{
				Cell localCellFromDirection = ParentObject.CurrentCell;
				if (!Direction.IsNullOrEmpty() && Direction != ".")
				{
					localCellFromDirection = localCellFromDirection.GetLocalCellFromDirection(Direction);
				}
				_Instance = localCellFromDirection?.GetFirstObjectPart(PartType);
			}
			return _Instance;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == GetInventoryActionsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return Instance?.HandleEvent(E) ?? base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		return Instance?.HandleEvent(E) ?? base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		return Instance?.HandleEvent(E) ?? base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		return Instance?.HandleEvent(E) ?? base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		return Instance?.HandleEvent(E) ?? base.HandleEvent(E);
	}
}
