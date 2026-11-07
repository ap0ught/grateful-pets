using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Anatomy;

namespace XRL.CharacterBuilds.Qud;

public class QudCyberneticsModule : QudEmbarkBuilderModule<QudCyberneticsModuleData>
{
	public struct CyberneticsChoice
	{
		public GameObjectBlueprint blueprint;

		public string slot;

		private Renderable _renderable;

		public string GetDescription()
		{
			if (blueprint == null)
			{
				return "<none>";
			}
			sb.Clear();
			sb.Append(blueprint.GetPartParameter("Render", "DisplayName", blueprint.Name));
			sb.Append(" (");
			sb.Append(slot);
			sb.Append(")");
			return sb.ToString();
		}

		public string GetLongDescription()
		{
			if (blueprint == null)
			{
				return "{{C|-2 License Tier\n+1 Toughness}}";
			}
			sb.Clear();
			sb.Append(blueprint.GetPartParameter<string>("Description", "Short"));
			sb.Append("\n\n");
			sb.Append("{{rules|").Append(blueprint.GetPartParameter<string>("CyberneticsBaseItem", "BehaviorDescription")).Append("}}");
			return sb.ToString();
		}

		public Renderable GetRenderable()
		{
			if (_renderable == null && blueprint != null)
			{
				_renderable = new Renderable(blueprint);
			}
			return _renderable;
		}
	}

	protected static StringBuilder sb = new StringBuilder();

	public List<CyberneticsChoice> cybernetics = new List<CyberneticsChoice>();

	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public override bool shouldBeEnabled()
	{
		QudGenotypeModule module = builder.GetModule<QudGenotypeModule>();
		if (module != null && module.data?.Entry?.CyberneticsLicensePoints > 0)
		{
			return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
		}
		return false;
	}

	public override string DataErrors()
	{
		if (data != null && !cybernetics.Any(IsSelected))
		{
			return "Invalid choice selected";
		}
		return base.DataErrors();
	}

	public override void handleModuleDataChange(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		if (!(module is QudSubtypeModule) && !(module is QudGenotypeModule))
		{
			return;
		}
		cybernetics.Clear();
		IEnumerable<CyberneticsChoice> collection = GameObjectFactory.Factory.GetBlueprintsWithTag("StartingCybernetic:General").Concat(GameObjectFactory.Factory.GetBlueprintsWithTag("StartingCybernetic:" + builder.GetModule<QudSubtypeModule>().data?.Entry?.Name)).Concat(GameObjectFactory.Factory.GetBlueprintsWithTag("StartingCybernetic:" + builder.GetModule<QudGenotypeModule>().data?.Entry?.Name))
			.Distinct()
			.SelectMany((GameObjectBlueprint go) => from slot in go.GetPartParameter<string>("CyberneticsBaseItem", "Slots").Split(',')
				select new CyberneticsChoice
				{
					blueprint = go,
					slot = slot
				});
		cybernetics.AddRange(collection);
		cybernetics.Add(new CyberneticsChoice
		{
			blueprint = null,
			slot = null
		});
	}

	public override void InitFromSeed(string seed)
	{
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
		{
			GameObject gameObject = element as GameObject;
			if (data == null || data.selections == null)
			{
				MetricsManager.LogWarning("Cybernetics module was active but data or selections was null");
				return element;
			}
			string text = ((data.selections.Count == 0) ? null : data.selections[0].Cybernetic);
			string requiredType = ((data.selections.Count == 0) ? null : data.selections[0].Variant);
			if (text == null)
			{
				gameObject.SetIntProperty("CyberneticsLicenses", 0);
				gameObject.Statistics["Toughness"].BaseValue++;
			}
			else
			{
				List<BodyPart> part = gameObject.Body.GetPart(requiredType);
				if (part.Count > 0)
				{
					GameObject gameObject2 = GameObject.Create(text);
					gameObject2.MakeUnderstood();
					part.GetRandomElement().Implant(gameObject2);
				}
			}
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public bool IsSelected(CyberneticsChoice c)
	{
		if (data == null)
		{
			return false;
		}
		if (data.selections.Count == 0)
		{
			return c.blueprint == null;
		}
		QudCyberneticsModuleDataRow qudCyberneticsModuleDataRow = data.selections[0];
		if (qudCyberneticsModuleDataRow.Cybernetic == c.blueprint?.Name)
		{
			return qudCyberneticsModuleDataRow.Variant == c.slot;
		}
		return false;
	}

	public CyberneticsChoice SelectedChoice()
	{
		return cybernetics.Find(IsSelected);
	}

	public override SummaryBlockData GetSummaryBlock()
	{
		CyberneticsChoice cyberneticsChoice = cybernetics.Find(IsSelected);
		cyberneticsChoice.GetRenderable();
		return new SummaryBlockData
		{
			Id = GetType().FullName,
			Title = "Cybernetics",
			Description = cyberneticsChoice.GetDescription(),
			SortOrder = 100
		};
	}
}
