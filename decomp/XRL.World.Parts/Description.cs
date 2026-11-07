using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;
using Qud.UI;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
[GenerateSerializationPartial]
[GeneratePoolingPartial(Capacity = 64)]
public class Description : IPart
{
	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool DescriptionPool = new IPartPool(64);

	public string Mark = "";

	public string _Short = "A really ugly specimen.";

	[NonSerialized]
	private List<string> FeatureItems = new List<string>();

	[NonSerialized]
	private List<string> EquipItems = new List<string>();

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => DescriptionPool;

	public string Short
	{
		get
		{
			return GetShortDescription();
		}
		set
		{
			_Short = (value.Contains("~") ? value.Split('~')[0] : value);
		}
	}

	public string Long => GetLongDescription();

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override void Reset()
	{
		base.Reset();
		Mark = "";
		_Short = "A really ugly specimen.";
		FeatureItems.Clear();
		EquipItems.Clear();
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(Mark);
		Writer.WriteOptimized(_Short);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Mark = Reader.ReadOptimizedString();
		_Short = Reader.ReadOptimizedString();
	}

	public string GetShortDescription(bool AsIfKnown = false, bool NoConfusion = false, string Context = null)
	{
		if (!NoConfusion && The.Player != null && The.Player.IsConfused)
		{
			return "???";
		}
		IShortDescriptionEvent shortDescriptionEvent = null;
		IShortDescriptionEvent oE = null;
		int num = (AsIfKnown ? 2 : ParentObject.GetEpistemicStatus());
		switch (num)
		{
		case 0:
			shortDescriptionEvent = GetUnknownShortDescriptionEvent.FromPool(ParentObject, ParentObject.GetPart<Examiner>()?.GetActiveShortDescription(num), Context, AsIfKnown);
			if (IComponent<GameObject>.ThePlayer != null)
			{
				oE = OwnerGetUnknownShortDescriptionEvent.FromPool();
			}
			break;
		case 1:
			shortDescriptionEvent = GetUnknownShortDescriptionEvent.FromPool(ParentObject, ParentObject.GetPart<Examiner>()?.GetActiveShortDescription(num), Context, AsIfKnown);
			if (IComponent<GameObject>.ThePlayer != null)
			{
				oE = OwnerGetUnknownShortDescriptionEvent.FromPool();
			}
			break;
		default:
			shortDescriptionEvent = GetShortDescriptionEvent.FromPool(ParentObject, _Short, Context, AsIfKnown);
			if (IComponent<GameObject>.ThePlayer != null)
			{
				oE = OwnerGetShortDescriptionEvent.FromPool();
			}
			break;
		}
		return GetDescription(shortDescriptionEvent, oE);
	}

