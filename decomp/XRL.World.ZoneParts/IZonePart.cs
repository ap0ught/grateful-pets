using System;
using System.Reflection;
using Newtonsoft.Json;

namespace XRL.World.ZoneParts;

[Serializable]
public class IZonePart : IComponent<Zone>
{
	public class EventBinder : IEventBinder
	{
		public static readonly EventBinder Instance = new EventBinder();

		public override void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
		{
			Writer.WriteOptimized(((IZonePart)Handler).ParentZone?.ZoneID);
			Writer.WriteTokenized(Handler.GetType());
		}

		public override IEventHandler ReadBind(SerializationReader Reader, int ID)
		{
			string text = Reader.ReadOptimizedString();
			Type type = Reader.ReadTokenizedType();
			if (!text.IsNullOrEmpty() && The.ZoneManager.CachedZones.TryGetValue(text, out var value) && !value.Parts.IsNullOrEmpty())
			{
				foreach (IZonePart part in value.Parts)
				{
					if ((object)part.GetType() == type)
					{
						return part;
					}
				}
			}
			return null;
		}
	}

	[NonSerialized]
	public Zone ParentZone;

	[NonSerialized]
	private string _Name;

	public sealed override IEventBinder Binder => EventBinder.Instance;

	[JsonIgnore]
	public string Name => _Name = ModManager.ResolveTypeName(GetType());

	public sealed override void RegisterEvent(int EventID, int Order = 0, bool Serialize = false)
	{
		throw new NotImplementedException();
	}

	public sealed override void UnregisterEvent(int EventID)
	{
		throw new NotImplementedException();
	}

	public override Zone GetComponentBasis()
	{
		return ParentZone;
	}

	public virtual bool SameAs(IZonePart Part)
	{
		if (Part.ParentZone != ParentZone)
		{
			return false;
		}
		if (Part.GetType().Name != GetType().Name)
		{
			return false;
		}
		return true;
	}

	public virtual IZonePart DeepCopy(Zone Parent)
	{
		IZonePart zonePart = Activator.CreateInstance(GetType()) as IZonePart;
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0 && !fieldInfo.IsLiteral)
			{
				fieldInfo.SetValue(zonePart, fieldInfo.GetValue(this));
			}
		}
		zonePart.ParentZone = Parent;
		return zonePart;
	}

	public static void Save(IZonePart Part, SerializationWriter Writer)
	{
		SerializationWriter.Block block = Writer.StartBlock();
		Type type = Part.GetType();
		try
		{
			Writer.WriteTokenized(type);
			Part.Write(Part.ParentZone, Writer);
		}
		catch (Exception ex)
		{
			block.Reset();
			MetricsManager.LogAssemblyError(type, "Skipping failed serialization of zone part '" + type.FullName + "': " + ex);
		}
		finally
		{
			block.Dispose();
		}
	}

	public static IZonePart Load(Zone Zone, SerializationReader Reader)
	{
		Reader.StartBlock(out var Position, out var Length);
		if (Length == 0)
		{
			return null;
		}
		Type type = null;
		IZonePart zonePart = null;
		try
		{
			type = Reader.ReadTokenizedType();
			zonePart = (IZonePart)Activator.CreateInstance(type);
			zonePart.ParentZone = Zone;
			zonePart.Read(Zone, Reader);
		}
		catch (Exception exception)
		{
			if (zonePart == null || !zonePart.ReadError(exception, Reader, Position, Length))
			{
				Reader.SkipBlock(exception, type, Position, Length);
			}
		}
		return zonePart;
	}

	public virtual void Setup()
	{
	}

	public virtual void Remove()
	{
	}

	public virtual void Initialize()
	{
	}

	public virtual void Attach()
	{
	}

	public virtual void AddedAfterCreation()
	{
	}
}
