using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SocialRoles : IPart
{
	public string _Roles;

	[NonSerialized]
	private List<string> _RoleList;

	public string Roles
	{
		get
		{
			return _Roles;
		}
		set
		{
			_Roles = value;
			_RoleList = null;
			if (_Roles.Contains(","))
			{
				RoleList.Sort();
				UpdateRoles();
			}
		}
	}

	public List<string> RoleList
	{
		get
		{
			if (_RoleList == null)
			{
				if (_Roles.IsNullOrEmpty())
				{
					_RoleList = new List<string>();
				}
				else
				{
					_RoleList = new List<string>(_Roles.Split(','));
				}
			}
			return _RoleList;
		}
	}

	public void AddRole(string Role)
	{
		RoleList.Add(Role);
		RoleList.Sort();
		UpdateRoles();
	}

	public void RequireRole(string Role)
	{
		if (!RoleList.Contains(Role))
		{
			AddRole(Role);
		}
	}

	public void RemoveRole(string Role)
	{
		RoleList.Remove(Role);
		if (RoleList.Count == 0)
		{
			ParentObject.RemovePart(this);
		}
		else
		{
			UpdateRoles();
		}
	}

	public void UpdateRoles()
	{
		_Roles = string.Join(",", RoleList.ToArray());
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject && Cloning.IsCloning(E.Context))
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.WithoutTitles)
		{
			int i = 0;
			for (int count = RoleList.Count; i < count; i++)
			{
				string text = RoleList[i];
				if (text.Contains("="))
				{
					text = GameText.VariableReplace(text, ParentObject, (GameObject)null, E.NoColor);
				}
				E.AddTitle(text);
			}
		}
		return base.HandleEvent(E);
	}
}
