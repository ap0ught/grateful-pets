using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Hidden : IPart
{
	public bool _Found;

	public int Difficulty = 15;

	public int FoundNavigationWeight;

	public int FoundAdjacentNavigationWeight;

	public bool Silent;

	public bool Found
	{
		get
		{
			return _Found;
		}
		set
		{
			if (_Found != value)
			{
				bool flag = false;
				if (!_Found & value)
				{
					flag = true;
				}
				_Found = value;
				if (flag)
				{
					RevealInternal();
				}
			}
		}
	}

	public Hidden()
	{
	}

	public Hidden(int Difficulty = 15, bool Silent = false)
		: this()
	{
		this.Difficulty = Difficulty;
		this.Silent = Silent;
	}

	public Hidden(Hidden source)
		: this()
	{
		Difficulty = source.Difficulty;
		_Found = source._Found;
		Silent = source.Silent;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!Found)
		{
			if (ParentObject.Render.CustomRender && !ParentObject.HasIntProperty("CustomRenderSources"))
			{
				ParentObject.ModIntProperty("CustomRenderSources", 1);
			}
			ParentObject.Render.CustomRender = true;
			ParentObject.ModIntProperty("CustomRenderSources", 1);
		}
	}

	public override bool SameAs(IPart p)
	{
		Hidden hidden = p as Hidden;
		if (hidden._Found != _Found)
		{
			return false;
		}
		if (hidden.FoundNavigationWeight != FoundNavigationWeight)
		{
			return false;
		}
		if (hidden.FoundAdjacentNavigationWeight != FoundAdjacentNavigationWeight)
		{
			return false;
		}
		if (hidden.Difficulty != Difficulty)
		{
			return false;
		}
		if (hidden.Silent != Silent)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && (ID != GetAdjacentNavigationWeightEvent.ID || (_Found && FoundAdjacentNavigationWeight == 0)) && (ID != GetNavigationWeightEvent.ID || (_Found && FoundNavigationWeight == 0)))
		{
			return ID == PooledEvent<TakeOnRoleEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (_Found)
		{
			E.MinWeight(FoundNavigationWeight);
			return base.HandleEvent(E);
		}
		E.Weight = E.PriorWeight;
		return false;
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (_Found)
		{
			E.MinWeight(FoundAdjacentNavigationWeight);
			return base.HandleEvent(E);
		}
		E.Weight = E.PriorWeight;
		return false;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (!Found)
		{
			ParentObject.Render.Visible = false;
		}
		else
		{
			ParentObject.Render.Visible = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakeOnRoleEvent E)
	{
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CustomRender");
		Registrar.Register("Searched");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CustomRender")
		{
			if (!Found && E.GetParameter("RenderEvent") is RenderEvent renderEvent && (renderEvent.Lit == LightLevel.Radar || renderEvent.Lit == LightLevel.LitRadar))
			{
				Found = true;
			}
		}
		else if (E.ID == "Searched" && !Found)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Searcher");
			if (E.GetIntParameter("Bonus") + Stat.Random(1, gameObjectParameter.Stat("Intelligence")) >= Difficulty)
			{
				Found = true;
			}
		}
		return base.FireEvent(E);
	}

	public new void Reveal()
	{
		Found = true;
	}

	public void Hide()
	{
		Found = false;
	}

	public void Reveal(bool Silent = false)
	{
		if (Silent && !Found)
		{
			_Found = true;
			RevealInternal(Silent: true);
		}
		else
		{
			Reveal();
		}
	}

	private void RevealInternal(bool Silent = false)
	{
		if (ParentObject.Render != null)
		{
			ParentObject.Render.Visible = true;
			ParentObject.ModIntProperty("CustomRenderSources", -1);
			if (ParentObject.GetIntProperty("CustomRenderSources") <= 0)
			{
				ParentObject.Render.CustomRender = false;
			}
		}
		if (!Silent && !this.Silent && Visible())
		{
			DidX("are", "revealed", "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
		}
		ParentObject.FireEvent("Found");
	}

	private void HideInternal(bool Silent = false)
	{
		if (ParentObject.Render != null)
		{
			ParentObject.Render.Visible = false;
			ParentObject.ModIntProperty("CustomRenderSources", 1);
			if (ParentObject.GetIntProperty("CustomRenderSources") > 0)
			{
				ParentObject.Render.CustomRender = true;
			}
		}
		if (!Silent && !this.Silent && Visible())
		{
			DidX("disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		}
		ParentObject.FireEvent("Hidden");
	}

	public static void CarryOver(GameObject src, GameObject dest)
	{
		if (src.TryGetPart<Hidden>(out var Part))
		{
			dest.RemovePart<Hidden>();
			dest.AddPart(new Hidden(Part));
		}
	}
}
