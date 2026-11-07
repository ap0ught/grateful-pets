using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Titles : IPart
{
	public string TitleList;

	public string TitleOrder;

	public string Primary
	{
		set
		{
			AddTitle(value, -40);
		}
	}

	public string Ordinary
	{
		set
		{
			AddTitle(value);
		}
	}

	public Titles()
	{
	}

	public Titles(string List, string Order = null)
		: this()
	{
		TitleList = List;
		TitleOrder = Order;
	}

	public override bool SameAs(IPart p)
	{
		Titles titles = p as Titles;
		if (titles.TitleList != TitleList)
		{
			return false;
		}
		if (titles.TitleOrder != TitleOrder)
		{
			return false;
		}
		return base.SameAs(p);
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
		if (!E.WithoutTitles && E.Understood() && !TitleList.IsNullOrEmpty())
		{
			if (TitleOrder.IsNullOrEmpty())
			{
				foreach (string item in TitleList.CachedDoubleSemicolonExpansion())
				{
					string text = item;
					if (text.Contains("="))
					{
						text = GameText.VariableReplace(text, ParentObject, (GameObject)null, E.NoColor);
					}
					E.AddTitle(text);
				}
			}
			else
			{
				Dictionary<string, int> dictionary = TitleOrder.CachedNumericDictionaryExpansion();
				foreach (string item2 in TitleList.CachedDoubleSemicolonExpansion())
				{
					dictionary.TryGetValue(item2, out var value);
					E.AddTitle(item2, value);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void AddTitle(string Title, int Order = 0)
	{
		if (Title.Contains(";;"))
		{
			MetricsManager.LogError("Cannot track title containing double semicolon, had " + Title);
			return;
		}
		if (TitleList.IsNullOrEmpty())
		{
			TitleList = Title;
		}
		else
		{
			if (TitleList.HasDelimitedSubstring(";;", Title))
			{
				MetricsManager.LogError("Already have title " + Title);
				return;
			}
			TitleList = TitleList + ";;" + Title;
		}
		if (Order != 0)
		{
			if (Title.Contains("::"))
			{
				MetricsManager.LogError("Cannot track order for title containing double colon, had " + Title);
				return;
			}
			if (TitleOrder.IsNullOrEmpty())
			{
				TitleOrder = Title + "::" + Order;
				return;
			}
			TitleOrder = TitleOrder + ";;" + Title + "::" + Order;
		}
	}
}
