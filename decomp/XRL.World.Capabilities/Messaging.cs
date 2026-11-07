using System;
using System.Text;
using XRL.Language;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Capabilities;

public static class Messaging
{
	private static void HandleMessage(GameObject Source, string Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		if (!UsePopup && (!FromDialog || Source == null || !Source.IsPlayer()))
		{
			if (ColorAsGoodFor != null || ColorAsBadFor != null)
			{
				MessageQueue.AddPlayerMessage(ColorCoding.ConsequentialColorize(Msg, ColorAsGoodFor, ColorAsBadFor, Color));
			}
			else
			{
				MessageQueue.AddPlayerMessage(Msg, Color);
			}
		}
		else
		{
			Popup.Show(Msg);
		}
	}

	private static void HandleMessage(GameObject Source, StringBuilder Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		HandleMessage(Source, Msg.ToString(), Color, FromDialog, UsePopup, ColorAsGoodFor, ColorAsBadFor);
	}

	public static void EmitMessage(GameObject Source, string Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, bool AlwaysVisible = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		if (Source != null && (AlwaysVisible || Source.IsPlayer() || Source.IsVisible()))
		{
			HandleMessage(Source, Msg, Color, FromDialog, UsePopup, ColorAsGoodFor, ColorAsBadFor);
		}
	}

	public static void EmitMessage(GameObject Source, StringBuilder Msg, char Color = ' ', bool FromDialog = false, bool UsePopup = false, bool AlwaysVisible = false, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		EmitMessage(Source, Msg.ToString(), Color, FromDialog, UsePopup, AlwaysVisible, ColorAsGoodFor, ColorAsBadFor);
	}

