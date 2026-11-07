using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Tattoos : IPart
{
	public enum ApplyResult
	{
		Tattooable,
		Engravable,
		TooManyTattoos,
		AbstractBodyPart,
		NonContactBodyPart,
		InappropriateBodyPart,
		NoUsableBodyParts
	}

	public static readonly int ICON_COLOR_PRIORITY = 150;

	[NonSerialized]
	public Dictionary<int, List<string>> Descriptions = new Dictionary<int, List<string>>();

	public string ColorString;

	public string DetailColor;

	[NonSerialized]
	public static char[] splitterColon = new char[1] { ':' };

	public string InitialTattoos
	{
		set
		{
			ParentObject.SetStringProperty("InitialTattoos", value);
		}
	}

	public static bool IsSuccess(ApplyResult Result)
	{
		if (Result != ApplyResult.Tattooable)
		{
			return Result == ApplyResult.Engravable;
		}
		return true;
	}

	public static ApplyResult GetBodyPartGeneralTattooability(BodyPart part)
	{
		if (part.Abstract)
		{
			return ApplyResult.AbstractBodyPart;
		}
		if (!part.Contact)
		{
			return ApplyResult.NonContactBodyPart;
		}
		switch (part.Category)
		{
		case 1:
		case 3:
		case 4:
		case 5:
		case 12:
			return ApplyResult.Tattooable;
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 13:
		case 14:
		case 15:
		case 21:
			return ApplyResult.Engravable;
		default:
			return ApplyResult.InappropriateBodyPart;
		}
	}

	public static ApplyResult CanApplyTattoo(GameObject Subject, BodyPart Part, bool CanTattoo, bool CanEngrave)
	{
		ApplyResult bodyPartGeneralTattooability = GetBodyPartGeneralTattooability(Part);
		switch (bodyPartGeneralTattooability)
		{
		case ApplyResult.Tattooable:
			if (!CanTattoo)
			{
				return ApplyResult.InappropriateBodyPart;
			}
			break;
		case ApplyResult.Engravable:
			if (!CanEngrave)
			{
				return ApplyResult.InappropriateBodyPart;
			}
			break;
		}
		Tattoos part = Subject.GetPart<Tattoos>();
		if (part != null && !part.CanAddTattoo(Part))
		{
			return ApplyResult.TooManyTattoos;
		}
		return bodyPartGeneralTattooability;
	}

	public static ApplyResult ApplyTattoo(GameObject Subject, BodyPart Part, bool CanTattoo, bool CanEngrave, string Desc, string Color = null, string Detail = null)
	{
		ApplyResult bodyPartGeneralTattooability = GetBodyPartGeneralTattooability(Part);
		if (!IsSuccess(bodyPartGeneralTattooability))
		{
			return bodyPartGeneralTattooability;
		}
		switch (bodyPartGeneralTattooability)
		{
		case ApplyResult.Tattooable:
			if (!CanTattoo)
			{
				return ApplyResult.InappropriateBodyPart;
			}
			break;
		case ApplyResult.Engravable:
			if (!CanEngrave)
			{
				return ApplyResult.InappropriateBodyPart;
			}
			break;
		}
		Tattoos tattoos = Subject.RequirePart<Tattoos>();
		if (!tattoos.AddTattoo(Part, Desc))
		{
			return ApplyResult.TooManyTattoos;
		}
		if (!Color.IsNullOrEmpty())
		{
			tattoos.ColorString = Color;
			if (!Detail.IsNullOrEmpty())
			{
				tattoos.DetailColor = Detail;
			}
		}
		Subject.CheckMarkOfDeath();
		return bodyPartGeneralTattooability;
	}

	public static ApplyResult ApplyTattoo(GameObject Subject, bool CanTattoo, bool CanEngrave, string Desc, string Color = null, string Detail = null)
	{
		Body body = Subject.Body;
		if (body == null)
		{
			return ApplyResult.NoUsableBodyParts;
		}
		List<BodyPart> parts = body.GetParts();
		List<BodyPart> list = new List<BodyPart>(parts.Count);
		foreach (BodyPart item in parts)
		{
			if (IsSuccess(CanApplyTattoo(Subject, item, CanTattoo, CanEngrave)))
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return ApplyResult.NoUsableBodyParts;
		}
		return ApplyTattoo(Subject, list.GetRandomElement(), CanTattoo, CanEngrave, Desc, Color, Detail);
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		Tattoos tattoos = base.DeepCopy(Parent) as Tattoos;
		tattoos.Descriptions = new Dictionary<int, List<string>>(Descriptions.Count);
		foreach (KeyValuePair<int, List<string>> description in Descriptions)
		{
			tattoos.Descriptions[description.Key] = new List<string>(description.Value);
		}
		return tattoos;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(Descriptions.Count);
		foreach (int key in Descriptions.Keys)
		{
			Writer.Write(key);
			List<string> list = Descriptions[key];
			Writer.Write(list.Count);
			foreach (string item in list)
			{
				Writer.Write(item);
			}
		}
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int key = Reader.ReadInt32();
			int num2 = Reader.ReadInt32();
			List<string> list = new List<string>(num2);
			for (int j = 0; j < num2; j++)
			{
				list.Add(Reader.ReadString());
			}
			Descriptions.Add(key, list);
		}
		base.Read(Basis, Reader);
	}

	public void ValidateTattoos()
	{
		if (Descriptions.Count == 0)
		{
			return;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			RemoveTattoos();
			return;
		}
		List<int> list = null;
		foreach (int key in Descriptions.Keys)
		{
			if (body.GetPartByID(key) == null)
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(key);
			}
		}
		if (list != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				RemoveTattoo(list[i]);
			}
		}
	}

	public bool AnyTattoosOnTattooablePart()
	{
		if (Descriptions.Count == 0)
		{
			return false;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return false;
		}
		foreach (int key in Descriptions.Keys)
		{
			BodyPart partByID = body.GetPartByID(key);
			if (partByID != null && GetBodyPartGeneralTattooability(partByID) == ApplyResult.Tattooable)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyTattoosOnEngravablePart()
	{
		if (Descriptions.Count == 0)
		{
			return false;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return false;
		}
		foreach (int key in Descriptions.Keys)
		{
			BodyPart partByID = body.GetPartByID(key);
			if (partByID != null && GetBodyPartGeneralTattooability(partByID) == ApplyResult.Engravable)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanAddTattoo(BodyPart part)
	{
		if (part._ID != 0 && Descriptions.ContainsKey(part._ID) && Descriptions[part._ID].Count >= RuleSettings.MAXIMUM_TATTOOS_PER_BODY_PART)
		{
			return false;
		}
		return true;
	}

	public bool CanAddTattoo(int BodyPartID)
	{
		if (Descriptions.ContainsKey(BodyPartID) && Descriptions[BodyPartID].Count >= RuleSettings.MAXIMUM_TATTOOS_PER_BODY_PART)
		{
			return false;
		}
		return true;
	}

	public bool AddTattoo(BodyPart part, string desc)
	{
		if (part._ID != 0 && Descriptions.ContainsKey(part._ID))
		{
			List<string> list = Descriptions[part.ID];
			if (list.Count >= RuleSettings.MAXIMUM_TATTOOS_PER_BODY_PART)
			{
				return false;
			}
			list.Add(desc);
		}
		else
		{
			Descriptions.Add(part.ID, new List<string> { desc });
		}
		return true;
	}

	public bool AddTattoo(int BodyPartID, string desc)
	{
		if (Descriptions.ContainsKey(BodyPartID))
		{
			List<string> list = Descriptions[BodyPartID];
			if (list.Count >= RuleSettings.MAXIMUM_TATTOOS_PER_BODY_PART)
			{
				return false;
			}
			list.Add(desc);
		}
		else
		{
			Descriptions.Add(BodyPartID, new List<string> { desc });
		}
		return true;
	}

	public override void Remove()
	{
		ParentObject?.CheckMarkOfDeath(this);
		base.Remove();
	}

	public bool RemoveTattoos()
	{
		bool result = Descriptions.Count > 0;
		Descriptions.Clear();
		ColorString = null;
		DetailColor = null;
		GameObject parentObject = ParentObject;
		if (parentObject != null)
		{
			parentObject.CheckMarkOfDeath();
			return result;
		}
		return result;
	}

	public bool RemoveTattoo(int BodyPartID)
	{
		bool result = Descriptions.ContainsKey(BodyPartID);
		Descriptions.Remove(BodyPartID);
		if (Descriptions.Count == 0)
		{
			ColorString = null;
			DetailColor = null;
		}
		GameObject parentObject = ParentObject;
		if (parentObject != null)
		{
			parentObject.CheckMarkOfDeath();
			return result;
		}
		return result;
	}

	public List<int> GetBodyPartIDsSortedByPosition()
	{
		List<int> list = new List<int>(Descriptions.Keys);
		if (list.Count < 2)
		{
			return list;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return list;
		}
		List<BodyPart> parts = body.GetParts();
		List<int> PartIDs = new List<int>(parts.Count);
		foreach (BodyPart item in parts)
		{
			if (item._ID != 0)
			{
				PartIDs.Add(item._ID);
			}
		}
		list.Sort((int a, int b) => PartIDs.IndexOf(a).CompareTo(PartIDs.IndexOf(b)));
		return list;
	}

	public string GetTattoosDescription()
	{
		ValidateTattoos();
		if (Descriptions.Count == 0)
		{
			return null;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return null;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		foreach (int item in GetBodyPartIDsSortedByPosition())
		{
			BodyPart partByID = body.GetPartByID(item);
			List<string> list = Descriptions[item];
			stringBuilder.Compound(ParentObject.Its).Append(' ').Append(partByID.Name)
				.Append(' ')
				.Append(partByID.Plural ? "bear" : "bears")
				.Append(' ');
			bool flag = GetBodyPartGeneralTattooability(partByID) == ApplyResult.Engravable;
			string text;
			if (list.Count == 1)
			{
				stringBuilder.Append(flag ? "an engraving of " : "a tattoo of ").Append(list[0]);
				text = list[0];
			}
			else
			{
				stringBuilder.Append(flag ? "engravings of " : "tattoos of ").Append(Grammar.MakeAndList(list));
				text = list[list.Count - 1];
			}
			if (text.IndexOf('.') != -1 || text.IndexOf('!') != -1 || text.IndexOf('?') != -1)
			{
				char c = ColorUtility.LastCharacterExceptFormatting(text);
				if (c != '.' && c != '!' && c != '?')
				{
					stringBuilder.Append('.');
				}
			}
			else
			{
				stringBuilder.Append('.');
			}
		}
		return stringBuilder.ToString();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (!ColorString.IsNullOrEmpty() || !DetailColor.IsNullOrEmpty())
		{
			E.ApplyColors(ColorString, DetailColor, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ColorString", ColorString);
		E.AddEntry(this, "DetailColor", DetailColor);
		if (Descriptions != null && Descriptions.Count > 0)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			foreach (int item in GetBodyPartIDsSortedByPosition())
			{
				BodyPart bodyPartByID = ParentObject.GetBodyPartByID(item);
				foreach (string item2 in Descriptions[item])
				{
					stringBuilder.Append(bodyPartByID.GetOrdinalName()).Append(": ").Append(item2)
						.Append('\n');
				}
			}
			E.AddEntry(this, "Descriptions", stringBuilder.ToString());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		string tattoosDescription = GetTattoosDescription();
		if (!tattoosDescription.IsNullOrEmpty())
		{
			if (E.Postfix.Length > 0 && E.Postfix[E.Postfix.Length - 1] != '\n')
			{
				E.Postfix.Append('\n');
			}
			E.Postfix.Append('\n').Append(tattoosDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject && Cloning.IsCloning(E.Context))
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		string stringProperty = ParentObject.GetStringProperty("InitialTattoos");
		if (stringProperty != null)
		{
			ParentObject.RemoveStringProperty("InitialTattoos");
			Body body = ParentObject.Body;
			if (body != null)
			{
				string[] array = stringProperty.Split('|');
				foreach (string text in array)
				{
					string[] array2 = text.Split(splitterColon, 2);
					if (array2.Length == 2)
					{
						string text2 = array2[0];
						string desc = array2[1];
						if (text2 == "*")
						{
							ApplyTattoo(ParentObject, CanTattoo: true, CanEngrave: true, desc);
							continue;
						}
						BodyPart bodyPart = body.GetPartByName(text2) ?? body.GetFirstPart(text2);
						if (bodyPart != null)
						{
							ApplyTattoo(ParentObject, bodyPart, CanTattoo: true, CanEngrave: true, desc);
							continue;
						}
						MetricsManager.LogError("could not find body part " + text2 + " on " + ParentObject.Blueprint + " for InitialTattoos spec " + text);
					}
					else
					{
						MetricsManager.LogError("bad InitialTattoos spec: " + text);
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

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("HasTattoo");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "HasTattoo")
		{
			string stringParameter = E.GetStringParameter("MatchText");
			if (!stringParameter.IsNullOrEmpty())
			{
				CompareOptions comp = ((!E.HasFlag("CaseSensitive")) ? CompareOptions.IgnoreCase : CompareOptions.None);
				foreach (List<string> value in Descriptions.Values)
				{
					foreach (string item in value)
					{
						if (item.Contains(stringParameter, comp))
						{
							return false;
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
