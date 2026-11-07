using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsHasRandomImplants : IPart
{
	public string ImplantTable = "Implants_1and2Pointers";

	public string ImplantChance = "100";

	public string LicensesAtLeast = "1";

	public string Adjective = "{{implanted|implanted}}";

	public bool ChangeColor = true;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!string.IsNullOrEmpty(Adjective) && ParentObject.AnyInstalledCybernetics())
		{
			E.AddAdjective(Adjective);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (ImplantChance.RollCached().in100())
		{
			int num = 0;
			int num2 = LicensesAtLeast.RollCached();
			int intProperty = ParentObject.GetIntProperty("CyberneticsLicenses");
			int num3 = 0;
			if (intProperty < num2)
			{
				ParentObject.SetIntProperty("CyberneticsLicenses", num2);
			}
			else
			{
				num2 = intProperty;
			}
			Body body = ParentObject.Body;
			if (body != null)
			{
				while (++num <= 30 && num3 < num2)
				{
					string blueprint = PopulationManager.RollOneFrom(ImplantTable).Blueprint;
					if (blueprint == null)
					{
						MetricsManager.LogError("got null blueprint from " + ImplantTable);
						continue;
					}
					if (!GameObjectFactory.Factory.Blueprints.TryGetValue(blueprint, out var value))
					{
						MetricsManager.LogError("got invalid blueprint \"" + blueprint + "\" from " + ImplantTable);
						continue;
					}
					string[] array = value.GetPartParameter<string>("CyberneticsBaseItem", "Slots")?.Split(',');
					if (array.IsNullOrEmpty())
					{
						MetricsManager.LogError("Weird blueprint in random cybernetics table: " + blueprint + " from table " + ImplantTable);
						continue;
					}
					array.ShuffleInPlace();
					string[] array2 = array;
					foreach (string requiredType in array2)
					{
						List<BodyPart> part = body.GetPart(requiredType);
						part.ShuffleInPlace();
						foreach (BodyPart item in part)
						{
							if (item == null || item._Cybernetics != null)
							{
								continue;
							}
							GameObject gameObject = GameObject.Create(blueprint);
							CyberneticsBaseItem part2 = gameObject.GetPart<CyberneticsBaseItem>();
							if (part2 != null)
							{
								if (num2 - num3 >= part2.Cost)
								{
									num3 += part2.Cost;
									item.Implant(gameObject);
								}
								else
								{
									gameObject.Obliterate();
								}
								goto end_IL_01dc;
							}
							gameObject.Obliterate();
						}
						continue;
						end_IL_01dc:
						break;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (ChangeColor && ParentObject.AnyInstalledCybernetics())
		{
			if (E.ColorString == "&C")
			{
				E.ColorString = "&W";
			}
			else
			{
				E.ColorString = "&C";
			}
		}
		return base.Render(E);
	}
}