	public static void XDidY(GameObject Actor, string Verb, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, GameObject SubjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		if (!UsePopup & FromDialog)
		{
			bool num;
			if (SubjectPossessedBy != null)
			{
				num = SubjectPossessedBy.IsPlayer();
			}
			else
			{
				if (Actor == null)
				{
					goto IL_0056;
				}
				num = Actor.Holder?.IsPlayer() == true;
			}
			if (num)
			{
				UsePopup = true;
			}
		}
		goto IL_0056;
		IL_0056:
		string value = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
		if (SubjectOverride == null && Actor != null && Actor.IsPlayer())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (!value.IsNullOrEmpty())
			{
				stringBuilder.Append("{{").Append(value).Append('|');
			}
			stringBuilder.Append("You ").Append(Verb);
			AppendWithSpaceIfNeeded(stringBuilder, Extra);
			stringBuilder.Append(EndMark ?? ".");
			if (!value.IsNullOrEmpty())
			{
				stringBuilder.Append("}}");
			}
			HandleMessage(Source ?? Actor ?? The.Player, stringBuilder, ' ', FromDialog, UsePopup);
			return;
		}
		if (!AlwaysVisible)
		{
			GameObject gameObject = UseVisibilityOf ?? Source ?? Actor;
			if (gameObject == null || !gameObject.IsVisible())
			{
				return;
			}
		}
		StringBuilder stringBuilder2 = Event.NewStringBuilder();
		if (!value.IsNullOrEmpty())
		{
			stringBuilder2.Append("{{").Append(value).Append('|');
		}
		string value2 = null;
		if (DescribeSubjectDirection | DescribeSubjectDirectionLate)
		{
			value2 = The.Player.DescribeDirectionToward(SubjectPossessedBy ?? Source ?? Actor ?? The.Player);
		}
		string text = null;
		bool withIndefiniteArticle = IndefiniteSubject;
		if (SubjectPossessedBy != null && (value2.IsNullOrEmpty() | DescribeSubjectDirectionLate))
		{
			if (SubjectPossessedBy.IsPlayer())
			{
				text = "your";
				value2 = null;
			}
			else
			{
				bool flag = !UseFullNames;
				text = Grammar.MakePossessive(SubjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject));
			}
			withIndefiniteArticle = false;
		}
		else
		{
			GameObject gameObject2 = Actor?.Holder;
			if (gameObject2 != null)
			{
				if (gameObject2.IsPlayer())
				{
					text = "your";
					value2 = null;
				}
				else
				{
					bool flag = !UseFullNames;
					text = Grammar.MakePossessive(gameObject2.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject));
				}
				withIndefiniteArticle = false;
			}
		}
		if (SubjectOverride == null)
		{
			if (Actor != null)
			{
				bool flag = !UseFullNames;
				stringBuilder2.Append(Actor.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle, text));
			}
		}
		else if (stringBuilder2.Length > 0)
		{
			if (!text.IsNullOrEmpty())
			{
				stringBuilder2.Append(text).Append(' ');
			}
			stringBuilder2.Append(SubjectOverride);
		}
		else if (!text.IsNullOrEmpty())
		{
			stringBuilder2.Append(text.Capitalize()).Append(' ').Append(SubjectOverride);
		}
		else
		{
			stringBuilder2.Append(SubjectOverride.Capitalize());
		}
		if (!value2.IsNullOrEmpty() && !DescribeSubjectDirectionLate)
		{
			if (SubjectPossessedBy != null && (Actor == null || !Actor.HasProperName))
			{
				StringBuilder stringBuilder3 = stringBuilder2.Append(" of ");
				bool flag = !UseFullNames;
				stringBuilder3.Append(SubjectPossessedBy.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject)).Append(' ').Append(value2);
			}
			else
			{
				stringBuilder2.Append(' ').Append(value2);
			}
		}
		if (Actor != null)
		{
			stringBuilder2.Append(Actor.GetVerb(Verb));
		}
		else
		{
			stringBuilder2.Append(' ').Append(Verb);
		}
		AppendWithSpaceIfNeeded(stringBuilder2, Extra);
		if (!value2.IsNullOrEmpty() & DescribeSubjectDirectionLate)
		{
			stringBuilder2.Append(' ').Append(value2);
		}
		stringBuilder2.Append(EndMark ?? ".");
		if (!value.IsNullOrEmpty())
		{
			stringBuilder2.Append("}}");
		}
		HandleMessage(Source ?? Actor ?? The.Player, stringBuilder2, ' ', FromDialog, UsePopup);
	}

	public static void XDidYToZ(GameObject Actor, string Verb, string Preposition = null, GameObject Object = null, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		if (!GameObject.Validate(ref Object))
		{
			return;
		}
		try
		{
			if (!UsePopup & FromDialog)
			{
				if (!Object.IsPlayer())
				{
					bool num;
					if (SubjectPossessedBy != null)
					{
						num = SubjectPossessedBy.IsPlayer();
					}
					else
					{
						if (Actor == null)
						{
							goto IL_006c;
						}
						num = Actor.Holder?.IsPlayer() == true;
					}
					if (!num)
					{
						goto IL_006c;
					}
				}
				goto IL_00b5;
			}
			goto IL_00b8;
			IL_00b8:
			if (SubjectOverride == null && Actor != null && Actor.IsPlayer())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				string value = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
				if (!value.IsNullOrEmpty())
				{
					stringBuilder.Append("{{").Append(value).Append('|');
				}
				stringBuilder.Append("You ").Append(Verb).Append(' ');
				if (!Preposition.IsNullOrEmpty())
				{
					stringBuilder.Append(Preposition).Append(' ');
				}
				if (Object.IsPlayer())
				{
					stringBuilder.Append(PossessiveObject ? Object.its : Object.itself);
				}
				else
				{
					string defaultDefiniteArticle = null;
					bool withIndefiniteArticle = IndefiniteObject;
					bool flag;
					if (ObjectPossessedBy != null)
					{
						if (ObjectPossessedBy == Actor)
						{
							defaultDefiniteArticle = ObjectPossessedBy.its;
						}
						else
						{
							flag = !UseFullNames;
							defaultDefiniteArticle = Grammar.MakePossessive(ObjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject));
						}
						withIndefiniteArticle = false;
					}
					GameObject gameObject = Object;
					flag = !UseFullNames;
					string text = gameObject.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle, defaultDefiniteArticle);
					if (PossessiveObject)
					{
						text = Grammar.MakePossessive(text);
					}
					stringBuilder.Append(text);
				}
				AppendWithSpaceIfNeeded(stringBuilder, Extra);
				stringBuilder.Append(EndMark ?? ".");
				if (!value.IsNullOrEmpty())
				{
					stringBuilder.Append("}}");
				}
				HandleMessage(Source ?? Actor ?? The.Player, stringBuilder, ' ', FromDialog, UsePopup);
				return;
			}
			if (!AlwaysVisible)
			{
				GameObject gameObject2 = UseVisibilityOf ?? Source ?? Actor;
				if (gameObject2 == null || !gameObject2.IsVisible())
				{
					return;
				}
			}
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			string value2 = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
			if (!value2.IsNullOrEmpty())
			{
				stringBuilder2.Append("{{").Append(value2).Append('|');
			}
			string value3 = null;
			if (DescribeSubjectDirection | DescribeSubjectDirectionLate)
			{
				value3 = The.Player.DescribeDirectionToward(SubjectPossessedBy ?? Source ?? Actor ?? The.Player);
			}
			string text2 = null;
			bool withIndefiniteArticle2 = IndefiniteSubject;
			if (SubjectPossessedBy != null && (value3.IsNullOrEmpty() | DescribeSubjectDirectionLate))
			{
				if (SubjectPossessedBy.IsPlayer())
				{
					text2 = "your";
					value3 = null;
				}
				else
				{
					bool flag = !UseFullNames;
					text2 = Grammar.MakePossessive(SubjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject));
				}
			}
			else
			{
				GameObject gameObject3 = Actor?.Holder;
				if (gameObject3 != null)
				{
					if (gameObject3.IsPlayer())
					{
						text2 = "your";
						value3 = null;
					}
					else
					{
						bool flag = !UseFullNames;
						text2 = Grammar.MakePossessive(gameObject3.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject));
					}
					withIndefiniteArticle2 = false;
				}
			}
			if (SubjectOverride == null)
			{
				if (Actor != null)
				{
					bool flag = !UseFullNames;
					stringBuilder2.Append(Actor.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle2, text2));
				}
			}
			else if (stringBuilder2.Length > 0)
			{
				if (!text2.IsNullOrEmpty())
				{
					stringBuilder2.Append(text2).Append(' ');
				}
				stringBuilder2.Append(SubjectOverride);
			}
			else if (!text2.IsNullOrEmpty())
			{
				stringBuilder2.Append(text2.Capitalize()).Append(' ').Append(SubjectOverride);
			}
			else
			{
				stringBuilder2.Append(SubjectOverride.Capitalize());
			}
			if (!value3.IsNullOrEmpty() && !DescribeSubjectDirectionLate)
			{
				if (SubjectPossessedBy != null && (Actor == null || !Actor.HasProperName))
				{
					StringBuilder stringBuilder3 = stringBuilder2.Append(" of ");
					bool flag = !UseFullNames;
					stringBuilder3.Append(SubjectPossessedBy.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject)).Append(' ').Append(value3);
				}
				else
				{
					stringBuilder2.Append(' ').Append(value3);
				}
			}
			if (Actor != null)
			{
				stringBuilder2.Append(Actor.GetVerb(Verb));
			}
			else
			{
				stringBuilder2.Append(' ').Append(Verb);
			}
			AppendWithSpaceIfNeeded(stringBuilder2, Preposition);
			stringBuilder2.Append(' ');
			if (Actor != null && Object == Actor && SubjectOverride == null)
			{
				stringBuilder2.Append(PossessiveObject ? Actor.its : Actor.itself);
			}
			else if (Object.IsPlayer())
			{
				stringBuilder2.Append(PossessiveObject ? "your" : "you");
			}
			else
			{
				string defaultDefiniteArticle2 = null;
				bool withIndefiniteArticle3 = IndefiniteObject | IndefiniteObjectForOthers;
				bool flag;
				if (ObjectPossessedBy != null)
				{
					if (ObjectPossessedBy == Actor && SubjectOverride == null)
					{
						defaultDefiniteArticle2 = ObjectPossessedBy.its;
					}
					else
					{
						flag = !UseFullNames;
						defaultDefiniteArticle2 = Grammar.MakePossessive(ObjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteObject));
					}
					withIndefiniteArticle3 = false;
				}
				GameObject gameObject4 = Object;
				flag = !UseFullNames;
				string text3 = gameObject4.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle3, defaultDefiniteArticle2);
				if (PossessiveObject)
				{
					text3 = Grammar.MakePossessive(text3);
				}
				stringBuilder2.Append(text3);
			}
			AppendWithSpaceIfNeeded(stringBuilder2, Extra);
			if (!value3.IsNullOrEmpty() & DescribeSubjectDirectionLate)
			{
				stringBuilder2.Append(' ').Append(value3);
			}
			stringBuilder2.Append(EndMark ?? ".");
			if (!value2.IsNullOrEmpty())
			{
				stringBuilder2.Append("}}");
			}
			HandleMessage(Source ?? Actor ?? The.Player, stringBuilder2, ' ', FromDialog, UsePopup);
			return;
			IL_00b5:
			UsePopup = true;
			goto IL_00b8;
			IL_006c:
			bool num2;
			if (ObjectPossessedBy != null)
			{
				num2 = ObjectPossessedBy.IsPlayer();
			}
			else
			{
				if (Object == null)
				{
					goto IL_00b8;
				}
				num2 = Object.Holder?.IsPlayer() == true;
			}
			if (num2)
			{
				goto IL_00b5;
			}
			goto IL_00b8;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("XDidYToZ", x);
		}
	}

	public static void WDidXToYWithZ(GameObject Actor, string Verb, string DirectPreposition, GameObject DirectObject, string IndirectPreposition, GameObject IndirectObject, string Extra = null, string EndMark = null, string SubjectOverride = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteDirectObject = false, bool IndefiniteIndirectObject = false, bool IndefiniteDirectObjectForOthers = false, bool IndefiniteIndirectObjectForOthers = false, bool PossessiveDirectObject = false, bool PossessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject DirectObjectPossessedBy = null, GameObject IndirectObjectPossessedBy = null, GameObject Source = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		if (!GameObject.Validate(ref DirectObject) || !GameObject.Validate(ref IndirectObject))
		{
			return;
		}
		if (!UsePopup & FromDialog)
		{
			if (!DirectObject.IsPlayer() && !IndirectObject.IsPlayer())
			{
				bool num;
				if (SubjectPossessedBy != null)
				{
					num = SubjectPossessedBy.IsPlayer();
				}
				else
				{
					if (Actor == null)
					{
						goto IL_0083;
					}
					num = Actor.Holder?.IsPlayer() == true;
				}
				if (!num)
				{
					goto IL_0083;
				}
			}
			goto IL_0117;
		}
		goto IL_011a;
		IL_011a:
		if (SubjectOverride == null && Actor != null && Actor.IsPlayer())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			string value = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
			if (!value.IsNullOrEmpty())
			{
				stringBuilder.Append("{{").Append(value).Append('|');
			}
			stringBuilder.Append("You ").Append(Verb);
			AppendWithSpaceIfNeeded(stringBuilder, DirectPreposition);
			stringBuilder.Append(' ');
			if (DirectObject.IsPlayer())
			{
				stringBuilder.Append(PossessiveDirectObject ? DirectObject.its : DirectObject.itself);
			}
			else
			{
				string defaultDefiniteArticle = null;
				bool withIndefiniteArticle = IndefiniteDirectObject;
				if (DirectObjectPossessedBy != null)
				{
					defaultDefiniteArticle = DirectObjectPossessedBy.its;
					withIndefiniteArticle = false;
				}
				GameObject gameObject = DirectObject;
				bool flag = !UseFullNames;
				string text = gameObject.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle, defaultDefiniteArticle);
				if (PossessiveDirectObject)
				{
					text = Grammar.MakePossessive(text);
				}
				stringBuilder.Append(text);
			}
			AppendWithSpaceIfNeeded(stringBuilder, IndirectPreposition);
			stringBuilder.Append(' ');
			if (IndirectObject == DirectObject)
			{
				stringBuilder.Append(PossessiveIndirectObject ? IndirectObject.itself : IndirectObject.them);
			}
			else if (Actor != null && IndirectObject == Actor && SubjectOverride == null)
			{
				stringBuilder.Append(PossessiveIndirectObject ? Actor.its : Actor.itself);
			}
			else if (IndirectObject.IsPlayer())
			{
				stringBuilder.Append(PossessiveIndirectObject ? "yours" : "you");
			}
			else
			{
				string defaultDefiniteArticle2 = null;
				bool withIndefiniteArticle2 = IndefiniteIndirectObject;
				if (IndirectObjectPossessedBy != null)
				{
					defaultDefiniteArticle2 = IndirectObjectPossessedBy.its;
				}
				GameObject gameObject2 = IndirectObject;
				bool flag = !UseFullNames;
				string text2 = gameObject2.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle2, defaultDefiniteArticle2);
				if (PossessiveIndirectObject)
				{
					text2 = Grammar.MakePossessive(text2);
				}
				stringBuilder.Append(text2);
			}
			AppendWithSpaceIfNeeded(stringBuilder, Extra);
			stringBuilder.Append(EndMark ?? ".");
			if (!value.IsNullOrEmpty())
			{
				stringBuilder.Append("}}");
			}
			HandleMessage(Source ?? Actor ?? The.Player, stringBuilder, ' ', FromDialog, UsePopup);
			return;
		}
		if (!AlwaysVisible)
		{
			GameObject gameObject3 = UseVisibilityOf ?? Source ?? Actor;
			if (gameObject3 == null || !gameObject3.IsVisible())
			{
				return;
			}
		}
		StringBuilder stringBuilder2 = Event.NewStringBuilder();
		string value2 = Color ?? ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
		if (!value2.IsNullOrEmpty())
		{
			stringBuilder2.Append("{{").Append(value2).Append('|');
		}
		string value3 = null;
		if (DescribeSubjectDirection | DescribeSubjectDirectionLate)
		{
			value3 = The.Player.DescribeDirectionToward(SubjectPossessedBy ?? Source ?? Actor ?? The.Player);
		}
		string text3 = null;
		bool withIndefiniteArticle3 = IndefiniteSubject;
		if (SubjectPossessedBy != null && (value3.IsNullOrEmpty() | DescribeSubjectDirectionLate))
		{
			if (SubjectPossessedBy.IsPlayer())
			{
				text3 = "your";
				value3 = null;
			}
			else
			{
				bool flag = !UseFullNames;
				text3 = Grammar.MakePossessive(SubjectPossessedBy.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject));
			}
			withIndefiniteArticle3 = false;
		}
		else
		{
			GameObject gameObject4 = Actor?.Holder;
			if (gameObject4 != null)
			{
				if (gameObject4.IsPlayer())
				{
					text3 = "your";
					value3 = null;
				}
				else
				{
					bool flag = !UseFullNames;
					text3 = Grammar.MakePossessive(gameObject4.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject));
				}
				withIndefiniteArticle3 = false;
			}
		}
		if (SubjectOverride == null)
		{
			if (Actor != null)
			{
				bool flag = !UseFullNames;
				stringBuilder2.Append(Actor.One(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle3, text3));
			}
		}
		else if (stringBuilder2.Length > 0)
		{
			if (!text3.IsNullOrEmpty())
			{
				stringBuilder2.Append(text3).Append(' ');
			}
			stringBuilder2.Append(SubjectOverride);
		}
		else if (!text3.IsNullOrEmpty())
		{
			stringBuilder2.Append(text3.Capitalize()).Append(' ').Append(SubjectOverride);
		}
		else
		{
			stringBuilder2.Append(SubjectOverride.Capitalize());
		}
		if (!value3.IsNullOrEmpty() && !DescribeSubjectDirectionLate)
		{
			if (SubjectPossessedBy != null && (Actor != null || !Actor.HasProperName))
			{
				StringBuilder stringBuilder3 = stringBuilder2.Append(" of ");
				bool flag = !UseFullNames;
				stringBuilder3.Append(SubjectPossessedBy.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, IndefiniteSubject)).Append(' ').Append(value3);
			}
			else
			{
				stringBuilder2.Append(' ').Append(value3);
			}
		}
		if (Actor != null)
		{
			stringBuilder2.Append(Actor.GetVerb(Verb));
		}
		else
		{
			stringBuilder2.Append(' ').Append(Verb);
		}
		AppendWithSpaceIfNeeded(stringBuilder2, DirectPreposition);
		stringBuilder2.Append(' ');
		if (Actor != null && DirectObject == Actor && SubjectOverride == null)
		{
			stringBuilder2.Append(PossessiveDirectObject ? Actor.its : Actor.itself);
		}
		else if (DirectObject.IsPlayer())
		{
			stringBuilder2.Append(PossessiveDirectObject ? "yours" : "you");
		}
		else
		{
			string defaultDefiniteArticle3 = null;
			bool withIndefiniteArticle4 = IndefiniteDirectObject | IndefiniteDirectObjectForOthers;
			if (DirectObjectPossessedBy != null && !DirectObject.HasProperName)
			{
				defaultDefiniteArticle3 = DirectObjectPossessedBy.its;
				withIndefiniteArticle4 = false;
			}
			GameObject gameObject5 = DirectObject;
			bool flag = !UseFullNames;
			string text4 = gameObject5.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle4, defaultDefiniteArticle3);
			if (PossessiveDirectObject)
			{
				text4 = Grammar.MakePossessive(text4);
			}
			stringBuilder2.Append(text4);
		}
		AppendWithSpaceIfNeeded(stringBuilder2, IndirectPreposition);
		stringBuilder2.Append(' ');
		if (IndirectObject == DirectObject)
		{
			stringBuilder2.Append(PossessiveIndirectObject ? IndirectObject.itself : IndirectObject.them);
		}
		else if (Actor != null && IndirectObject == Actor && SubjectOverride == null)
		{
			stringBuilder2.Append(PossessiveIndirectObject ? Actor.its : Actor.itself);
		}
		else if (IndirectObject.IsPlayer())
		{
			stringBuilder2.Append(PossessiveIndirectObject ? "yours" : "you");
		}
		else
		{
			string defaultDefiniteArticle4 = null;
			bool withIndefiniteArticle5 = IndefiniteIndirectObject | IndefiniteIndirectObjectForOthers;
			if (IndirectObjectPossessedBy != null)
			{
				defaultDefiniteArticle4 = IndirectObjectPossessedBy.its;
				withIndefiniteArticle5 = false;
			}
			GameObject gameObject6 = IndirectObject;
			bool flag = !UseFullNames;
			string text5 = gameObject6.one(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, !UseFullNames, flag, BaseOnly: false, withIndefiniteArticle5, defaultDefiniteArticle4);
			if (PossessiveIndirectObject)
			{
				text5 = Grammar.MakePossessive(text5);
			}
			stringBuilder2.Append(text5);
		}
		AppendWithSpaceIfNeeded(stringBuilder2, Extra);
		if (!value3.IsNullOrEmpty() & DescribeSubjectDirectionLate)
		{
			stringBuilder2.Append(' ').Append(value3);
		}
		stringBuilder2.Append(EndMark ?? ".");
		if (!value2.IsNullOrEmpty())
		{
			stringBuilder2.Append("}}");
		}
		HandleMessage(Source ?? Actor ?? The.Player, stringBuilder2, ' ', FromDialog, UsePopup);
		return;
		IL_00cc:
		bool num2;
		if (IndirectObjectPossessedBy != null)
		{
			num2 = IndirectObjectPossessedBy.IsPlayer();
		}
		else
		{
			if (IndirectObject == null)
			{
				goto IL_011a;
			}
			num2 = IndirectObject.Holder?.IsPlayer() == true;
		}
		if (num2)
		{
			goto IL_0117;
		}
		goto IL_011a;
		IL_0083:
		bool num3;
		if (DirectObjectPossessedBy != null)
		{
			num3 = DirectObjectPossessedBy.IsPlayer();
		}
		else
		{
			if (DirectObject == null)
			{
				goto IL_00cc;
			}
			num3 = DirectObject.Holder?.IsPlayer() == true;
		}
		if (!num3)
		{
			goto IL_00cc;
		}
		goto IL_0117;
		IL_0117:
		UsePopup = true;
		goto IL_011a;
	}

	private static void AppendWithSpaceIfNeeded(StringBuilder SB, string Text)
	{
		if (!Text.IsNullOrEmpty())
		{
			if (!Text.StartsWith(",") && !Text.StartsWith(";") && !Text.StartsWith("."))
			{
				SB.Append(' ');
			}
			SB.Append(Text);
		}
	}
}
