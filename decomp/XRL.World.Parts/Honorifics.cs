using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Honorifics : IPart
{
	public string HonorificList;

	public string HonorificOrder;

	public string Primary
	{
		set
		{
			AddHonorific(value, 40);
		}
	}

	public string Ordinary
	{
		set
		{
			AddHonorific(value);
		}
	}

	public Honorifics()
	{
	}

	public Honorifics(string List, string Order = null)
		: this()
	{
		HonorificList = List;
		HonorificOrder = Order;
	}

	public override bool SameAs(IPart p)
	{
		Honorifics honorifics = p as Honorifics;
		if (honorifics.HonorificList != HonorificList)
		{
			return false;
		}
		if (honorifics.HonorificOrder != HonorificOrder)
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
		if (!HonorificList.IsNullOrEmpty() && E.Understood())
		{
			if (HonorificOrder.IsNullOrEmpty())
			{
				if (!E.WithoutTitles)
				{
					foreach (string item in HonorificList.CachedDoubleSemicolonExpansion())
					{
						string text = item;
						if (text.Contains("="))
						{
							text = GameText.VariableReplace(text, ParentObject, (GameObject)null, E.NoColor);
						}
						E.AddHonorific(text);
					}
				}
			}
			else
			{
				Dictionary<string, int> dictionary = HonorificOrder.CachedNumericDictionaryExpansion();
				foreach (string item2 in HonorificList.CachedDoubleSemicolonExpansion())
				{
					dictionary.TryGetValue(item2, out var value);
					if (!E.WithoutTitles || value >= 20)
					{
						E.AddHonorific(item2, value);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void AddHonorific(string Honorific, int Order = 0)
	{
		if (Honorific.Contains(";;"))
		{
			MetricsManager.LogError("Cannot track honorific containing double semicolon, had " + Honorific);
			return;
		}
		if (HonorificList.IsNullOrEmpty())
		{
			HonorificList = Honorific;
		}
		else
		{
			if (HonorificList.HasDelimitedSubstring(";;", Honorific))
			{
				MetricsManager.LogError("Already have honorific " + Honorific);
				return;
			}
			HonorificList = HonorificList + ";;" + Honorific;
		}
		if (Order != 0)
		{
			if (Honorific.Contains("::"))
			{
				MetricsManager.LogError("Cannot track order for honorific containing double colon, had " + Honorific);
				return;
			}
			if (HonorificOrder.IsNullOrEmpty())
			{
				HonorificOrder = Honorific + "::" + Order;
				return;
			}
			HonorificOrder = HonorificOrder + ";;" + Honorific + "::" + Order;
		}
	}
}