	private string GetDescription(IShortDescriptionEvent E, IShortDescriptionEvent OE)
	{
		E.Process(ParentObject);
		if (OE != null && IComponent<GameObject>.ThePlayer != null)
		{
			OE.Process(IComponent<GameObject>.ThePlayer, E);
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (E.Prefix.Length > 0)
		{
			stringBuilder.Append(E.Prefix);
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
		}
		stringBuilder.Append(E.Base);
		if (E.Infix.Length > 0)
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(E.Infix);
		}
		if (E.Postfix.Length > 0)
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(E.Postfix);
		}
		if (!Mark.IsNullOrEmpty())
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append('\n').Append(Mark);
		}
		if (ParentObject.Physics != null && ParentObject.Physics.Takeable)
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append("\n{{K|Weight: ").Append(ParentObject.Weight).Append(" lbs.}}");
		}
		return GameText.VariableReplace(stringBuilder, ParentObject);
	}

	public string GetLongDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GetLongDescription(stringBuilder);
		return stringBuilder.ToString();
	}

	public void GetLongDescription(StringBuilder SB)
	{
		SB.Append(Short);
		if (!ParentObject.HasProperty("HideCon"))
		{
			Body body = ParentObject.Body;
			if (body != null)
			{
				FeatureItems.Clear();
				EquipItems.Clear();
				List<GameObject> list = Event.NewGameObjectList();
				_ = SB.Length;
				foreach (BodyPart item in body.LoopParts())
				{
					if (item.Equipped != null && !item.Equipped.HasPropertyOrTag("SuppressInLookDisplay") && !list.Contains(item.Equipped))
					{
						if (item.Equipped.HasPropertyOrTag("ShowAsPhysicalFeature") || (item.Equipped.HasPropertyOrTag("VisibleAsDefaultBehavior") && !item.Equipped.HasPropertyOrTag("UndesirableWeapon")))
						{
							FeatureItems.Add(item.Equipped.ShortDisplayName);
						}
						else
						{
							EquipItems.Add(item.Equipped.ShortDisplayName);
						}
						list.Add(item.Equipped);
					}
					if (item.Cybernetics != null && !item.Cybernetics.HasPropertyOrTag("SuppressInLookDisplay") && !list.Contains(item.Cybernetics))
					{
						EquipItems.Add(item.Cybernetics.ShortDisplayName);
						list.Add(item.Cybernetics);
					}
					if (item.DefaultBehavior != null && !item.DefaultBehavior.HasPropertyOrTag("SuppressInLookDisplay") && !list.Contains(item.DefaultBehavior) && (item.DefaultBehaviorBlueprint == null || item.DefaultBehaviorBlueprint != item.DefaultBehavior.Blueprint || item.DefaultBehavior.HasPropertyOrTag("ShowAsPhysicalFeature") || (item.DefaultBehavior.HasPropertyOrTag("VisibleAsDefaultBehavior") && !item.DefaultBehavior.HasPropertyOrTag("UndesirableWeapon"))))
					{
						FeatureItems.Add(item.DefaultBehavior.ShortDisplayName);
						list.Add(item.DefaultBehavior);
					}
				}
				GetExtraPhysicalFeaturesEvent.Send(ParentObject, FeatureItems);
				if (Gender.EnableDisplay)
				{
					string value = ParentObject.GetGender()?.Name;
					if (!value.IsNullOrEmpty())
					{
						SB.Append("\n\nGender: ").Append(value);
					}
				}
				if (FeatureItems.Count > 0)
				{
					SB.Append("\n\nPhysical features: ").Append(FeatureItems[0]);
					int i = 1;
					for (int count = FeatureItems.Count; i < count; i++)
					{
						SB.Append(", ").Append(FeatureItems[i]);
					}
				}
				if (EquipItems.Count > 0)
				{
					if (FeatureItems.Count <= 0)
					{
						SB.Append('\n');
					}
					SB.Append("\nEquipped: ").Append(EquipItems[0]);
					int j = 1;
					for (int count2 = EquipItems.Count; j < count2; j++)
					{
						SB.Append(", ").Append(EquipItems[j]);
					}
				}
			}
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (ParentObject.Effects != null)
		{
			foreach (Effect effect in ParentObject.Effects)
			{
				if (effect.SuppressInLookDisplay())
				{
					continue;
				}
				string description = effect.GetDescription();
				if (!string.IsNullOrEmpty(description))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(description);
				}
			}
		}
		ParentObject.FireEvent(Event.New("GetEffectsBlock", "Block", stringBuilder));
		if (stringBuilder.Length > 0)
		{
			SB.Append("\n\n").Append(stringBuilder);
		}
	}

	public string GetFeelingDescription(GameObject who = null)
	{
		if (who == null)
		{
			who = IComponent<GameObject>.ThePlayer;
			if (who == null)
			{
				return null;
			}
		}
		if (ParentObject.HasProperty("HideCon"))
		{
			return null;
		}
		Brain brain = ParentObject.Brain;
		if (brain == null)
		{
			return null;
		}
		return brain.GetFeelingLevel(who) switch
		{
			Brain.FeelingLevel.Allied => "{{G|Friendly}}", 
			Brain.FeelingLevel.Hostile => "{{R|Hostile}}", 
			_ => "Neutral", 
		};
	}

	public string GetDifficultyDescription(GameObject who = null)
	{
		return DifficultyEvaluation.GetDifficultyDescription(ParentObject, who);
	}

	public override bool SameAs(IPart p)
	{
		Description description = p as Description;
		if (description.Mark != Mark)
		{
			return false;
		}
		if (description._Short != _Short)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (Visible())
		{
			E.AddAction("Look", "look", "Look", null, 'l', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
			if (ParentObject.Understood() && ParentObject.HasStringProperty("Story"))
			{
				E.AddAction("Recall Story", "recall story", "ReadStory", null, 's', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Look")
		{
			Look.TooltipInformation tooltipInformation = Look.GenerateTooltipInformation(ParentObject);
			StringBuilder stringBuilder = Event.NewStringBuilder().Append(tooltipInformation.LongDescription);
			if (!string.IsNullOrEmpty(tooltipInformation.WoundLevel))
			{
				stringBuilder.Append("\n\n").Append(tooltipInformation.WoundLevel);
			}
			StringBuilder stringBuilder2 = Event.NewStringBuilder().Append(tooltipInformation.DisplayName);
			if (!string.IsNullOrEmpty(tooltipInformation.SubHeader))
			{
				stringBuilder2.Append('\n').Append(tooltipInformation.SubHeader);
			}
			List<QudMenuItem> list = new List<QudMenuItem>();
			list.AddRange(PopupMessage.SingleButton);
			if (ParentObject.HasStringProperty("Story") && ParentObject.Understood())
			{
				list.Add(new QudMenuItem
				{
					command = "Story",
					hotkey = "S",
					text = "Recall {{W|S}}tory"
				});
			}
			if (Options.ModernUI)
			{
				if (Popup.NewPopupMessageAsync(stringBuilder.ToString(), list, null, null, null, 0, stringBuilder2.ToString(), tooltipInformation.IconRenderable).Result.command == "Story")
				{
					BookUI.ShowBookByID(ParentObject.Property["Story"]);
				}
			}
			else
			{
				stringBuilder2.Append("\n\n").Append(stringBuilder);
				if (Popup.ShowBlockPrompt(stringBuilder2.ToString(), (list.Count == 2) ? "[press {{W|space}} or recall {{W|s}}tory]" : "[press {{W|space}}]", "Sounds/UI/ui_notification", ParentObject.RenderForUI("Look"), Capitalize: false, MuteBackground: true, CenterIcon: false, RightIcon: true, LogMessage: false) == Keys.S && list.Count == 2)
				{
					BookUI.ShowBookByID(ParentObject.Property["Story"]);
				}
			}
			E.Actor.FireEvent(Event.New("LookedAt", "Object", ParentObject));
			ParentObject.FireEvent(Event.New("AfterLookedAt", "Looker", E.Actor));
		}
		else if (E.Command == "ReadStory")
		{
			BookUI.ShowBookByID(ParentObject.GetStringProperty("Story"));
		}
		return base.HandleEvent(E);
	}
}
