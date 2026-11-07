using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Epithets : IPart
{
	public string EpithetList;

	public string EpithetOrder;

	public string Primary
	{
		set
		{
			AddEpithet(value, -40);
		}
	}

	public string Ordinary
	{
		set
		{
			AddEpithet(value);
		}
	}

	public Epithets()
	{
	}

	public Epithets(string List, string Order = null)
		: this()
	{
		EpithetList = List;
		EpithetOrder = Order;
	}

	public override bool SameAs(IPart Part)
	{
		Epithets epithets = Part as Epithets;
		if (epithets.EpithetList != EpithetList)
		{
			return false;
		}
		if (epithets.EpithetOrder != EpithetOrder)
		{
			return false;
		}
		return base.SameAs(Part);
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
		if (!E.WithoutTitles && E.Understood() && !EpithetList.IsNullOrEmpty())
		{
			if (EpithetOrder.IsNullOrEmpty())
			{
				foreach (string item in EpithetList.CachedDoubleSemicolonExpansion())
				{
					string text = item;
					if (text.Contains("="))
					{
						text = GameText.VariableReplace(text, ParentObject, (GameObject)null, E.NoColor);
					}
					E.AddEpithet(text);
				}
			}
			else
			{
				Dictionary<string, int> dictionary = EpithetOrder.CachedNumericDictionaryExpansion();
				foreach (string item2 in EpithetList.CachedDoubleSemicolonExpansion())
				{
					dictionary.TryGetValue(item2, out var value);
					E.AddEpithet(item2, value);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void AddEpithet(string Epithet, int Order = 0)
	{
		if (Epithet.Contains(";;"))
		{
			MetricsManager.LogError("Cannot track epithet containing double semicolon, had " + Epithet);
			return;
		}
		if (EpithetList.IsNullOrEmpty())
		{
			EpithetList = Epithet;
		}
		else
		{
			if (EpithetList.HasDelimitedSubstring(";;", Epithet))
			{
				MetricsManager.LogError("Already have epithet " + Epithet);
				return;
			}
			EpithetList = EpithetList + ";;" + Epithet;
		}
		if (Order != 0)
		{
			if (Epithet.Contains(";;"))
			{
				MetricsManager.LogError("Cannot track order for epithet containing double semicolon, had " + Epithet);
				return;
			}
			if (EpithetOrder.IsNullOrEmpty())
			{
				EpithetOrder = Epithet + "::" + Order;
				return;
			}
			EpithetOrder = EpithetOrder + ";;" + Epithet + "::" + Order;
		}
	}
}
