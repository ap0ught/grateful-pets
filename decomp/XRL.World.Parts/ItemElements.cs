using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ItemElements : IActivePart
{
	public string _Elements;

	[NonSerialized]
	private Dictionary<string, int> _ElementMap;

	public string Elements
	{
		get
		{
			return _Elements;
		}
		set
		{
			_ElementMap = null;
			_Elements = value;
		}
	}

	public Dictionary<string, int> ElementMap
	{
		get
		{
			if (_ElementMap == null && !_Elements.IsNullOrEmpty())
			{
				_ElementMap = _Elements.CachedNumericDictionaryExpansion();
			}
			return _ElementMap;
		}
	}

	public override bool SameAs(IPart Part)
	{
		if ((Part as ItemElements)._Elements != _Elements)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (ElementMap != null && E.IsRelevantObject(ParentObject))
		{
			foreach (KeyValuePair<string, int> item in ElementMap)
			{
				E.Add(item.Key, item.Value);
			}
		}
		return base.HandleEvent(E);
	}
}
