using System.Text;
using ConsoleLib.Console;
using XRL.UI.Framework;
using XRL.World;

namespace XRL.CharacterBuilds.Qud;

public class QudBuildSummaryModule : QudEmbarkBuilderModule<QudBuildSummaryModuleData>
{
	public override bool IncludeInBuildCodes()
	{
		return false;
	}

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
	}

	public override string DataErrors()
	{
		return null;
	}

	public override string DataWarnings()
	{
		return null;
	}

	public override void InitFromSeed(string seed)
	{
	}

	public override void onNext()
	{
		builder.advance(force: false, editableOnly: true);
	}

	public override SummaryBlockData GetSummaryBlock()
	{
		string text = "creatures/caste_16.bmp";
		string text2 = "Y";
		string text3 = "k";
		string text4 = "w";
		string text5 = "Humanoid";
		StringBuilder stringBuilder = new StringBuilder();
		if (GenotypeFactory.TryGetGenotypeEntry(builder.GetModule<QudGenotypeModule>()?.data?.Genotype, out var Entry))
		{
			stringBuilder.Compound(Entry.DisplayName, '\n');
			text5 = (Entry.BodyObject.IsNullOrEmpty() ? text5 : Entry.BodyObject);
		}
		if (SubtypeFactory.TryGetSubtypeEntry(builder.GetModule<QudSubtypeModule>()?.data?.Subtype, out var Entry2))
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Insert(0, '\n');
			}
			stringBuilder.Insert(0, Entry2.DisplayName);
			text5 = (Entry2.BodyObject.IsNullOrEmpty() ? text5 : Entry2.BodyObject);
		}
		text5 = builder.fireBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT, text5);
		text = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE, null) ?? text;
		text2 = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEFOREGROUND, null) ?? text2;
		text3 = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEBACKGROUND, null) ?? text3;
		text4 = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEDETAIL, null) ?? text4;
		if (GameObjectFactory.Factory.Blueprints.TryGetValue(text5, out var value))
		{
			stringBuilder.Compound(value.GetTag("BodyDisplayName", text5), '\n');
		}
		return new SummaryBlockData
		{
			Id = "overall",
			Description = stringBuilder.ToString(),
			IconPath = text,
			IconDetailColor = ColorUtility.colorFromChar(text4[0]),
			IconForegroundColor = ColorUtility.colorFromChar(text2[0]),
			SortOrder = 0
		};
	}
}
