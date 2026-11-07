using System;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsRefractLight : IPart
{
	public int Chance = 100;

	public string RetroVariance;

	public string Verb;

	public bool ShowInShortDescription;

	public override bool SameAs(IPart p)
	{
		RefractLight refractLight = p as RefractLight;
		if (refractLight.Chance != Chance)
		{
			return false;
		}
		if (refractLight.RetroVariance != RetroVariance)
		{
			return false;
		}
		if (refractLight.Verb != Verb)
		{
			return false;
		}
		if (refractLight.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != GetShortDescriptionEvent.ID || !ShowInShortDescription) && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "RefractLight");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "RefractLight");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (Chance >= 100)
			{
				if (Verb.IsNullOrEmpty())
				{
					stringBuilder.Append("Refracts");
				}
				else
				{
					stringBuilder.Append(ColorUtility.CapitalizeExceptFormatting(Grammar.ThirdPerson(Verb)));
				}
			}
			else
			{
				stringBuilder.Append("Has ").Append(Grammar.A(Chance)).Append("% chance to ")
					.Append(Verb ?? "refract");
			}
			stringBuilder.Append(" light-based attacks, sending them");
			if (RetroVariance.IsNullOrEmpty())
			{
				stringBuilder.Append(" in a random direction");
			}
			else
			{
				int num = RetroVariance.RollMinCached();
				int num2 = RetroVariance.RollMaxCached();
				int num3 = Math.Abs(num);
				int val = Math.Abs(num2);
				if (num == num2)
				{
					if (num == 0)
					{
						stringBuilder.Append(" back the way they came");
					}
					else
					{
						stringBuilder.Append(" away at an angle ").Append(num3).Append(" off from the way they came");
					}
				}
				else
				{
					stringBuilder.Append(" back the way they came, plus or minus up to ").Append(Math.Max(num3, val)).Append(" degrees");
				}
			}
			stringBuilder.Append('.');
			E.Postfix.AppendRules(stringBuilder.ToString());
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("RefractLight");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RefractLight" && Chance.in100())
		{
			E.SetParameter("By", ParentObject);
			if (!RetroVariance.IsNullOrEmpty())
			{
				float num = (float)E.GetParameter("Angle");
				E.SetParameter("Direction", (int)num + 180 + RetroVariance.RollCached());
			}
			if (!Verb.IsNullOrEmpty())
			{
				E.SetParameter("Verb", Verb);
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
