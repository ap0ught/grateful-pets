using System.Text;
using XRL.CharacterBuilds.Qud.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud;

public class QudMutationsModule : QudEmbarkBuilderModule<QudMutationsModuleData>
{
	public override bool shouldBeEnabled()
	{
		if ((int)builder.handleUIEvent(QudMutationsModuleWindow.EID_GET_BASE_MP, 0) > 0)
		{
			return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
		}
		return false;
	}

	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public override string DataWarnings()
	{
		if (data.mp > 0)
		{
			return "You have unspent mutation points.";
		}
		return base.DataWarnings();
	}

	public override string DataErrors()
	{
		if (data.mp < 0)
		{
			return "You have spent too many mutation points.";
		}
		return base.DataErrors();
	}

	public override void handleModuleDataChange(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		base.handleModuleDataChange(module, oldValues, newValues);
	}

	public override SummaryBlockData GetSummaryBlock()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (data != null)
		{
			foreach (QudMutationModuleDataRow selection in data.selections)
			{
				if (selection.Count > 0)
				{
					stringBuilder.Append(selection.DisplayName);
					if (selection.Count > 1)
					{
						stringBuilder.AppendFormat("x{0}", selection.Count);
					}
					stringBuilder.AppendLine();
				}
			}
		}
		else
		{
			stringBuilder.AppendLine("none");
		}
		return new SummaryBlockData
		{
			Title = "Mutations",
			Description = stringBuilder.ToString(),
			SortOrder = 100
		};
	}

	public override void InitFromSeed(string seed)
	{
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT && data != null)
		{
			GameObject gameObject = element as GameObject;
			Mutations mutations = gameObject.RequirePart<Mutations>();
			foreach (QudMutationModuleDataRow selection in data.selections)
			{
				if (selection.Entry.Mutation != null)
				{
					BaseMutation baseMutation = selection.Entry.CreateInstance();
					baseMutation.SetVariant(selection.Variant);
					mutations.AddMutation(baseMutation, selection.Count);
				}
				else if (selection.Entry.Name == "Chimera")
				{
					gameObject.SetStringProperty("MutationLevel", "Chimera");
				}
				else if (selection.Entry.Name == "Esper")
				{
					gameObject.SetStringProperty("MutationLevel", "Esper");
				}
			}
		}
		return base.handleBootEvent(id, game, info, element);
	}
}
