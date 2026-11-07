using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class DisplayNameAdjectives : IPart
{
	public string _Adjectives;

	[NonSerialized]
	private List<string> _AdjectiveList;

	public string Adjectives
	{
		get
		{
			return _Adjectives;
		}
		set
		{
			_Adjectives = value;
			_AdjectiveList = null;
		}
	}

	public List<string> AdjectiveList
	{
		get
		{
			if (_AdjectiveList == null)
			{
				if (string.IsNullOrEmpty(_Adjectives))
				{
					_AdjectiveList = new List<string>();
				}
				else
				{
					_AdjectiveList = new List<string>(_Adjectives.Split(','));
				}
			}
			return _AdjectiveList;
		}
	}

	public void AddAdjective(string Adjective)
	{
		AdjectiveList.Add(Adjective);
		_Adjectives = string.Join(",", AdjectiveList.ToArray());
	}

	public void RequireAdjective(string Adjective)
	{
		if (!AdjectiveList.Contains(Adjective))
		{
			AddAdjective(Adjective);
		}
	}

	public void RemoveAdjective(string Adjective)
	{
		AdjectiveList.Remove(Adjective);
		if (AdjectiveList.Count == 0)
		{
			ParentObject.RemovePart(this);
		}
		else
		{
			_Adjectives = string.Join(",", AdjectiveList.ToArray());
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		foreach (string adjective in AdjectiveList)
		{
			E.AddAdjective(adjective);
		}
		return base.HandleEvent(E);
	}
}
