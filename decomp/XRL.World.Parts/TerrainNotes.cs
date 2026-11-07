using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Occult.Engine.CodeGeneration;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
[GenerateSerializationPartial]
public class TerrainNotes : IPartWithPrefabImposter
{
	public bool tracked;

	public bool shown;

	[NonSerialized]
	private string descriptionCache;

	[NonSerialized]
	public List<JournalMapNote> notes;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(tracked);
		Writer.Write(shown);
		Writer.WriteOptimized(prefabID);
		Writer.Write(ImposterActive);
		Writer.Write(VisibleOnly);
		Writer.WriteOptimized(X);
		Writer.WriteOptimized(Y);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		tracked = Reader.ReadBoolean();
		shown = Reader.ReadBoolean();
		prefabID = Reader.ReadOptimizedString();
		ImposterActive = Reader.ReadBoolean();
		VisibleOnly = Reader.ReadBoolean();
		X = Reader.ReadOptimizedInt32();
		Y = Reader.ReadOptimizedInt32();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		descriptionCache = null;
		notes = null;
		tracked = false;
		InitNotes();
		shown = false;
		foreach (JournalMapNote note in notes)
		{
			if (JournalAPI.GetCategoryMapNoteToggle(note.Category))
			{
				shown = true;
				break;
			}
		}
		IEnumerable<JournalMapNote> source = notes.Where((JournalMapNote n) => n?.WorldID == ParentObject?.CurrentCell?.ParentZone?.GetZoneWorld());
		if (source.Count() > 0 && shown)
		{
			string value = "b";
			if (source.Any((JournalMapNote item) => item.Tracked))
			{
				tracked = true;
			}
			if (source.Any((JournalMapNote item) => item.Category == "lairs"))
			{
				value = "M";
			}
			else if (source.Any((JournalMapNote item) => item.Category == "oddities"))
			{
				value = "G";
			}
			else if (source.Any((JournalMapNote item) => item.Category == "settlements"))
			{
				value = "W";
			}
			ParentObject.SetStringProperty("OverlayDetailColor", value);
			ParentObject.SetStringProperty("OverlayTile", "assets_content_textures_text_42.bmp");
			ParentObject.SetStringProperty("OverlayRenderString", "*");
			prefabID = "Prefabs/Imposters/NoteMarker";
		}
		else
		{
			ParentObject.DeleteStringProperty("OverlayDetailColor");
			ParentObject.DeleteStringProperty("OverlayTile");
			ParentObject.DeleteStringProperty("OverlayRenderString");
			prefabID = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (descriptionCache == null)
		{
			InitNotes();
			StringBuilder stringBuilder = Event.NewStringBuilder();
			IEnumerable<JournalMapNote> enumerable = notes.Where((JournalMapNote n) => n?.WorldID == ParentObject?.CurrentCell?.ParentZone?.GetZoneWorld());
			if (enumerable.Count() > 0)
			{
				stringBuilder.AppendLine(" ");
				stringBuilder.AppendLine("&CNotes:&y");
			}
			foreach (JournalMapNote item in enumerable)
			{
				stringBuilder.AppendLine(item.Text);
			}
			descriptionCache = stringBuilder.ToString();
		}
		if (!string.IsNullOrEmpty(descriptionCache))
		{
			E.Postfix.Append(descriptionCache);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void InitNotes()
	{
		if (notes == null)
		{
			notes = JournalAPI.GetRevealedMapNotesForWorldMapCell(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (tracked)
		{
			if (DateTime.Now.Millisecond % 500 <= 250)
			{
				E.ColorString += "&g^G";
				E.DetailColor = "G";
			}
			else
			{
				E.ColorString += "&g^g";
				E.DetailColor = "g";
			}
		}
		return base.Render(E);
	}
}
