using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;
using Qud.API;
using XRL.Language;
using XRL.Messages;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Anatomy;

[Serializable]
[GenerateSerializationPartial(Write = "WriteValues", Read = "ReadValues")]
public class BodyPart : IComposite
{
	public const int FLAG_DEFAULT_PRIMARY = 1;

	public const int FLAG_PREFERRED_PRIMARY = 2;

	public const int FLAG_PRIMARY = 4;

	public const int FLAG_NATIVE = 8;

	public const int FLAG_APPENDAGE = 16;

	public const int FLAG_INTEGRAL = 32;

	public const int FLAG_MORTAL = 64;

	public const int FLAG_ABSTRACT = 128;

	public const int FLAG_EXTRINSIC = 256;

	public const int FLAG_DYNAMIC = 512;

	public const int FLAG_PLURAL = 1024;

	public const int FLAG_MASS = 2048;

	public const int FLAG_CONTACT = 4096;

	public const int FLAG_IGNORE_POSITION = 8192;

	public const int FLAG_FIRST_EQUIPPED = 16384;

	public const int FLAG_FIRST_CYBERNETIC = 32768;

	public const int FLAG_FIRST_DEFAULT = 65536;

	public const int FLAG_THROWN = 131072;

	public string Type;

	public string VariantType;

	public string Description;

	public string DescriptionPrefix;

	public string Name;

	public string SupportsDependent;

	public string DependsOn;

	public string RequiresType;

	public string Manager;

	public int Category = 1;

	public int Laterality;

	public int RequiresLaterality = 65535;

	public int Mobility;

	public int Flags = 4096;

	[NonSerialized]
	public Body ParentBody;

	[NonSerialized]
	public BodyPart ParentPart;

	public string DefaultBehaviorBlueprint;

	[NonSerialized]
	public GameObject _DefaultBehavior;

	public int Position = -1;

	[NonSerialized]
	public GameObject _Cybernetics;

	[NonSerialized]
	public GameObject _Equipped;

	[NonSerialized]
	public List<BodyPart> Parts;

	public static int IDPreloadSequence = -1;

	public int _ID;

	[NonSerialized]
	private static List<BodyPart> ToClearEquip = new List<BodyPart>();

	[NonSerialized]
	private static List<BodyPart> SeverWork = new List<BodyPart>(8);

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	public bool DefaultPrimary
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	public bool PreferredPrimary
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	public bool Primary
	{
		get
		{
			return (Flags & 4) == 4;
		}
		set
		{
			Flags = (value ? (Flags | 4) : (Flags & -5));
		}
	}

	public bool Native
	{
		get
		{
			return (Flags & 8) == 8;
		}
		set
		{
			Flags = (value ? (Flags | 8) : (Flags & -9));
		}
	}

	public bool Appendage
	{
		get
		{
			return (Flags & 0x10) == 16;
		}
		set
		{
			Flags = (value ? (Flags | 0x10) : (Flags & -17));
		}
	}

	public bool Integral
	{
		get
		{
			return (Flags & 0x20) == 32;
		}
		set
		{
			Flags = (value ? (Flags | 0x20) : (Flags & -33));
		}
	}

	public bool Mortal
	{
		get
		{
			return (Flags & 0x40) == 64;
		}
		set
		{
			Flags = (value ? (Flags | 0x40) : (Flags & -65));
		}
	}

	public bool Abstract
	{
		get
		{
			return (Flags & 0x80) == 128;
		}
		set
		{
			Flags = (value ? (Flags | 0x80) : (Flags & -129));
		}
	}

	public bool Extrinsic
	{
		get
		{
			return (Flags & 0x100) == 256;
		}
		set
		{
			Flags = (value ? (Flags | 0x100) : (Flags & -257));
		}
	}

	public bool Dynamic
	{
		get
		{
			return (Flags & 0x200) == 512;
		}
		set
		{
			Flags = (value ? (Flags | 0x200) : (Flags & -513));
		}
	}

	public bool Plural
	{
		get
		{
			return (Flags & 0x400) == 1024;
		}
		set
		{
			Flags = (value ? (Flags | 0x400) : (Flags & -1025));
		}
	}

	public bool Mass
	{
		get
		{
			return (Flags & 0x800) == 2048;
		}
		set
		{
			Flags = (value ? (Flags | 0x800) : (Flags & -2049));
		}
	}

	public bool Contact
	{
		get
		{
			return (Flags & 0x1000) == 4096;
		}
		set
		{
			Flags = (value ? (Flags | 0x1000) : (Flags & -4097));
		}
	}

	public bool Thrown
	{
		get
		{
			return (Flags & 0x20000) == 131072;
		}
		set
		{
			Flags = (value ? (Flags | 0x20000) : (Flags & -131073));
		}
	}

	public bool IgnorePosition
	{
		get
		{
			return (Flags & 0x2000) == 8192;
		}
		set
		{
			Flags = (value ? (Flags | 0x2000) : (Flags & -8193));
		}
	}

	public bool FirstSlotForEquipped
	{
		get
		{
			return (Flags & 0x4000) == 16384;
		}
		set
		{
			Flags = (value ? (Flags | 0x4000) : (Flags & -16385));
		}
	}

	public bool FirstSlotForCybernetics
	{
		get
		{
			return (Flags & 0x8000) == 32768;
		}
		set
		{
			Flags = (value ? (Flags | 0x8000) : (Flags & -32769));
		}
	}

	public bool FirstSlotForDefaultBehavior
	{
		get
		{
			return (Flags & 0x10000) == 65536;
		}
		set
		{
			Flags = (value ? (Flags | 0x10000) : (Flags & -65537));
		}
	}

	public GameObject DefaultBehavior
	{
		get
		{
			return _DefaultBehavior;
		}
		set
		{
			_DefaultBehavior = value;
			if (_DefaultBehavior != null)
			{
				_DefaultBehavior.CheckDefaultBehaviorGiganticness(ParentBody?.ParentObject);
				if (ParentBody != null && ParentBody.ParentObject != null)
				{
					DecorateDefaultEquipmentEvent.Send(ParentBody.ParentObject, ParentBody);
				}
			}
			else
			{
				FirstSlotForDefaultBehavior = false;
			}
		}
	}

	public GameObject Cybernetics => _Cybernetics;

	public GameObject Equipped => _Equipped;

	public bool IsDismembered
	{
		get
		{
			if (ParentPart == null)
			{
				if (ParentBody != null)
				{
					return ParentBody._Body != this;
				}
				return true;
			}
			return false;
		}
	}

	public int ID
	{
		get
		{
			if (_ID == 0)
			{
				XRLGame game = The.Game;
				if (game != null)
				{
					_ID = ++game.BodyPartIDSequence;
				}
				else
				{
					_ID = IDPreloadSequence--;
				}
			}
			return _ID;
		}
		set
		{
			_ID = value;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void WriteValues(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Type);
		Writer.WriteOptimized(VariantType);
		Writer.WriteOptimized(Description);
		Writer.WriteOptimized(DescriptionPrefix);
		Writer.WriteOptimized(Name);
		Writer.WriteOptimized(SupportsDependent);
		Writer.WriteOptimized(DependsOn);
		Writer.WriteOptimized(RequiresType);
		Writer.WriteOptimized(Manager);
		Writer.WriteOptimized(Category);
		Writer.WriteOptimized(Laterality);
		Writer.WriteOptimized(RequiresLaterality);
		Writer.WriteOptimized(Mobility);
		Writer.WriteOptimized(Flags);
		Writer.WriteOptimized(DefaultBehaviorBlueprint);
		Writer.WriteOptimized(Position);
		Writer.WriteOptimized(_ID);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void ReadValues(SerializationReader Reader)
	{
		Type = Reader.ReadOptimizedString();
		VariantType = Reader.ReadOptimizedString();
		Description = Reader.ReadOptimizedString();
		DescriptionPrefix = Reader.ReadOptimizedString();
		Name = Reader.ReadOptimizedString();
		SupportsDependent = Reader.ReadOptimizedString();
		DependsOn = Reader.ReadOptimizedString();
		RequiresType = Reader.ReadOptimizedString();
		Manager = Reader.ReadOptimizedString();
		Category = Reader.ReadOptimizedInt32();
		Laterality = Reader.ReadOptimizedInt32();
		RequiresLaterality = Reader.ReadOptimizedInt32();
		Mobility = Reader.ReadOptimizedInt32();
		Flags = Reader.ReadOptimizedInt32();
		DefaultBehaviorBlueprint = Reader.ReadOptimizedString();
		Position = Reader.ReadOptimizedInt32();
		_ID = Reader.ReadOptimizedInt32();
	}

	public bool IDMatch(int TestID)
	{
		return _ID == TestID;
	}

	public bool HasID()
	{
		return _ID != 0;
	}

	public BodyPart()
	{
	}

	public BodyPart(BodyPart Source)
		: this()
	{
		Type = Source.Type;
		VariantType = Source.VariantType;
		Description = Source.Description;
		DescriptionPrefix = Source.DescriptionPrefix;
		Name = Source.Name;
		SupportsDependent = Source.SupportsDependent;
		DependsOn = Source.DependsOn;
		RequiresType = Source.RequiresType;
		Manager = Source.Manager;
		Category = Source.Category;
		Laterality = Source.Laterality;
		RequiresLaterality = Source.RequiresLaterality;
		Mobility = Source.Mobility;
		Flags = Source.Flags;
		ParentBody = Source.ParentBody;
		DefaultBehaviorBlueprint = Source.DefaultBehaviorBlueprint;
		DefaultBehavior = Source.DefaultBehavior;
		Position = Source.Position;
		_Cybernetics = Source._Cybernetics;
		_Equipped = Source._Equipped;
		Parts = ((Source.Parts != null) ? new List<BodyPart>(Source.Parts) : null);
		Thrown = Type == "Thrown Weapon";
	}

	private BodyPart(int Alloc)
	{
		if (Parts == null)
		{
			Parts = new List<BodyPart>(Alloc);
		}
	}

	public BodyPart(Body ParentBody)
		: this()
	{
		this.ParentBody = ParentBody;
	}

	public BodyPart(Body ParentBody, int Alloc)
		: this(Alloc)
	{
		this.ParentBody = ParentBody;
	}

	public BodyPart(Body ParentBody, BodyPart ParentPart)
		: this()
	{
		this.ParentBody = ParentBody;
		this.ParentPart = ParentPart;
	}

	public BodyPart(BodyPartType Base, Body ParentBody)
		: this(ParentBody)
	{
		Base.ApplyTo(this);
		Thrown = Type == "Thrown Weapon";
	}

	public BodyPart(BodyPartType Base, Body ParentBody, int Alloc)
		: this(ParentBody, Alloc)
	{
		Base.ApplyTo(this);
	}

	public BodyPart(string Base, Body ParentBody)
		: this(Anatomies.GetBodyPartTypeOrFail(Base), ParentBody)
	{
	}

	public BodyPart(string Base, Body ParentBody, int Alloc)
		: this(Anatomies.GetBodyPartTypeOrFail(Base), ParentBody, Alloc)
	{
	}

	public BodyPart(string Base, int Laterality, Body ParentBody)
		: this(Base, ParentBody)
	{
		ChangeLaterality(Laterality);
	}

	public BodyPart(string Type, string Description, string Name, Body ParentBody)
		: this(ParentBody)
	{
		this.Type = Type;
		this.Description = Description ?? Type;
		this.Name = Name ?? Description.ToLower();
		Thrown = Type == "Thrown Weapon";
	}

	public BodyPart(BodyPartType Base, Body ParentBody, string DescriptionPrefix = null, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Mortal = null, bool? Appendage = null, bool? Integral = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null)
		: this(Base, ParentBody)
	{
		if (DescriptionPrefix != null)
		{
			this.DescriptionPrefix = DescriptionPrefix;
		}
		if (DefaultBehavior != null)
		{
			DefaultBehaviorBlueprint = DefaultBehavior;
		}
		if (SupportsDependent != null)
		{
			this.SupportsDependent = SupportsDependent;
		}
		if (DependsOn != null)
		{
			this.DependsOn = DependsOn;
		}
		if (RequiresType != null)
		{
			this.RequiresType = RequiresType;
		}
		if (Manager != null)
		{
			this.Manager = Manager;
		}
		if (Category.HasValue)
		{
			this.Category = Category.Value;
		}
		if (RequiresLaterality.HasValue)
		{
			this.RequiresLaterality = RequiresLaterality.Value;
		}
		if (Mobility.HasValue)
		{
			this.Mobility = Mobility.Value;
		}
		if (Appendage.HasValue)
		{
			this.Appendage = Appendage.Value;
		}
		if (Integral.HasValue)
		{
			this.Integral = Integral.Value;
		}
		if (Mortal.HasValue)
		{
			this.Mortal = Mortal.Value;
		}
		if (Abstract.HasValue)
		{
			this.Abstract = Abstract.Value;
		}
		if (Extrinsic.HasValue)
		{
			this.Extrinsic = Extrinsic.Value;
		}
		if (Dynamic.HasValue)
		{
			this.Dynamic = Dynamic.Value;
		}
		if (Plural.HasValue)
		{
			this.Plural = Plural.Value;
		}
		if (Mass.HasValue)
		{
			this.Mass = Mass.Value;
		}
		if (Contact.HasValue)
		{
			this.Contact = Contact.Value;
		}
		if (IgnorePosition.HasValue)
		{
			this.IgnorePosition = IgnorePosition.Value;
		}
	}

	public BodyPart(BodyPartType Base, int Laterality, Body ParentBody, string DescriptionPrefix = null, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Mortal = null, bool? Appendage = null, bool? Integral = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null)
		: this(Base, ParentBody)
	{
		ChangeLaterality(Laterality);
		if (DescriptionPrefix != null)
		{
			this.DescriptionPrefix = DescriptionPrefix;
		}
		if (DefaultBehavior != null)
		{
			DefaultBehaviorBlueprint = DefaultBehavior;
		}
		if (SupportsDependent != null)
		{
			this.SupportsDependent = SupportsDependent;
		}
		if (DependsOn != null)
		{
			this.DependsOn = DependsOn;
		}
		if (RequiresType != null)
		{
			this.RequiresType = RequiresType;
		}
		if (Manager != null)
		{
			this.Manager = Manager;
		}
		if (Category.HasValue)
		{
			this.Category = Category.Value;
		}
		if (RequiresLaterality.HasValue)
		{
			this.RequiresLaterality = RequiresLaterality.Value;
		}
		if (Mobility.HasValue)
		{
			this.Mobility = Mobility.Value;
		}
		if (Appendage.HasValue)
		{
			this.Appendage = Appendage.Value;
		}
		if (Integral.HasValue)
		{
			this.Integral = Integral.Value;
		}
		if (Mortal.HasValue)
		{
			this.Mortal = Mortal.Value;
		}
		if (Abstract.HasValue)
		{
			this.Abstract = Abstract.Value;
		}
		if (Extrinsic.HasValue)
		{
			this.Extrinsic = Extrinsic.Value;
		}
		if (Dynamic.HasValue)
		{
			this.Dynamic = Dynamic.Value;
		}
		if (Plural.HasValue)
		{
			this.Plural = Plural.Value;
		}
		if (Mass.HasValue)
		{
			this.Mass = Mass.Value;
		}
		if (Contact.HasValue)
		{
			this.Contact = Contact.Value;
		}
		if (IgnorePosition.HasValue)
		{
			this.IgnorePosition = IgnorePosition.Value;
		}
	}

	public BodyPart(string Base, int Laterality, Body ParentBody, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Mortal = null, bool? Appendage = null, bool? Integral = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null)
		: this(Base, Laterality, ParentBody)
	{
		if (DefaultBehavior != null)
		{
			DefaultBehaviorBlueprint = DefaultBehavior;
		}
		if (SupportsDependent != null)
		{
			this.SupportsDependent = SupportsDependent;
		}
		if (DependsOn != null)
		{
			this.DependsOn = DependsOn;
		}
		if (RequiresType != null)
		{
			this.RequiresType = RequiresType;
		}
		if (Manager != null)
		{
			this.Manager = Manager;
		}
		if (Category.HasValue)
		{
			this.Category = Category.Value;
		}
		if (RequiresLaterality.HasValue)
		{
			this.RequiresLaterality = RequiresLaterality.Value;
		}
		if (Mobility.HasValue)
		{
			this.Mobility = Mobility.Value;
		}
		if (Appendage.HasValue)
		{
			this.Appendage = Appendage.Value;
		}
		if (Integral.HasValue)
		{
			this.Integral = Integral.Value;
		}
		if (Mortal.HasValue)
		{
			this.Mortal = Mortal.Value;
		}
		if (Abstract.HasValue)
		{
			this.Abstract = Abstract.Value;
		}
		if (Extrinsic.HasValue)
		{
			this.Extrinsic = Extrinsic.Value;
		}
		if (Dynamic.HasValue)
		{
			this.Dynamic = Dynamic.Value;
		}
		if (Plural.HasValue)
		{
			this.Plural = Plural.Value;
		}
		if (Mass.HasValue)
		{
			this.Mass = Mass.Value;
		}
		if (Contact.HasValue)
		{
			this.Contact = Contact.Value;
		}
		if (IgnorePosition.HasValue)
		{
			this.IgnorePosition = IgnorePosition.Value;
		}
	}

	public BodyPart Clone()
	{
		return new BodyPart(this);
	}

	public BodyPartType TypeModel()
	{
		return Anatomies.GetBodyPartTypeOrFail(Type);
	}

	public BodyPartType VariantTypeModel()
	{
		return Anatomies.GetBodyPartTypeOrFail(VariantType ?? Type);
	}

	public BodyPartType GetVariantTypeModelIfExists()
	{
		return Anatomies.GetBodyPartType(VariantType ?? Type);
	}

	public static int Sort(BodyPart a, BodyPart b)
	{
		return a.Position.CompareTo(b.Position);
	}

	public bool ObjectEquippedOnThisOrAnyParent(GameObject Object)
	{
		if (Object == Equipped)
		{
			return true;
		}
		if (Object == Cybernetics)
		{
			return true;
		}
		if (Object == DefaultBehavior)
		{
			return true;
		}
		if (ParentPart == null)
		{
			return false;
		}
		return ParentPart.ObjectEquippedOnThisOrAnyParent(Object);
	}

	public BodyPart ChangeLaterality(int NewLaterality, bool Recursive = false)
	{
		BodyPartType bodyPartType = VariantTypeModel();
		if (IsLateralityConsistent(bodyPartType))
		{
			Laterality = NewLaterality;
			Description = XRL.World.Capabilities.Laterality.WithLateralityAdjective(bodyPartType.Description, Laterality, Capitalized: true);
			Name = XRL.World.Capabilities.Laterality.WithLateralityAdjective(bodyPartType.Name, Laterality);
			RequiresLaterality = bodyPartType.RequiresLateralityFor(Laterality);
		}
		else
		{
			string text = XRL.World.Capabilities.Laterality.StripLateralityAdjective(Description, Laterality, Capitalized: true);
			string text2 = XRL.World.Capabilities.Laterality.StripLateralityAdjective(Name, Laterality);
			Laterality = NewLaterality;
			Description = XRL.World.Capabilities.Laterality.WithLateralityAdjective(text, Laterality, Capitalized: true);
			Name = XRL.World.Capabilities.Laterality.WithLateralityAdjective(text2, Laterality);
			if (RequiresLaterality == 65535)
			{
				RequiresLaterality = bodyPartType.RequiresLateralityFor(Laterality);
			}
			else
			{
				RequiresLaterality |= bodyPartType.RequiresLateralityFor(Laterality);
			}
		}
		if (Recursive && Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].ChangeLaterality(NewLaterality, Recursive: true);
			}
		}
		return this;
	}

	public bool IsLateralityConsistent(BodyPartType UseType = null)
	{
		if (UseType == null)
		{
			UseType = VariantTypeModel();
		}
		if (Description != XRL.World.Capabilities.Laterality.WithLateralityAdjective(UseType.Description, Laterality, Capitalized: true))
		{
			return false;
		}
		if (Name != XRL.World.Capabilities.Laterality.WithLateralityAdjective(UseType.Name, Laterality))
		{
			return false;
		}
		if (RequiresLaterality != 65535 && RequiresLaterality != UseType.RequiresLateralityFor(Laterality))
		{
			return false;
		}
		return true;
	}

	public bool IsLateralitySafeToChange(int ExpectLaterality = 0, Body BackupParentBody = null, BodyPart ChildPart = null)
	{
		if (Laterality != ExpectLaterality)
		{
			return false;
		}
		BodyPartType useType = VariantTypeModel();
		if (!IsLateralityConsistent(useType))
		{
			return false;
		}
		if (ChildPart != null)
		{
			if (!IsParentPartOf(ChildPart, BackupParentBody, EvenIfDismembered: true))
			{
				return false;
			}
			if (!ChildPart.IsLateralitySafeToChange(ExpectLaterality, BackupParentBody))
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerable<BodyPart> LoopParts()
	{
		yield return this;
		if (Parts == null)
		{
			yield break;
		}
		foreach (BodyPart part in Parts)
		{
			foreach (BodyPart item in part.LoopParts())
			{
				yield return item;
			}
		}
	}

	public IEnumerable<BodyPart> LoopOtherParts()
	{
		if (Parts == null)
		{
			yield break;
		}
		foreach (BodyPart part in Parts)
		{
			foreach (BodyPart item in part.LoopParts())
			{
				yield return item;
			}
		}
	}

	public IEnumerable<BodyPart> LoopSubparts()
	{
		if (Parts == null)
		{
			yield break;
		}
		foreach (BodyPart part in Parts)
		{
			yield return part;
		}
	}

	public BodyPart GetParentPart()
	{
		return ParentPart;
	}

	public BodyPart GetPreviousPart()
	{
		return ParentBody?.FindPreviousPartOf(this);
	}

	public BodyPart GetNextPart()
	{
		return ParentBody?.FindNextPartOf(this);
	}

	public BodyPart FindByManager(string Manager)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindByManager(Manager);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		if (this.Manager == Manager)
		{
			return this;
		}
		return null;
	}

	public BodyPart FindByManager(string Manager, string Type)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindByManager(Manager, Type);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		if (this.Manager == Manager && this.Type == Type)
		{
			return this;
		}
		return null;
	}

	public void FindByManager(string Manager, List<BodyPart> Store)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].FindByManager(Manager, Store);
			}
		}
		if (this.Manager == Manager)
		{
			Store.Add(this);
		}
	}

	public void FindByManager(string Manager, string Type, List<BodyPart> Store)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].FindByManager(Manager, Type, Store);
			}
		}
		if (this.Manager == Manager && this.Type == Type)
		{
			Store.Add(this);
		}
	}

	public BodyPart FindParentPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			if (Parts.Contains(FindPart))
			{
				return this;
			}
			foreach (BodyPart part in Parts)
			{
				BodyPart bodyPart = part.FindParentPartOf(FindPart);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public bool IsParentPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			return Parts.Contains(FindPart);
		}
		return false;
	}

	public bool IsParentPartOf(BodyPart FindPart, Body BackupParentBody, bool EvenIfDismembered)
	{
		if (IsParentPartOf(FindPart))
		{
			return true;
		}
		Body body = ((ParentBody != null) ? ParentBody : BackupParentBody);
		if (body != null && body.ValidateDismemberedParentPart(this, FindPart))
		{
			return true;
		}
		return false;
	}

	public bool IsAncestorPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			if (Parts.Contains(FindPart))
			{
				return true;
			}
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].IsAncestorPartOf(FindPart))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart FindPreviousPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			int num = Parts.IndexOf(FindPart);
			if (num != -1)
			{
				if (num <= 0)
				{
					return null;
				}
				return Parts[num - 1];
			}
			foreach (BodyPart part in Parts)
			{
				BodyPart bodyPart = part.FindPreviousPartOf(FindPart);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public BodyPart FindNextPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			int num = Parts.IndexOf(FindPart);
			if (num != -1)
			{
				if (num >= Parts.Count - 1)
				{
					return null;
				}
				return Parts[num + 1];
			}
			foreach (BodyPart part in Parts)
			{
				BodyPart bodyPart = part.FindNextPartOf(FindPart);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public void FindPartsEquipping(GameObject GO, List<BodyPart> Return)
	{
		if (Equipped == GO)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].FindPartsEquipping(GO, Return);
			}
		}
	}

	public BodyPart FindEquippedItem(GameObject GO)
	{
		if (Equipped == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindEquippedItem(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public BodyPart FindEquippedItem(string Blueprint)
	{
		if (Equipped != null && Equipped.Blueprint == Blueprint)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindEquippedItem(Blueprint);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public BodyPart FindDefaultOrEquippedItem(GameObject GO)
	{
		if (Equipped == GO)
		{
			return this;
		}
		if (DefaultBehavior == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindDefaultOrEquippedItem(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public BodyPart FindDefaultOrEquippedItem(string Blueprint)
	{
		if (Equipped != null && Equipped.Blueprint == Blueprint)
		{
			return this;
		}
		if (DefaultBehavior != null && DefaultBehavior.Blueprint == Blueprint)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindDefaultOrEquippedItem(Blueprint);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentByEvent(string ID)
	{
		if (Equipped != null && Equipped.HasRegisteredEvent(ID) && !Equipped.FireEvent(ID))
		{
			return Equipped;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentByEvent(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentOrDefaultByBlueprint(string Blueprint)
	{
		if (Equipped != null && Equipped.Blueprint == Blueprint)
		{
			return Equipped;
		}
		if (DefaultBehavior != null && DefaultBehavior.Blueprint == Blueprint)
		{
			return DefaultBehavior;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentOrDefaultByBlueprint(Blueprint);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentOrDefaultByID(string ID)
	{
		if (Equipped != null && Equipped.IDMatch(ID))
		{
			return Equipped;
		}
		if (DefaultBehavior != null && DefaultBehavior.IDMatch(ID))
		{
			return DefaultBehavior;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentOrDefaultByID(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentByEvent(Event E)
	{
		if (Equipped != null && !Equipped.FireEvent(E))
		{
			return Equipped;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentByEvent(E);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(string ID)
	{
		if (Equipped != null && Equipped.HasRegisteredEvent(ID) && !Equipped.FireEvent(ID))
		{
			return Equipped;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.HasRegisteredEvent(ID) && !Cybernetics.FireEvent(ID))
		{
			return Cybernetics;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentOrCyberneticsByEvent(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(Event E)
	{
		if (Equipped != null && !Equipped.FireEvent(E))
		{
			return Equipped;
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Cybernetics.FireEvent(E))
		{
			return Cybernetics;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentOrCyberneticsByEvent(E);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquippedItem(Predicate<GameObject> Filter)
	{
		if (Equipped != null && (Filter == null || Filter(Equipped)))
		{
			return Equipped;
		}
		if (DefaultBehavior != null && (Filter == null || Filter(DefaultBehavior)))
		{
			return DefaultBehavior;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquippedItem(Filter);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public bool HasEquippedItem(GameObject GO)
	{
		if (Equipped == GO)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].HasEquippedItem(GO))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEquippedItem(string Blueprint)
	{
		if (Equipped != null && Equipped.Blueprint == Blueprint)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].HasEquippedItem(Blueprint))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEquippedItem(Predicate<GameObject> Filter)
	{
		if (Equipped != null && (Filter == null || Filter(Equipped)))
		{
			return true;
		}
		if (DefaultBehavior != null && (Filter == null || Filter(DefaultBehavior)))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].HasEquippedItem(Filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsItemEquippedOnLimbType(GameObject GO, string FindType)
	{
		if (Equipped == GO && Type == FindType)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].IsItemEquippedOnLimbType(GO, FindType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart FindCybernetics(GameObject GO)
	{
		if (Cybernetics == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindCybernetics(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public bool IsItemImplantedInLimbType(GameObject GO, string FindType)
	{
		if (Cybernetics == GO && Type == FindType)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].IsItemImplantedInLimbType(GO, FindType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart FindDefaultBehavior(GameObject GO)
	{
		if (DefaultBehavior == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindDefaultBehavior(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public bool IsItemDefaultBehaviorOnLimbType(GameObject GO, string FindType)
	{
		if (DefaultBehavior == GO && Type == FindType)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].IsItemDefaultBehaviorOnLimbType(GO, FindType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void FlushTransientCaches()
	{
		ParentBody?.ParentObject?.FlushTransientCache();
	}

	public GameObject Unimplant(bool MoveToInventory = true)
	{
		GameObject cybernetics = _Cybernetics;
		if (cybernetics != null)
		{
			UnimplantedEvent.Send(ParentBody.ParentObject, cybernetics, this);
			ImplantRemovedEvent.Send(ParentBody.ParentObject, cybernetics, this);
			if (_Equipped == cybernetics)
			{
				_Equipped = null;
				FirstSlotForEquipped = false;
			}
			if (MoveToInventory && ParentBody != null && ParentBody.ParentObject != null)
			{
				Inventory inventory = ParentBody.ParentObject.Inventory;
				if (inventory != null && !inventory.Objects.Contains(cybernetics))
				{
					inventory.AddObject(cybernetics);
				}
			}
			_Cybernetics = null;
			FirstSlotForCybernetics = false;
			FlushTransientCaches();
		}
		return cybernetics;
	}

	public GameObject Dismember(bool Obliterate = false)
	{
		if (ParentBody == null)
		{
			return null;
		}
		return ParentBody.Dismember(this, null, null, Obliterate);
	}

	public void Implant(GameObject Cybernetic, bool ForDeepCopy = false, bool Silent = false)
	{
		if (_Cybernetics != null)
		{
			Unimplant();
		}
		_Cybernetics = Cybernetic;
		ParentBody.RecalculateFirstCybernetics(Cybernetic);
		if (Cybernetic.HasTag("CyberneticsUsesEqSlot"))
		{
			if (_Equipped != null && !TryUnequip())
			{
				ForceUnequip(Silent);
			}
			Equip(Cybernetic, null, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true);
		}
		ImplantedEvent.Send(ParentBody.ParentObject, Cybernetic, this, null, ForDeepCopy, Silent);
		ImplantAddedEvent.Send(ParentBody.ParentObject, Cybernetic, this, null, ForDeepCopy, Silent);
		ParentBody.RegenerateDefaultEquipment();
		if (!ForDeepCopy && !Silent && ParentBody.ParentObject.IsPlayer())
		{
			string baseDisplayName = Cybernetic.BaseDisplayName;
			JournalAPI.AddAccomplishment("You installed " + (Cybernetic.IsPlural ? "some" : "a") + " cybernetic " + baseDisplayName + " and came one step closer to the Grand Unification.", "Gaze upon the sublime body of =name=! See the glistening " + baseDisplayName + "!", "While wandering around  " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= stumbled upon a clan of machine folk. Because of " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " <spice.elements." + The.Player.GetMythicDomain() + ".quality.!random>, they accepted " + The.Player.GetPronounProvider().Objective + " into their fold and helped " + The.Player.GetPronounProvider().Objective + " Become by installing " + Cybernetic.a + Cybernetic.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " implant.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.Medium, null, -1L);
		}
	}

	public bool Equip(GameObject Item, int? EnergyCost = null, bool Silent = false, bool ForDeepCopy = false, bool Forced = false, bool SemiForced = false)
	{
		Event obj = Event.New("CommandEquipObject");
		if (Silent)
		{
			obj.SetSilent(Silent: true);
		}
		if (ForDeepCopy)
		{
			obj.SetFlag("ForDeepCopy", State: true);
		}
		obj.SetParameter("Object", Item);
		obj.SetParameter("BodyPart", this);
		if (Forced)
		{
			obj.SetFlag("Forced", State: true);
		}
		if (SemiForced)
		{
			obj.SetFlag("SemiForced", State: true);
		}
		if (EnergyCost.HasValue)
		{
			obj.SetParameter("EnergyCost", EnergyCost.Value);
		}
		return ParentBody.ParentObject.FireEvent(obj);
	}

	public bool DoEquip(GameObject GO, bool Silent = false, bool ForDeepCopy = false)
	{
		string FailureMessage = null;
		bool result = DoEquip(GO, ref FailureMessage, Silent, ForDeepCopy);
		if (!FailureMessage.IsNullOrEmpty() && !Silent)
		{
			Body parentBody = ParentBody;
			if (parentBody != null && parentBody.ParentObject?.IsPlayer() == true)
			{
				Popup.ShowFail(FailureMessage);
			}
		}
		return result;
	}

	public bool DoEquip(GameObject GO, ref string FailureMessage, bool Silent = false, bool ForDeepCopy = false, bool UnequipOthers = false, int AutoEquipTry = 0, List<GameObject> WasUnequipped = null)
	{
		if (ParentBody == null)
		{
			MetricsManager.LogError("Cannot DoEquip without parentbody on part " + Name + " of type " + Type);
			return true;
		}
		ParentBody.WeightCache = -1;
		bool flag = GO.EquipAsDefaultBehavior();
		string usesSlots = GO.UsesSlots;
		if (!usesSlots.IsNullOrEmpty() && (!Thrown || usesSlots.Contains("Thrown Weapon")) && (!(Type == "Hand") || usesSlots.Contains("Hand")))
		{
			if (ParentBody.ParentObject.IsGiganticCreature)
			{
				if (Type != "Floating Nearby" && !GO.IsGiganticEquipment && !GO.HasPropertyOrTag("GiganticEquippable") && !GO.IsNatural())
				{
					FailureMessage = "This item is too small for you to equip.";
					return false;
				}
			}
			else if (Type != "Hand" && Type != "Missile Weapon" && Type != "Floating Nearby" && GO.IsGiganticEquipment && !GO.IsNatural())
			{
				FailureMessage = "This item is too large for you to equip.";
				return false;
			}
			List<string> list = usesSlots.CachedCommaExpansion();
			bool flag2 = true;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				string text = list[i];
				string Text = text;
				int codeFromAdjective = XRL.World.Capabilities.Laterality.GetCodeFromAdjective(ref Text);
				if (((Text == Type || Text == VariantType) && (codeFromAdjective == 0 || (Laterality & codeFromAdjective) == codeFromAdjective)) & flag2)
				{
					flag2 = false;
					continue;
				}
				int num = ParentBody.GetUnequippedPartCountExcept(Text, codeFromAdjective, this);
				int num2 = 0;
				bool flag3 = true;
				foreach (string item in list)
				{
					if ((item == Type || item == VariantType) & flag3)
					{
						flag3 = false;
					}
					else if (item == Text)
					{
						num2++;
					}
				}
				if (num2 <= num)
				{
					continue;
				}
				if (UnequipOthers)
				{
					foreach (BodyPart item2 in ParentBody.GetPart(Text, codeFromAdjective))
					{
						if (item2 == this)
						{
							continue;
						}
						GameObject Object = item2.Equipped;
						if (Object == null)
						{
							continue;
						}
						Event obj = Event.New("CommandUnequipObject", "BodyPart", item2);
						if (Silent)
						{
							obj.SetSilent(Silent: true);
						}
						if (AutoEquipTry > 0)
						{
							obj.SetParameter("AutoEquipTry", AutoEquipTry);
						}
						if (!FailureMessage.IsNullOrEmpty())
						{
							obj.SetParameter("FailureMessage", FailureMessage);
						}
						if (ForDeepCopy)
						{
							obj.SetFlag("ForDeepCopy", State: true);
						}
						if (ParentBody.ParentObject.FireEvent(obj))
						{
							if (item2.Equipped == null)
							{
								if (WasUnequipped != null && GameObject.Validate(ref Object))
								{
									WasUnequipped.Add(Object);
								}
								num++;
								if (num >= num2)
								{
									break;
								}
							}
						}
						else
						{
							string stringParameter = obj.GetStringParameter("FailureMessage");
							if (!stringParameter.IsNullOrEmpty() && stringParameter != FailureMessage)
							{
								FailureMessage = stringParameter;
							}
						}
					}
				}
				if (num2 > num)
				{
					if (num2 == num + 1)
					{
						FailureMessage = "You need " + ((num > 0) ? "another" : "a") + " free " + text + " slot to equip that!";
					}
					else
					{
						FailureMessage = "You need another " + Grammar.Cardinal(num2 - num) + " free " + text + " slots to equip that!";
					}
					return false;
				}
			}
			bool flag4 = false;
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				string text2 = list[j];
				string Text2 = text2;
				int codeFromAdjective2 = XRL.World.Capabilities.Laterality.GetCodeFromAdjective(ref Text2);
				List<BodyPart> part = ParentBody.GetPart(Text2, codeFromAdjective2);
				if (!flag4 && part.Contains(this))
				{
					flag4 = true;
					continue;
				}
				BodyPart bodyPart = null;
				int num3 = int.MaxValue;
				int k = 0;
				for (int count3 = part.Count; k < count3; k++)
				{
					BodyPart bodyPart2 = part[k];
					if (bodyPart2.Equipped == null && bodyPart2 != this)
					{
						int num4 = Math.Abs(bodyPart2.Position - Position);
						if (num4 < num3)
						{
							bodyPart = bodyPart2;
							num3 = num4;
						}
					}
				}
				if (bodyPart != null)
				{
					if (flag)
					{
						bodyPart.DefaultBehavior = GO;
					}
					else
					{
						bodyPart._Equipped = GO;
					}
				}
				else
				{
					MetricsManager.LogError("internal inconsistency, no closest " + text2 + " found for " + GO.DebugName);
				}
			}
		}
		else if (Thrown)
		{
			if (ParentBody.ParentObject.IsGiganticCreature)
			{
				if (!GO.IsGiganticEquipment && !GO.HasPropertyOrTag("GiganticEquippable") && !GO.IsNatural())
				{
					FailureMessage = "This item is too small for you to equip.";
					return false;
				}
			}
			else if (GO.IsGiganticEquipment && !GO.IsNatural())
			{
				FailureMessage = "This item is too large for you to equip.";
				return false;
			}
		}
		else
		{
			if (ParentBody.ParentObject.IsGiganticCreature && Type != "Floating Nearby" && !GO.IsGiganticEquipment && !GO.HasPropertyOrTag("GiganticEquippable") && !GO.IsNatural())
			{
				FailureMessage = "This item is too small for you to equip.";
				return false;
			}
			int slotsRequiredFor = GO.GetSlotsRequiredFor(ParentBody.ParentObject, Type, FloorAtOne: false);
			if (slotsRequiredFor < 1)
			{
				FailureMessage = "This item is too small for you to equip.";
				return false;
			}
			if (slotsRequiredFor == 2)
			{
				List<BodyPart> part2 = ParentBody.GetPart(Type);
				BodyPart bodyPart3 = null;
				foreach (BodyPart item3 in part2)
				{
					if (item3 != this && item3.Equipped == null)
					{
						bodyPart3 = item3;
						break;
					}
				}
				if ((bodyPart3 == null) & UnequipOthers)
				{
					foreach (BodyPart item4 in part2)
					{
						if (item4 == this)
						{
							continue;
						}
						GameObject Object2 = item4.Equipped;
						if (Object2 == null)
						{
							continue;
						}
						Event obj2 = Event.New("CommandUnequipObject", "BodyPart", item4);
						if (Silent)
						{
							obj2.SetSilent(Silent: true);
						}
						if (AutoEquipTry > 0)
						{
							obj2.SetParameter("AutoEquipTry", AutoEquipTry);
						}
						if (!FailureMessage.IsNullOrEmpty())
						{
							obj2.SetParameter("FailureMessage", FailureMessage);
						}
						if (ForDeepCopy)
						{
							obj2.SetFlag("ForDeepCopy", State: true);
						}
						if (ParentBody.ParentObject.FireEvent(obj2))
						{
							if (item4.Equipped == null)
							{
								if (WasUnequipped != null && GameObject.Validate(ref Object2))
								{
									WasUnequipped.Add(Object2);
								}
								bodyPart3 = item4;
								break;
							}
						}
						else
						{
							string stringParameter2 = obj2.GetStringParameter("FailureMessage");
							if (!stringParameter2.IsNullOrEmpty() && stringParameter2 != FailureMessage)
							{
								FailureMessage = stringParameter2;
							}
						}
					}
				}
				if (bodyPart3 == null)
				{
					FailureMessage = "You need another free " + Anatomies.GetBodyPartType(Type).Type + " slot to equip that!";
					return false;
				}
				if (flag)
				{
					bodyPart3.DefaultBehavior = GO;
				}
				else
				{
					bodyPart3._Equipped = GO;
				}
			}
			else if (slotsRequiredFor > 2)
			{
				List<BodyPart> part3 = ParentBody.GetPart(Type);
				List<BodyPart> list2 = new List<BodyPart>();
				foreach (BodyPart item5 in part3)
				{
					if (item5 != this && item5.Equipped == null)
					{
						list2.Add(item5);
						if (list2.Count >= slotsRequiredFor - 1)
						{
							break;
						}
					}
				}
				if ((list2.Count < slotsRequiredFor - 1) & UnequipOthers)
				{
					foreach (BodyPart item6 in part3)
					{
						if (item6 == this)
						{
							continue;
						}
						GameObject Object3 = item6.Equipped;
						if (Object3 == null)
						{
							continue;
						}
						Event obj3 = Event.New("CommandUnequipObject", "BodyPart", item6);
						if (Silent)
						{
							obj3.SetSilent(Silent: true);
						}
						if (AutoEquipTry > 0)
						{
							obj3.SetParameter("AutoEquipTry", AutoEquipTry);
						}
						if (!FailureMessage.IsNullOrEmpty())
						{
							obj3.SetParameter("FailureMessage", FailureMessage);
						}
						if (ForDeepCopy)
						{
							obj3.SetFlag("ForDeepCopy", State: true);
						}
						if (ParentBody.ParentObject.FireEvent(obj3))
						{
							if (item6.Equipped == null)
							{
								if (WasUnequipped != null && GameObject.Validate(ref Object3))
								{
									WasUnequipped.Add(Object3);
								}
								list2.Add(item6);
								if (list2.Count >= slotsRequiredFor - 1)
								{
									break;
								}
							}
						}
						else
						{
							string stringParameter3 = obj3.GetStringParameter("FailureMessage");
							if (!stringParameter3.IsNullOrEmpty() && stringParameter3 != FailureMessage)
							{
								FailureMessage = stringParameter3;
							}
						}
					}
				}
				int num5 = slotsRequiredFor - list2.Count - 1;
				if (num5 > 0)
				{
					if (num5 == 1)
					{
						FailureMessage = "You need another free " + Anatomies.GetBodyPartType(Type).Type + " slot to equip that!";
					}
					else
					{
						FailureMessage = "You need another " + num5 + " free " + Anatomies.GetBodyPartType(Type).Type + " slots to equip that!";
					}
					return false;
				}
				if (flag)
				{
					foreach (BodyPart item7 in list2)
					{
						item7.DefaultBehavior = GO;
					}
				}
				else
				{
					foreach (BodyPart item8 in list2)
					{
						item8._Equipped = GO;
					}
				}
			}
		}
		ParentBody.WeightCache = -1;
		if (flag)
		{
			DefaultBehavior = GO;
			ParentBody.RecalculateFirstDefaultBehavior(GO);
		}
		else
		{
			_Equipped = GO;
			ParentBody.RecalculateFirstEquipped(GO);
		}
		FlushTransientCaches();
		return true;
	}

	public void RecalculateFirstEquipped(GameObject Object, ref bool Found)
	{
		if (_Equipped == null)
		{
			FirstSlotForEquipped = false;
		}
		else if (_Equipped == Object)
		{
			FirstSlotForEquipped = !Found;
			Found = true;
		}
		int num = Parts?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			Parts[i].RecalculateFirstEquipped(Object, ref Found);
		}
	}

	public void RecalculateFirstCybernetics(GameObject Object, ref bool Found)
	{
		if (_Cybernetics == null)
		{
			FirstSlotForCybernetics = false;
		}
		else if (_Cybernetics == Object)
		{
			FirstSlotForCybernetics = !Found;
			Found = true;
		}
		int num = Parts?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			Parts[i].RecalculateFirstCybernetics(Object, ref Found);
		}
	}

	public void RecalculateFirstDefaultBehavior(GameObject Object, ref bool Found)
	{
		if (_DefaultBehavior == null)
		{
			FirstSlotForDefaultBehavior = false;
		}
		else if (_DefaultBehavior == Object)
		{
			FirstSlotForDefaultBehavior = !Found;
			Found = true;
		}
		int num = Parts?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			Parts[i].RecalculateFirstDefaultBehavior(Object, ref Found);
		}
	}

	private void ClearEquipped()
	{
		if (ParentBody != null)
		{
			ParentBody.WeightCache = -1;
		}
		_Equipped = null;
		FirstSlotForEquipped = false;
		FlushTransientCaches();
	}

	public bool TryUnequip(bool Silent = false, bool SemiForced = false, bool NoStack = false, bool NoTake = false)
	{
		Event obj = Event.New("CommandUnequipObject", "BodyPart", this);
		if (SemiForced)
		{
			obj.SetFlag("SemiForced", State: true);
		}
		if (NoStack)
		{
			obj.SetFlag("NoStack", State: true);
		}
		if (NoTake)
		{
			obj.SetFlag("NoTake", State: true);
		}
		if (Silent)
		{
			obj.SetSilent(Silent: true);
		}
		return ParentBody.ParentObject.FireEvent(obj);
	}

	public bool ForceUnequip(bool Silent = false, bool NoStack = false, bool NoTake = false)
	{
		Event obj = Event.New("CommandForceUnequipObject", "BodyPart", this);
		if (NoStack)
		{
			obj.SetFlag("NoStack", State: true);
		}
		if (NoTake)
		{
			obj.SetFlag("NoTake", State: true);
		}
		if (Silent)
		{
			obj.SetSilent(Silent: true);
		}
		return ParentBody.ParentObject.FireEvent(obj);
	}

	public void Unequip()
	{
		if (Equipped == null)
		{
			return;
		}
		GameObject equipped = Equipped;
		ToClearEquip.Clear();
		if (ParentBody == null)
		{
			FindPartsEquipping(equipped, ToClearEquip);
		}
		else
		{
			ParentBody.FindPartsEquipping(equipped, ToClearEquip);
		}
		int i = 0;
		for (int count = ToClearEquip.Count; i < count; i++)
		{
			if (ToClearEquip[i].Equipped == equipped)
			{
				ToClearEquip[i].ClearEquipped();
			}
		}
		ToClearEquip.Clear();
	}

	private GameObject MakeCopyWithMap(GameObject obj, Func<GameObject, GameObject> map)
	{
		if (map != null)
		{
			GameObject gameObject = map(obj);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return obj.DeepCopy(CopyEffects: false, CopyID: false, map);
	}

	public BodyPart DeepCopy(GameObject ParentObject = null, Body ParentBody = null, BodyPart ParentPart = null, Func<GameObject, GameObject> MapInv = null, bool CopyGameObjects = true)
	{
		BodyPart bodyPart = new BodyPart();
		bodyPart.Type = Type;
		bodyPart.VariantType = VariantType;
		bodyPart.Description = Description;
		bodyPart.DescriptionPrefix = DescriptionPrefix;
		bodyPart.Name = Name;
		bodyPart.SupportsDependent = SupportsDependent;
		bodyPart.DependsOn = DependsOn;
		bodyPart.RequiresType = RequiresType;
		bodyPart.Manager = Manager;
		bodyPart.Category = Category;
		bodyPart.Laterality = Laterality;
		bodyPart.RequiresLaterality = RequiresLaterality;
		bodyPart.Mobility = Mobility;
		bodyPart.Flags = Flags;
		bodyPart.Position = Position;
		bodyPart._ID = _ID;
		bodyPart.ParentBody = ParentBody;
		bodyPart.ParentPart = ParentPart;
		bodyPart.DefaultBehaviorBlueprint = DefaultBehaviorBlueprint;
		if (CopyGameObjects && ParentObject != null)
		{
			if (DefaultBehavior != null)
			{
				GameObject gameObject = MakeCopyWithMap(DefaultBehavior, MapInv);
				if (gameObject.Physics != null)
				{
					ParentObject.DeepCopyInventoryObjectMap[DefaultBehavior] = gameObject;
					gameObject.Physics._Equipped = ParentObject;
					bodyPart.DefaultBehavior = gameObject;
					if (DefaultBehavior == Equipped || DefaultBehavior == Cybernetics)
					{
						Body.DeepCopyEquipMap[gameObject] = bodyPart;
					}
				}
			}
			if (Equipped != null && !ParentObject.DeepCopyInventoryObjectMap.ContainsKey(Equipped))
			{
				GameObject gameObject2 = MakeCopyWithMap(Equipped, MapInv);
				ParentObject.DeepCopyInventoryObjectMap[Equipped] = gameObject2;
				Body.DeepCopyEquipMap[gameObject2] = bodyPart;
			}
			if (Cybernetics != null && Cybernetics != Equipped && !ParentObject.DeepCopyInventoryObjectMap.ContainsKey(Cybernetics))
			{
				GameObject gameObject3 = MakeCopyWithMap(Cybernetics, MapInv);
				ParentObject.DeepCopyInventoryObjectMap[Cybernetics] = gameObject3;
				Body.DeepCopyEquipMap[gameObject3] = bodyPart;
			}
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (!part.Extrinsic)
				{
					bodyPart.AddPart(part.DeepCopy(ParentObject, ParentBody, this, MapInv, CopyGameObjects));
				}
			}
		}
		return bodyPart;
	}

	[Obsolete("Use Write(SerializationWriter)")]
	public void Save(SerializationWriter Writer)
	{
		Write(Writer);
	}

	[Obsolete("Use Read(SerializationReader)")]
	public static BodyPart Load(SerializationReader Reader, Body ParentBody)
	{
		BodyPart bodyPart = new BodyPart();
		bodyPart.ParentBody = ParentBody;
		bodyPart.Read(Reader);
		return bodyPart;
	}

	public void Write(SerializationWriter Writer)
	{
		WriteValues(Writer);
		Writer.WriteGameObject(_DefaultBehavior);
		Writer.WriteGameObject(_Equipped);
		Writer.WriteGameObject(_Cybernetics);
		int num = Parts?.Count ?? 0;
		Writer.WriteOptimized(num);
		for (int i = 0; i < num; i++)
		{
			Parts[i].Write(Writer);
		}
	}

	public void Read(SerializationReader Reader)
	{
		ReadValues(Reader);
		_DefaultBehavior = Reader.ReadGameObject();
		_Equipped = Reader.ReadGameObject();
		_Cybernetics = Reader.ReadGameObject();
		Thrown = Type == "Thrown Weapon";
		int num = Reader.ReadOptimizedInt32();
		if (num > 0)
		{
			Parts = new List<BodyPart>(num);
			for (int i = 0; i < num; i++)
			{
				BodyPart bodyPart = new BodyPart(ParentBody, this);
				bodyPart.Read(Reader);
				Parts.Add(bodyPart);
			}
		}
	}

	public int GetWeight()
	{
		int num = 0;
		if (Equipped != null && FirstSlotForEquipped)
		{
			CyberneticsBaseItem part = Equipped.GetPart<CyberneticsBaseItem>();
			if (part != null)
			{
				if (part.ImplantedOn == null)
				{
					num += Equipped.Weight;
				}
			}
			else
			{
				num += Equipped.Weight;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetWeight();
				}
			}
		}
		return num;
	}

	public void Clear()
	{
		DefaultBehavior = null;
		ClearEquipped();
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				Parts[i]?.Clear();
			}
		}
		Parts = null;
	}

	public void StripAllEquipment(List<GameObject> Return)
	{
		if (Equipped != null)
		{
			Return.Add(Equipped);
			BeforeUnequippedEvent.Send(Equipped, ParentBody.ParentObject);
			UnequippedEvent.Send(Equipped, ParentBody.ParentObject);
			Unequip();
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item.StripAllEquipment(Return);
		}
	}

	public void GetPrimaryHandEquippedObjects(List<GameObject> Return)
	{
		if (Primary && Type == "Hand")
		{
			if (Equipped != null)
			{
				Return.Add(Equipped);
			}
			else if (DefaultBehavior != null)
			{
				Return.Add(DefaultBehavior);
			}
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				Parts[i]?.GetPrimaryHandEquippedObjects(Return);
			}
		}
	}

	public void GetPrimaryEquippedObjects(List<GameObject> Return)
	{
		if (Primary)
		{
			if (Equipped != null)
			{
				Return.Add(Equipped);
			}
			else if (DefaultBehavior != null)
			{
				Return.Add(DefaultBehavior);
			}
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				Parts[i]?.GetPrimaryEquippedObjects(Return);
			}
		}
	}

	public void GetShield(ref GameObject obj, Predicate<GameObject> Filter, GameObject Attacker, GameObject Defender)
	{
		if (Equipped != null)
		{
			Shield part = Equipped.GetPart<Shield>();
			if (part != null && (part.WornOn == "*" || part.WornOn == Type) && (Filter == null || Filter(Equipped)))
			{
				if (obj == null)
				{
					obj = Equipped;
				}
				else
				{
					int num = GetShieldBlockPreferenceEvent.GetFor(Equipped, Attacker, Defender);
					int num2 = GetShieldBlockPreferenceEvent.GetFor(obj, Attacker, Defender);
					if (num > num2)
					{
						obj = Equipped;
					}
				}
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetShield(ref obj, Filter, Attacker, Defender);
			}
		}
	}

	public void GetShieldWithHighestAV(ref GameObject obj, ref int highestAV, Predicate<GameObject> Filter, GameObject Attacker, GameObject Defender)
	{
		if (Equipped != null)
		{
			Shield part = Equipped.GetPart<Shield>();
			if (part != null && (part.WornOn == "*" || part.WornOn == Type) && (Filter == null || Filter(Equipped)) && (obj == null || part.AV > highestAV || (part.AV == highestAV && 50.in100())))
			{
				obj = Equipped;
				highestAV = part.AV;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetShieldWithHighestAV(ref obj, ref highestAV, Filter, Attacker, Defender);
			}
		}
	}

	public void SetAsPreferredDefault(bool force = false)
	{
		if (!force && ParentBody.ParentObject.IsPlayer())
		{
			if (The.Player.AreHostilesNearby())
			{
				Popup.Show("You can't switch primary limbs in combat.");
				return;
			}
			if (Abstract || Extrinsic)
			{
				Popup.Show("This body part cannot be set as your primary.");
				return;
			}
		}
		ParentBody.ForeachPart((BodyPart p) =>
		{
			p.PreferredPrimary = false;
			return true;
		});
		PreferredPrimary = true;
		ParentBody.RecalculateFirsts();
	}

	public void SetPrimaryScan(string WantType, ref bool PrimarySet, ref bool HasPreferredPrimary, ref BodyPart CurrentPrimaryPart)
	{
		if (PrimarySet)
		{
			Primary = false;
			DefaultPrimary = false;
		}
		else if (Type == WantType)
		{
			PrimarySet = true;
			DefaultPrimary = true;
			if (CurrentPrimaryPart == null)
			{
				Primary = true;
				CurrentPrimaryPart = this;
			}
		}
		if (PreferredPrimary)
		{
			HasPreferredPrimary = true;
			Primary = true;
			if (CurrentPrimaryPart != null && CurrentPrimaryPart != this)
			{
				CurrentPrimaryPart.Primary = false;
			}
			CurrentPrimaryPart = this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].SetPrimaryScan(WantType, ref PrimarySet, ref HasPreferredPrimary, ref CurrentPrimaryPart);
			}
		}
	}

	private int GetPrimaryPickPriority()
	{
		if (PreferredPrimary && !DefaultPrimary)
		{
			return 20;
		}
		if (Equipped != null)
		{
			return 10;
		}
		return 8;
	}

	public void ScanForWeapon(bool NeedPrimary, GameObject Target, ref GameObject Weapon, ref BodyPart EquippedOn, ref int PickPriority, ref int PossibleWeapons, ref bool HadPrimary, List<BodyPart> PartList = null)
	{
		GameObject gameObject = Equipped ?? DefaultBehavior;
		if (gameObject != null)
		{
			if (gameObject == Weapon)
			{
				if (Primary && !HadPrimary)
				{
					HadPrimary = true;
					PickPriority = GetPrimaryPickPriority();
				}
			}
			else
			{
				if (Equipped != null && DefaultBehavior != null && PreferDefaultBehaviorEvent.Check(ParentBody.ParentObject, Target, this))
				{
					gameObject = DefaultBehavior;
				}
				MeleeWeapon part = gameObject.GetPart<MeleeWeapon>();
				if (part != null && part.AttackFromPart(this))
				{
					PossibleWeapons++;
					PartList?.Add(this);
					int num;
					if (Primary)
					{
						num = GetPrimaryPickPriority();
					}
					else if (gameObject.HasTag("PreferredDefault"))
					{
						num = 9;
					}
					else if (Type == "Hand")
					{
						num = 7;
					}
					else
					{
						num = ((this == ParentBody._Body) ? 5 : 6);
					}
					if (gameObject == DefaultBehavior && gameObject.HasTag("UndesirableWeapon"))
					{
						num -= 4;
					}
					if (Target != null && Target.IsDiggable() && gameObject != null && gameObject.HasPart<DiggingTool>())
					{
						num = GetPrimaryPickPriority() + 1;
					}
					bool flag = false;
					if (Weapon == null || num > PickPriority)
					{
						flag = true;
					}
					else if (num == PickPriority && Brain.IsNewWeaponBetter(gameObject, Weapon, ParentBody.ParentObject))
					{
						flag = true;
					}
					if (flag)
					{
						Weapon = gameObject;
						EquippedOn = this;
						PickPriority = num;
						HadPrimary = Primary;
					}
				}
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i]?.ScanForWeapon(NeedPrimary, Target, ref Weapon, ref EquippedOn, ref PickPriority, ref PossibleWeapons, ref HadPrimary, PartList);
			}
		}
	}

	public GameObject ThisPartWeaponOfType(string WeaponType, bool NeedPrimary)
	{
		if (!NeedPrimary || Primary || PreferredPrimary)
		{
			if (Equipped != null)
			{
				MeleeWeapon part = Equipped.GetPart<MeleeWeapon>();
				if (part != null && WeaponType == part.Skill && part.AttackFromPart(this))
				{
					return Equipped;
				}
			}
			else if (DefaultBehavior != null)
			{
				MeleeWeapon part2 = DefaultBehavior.GetPart<MeleeWeapon>();
				if (part2 != null && WeaponType == part2.Skill && part2.AttackFromPart(this))
				{
					return DefaultBehavior;
				}
			}
		}
		return null;
	}

	public GameObject ThisPartWeapon(Predicate<GameObject> Filter = null)
	{
		if (Equipped != null)
		{
			MeleeWeapon part = Equipped.GetPart<MeleeWeapon>();
			if (part != null && (Filter == null || Filter(Equipped)) && part.AttackFromPart(this))
			{
				return Equipped;
			}
		}
		else if (DefaultBehavior != null)
		{
			MeleeWeapon part2 = DefaultBehavior.GetPart<MeleeWeapon>();
			if (part2 != null && (Filter == null || Filter(DefaultBehavior)) && part2.AttackFromPart(this))
			{
				return DefaultBehavior;
			}
		}
		return null;
	}

	public GameObject GetWeaponOfType(string WeaponType, bool NeedPrimary)
	{
		GameObject gameObject = ThisPartWeaponOfType(WeaponType, NeedPrimary);
		if (gameObject != null)
		{
			return gameObject;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null)
				{
					gameObject = bodyPart.GetWeaponOfType(WeaponType, NeedPrimary);
					if (gameObject != null)
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public bool HasWeaponOfType(string WeaponType, bool NeedPrimary)
	{
		if (ThisPartWeaponOfType(WeaponType, NeedPrimary) != null)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.HasWeaponOfType(WeaponType, NeedPrimary))
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameObject GetWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary)
	{
		if (Type == BodyPartType)
		{
			GameObject gameObject = ThisPartWeaponOfType(WeaponType, NeedPrimary);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null)
				{
					GameObject weaponOfTypeOnBodyPartOfType = bodyPart.GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary);
					if (weaponOfTypeOnBodyPartOfType != null)
					{
						return weaponOfTypeOnBodyPartOfType;
					}
				}
			}
		}
		return null;
	}

	public bool HasWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary)
	{
		if (Type == BodyPartType && ThisPartWeaponOfType(WeaponType, NeedPrimary) != null)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.HasWeaponOfType(WeaponType, NeedPrimary))
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameObject GetWeapon(Predicate<GameObject> Filter = null)
	{
		GameObject gameObject = ThisPartWeapon(Filter);
		if (gameObject != null)
		{
			return gameObject;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null)
				{
					GameObject weapon = bodyPart.GetWeapon(Filter);
					if (weapon != null)
					{
						return weapon;
					}
				}
			}
		}
		return null;
	}

	public bool HasWeapon(Predicate<GameObject> Filter = null)
	{
		if (ThisPartWeapon(Filter) != null)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.HasWeapon(Filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPrimaryWeapon(GameObject Object)
	{
		if (Primary && (Object == Equipped || Object == DefaultBehavior))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsPrimaryWeapon(Object))
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameObject GetFirstValidWeapon(GameObject Target = null)
	{
		GameObject gameObject = null;
		MeleeWeapon meleeWeapon = DefaultBehavior?.GetPart<MeleeWeapon>();
		if (meleeWeapon != null && meleeWeapon.AttackFromPart(this))
		{
			gameObject = DefaultBehavior;
		}
		MeleeWeapon meleeWeapon2 = Equipped?.GetPart<MeleeWeapon>();
		if (meleeWeapon2 != null && meleeWeapon2.AttackFromPart(this))
		{
			if (gameObject == null)
			{
				return Equipped;
			}
			if (!PreferDefaultBehaviorEvent.Check(ParentBody.ParentObject, Target, this))
			{
				return Equipped;
			}
		}
		return gameObject;
	}

	public void ClearShieldBlocks()
	{
		if (Equipped != null)
		{
			Shield part = Equipped.GetPart<Shield>();
			if (part != null)
			{
				part.Blocks = 0;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i]?.ClearShieldBlocks();
			}
		}
	}

	public int GetEquippedObjectCount()
	{
		int num = 0;
		if (Equipped != null && FirstSlotForEquipped)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetEquippedObjectCount();
				}
			}
		}
		return num;
	}

	public int GetEquippedObjectCount(Predicate<GameObject> Filter)
	{
		int num = 0;
		if (Equipped != null && Filter(Equipped) && FirstSlotForEquipped)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetEquippedObjectCount(Filter);
				}
			}
		}
		return num;
	}

	public void GetEquippedObjects(List<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjects(Return);
		}
	}

	public List<GameObject> GetEquippedObjectsReadonly()
	{
		List<GameObject> list = null;
		if (Equipped != null)
		{
			list = Event.NewGameObjectList();
			list.Add(Equipped);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					if (list == null)
					{
						list = part.GetEquippedObjectsReadonly();
					}
					else
					{
						part.GetEquippedObjects(list);
					}
				}
			}
		}
		return list;
	}

	public List<GameObject> GetEquippedObjectsReadonly(Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return GetEquippedObjectsReadonly();
		}
		List<GameObject> list = null;
		if (Equipped != null && Filter(Equipped))
		{
			list = Event.NewGameObjectList();
			list.Add(Equipped);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					if (list == null)
					{
						list = part.GetEquippedObjectsReadonly(Filter);
					}
					else
					{
						part.GetEquippedObjects(list, Filter);
					}
				}
			}
		}
		return list;
	}

	public void GetEquippedObjects(List<GameObject> Return, Predicate<GameObject> Filter)
	{
		if (Equipped != null && !Return.Contains(Equipped) && Filter(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjects(Return, Filter);
		}
	}

	public void GetEquippedObjectsExceptNatural(IList<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped) && !Equipped.HasTag("NaturalGear"))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsExceptNatural(Return);
		}
	}

	public void GetEquippedObjectsExceptNatural(List<GameObject> Return, Predicate<GameObject> Filter)
	{
		if (Equipped != null && !Return.Contains(Equipped) && !Equipped.HasTag("NaturalGear") && Filter(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsExceptNatural(Return, Filter);
		}
	}

	public void ForeachEquippedObject(Action<GameObject> Proc)
	{
		if (Equipped != null && FirstSlotForEquipped)
		{
			Proc(Equipped);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i]?.ForeachEquippedObject(Proc);
			}
		}
	}

	public void SafeForeachEquippedObject(Action<GameObject> Proc)
	{
		if (Equipped != null && FirstSlotForEquipped)
		{
			Proc(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item?.SafeForeachEquippedObject(Proc);
		}
	}

	public void ForeachDefaultBehavior(Action<GameObject> Proc)
	{
		if (DefaultBehavior != null && FirstSlotForDefaultBehavior)
		{
			Proc(DefaultBehavior);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i]?.ForeachDefaultBehavior(Proc);
			}
		}
	}

	public void SafeForeachDefaultBehavior(Action<GameObject> Proc)
	{
		if (DefaultBehavior != null && FirstSlotForDefaultBehavior)
		{
			Proc(DefaultBehavior);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item?.SafeForeachDefaultBehavior(Proc);
		}
	}

	public bool HasInstalledCybernetics(string Blueprint = null, Predicate<GameObject> Filter = null)
	{
		if (Cybernetics != null && (Blueprint.IsNullOrEmpty() || Cybernetics.Blueprint == Blueprint) && (Filter == null || Filter(Cybernetics)))
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && part.HasInstalledCybernetics(Blueprint, Filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetInstalledCyberneticsCount()
	{
		int num = 0;
		if (Cybernetics != null && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetInstalledCyberneticsCount();
				}
			}
		}
		return num;
	}

	public int GetInstalledCyberneticsCount(Predicate<GameObject> Filter)
	{
		int num = 0;
		if (Cybernetics != null && Filter(Cybernetics) && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetInstalledCyberneticsCount(Filter);
				}
			}
		}
		return num;
	}

	public void GetInstalledCybernetics(List<GameObject> Return)
	{
		if (Cybernetics != null && !Return.Contains(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetInstalledCybernetics(Return);
		}
	}

	public List<GameObject> GetInstalledCyberneticsReadonly()
	{
		List<GameObject> list = null;
		if (Cybernetics != null)
		{
			list = Event.NewGameObjectList();
			list.Add(Cybernetics);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					if (list == null)
					{
						list = part.GetInstalledCyberneticsReadonly();
					}
					else
					{
						part.GetInstalledCybernetics(list);
					}
				}
			}
		}
		return list;
	}

	public void GetInstalledCybernetics(List<GameObject> Return, Predicate<GameObject> Filter)
	{
		if (Cybernetics != null && !Return.Contains(Cybernetics) && Filter(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetInstalledCybernetics(Return, Filter);
		}
	}

	public bool AnyInstalledCybernetics()
	{
		if (Cybernetics != null)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.AnyInstalledCybernetics())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyInstalledCybernetics(Predicate<GameObject> Filter)
	{
		if (Cybernetics != null && Filter(Cybernetics))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.AnyInstalledCybernetics(Filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		if (Cybernetics != null && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			aProc(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.ForeachInstalledCybernetics(aProc);
		}
	}

	public void SafeForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		if (Cybernetics != null && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			aProc(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item?.ForeachInstalledCybernetics(aProc);
		}
	}

	public void GetEquippedObjectsAndInstalledCybernetics(List<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Return.Contains(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsAndInstalledCybernetics(Return);
		}
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCyberneticsReadonly()
	{
		List<GameObject> list = null;
		if (Equipped != null)
		{
			list = Event.NewGameObjectList();
			list.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			if (list == null)
			{
				list = Event.NewGameObjectList();
			}
			list.Add(Cybernetics);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					if (list == null)
					{
						list = part.GetEquippedObjectsAndInstalledCyberneticsReadonly();
					}
					else
					{
						part.GetEquippedObjectsAndInstalledCybernetics(list);
					}
				}
			}
		}
		return list;
	}

	public void GetEquippedObjectsAndInstalledCybernetics(List<GameObject> Return, Predicate<GameObject> Filter)
	{
		if (Equipped != null && !Return.Contains(Equipped) && Filter(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Return.Contains(Cybernetics) && Filter(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsAndInstalledCybernetics(Return, Filter);
		}
	}

	public bool Render(RenderEvent E)
	{
		if (Equipped != null)
		{
			Equipped.ComponentRender(E);
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			Cybernetics.ComponentRender(E);
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].Render(E);
				}
			}
		}
		return true;
	}

	public virtual bool AnyRegisteredEvent(string ID)
	{
		if (Equipped != null && Equipped.HasRegisteredEvent(ID))
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.HasRegisteredEvent(ID))
		{
			return true;
		}
		if (DefaultBehavior != null && DefaultBehavior.HasRegisteredEvent(ID))
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part.AnyRegisteredEvent(ID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool FireEvent(Event E)
	{
		bool flag = true;
		if (Equipped != null && FirstSlotForEquipped)
		{
			if (!Equipped.FireEvent(E))
			{
				flag = false;
			}
			if (E.ID == "EndTurn")
			{
				Equipped?.CleanEffects();
			}
		}
		if (!flag)
		{
			return false;
		}
		if (Cybernetics != null && Cybernetics != Equipped && FirstSlotForCybernetics)
		{
			if (!Cybernetics.FireEvent(E))
			{
				flag = false;
			}
			if (E.ID == "EndTurn")
			{
				Cybernetics?.CleanEffects();
			}
		}
		if (!flag)
		{
			return false;
		}
		if (DefaultBehavior != null)
		{
			if (!DefaultBehavior.FireEvent(E))
			{
				flag = false;
			}
			if (E.ID == "EndTurn")
			{
				DefaultBehavior?.CleanEffects();
			}
		}
		if (!flag)
		{
			return false;
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && !bodyPart.FireEvent(E))
				{
					flag = false;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return flag;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Markup.Wrap(Description)).Append(": ");
		if (Equipped == null && DefaultBehavior == null)
		{
			stringBuilder.Append("{{K|-}}");
		}
		else if (Equipped == null)
		{
			stringBuilder.Append("{{K|").Append(DefaultBehavior.ShortDisplayName).Append("}}");
		}
		else
		{
			stringBuilder.Append("{{C|").Append(Equipped.ShortDisplayName).Append("}}");
		}
		if (Primary)
		{
			stringBuilder.Append(" {{G|*Primary}}");
		}
		return stringBuilder.ToString();
	}

	public void ToString(StringBuilder SB)
	{
		SB.Append(ToString());
		SB.Append("\n");
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part.ToString(SB);
		}
	}

	public void UnequipPartAndChildren(bool Silent = false, IInventory Where = null, bool ForDeath = false)
	{
		if (Equipped != null && Equipped.IsReal && !Equipped.IsNatural())
		{
			if (ParentBody != null)
			{
				GameObject equipped = Equipped;
				try
				{
					Event obj = Event.New("CommandForceUnequipObject", "BodyPart", this, "NoTake", 1);
					obj.SetSilent(Silent: true);
					ParentBody.ParentObject.FireEvent(obj);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("UnequipPartAndChildren unequip", x);
				}
				try
				{
					if (!ForDeath || ParentBody.ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
					{
						if ((Where == null) & ForDeath)
						{
							Where = ParentBody.ParentObject.GetDropInventory();
						}
						if (Where == null)
						{
							Inventory inventory = ParentBody.ParentObject.Inventory;
							Where = ((inventory == null) ? ParentBody.ParentObject.GetDropInventory() : inventory);
						}
						if (Where != null && (!ForDeath || DropOnDeathEvent.Check(equipped, Where, WasEquipped: true)))
						{
							GameObject parentObject = ParentBody.ParentObject;
							GameObject parentObject2 = ParentBody.ParentObject;
							IInventory inventoryTarget = Where;
							if (InventoryActionEvent.Check(parentObject, parentObject2, equipped, "CommandDropObject", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, Forced: true, Silent, 0, 0, 0, null, null, null, inventoryTarget) && !Silent && Where is Cell && Where.InventoryContains(equipped) && equipped.IsVisible())
							{
								GameObject parentObject3 = ParentBody.ParentObject;
								MessageQueue.AddPlayerMessage(equipped.Does("fall", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null, AsPossessed: true, parentObject3) + " to the ground.", ColorCoding.ConsequentialColor(null, ParentBody.ParentObject));
							}
						}
					}
				}
				catch (Exception x2)
				{
					MetricsManager.LogException("UnequipPartAndChildren drop", x2);
				}
			}
		}
		else
		{
			if (Equipped != null && ParentBody.ParentObject != null)
			{
				BeforeUnequippedEvent.Send(Equipped, ParentBody.ParentObject);
				UnequippedEvent.Send(Equipped, ParentBody.ParentObject);
			}
			Unequip();
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item.UnequipPartAndChildren(Silent, Where, ForDeath);
		}
	}

	public bool RemoveUnmanagedPartsByVariantPrefix(string Prefix)
	{
		bool result = false;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].RemoveUnmanagedPartsByVariantPrefix(Prefix))
				{
					result = true;
					count = Parts.Count;
					i--;
				}
			}
		}
		if (VariantType != Type && VariantType.StartsWith(Prefix) && ParentBody != null)
		{
			ParentBody.RemovePart(this);
			result = true;
		}
		return result;
	}

	public bool RemovePart(BodyPart Part, bool DoUpdate = true)
	{
		if (Parts == null)
		{
			return false;
		}
		if (Parts.Contains(Part))
		{
			Part.UnequipPartAndChildren();
			Parts.Remove(Part);
			if (Part.ParentPart == this)
			{
				Part.ParentPart = null;
			}
			if (DoUpdate && ParentBody != null)
			{
				ParentBody.UpdateBodyParts();
				ParentBody.RecalculateTypeArmor(Part.Type);
			}
			return true;
		}
		foreach (BodyPart part in Parts)
		{
			if (part.RemovePart(Part, DoUpdate))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemovePartByID(int removeID, bool DoUpdate = true)
	{
		BodyPart partByID = GetPartByID(removeID);
		if (partByID == null)
		{
			return false;
		}
		return RemovePart(partByID, DoUpdate);
	}

	private bool PositionOccupied(int Position)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Position == Position)
				{
					return true;
				}
			}
		}
		if (ParentBody != null && ParentBody.DismemberedPartHasPosition(this, Position))
		{
			return true;
		}
		return false;
	}

	private bool PositionOccupied(int Position, BodyPart ExceptPart)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Position == Position && Parts[i] != ExceptPart)
				{
					return true;
				}
			}
		}
		if (ParentBody != null && ParentBody.DismemberedPartHasPosition(this, Position, ExceptPart))
		{
			return true;
		}
		return false;
	}

	private void AssignPosition(BodyPart Part, int Position)
	{
		if (PositionOccupied(Position, Part))
		{
			if (Parts != null)
			{
				int i = 0;
				for (int count = Parts.Count; i < count; i++)
				{
					if (Parts[i].Position >= Position)
					{
						Parts[i].Position++;
					}
				}
			}
			if (ParentBody != null)
			{
				ParentBody.CheckDismemberedPartRenumberingOnPositionAssignment(this, Position, Part);
			}
		}
		Part.Position = Position;
	}

	private int NextPosition(BodyPart ExceptPart = null)
	{
		int num = ((ParentBody != null) ? ParentBody.GetLastDismemberedPartPosition(this, ExceptPart) : (-1));
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Position > num && Parts[i] != ExceptPart)
				{
					num = Parts[i].Position;
				}
			}
		}
		return num + 1;
	}

	private int FindIndexForPosition(int Position)
	{
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i].Position >= Position)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public BodyPart AddPart(BodyPart NewPart, int Position = -1, bool DoUpdate = true)
	{
		if (Parts == null)
		{
			Parts = new List<BodyPart>();
		}
		if (Parts.IndexOf(NewPart) != -1)
		{
			throw new Exception("part for add already attached");
		}
		if (Position == -1)
		{
			Position = NextPosition(NewPart);
		}
		else
		{
			while (Position > 0 && !PositionOccupied(Position - 1, NewPart))
			{
				Position--;
			}
		}
		int num = FindIndexForPosition(Position);
		if (num == -1)
		{
			Parts.Add(NewPart);
		}
		else
		{
			Parts.Insert(num, NewPart);
		}
		AssignPosition(NewPart, Position);
		NewPart.ParentPart = this;
		if (ParentBody != null)
		{
			NewPart.ParentBody = ParentBody;
			if (DoUpdate)
			{
				ParentBody.UpdateBodyParts();
				ParentBody.RecalculateTypeArmor(NewPart.Type);
			}
		}
		return NewPart;
	}

	public BodyPart AddPart(BodyPart NewPart, string InsertAfter, bool DoUpdate = true)
	{
		int num = LastPartTypePosition(InsertAfter);
		if (num != -1)
		{
			num++;
		}
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPart NewPart, string InsertAfter, string OrInsertBefore, bool DoUpdate = true)
	{
		if (OrInsertBefore == null)
		{
			return AddPart(NewPart, InsertAfter);
		}
		int num = LastPartTypePosition(InsertAfter);
		num = ((num != -1) ? (num + 1) : FirstPartTypePosition(OrInsertBefore));
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPart NewPart, string[] InsertBefore, bool DoUpdate = true)
	{
		int num = -1;
		if (InsertBefore != null)
		{
			foreach (string type in InsertBefore)
			{
				num = FirstPartTypePosition(type);
				if (num != -1)
				{
					break;
				}
			}
		}
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPart NewPart, string InsertAfter, string[] OrInsertBefore, bool DoUpdate = true)
	{
		if (OrInsertBefore == null)
		{
			return AddPart(NewPart, InsertAfter);
		}
		int num = LastPartTypePosition(InsertAfter);
		if (num == -1)
		{
			foreach (string type in OrInsertBefore)
			{
				num = FirstPartTypePosition(type);
				if (num != -1)
				{
					break;
				}
			}
		}
		else
		{
			num++;
		}
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), -1, DoUpdate);
	}

	public BodyPart AddPart(string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), -1, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string[] OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string[] OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPart InsertAfter, BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), InsertAfter.Position + 1, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPart InsertAfter, string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), InsertAfter.Position + 1, DoUpdate);
	}

	public BodyPart AddPartAt(string[] InsertBefore, BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Dynamic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Dynamic, Plural, Mass, Contact, IgnorePosition), InsertBefore, DoUpdate);
	}

	public int FirstPartTypePosition(string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Type == Type && Parts[i] != ExceptPart)
				{
					num = Parts[i].Position;
					break;
				}
			}
		}
		if (ParentBody != null)
		{
			int firstDismemberedPartTypePosition = ParentBody.GetFirstDismemberedPartTypePosition(this, Type, ExceptPart);
			if (firstDismemberedPartTypePosition != -1 && (num == -1 || firstDismemberedPartTypePosition < num))
			{
				num = firstDismemberedPartTypePosition;
			}
		}
		return num;
	}

	public int LastPartTypePosition(string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (Type == null)
		{
			return num;
		}
		if (Parts != null)
		{
			for (int num2 = Parts.Count - 1; num2 >= 0; num2--)
			{
				if (Parts[num2].Type == Type && Parts[num2] != ExceptPart)
				{
					num = Parts[num2].Position;
					break;
				}
			}
		}
		if (ParentBody != null)
		{
			int lastDismemberedPartTypePosition = ParentBody.GetLastDismemberedPartTypePosition(this, Type, ExceptPart);
			if (lastDismemberedPartTypePosition > num)
			{
				num = lastDismemberedPartTypePosition;
			}
		}
		return num;
	}

	public BodyPart GetPart(Predicate<BodyPart> Filter)
	{
		if (Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			foreach (BodyPart part2 in Parts)
			{
				if (part2 != null)
				{
					BodyPart part = part2.GetPart(Filter);
					if (part != null)
					{
						return part;
					}
				}
			}
		}
		return null;
	}

	public bool AnyPart(Predicate<BodyPart> Filter)
	{
		if (Filter(this))
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && part.AnyPart(Filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachPart(Action<BodyPart> aProc)
	{
		aProc(this);
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.ForeachPart(aProc);
		}
	}

	public bool ForeachPart(Predicate<BodyPart> pProc)
	{
		if (!pProc(this))
		{
			return false;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && !part.ForeachPart(pProc))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SafeForeachPart(Action<BodyPart> aProc)
	{
		aProc(this);
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item?.ForeachPart(aProc);
		}
	}

	public bool SafeForeachPart(Predicate<BodyPart> pProc)
	{
		if (!pProc(this))
		{
			return false;
		}
		if (Parts != null)
		{
			foreach (BodyPart item in new List<BodyPart>(Parts))
			{
				if (item != null && !item.ForeachPart(pProc))
				{
					return false;
				}
			}
		}
		return true;
	}

	public int GetPartCount()
	{
		int num = 1;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount();
				}
			}
		}
		return num;
	}

	public int GetConcretePartCount()
	{
		int num = ((!Abstract) ? 1 : 0);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetConcretePartCount();
				}
			}
		}
		return num;
	}

	public int GetAbstractPartCount()
	{
		int num = (Abstract ? 1 : 0);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetAbstractPartCount();
				}
			}
		}
		return num;
	}

	public int GetPartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount(RequiredType);
				}
			}
		}
		return num;
	}

	public int GetPartCount(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPartCount(RequiredType);
		}
		int num = 0;
		if (Type == RequiredType && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount(RequiredType, RequiredLaterality);
				}
			}
		}
		return num;
	}

	public int GetPartCount(Predicate<BodyPart> Filter)
	{
		int num = 0;
		if (Filter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount(Filter);
				}
			}
		}
		return num;
	}

	public int GetPartNameCount(string FindName)
	{
		int num = 0;
		if (Name == FindName)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartNameCount(FindName);
				}
			}
		}
		return num;
	}

	public int GetPartDescriptionCount(string FindDescription)
	{
		int num = 0;
		if (Description == FindDescription)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartDescriptionCount(FindDescription);
				}
			}
		}
		return num;
	}

	public int GetPartDescriptionCount(string FindDescription, string FindDescriptionPrefix)
	{
		int num = 0;
		if (Description == FindDescription && DescriptionPrefix.EqualsEmptyEqualsNull(FindDescriptionPrefix))
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartDescriptionCount(FindDescription, FindDescriptionPrefix);
				}
			}
		}
		return num;
	}

	public bool AnyCategoryParts(int FindCategory)
	{
		if (FindCategory == Category)
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && part.AnyCategoryParts(FindCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetCategoryPartCount(int FindCategory)
	{
		int num = ((FindCategory == Category) ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetCategoryPartCount(FindCategory);
				}
			}
		}
		return num;
	}

	public int GetCategoryPartCount(int FindCategory, string FindType)
	{
		int num = 0;
		if (FindCategory == Category && FindType == Type)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetCategoryPartCount(FindCategory, FindType);
			}
		}
		return num;
	}

	public int GetCategoryPartCount(int FindCategory, Predicate<BodyPart> Filter)
	{
		int num = 0;
		if (FindCategory == Category && Filter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetCategoryPartCount(FindCategory, Filter);
				}
			}
		}
		return num;
	}

	public int GetNativePartCount()
	{
		int num = (Native ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetNativePartCount();
				}
			}
		}
		return num;
	}

	public int GetNativePartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType && Native)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetNativePartCount(RequiredType);
			}
		}
		return num;
	}

	public int GetNativePartCount(Predicate<BodyPart> Filter)
	{
		int num = 0;
		if (Native && Filter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetNativePartCount(Filter);
				}
			}
		}
		return num;
	}

	public int GetAddedPartCount()
	{
		int num = ((!Native) ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetAddedPartCount();
				}
			}
		}
		return num;
	}

	public int GetAddedPartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType && !Native)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetAddedPartCount(RequiredType);
			}
		}
		return num;
	}

	public int GetAddedPartCount(Predicate<BodyPart> Filter)
	{
		int num = 0;
		if (!Native && Filter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetAddedPartCount(Filter);
				}
			}
		}
		return num;
	}

	public int GetMortalPartCount()
	{
		int num = (Mortal ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetMortalPartCount();
				}
			}
		}
		return num;
	}

	public int GetMortalPartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType && Mortal)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetMortalPartCount(RequiredType);
			}
		}
		return num;
	}

	public int GetMortalPartCount(Predicate<BodyPart> Filter)
	{
		int num = 0;
		if (Mortal && Filter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetMortalPartCount(Filter);
				}
			}
		}
		return num;
	}

	public bool AnyMortalParts()
	{
		if (Mortal)
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && part.AnyMortalParts())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEquippedOnType(GameObject FindObj, string FindType)
	{
		if (FindObj == Equipped && FindType == Type)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsEquippedOnType(FindObj, FindType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEquippedOnCategory(GameObject FindObj, int FindCategory)
	{
		if (FindObj == Equipped && FindCategory == Category)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsEquippedOnCategory(FindObj, FindCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEquippedOnPrimary(GameObject FindObj)
	{
		if (FindObj == Equipped && Primary)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsEquippedOnPrimary(FindObj))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsImplantedInCategory(GameObject FindObj, int FindCategory)
	{
		if (FindObj == Cybernetics && FindCategory == Category)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsImplantedInCategory(FindObj, FindCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetTotalMobility()
	{
		int num = Mobility;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetTotalMobility();
				}
			}
		}
		return num;
	}

	public void MarkAllNative()
	{
		Native = true;
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].MarkAllNative();
			}
		}
	}

	public void CategorizeAll(int ApplyCategory)
	{
		Category = ApplyCategory;
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].CategorizeAll(ApplyCategory);
			}
		}
	}

	public void CategorizeAllExcept(int ApplyCategory, int SkipCategory)
	{
		if (Category != SkipCategory)
		{
			Category = ApplyCategory;
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].CategorizeAllExcept(ApplyCategory, SkipCategory);
			}
		}
	}

	public List<BodyPart> GetParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount());
		list.Add(this);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetParts(list);
				}
			}
		}
		return list;
	}

	public void GetParts(List<BodyPart> Return)
	{
		Return.Add(this);
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetParts(Return);
			}
		}
	}

	public void GetTopLevelDynamicParts(List<BodyPart> Return)
	{
		if (Dynamic)
		{
			Return.Add(this);
		}
		else
		{
			if (Parts == null)
			{
				return;
			}
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetTopLevelDynamicParts(Return);
				}
			}
		}
	}

	public void GetPartsSkippingDynamicTrees(List<BodyPart> Return)
	{
		if (Dynamic)
		{
			return;
		}
		Return.Add(this);
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetPartsSkippingDynamicTrees(Return);
			}
		}
	}

	public void GetConcreteParts(List<BodyPart> Return)
	{
		if (!Abstract)
		{
			Return.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetConcreteParts(Return);
			}
		}
	}

	public void GetAbstractParts(List<BodyPart> Return)
	{
		if (Abstract)
		{
			Return.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetAbstractParts(Return);
			}
		}
	}

	public void GetPartsEquippedOn(GameObject obj, List<BodyPart> Result)
	{
		if (Equipped == obj)
		{
			Result.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetPartsEquippedOn(obj, Result);
			}
		}
	}

	public List<BodyPart> GetPartsEquippedOn(GameObject obj)
	{
		List<BodyPart> result = new List<BodyPart>(GetPartCountEquippedOn(obj));
		GetPartsEquippedOn(obj, result);
		return result;
	}

	public int GetPartCountEquippedOn(GameObject obj)
	{
		int num = 0;
		if (Equipped == obj)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCountEquippedOn(obj);
				}
			}
		}
		return num;
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType, List<BodyPart> Return)
	{
		if ((Type == RequiredType || VariantType == RequiredType) && Equipped == null)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetUnequippedPart(RequiredType, Return);
				}
			}
		}
		return Return;
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType, int RequiredLaterality, List<BodyPart> Return)
	{
		if (RequiredLaterality == 65535)
		{
			return GetUnequippedPart(RequiredType, Return);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Equipped == null)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetUnequippedPart(RequiredType, RequiredLaterality, Return);
				}
			}
		}
		return Return;
	}

	public int GetUnequippedPartCount(string RequiredType)
	{
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && Equipped == null)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCount(RequiredType);
				}
			}
		}
		return num;
	}

	public int GetUnequippedPartCount(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetUnequippedPartCount(RequiredType);
		}
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Equipped == null)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCount(RequiredType, RequiredLaterality);
				}
			}
		}
		return num;
	}

	public int GetUnequippedPartCountExcept(string RequiredType, BodyPart ExceptPart)
	{
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && Equipped == null && ExceptPart != this)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCountExcept(RequiredType, ExceptPart);
				}
			}
		}
		return num;
	}

	public int GetUnequippedPartCountExcept(string RequiredType, int RequiredLaterality, BodyPart ExceptPart)
	{
		if (RequiredLaterality == 65535)
		{
			return GetUnequippedPartCountExcept(RequiredType, ExceptPart);
		}
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Equipped == null && ExceptPart != this)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCountExcept(RequiredType, RequiredLaterality, ExceptPart);
				}
			}
		}
		return num;
	}

	public List<BodyPart> GetPart(string RequiredType, List<BodyPart> Return)
	{
		if (Type == RequiredType || VariantType == RequiredType)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetPart(RequiredType, Return);
				}
			}
		}
		return Return;
	}

	public List<BodyPart> GetPart(string RequiredType, int RequiredLaterality, List<BodyPart> Return)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPart(RequiredType, Return);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetPart(RequiredType, RequiredLaterality, Return);
				}
			}
		}
		return Return;
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType)
	{
		if (Type == RequiredType || VariantType == RequiredType)
		{
			yield return this;
		}
		if (Parts == null)
		{
			yield break;
		}
		int i = 0;
		for (int j = Parts.Count; i < j; i++)
		{
			if (Parts[i] == null)
			{
				continue;
			}
			foreach (BodyPart item in Parts[i].LoopPart(RequiredType))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType, int RequiredLaterality)
	{
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			yield return this;
		}
		if (Parts == null)
		{
			yield break;
		}
		int i = 0;
		for (int j = Parts.Count; i < j; i++)
		{
			if (Parts[i] == null)
			{
				continue;
			}
			foreach (BodyPart item in Parts[i].LoopPart(RequiredType, RequiredLaterality))
			{
				yield return item;
			}
		}
	}

	public BodyPart GetFirstPart(string RequiredType)
	{
		if (Type == RequiredType || VariantType == RequiredType)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstPart(RequiredType);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType, RequiredLaterality);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(Predicate<BodyPart> Filter)
	{
		if (Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(Filter);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		if ((Type == RequiredType || VariantType == RequiredType) && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType, Filter);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstPart(RequiredType, Filter);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType, RequiredLaterality, Filter);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType)
	{
		if (VariantType == RequiredType)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstVariantPart(RequiredType);
		}
		if (VariantType == RequiredType && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType, RequiredLaterality);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(Predicate<BodyPart> Filter)
	{
		if (Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(Filter);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		if (VariantType == RequiredType && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType, Filter);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstVariantPart(RequiredType, Filter);
		}
		if (VariantType == RequiredType && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType, RequiredLaterality, Filter);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && (Parts[i].Type == RequiredType || Parts[i].VariantType == RequiredType))
				{
					return Parts[i];
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType, Body BackupParentBody, bool EvenIfDismembered)
	{
		BodyPart bodyPart = GetFirstAttachedPart(RequiredType);
		if (EvenIfDismembered)
		{
			Body body = ParentBody ?? BackupParentBody;
			if (body != null)
			{
				BodyPart firstDismemberedPartByType = body.GetFirstDismemberedPartByType(this, RequiredType);
				if (firstDismemberedPartByType != null && (bodyPart == null || firstDismemberedPartByType.Position < bodyPart.Position))
				{
					bodyPart = firstDismemberedPartByType;
				}
			}
		}
		return bodyPart;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstAttachedPart(RequiredType);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && (Parts[i].Type == RequiredType || Parts[i].VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(Parts[i], RequiredLaterality))
				{
					return Parts[i];
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType, int RequiredLaterality, Body BackupParentBody, bool EvenIfDismembered)
	{
		BodyPart bodyPart = GetFirstAttachedPart(RequiredType, RequiredLaterality);
		if (EvenIfDismembered)
		{
			Body body = ParentBody ?? BackupParentBody;
			if (body != null)
			{
				BodyPart firstDismemberedPartByTypeAndLaterality = body.GetFirstDismemberedPartByTypeAndLaterality(this, RequiredType, RequiredLaterality);
				if (firstDismemberedPartByTypeAndLaterality != null && (bodyPart == null || firstDismemberedPartByTypeAndLaterality.Position < bodyPart.Position))
				{
					bodyPart = firstDismemberedPartByTypeAndLaterality;
				}
			}
		}
		return bodyPart;
	}

	public BodyPart GetPartByName(string RequiredPart)
	{
		if (Name == RequiredPart)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByName = Parts[i].GetPartByName(RequiredPart);
					if (partByName != null)
					{
						return partByName;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByNameStartsWith(string RequiredPart)
	{
		if (Name != null && Name.StartsWith(RequiredPart))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByNameStartsWith = Parts[i].GetPartByNameStartsWith(RequiredPart);
					if (partByNameStartsWith != null)
					{
						return partByNameStartsWith;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByNameWithoutCybernetics(string RequiredPart)
	{
		if (Name == RequiredPart && Cybernetics == null)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByNameWithoutCybernetics = Parts[i].GetPartByNameWithoutCybernetics(RequiredPart);
					if (partByNameWithoutCybernetics != null)
					{
						return partByNameWithoutCybernetics;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByDescription(string RequiredDescription)
	{
		if (Description == RequiredDescription)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByDescription = Parts[i].GetPartByDescription(RequiredDescription);
					if (partByDescription != null)
					{
						return partByDescription;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByDescription(string RequiredDescription, string RequiredDescriptionPrefix)
	{
		if (Description == RequiredDescription && DescriptionPrefix.EqualsEmptyEqualsNull(RequiredDescriptionPrefix))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByDescription = Parts[i].GetPartByDescription(RequiredDescription, RequiredDescriptionPrefix);
					if (partByDescription != null)
					{
						return partByDescription;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByDescriptionStartsWith(string RequiredDescription)
	{
		if (Description != null && Description.StartsWith(RequiredDescription))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByDescriptionStartsWith = Parts[i].GetPartByDescriptionStartsWith(RequiredDescription);
					if (partByDescriptionStartsWith != null)
					{
						return partByDescriptionStartsWith;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByID(int findID)
	{
		if (IDMatch(findID))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByID = Parts[i].GetPartByID(findID);
					if (partByID != null)
					{
						return partByID;
					}
				}
			}
		}
		return null;
	}

	public void GetPartBySupportsDependent(string findSupportsDependent, List<BodyPart> parts)
	{
		if (SupportsDependent == findSupportsDependent)
		{
			parts.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetPartBySupportsDependent(findSupportsDependent);
			}
		}
	}

	public BodyPart GetPartBySupportsDependent(string findSupportsDependent)
	{
		if (SupportsDependent == findSupportsDependent)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partBySupportsDependent = Parts[i].GetPartBySupportsDependent(findSupportsDependent);
					if (partBySupportsDependent != null)
					{
						return partBySupportsDependent;
					}
				}
			}
		}
		return null;
	}

	protected void GetNamePosition(BodyPart FindPart, ref int Found, ref bool Done)
	{
		if (FindPart == this)
		{
			Found++;
			Done = true;
			return;
		}
		if (FindPart.Name == Name)
		{
			Found++;
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetNamePosition(FindPart, ref Found, ref Done);
				if (Done)
				{
					break;
				}
			}
		}
	}

	public int GetNamePosition()
	{
		int Found = 0;
		bool Done = false;
		if (ParentBody != null)
		{
			ParentBody.GetBody().GetNamePosition(this, ref Found, ref Done);
		}
		else
		{
			GetNamePosition(this, ref Found, ref Done);
		}
		return Found;
	}

	protected void GetDescriptionPosition(BodyPart FindPart, ref int Found, ref bool Done)
	{
		if (FindPart == this)
		{
			Found++;
			Done = true;
			return;
		}
		if (FindPart.Description == Description && FindPart.DescriptionPrefix.EqualsEmptyEqualsNull(DescriptionPrefix))
		{
			Found++;
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part.GetDescriptionPosition(FindPart, ref Found, ref Done);
			if (Done)
			{
				break;
			}
		}
	}

	public int GetDescriptionPosition()
	{
		int Found = 0;
		bool Done = false;
		if (ParentBody != null)
		{
			ParentBody.GetBody().GetDescriptionPosition(this, ref Found, ref Done);
		}
		else
		{
			GetDescriptionPosition(this, ref Found, ref Done);
		}
		return Found;
	}

	public string GetCardinalName()
	{
		if (IgnorePosition)
		{
			return Name;
		}
		int namePosition = GetNamePosition();
		if (namePosition == 1)
		{
			return Name;
		}
		return Event.NewStringBuilder().Append(Name).Append(" (")
			.Append(namePosition)
			.Append(')')
			.ToString();
	}

	public string GetOrdinalName()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string text = (Abstract ? null : BodyPartCategory.GetColor(Category));
		if (text != null)
		{
			stringBuilder.Append("{{").Append(text).Append('|');
		}
		if (IgnorePosition || GetPartNameCount(Name) == 1)
		{
			stringBuilder.Append(Name);
		}
		else
		{
			stringBuilder.Append(Grammar.Ordinal(GetNamePosition())).Append(' ').Append(Name);
		}
		if (text != null)
		{
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public string GetCardinalDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string text = (Abstract ? null : BodyPartCategory.GetColor(Category));
		if (text != null)
		{
			stringBuilder.Append("{{").Append(text).Append('|');
		}
		if (!DescriptionPrefix.IsNullOrEmpty())
		{
			stringBuilder.Append(DescriptionPrefix).Append(' ');
		}
		stringBuilder.Append(Description);
		if (!IgnorePosition)
		{
			int descriptionPosition = GetDescriptionPosition();
			if (descriptionPosition != 1)
			{
				stringBuilder.Append(" (").Append(descriptionPosition).Append(')');
			}
		}
		if (text != null)
		{
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public string GetOrdinalDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string text = (Abstract ? null : BodyPartCategory.GetColor(Category));
		if (text != null)
		{
			stringBuilder.Append("{{").Append(text).Append('|');
		}
		if (!IgnorePosition && GetPartDescriptionCount(Description) != 1)
		{
			int descriptionPosition = GetDescriptionPosition();
			stringBuilder.Append(ColorUtility.CapitalizeExceptFormatting(Grammar.Ordinal(descriptionPosition))).Append(' ');
		}
		if (!DescriptionPrefix.IsNullOrEmpty())
		{
			stringBuilder.Append(DescriptionPrefix).Append(' ');
		}
		stringBuilder.Append(Description);
		if (text != null)
		{
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public bool IsConcretelyDependent()
	{
		return DependsOn != null;
	}

	public bool IsAbstractlyDependent()
	{
		return RequiresType != null;
	}

	public bool IsDependent()
	{
		if (!IsConcretelyDependent())
		{
			return IsAbstractlyDependent();
		}
		return true;
	}

	public bool IsConcretelyUnsupported(Body UseParentBody = null)
	{
		if (DependsOn == null)
		{
			return false;
		}
		if (UseParentBody != null)
		{
			return UseParentBody.GetPartBySupportsDependent(DependsOn) == null;
		}
		if (ParentBody != null)
		{
			return ParentBody.GetPartBySupportsDependent(DependsOn) == null;
		}
		return GetPartBySupportsDependent(DependsOn) == null;
	}

	public bool IsAbstractlyUnsupported(Body UseParentBody = null)
	{
		if (RequiresType == null)
		{
			return false;
		}
		if (UseParentBody != null)
		{
			return UseParentBody.GetFirstPart(RequiresType, RequiresLaterality) == null;
		}
		if (ParentBody != null)
		{
			return ParentBody.GetFirstPart(RequiresType, RequiresLaterality) == null;
		}
		return GetFirstPart(RequiresType, RequiresLaterality) == null;
	}

	public bool IsUnsupported(Body UseParentBody = null)
	{
		if (!IsConcretelyUnsupported(UseParentBody))
		{
			return IsAbstractlyUnsupported(UseParentBody);
		}
		return true;
	}

	public bool IsSeverable()
	{
		if (Abstract)
		{
			return false;
		}
		if (!Appendage)
		{
			return false;
		}
		if (Integral)
		{
			return false;
		}
		if (IsDependent())
		{
			return false;
		}
		return true;
	}

	public bool SeverRequiresDecapitate()
	{
		return Mortal;
	}

	public bool IsRegenerable()
	{
		if (Abstract)
		{
			return false;
		}
		if (IsDependent())
		{
			return false;
		}
		return true;
	}

	public bool IsRecoverable(Body UseParentBody = null)
	{
		if (!IsDependent() || IsUnsupported(UseParentBody))
		{
			return Abstract;
		}
		return true;
	}

	public void FindUnsupportedParts(ref List<BodyPart> Result)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].FindUnsupportedParts(ref Result);
				}
			}
		}
		if (IsUnsupported())
		{
			if (Result == null)
			{
				Result = new List<BodyPart>();
			}
			Result.Add(this);
		}
	}

	public List<BodyPart> FindUnsupportedParts()
	{
		List<BodyPart> Result = null;
		FindUnsupportedParts(ref Result);
		return Result;
	}

	public bool HasLaterality(int checkLaterality)
	{
		return (Laterality & checkLaterality) == checkLaterality;
	}

	[Obsolete]
	public bool IsFirstSlotForEquipped()
	{
		return FirstSlotForEquipped;
	}

	[Obsolete]
	public bool IsFirstSlotForCybernetics()
	{
		return FirstSlotForCybernetics;
	}

	[Obsolete]
	public bool IsFirstSlotForDefaultBehavior()
	{
		return FirstSlotForDefaultBehavior;
	}

	public bool IsDefaultBehaviorClearOfEquipped()
	{
		if (DefaultBehavior == null)
		{
			return false;
		}
		if (Equipped != null)
		{
			return false;
		}
		if (ParentBody == null)
		{
			return IsDefaultBehaviorClearOfEquipped(DefaultBehavior);
		}
		return ParentBody.IsDefaultBehaviorClearOfEquipped(DefaultBehavior);
	}

	public bool IsDefaultBehaviorClearOfEquipped(GameObject GO)
	{
		if (GO == DefaultBehavior && Equipped != null)
		{
			return false;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (!Parts[i].IsDefaultBehaviorClearOfEquipped(GO))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void GetTypeArmorInfo(string ForType, ref GameObject First, ref int Count, ref int AV, ref int DV)
	{
		if (ForType == Type)
		{
			Count++;
			if (Equipped != null)
			{
				Armor part = Equipped.GetPart<Armor>();
				if (part != null && (part.WornOn == Type || part.WornOn == "*") && FirstSlotForEquipped)
				{
					if (First == null)
					{
						First = Equipped;
					}
					AV += part.AV;
					DV += part.DV;
				}
			}
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetTypeArmorInfo(ForType, ref First, ref Count, ref AV, ref DV);
			}
		}
	}

	public void RecalculateArmor()
	{
		if (Equipped != null)
		{
			Armor part = Equipped.GetPart<Armor>();
			if (part != null && (part.WornOn == Type || part.WornOn == "*") && FirstSlotForEquipped)
			{
				part.RecalculateArmor();
			}
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateArmor();
			}
		}
	}

	public void RecalculateArmorExcept(GameObject obj)
	{
		if (Equipped != null && Equipped != obj)
		{
			Armor part = Equipped.GetPart<Armor>();
			if (part != null && (part.WornOn == Type || part.WornOn == "*") && FirstSlotForEquipped)
			{
				part.RecalculateArmor();
			}
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateArmorExcept(obj);
			}
		}
	}

	public void RecalculateTypeArmor(string ForType)
	{
		if (ForType == Type && Equipped != null)
		{
			Armor part = Equipped.GetPart<Armor>();
			if (part != null && (part.WornOn == Type || part.WornOn == "*") && FirstSlotForEquipped)
			{
				part.RecalculateArmor();
			}
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateTypeArmor(ForType);
			}
		}
	}

	public void RecalculateTypeArmorExcept(string ForType, GameObject obj)
	{
		if (ForType == Type && Equipped != null && Equipped != obj)
		{
			Armor part = Equipped.GetPart<Armor>();
			if (part != null && (part.WornOn == Type || part.WornOn == "*") && FirstSlotForEquipped)
			{
				part.RecalculateArmor();
			}
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateTypeArmorExcept(ForType, obj);
			}
		}
	}

	public bool CheckSlotEquippedMatch(GameObject obj, List<string> Slots)
	{
		if (obj == Equipped && (Slots.Contains(Type) || Slots.Contains(VariantType) || Slots.Contains("*")))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotEquippedMatch(obj, Slots))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotEquippedMatch(GameObject obj, string Slot)
	{
		if (obj == Equipped && (Slot == Type || Slot == VariantType || Slot == "*"))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotEquippedMatch(obj, Slot))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotCyberneticsMatch(GameObject obj, List<string> Slots)
	{
		if (obj == Cybernetics && (Slots.Contains(Type) || Slots.Contains(VariantType) || Slots.Contains("*")))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotCyberneticsMatch(obj, Slots))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotCyberneticsMatch(GameObject obj, string Slot)
	{
		if (obj == Cybernetics && (Slot == Type || Slot == VariantType || Slot == "*"))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotCyberneticsMatch(obj, Slot))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotDefaultBehaviorMatch(GameObject obj, List<string> Slots)
	{
		if (obj == DefaultBehavior && (Slots.Contains(Type) || Slots.Contains(VariantType) || Slots.Contains("*")))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotDefaultBehaviorMatch(obj, Slots))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotDefaultBehaviorMatch(GameObject obj, string Slot)
	{
		if (obj == DefaultBehavior && (Slot == Type || Slot == VariantType || Slot == "*"))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotDefaultBehaviorMatch(obj, Slot))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasReadyMissileWeapon(Predicate<GameObject> Filter = null, Predicate<MissileWeapon> PartFilter = null)
	{
		if (Equipped != null && Equipped.TryGetPart<MissileWeapon>(out var Part) && Part.FiresManually && Part.ValidSlotType(Type) && (Filter == null || Filter(Equipped)) && (PartFilter == null || PartFilter(Part)) && Part.ReadyToFire())
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.TryGetPart<MissileWeapon>(out var Part2) && Part2.FiresManually && Part2.ValidSlotType(Type, Cybernetic: true) && (Filter == null || Filter(Cybernetics)) && (PartFilter == null || PartFilter(Part2)) && Part2.ReadyToFire())
		{
			return true;
		}
		if (DefaultBehavior != null && DefaultBehavior.TryGetPart<MissileWeapon>(out var Part3) && Part3.FiresManually && Part3.ValidSlotType(Type) && (Filter == null || Filter(DefaultBehavior)) && (PartFilter == null || PartFilter(Part3)) && IsDefaultBehaviorClearOfEquipped() && Part3.ReadyToFire())
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].HasReadyMissileWeapon(Filter, PartFilter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMissileWeapon(Predicate<GameObject> Filter = null, Predicate<MissileWeapon> PartFilter = null)
	{
		if (Equipped != null && Equipped.TryGetPart<MissileWeapon>(out var Part) && Part.FiresManually && Part.ValidSlotType(Type) && (Filter == null || Filter(Equipped)) && (PartFilter == null || PartFilter(Part)))
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.TryGetPart<MissileWeapon>(out var Part2) && Part2.FiresManually && Part2.ValidSlotType(Type, Cybernetic: true) && (Filter == null || Filter(Cybernetics)) && (PartFilter == null || PartFilter(Part2)))
		{
			return true;
		}
		if (DefaultBehavior != null && DefaultBehavior.TryGetPart<MissileWeapon>(out var Part3) && Part3.FiresManually && Part3.ValidSlotType(Type) && (Filter == null || Filter(DefaultBehavior)) && (PartFilter == null || PartFilter(Part3)) && IsDefaultBehaviorClearOfEquipped())
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].HasMissileWeapon(Filter, PartFilter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetMissileWeapons(ref List<GameObject> List, Predicate<GameObject> Filter = null, Predicate<MissileWeapon> PartFilter = null)
	{
		if (Equipped != null && (List == null || !List.Contains(Equipped)) && Equipped.TryGetPart<MissileWeapon>(out var Part) && Part.FiresManually && Part.ValidSlotType(Type) && (Filter == null || Filter(Equipped)) && (PartFilter == null || PartFilter(Part)) && (List == null || !List.Contains(Equipped)))
		{
			if (List == null)
			{
				List = Event.NewGameObjectList();
			}
			List.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && (List == null || !List.Contains(Cybernetics)) && Cybernetics.TryGetPart<MissileWeapon>(out var Part2) && Part2.FiresManually && Part2.ValidSlotType(Type, Cybernetic: true) && (Filter == null || Filter(Cybernetics)) && (PartFilter == null || PartFilter(Part2)) && (List == null || !List.Contains(Cybernetics)))
		{
			if (List == null)
			{
				List = Event.NewGameObjectList();
			}
			List.Add(Cybernetics);
		}
		if (DefaultBehavior != null && (List == null || !List.Contains(DefaultBehavior)) && DefaultBehavior.TryGetPart<MissileWeapon>(out var Part3) && Part3.FiresManually && Part3.ValidSlotType(Type) && (Filter == null || Filter(DefaultBehavior)) && (PartFilter == null || PartFilter(Part3)) && (List == null || !List.Contains(DefaultBehavior)) && IsDefaultBehaviorClearOfEquipped())
		{
			if (List == null)
			{
				List = Event.NewGameObjectList();
			}
			List.Add(DefaultBehavior);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetMissileWeapons(ref List, Filter, PartFilter);
			}
		}
	}

	public bool HasHeavyWeaponEquipped()
	{
		if (Equipped != null && Equipped.TryGetPart<MissileWeapon>(out var Part) && Part.Skill == "HeavyWeapons")
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.TryGetPart<MissileWeapon>(out var Part2) && Part2.Skill == "HeavyWeapons")
		{
			return true;
		}
		if (DefaultBehavior != null && DefaultBehavior.TryGetPart<MissileWeapon>(out var Part3) && Part3.Skill == "HeavyWeapons")
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].HasHeavyWeaponEquipped())
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameObject GetFirstThrownWeapon(Predicate<GameObject> Filter = null, Predicate<ThrownWeapon> PartFilter = null)
	{
		if (Thrown && Equipped != null && (Filter == null || Filter(Equipped)) && (PartFilter == null || (Equipped.TryGetPart<ThrownWeapon>(out var Part) && PartFilter(Part))))
		{
			return Equipped;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					GameObject firstThrownWeapon = Parts[i].GetFirstThrownWeapon(Filter, PartFilter);
					if (firstThrownWeapon != null)
					{
						return firstThrownWeapon;
					}
				}
			}
		}
		return null;
	}

	public void GetThrownWeapons(ref IList<GameObject> List, Predicate<GameObject> Filter = null, Predicate<ThrownWeapon> PartFilter = null)
	{
		if (Thrown && Equipped != null && (Filter == null || Filter(Equipped)) && (PartFilter == null || (Equipped.TryGetPart<ThrownWeapon>(out var Part) && PartFilter(Part))))
		{
			if (List == null)
			{
				List = Event.NewGameObjectList();
			}
			List.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetThrownWeapons(ref List, Filter, PartFilter);
			}
		}
	}

	public bool IsADefaultBehavior(GameObject obj)
	{
		if (obj == DefaultBehavior)
		{
			return obj != null;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part.IsADefaultBehavior(obj))
				{
					return true;
				}
			}
		}
		return false;
	}

	[Obsolete]
	public bool WantTurnTick()
	{
		return true;
	}

	public void TurnTick(long TimeTick, int Amount)
	{
		if (_Equipped != null && FirstSlotForEquipped)
		{
			_Equipped.TurnTick(TimeTick, Amount);
		}
		if (_Cybernetics != null && _Cybernetics != _Equipped && FirstSlotForCybernetics)
		{
			_Cybernetics.TurnTick(TimeTick, Amount);
		}
		if (_DefaultBehavior != null && _DefaultBehavior != _Equipped && _DefaultBehavior != _Cybernetics && FirstSlotForDefaultBehavior)
		{
			_DefaultBehavior.TurnTick(TimeTick, Amount);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			BodyPart bodyPart = Parts[i];
			bodyPart.TurnTick(TimeTick, Amount);
			if (count != Parts.Count)
			{
				count = Parts.Count;
				if (i < count && Parts[i] != bodyPart)
				{
					i--;
				}
			}
		}
	}

	public void HandleEvent(EndTurnEvent E)
	{
		if (_Equipped != null && FirstSlotForEquipped)
		{
			_Equipped.HandleEvent(E);
		}
		if (_Cybernetics != null && _Cybernetics != _Equipped && FirstSlotForCybernetics)
		{
			_Cybernetics.HandleEvent(E);
		}
		if (_DefaultBehavior != null && _DefaultBehavior != _Equipped && _DefaultBehavior != _Cybernetics && FirstSlotForDefaultBehavior)
		{
			_DefaultBehavior.HandleEvent(E);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			BodyPart bodyPart = Parts[i];
			bodyPart.HandleEvent(E);
			if (count != Parts.Count)
			{
				count = Parts.Count;
				if (i < count && Parts[i] != bodyPart)
				{
					i--;
				}
			}
		}
	}

	public bool WantEvent(int ID, int cascade)
	{
		if (!MinEvent.CascadeTo(cascade, 16) || (Flags & 0x20000) == 0)
		{
			if (_Equipped != null && FirstSlotForEquipped && _Equipped.WantEvent(ID, cascade))
			{
				return true;
			}
			if (_Cybernetics != null && _Cybernetics != _Equipped && FirstSlotForCybernetics && _Cybernetics.WantEvent(ID, cascade))
			{
				return true;
			}
			if (_DefaultBehavior != null && _DefaultBehavior != _Equipped && _DefaultBehavior != _Cybernetics && FirstSlotForDefaultBehavior && _DefaultBehavior.WantEvent(ID, cascade))
			{
				return true;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantEvent(ID, cascade))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HandleEvent(MinEvent E)
	{
		if (!E.CascadeTo(16) || (Flags & 0x20000) == 0)
		{
			int iD = E.ID;
			int cascadeLevel = E.GetCascadeLevel();
			if (_Equipped != null && FirstSlotForEquipped && _Equipped.WantEvent(iD, cascadeLevel) && !E.Dispatch(_Equipped))
			{
				return false;
			}
			if (_Cybernetics != null && _Cybernetics != _Equipped && FirstSlotForCybernetics && _Cybernetics.WantEvent(iD, cascadeLevel) && !E.Dispatch(_Cybernetics))
			{
				return false;
			}
			if (_DefaultBehavior != null && _DefaultBehavior != _Equipped && _DefaultBehavior != _Cybernetics && FirstSlotForDefaultBehavior && _DefaultBehavior.WantEvent(iD, cascadeLevel) && !E.Dispatch(_DefaultBehavior))
			{
				return false;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (!bodyPart.HandleEvent(E))
				{
					return false;
				}
				if (count != Parts.Count)
				{
					count = Parts.Count;
					if (i < count && Parts[i] != bodyPart)
					{
						i--;
					}
				}
			}
		}
		return true;
	}

	public bool ProcessQuerySlotList(QuerySlotListEvent E)
	{
		if (DefaultBehavior == null || !DefaultBehavior.HasRegisteredEvent("CanEquipOverDefaultBehavior") || DefaultBehavior.FireEvent(Event.New("CanEquipOverDefaultBehavior", "Subject", ParentBody.ParentObject, "Object", E.Object, "Part", this)))
		{
			QueryEquippableListEvent queryEquippableListEvent = QueryEquippableListEvent.FromPool(ParentBody.ParentObject, E.Object, Type, RequireDesirable: false, RequirePossible: true);
			E.Object.HandleEvent(queryEquippableListEvent);
			if (queryEquippableListEvent.List.Count > 0)
			{
				E.SlotList.Add(this);
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (!bodyPart.ProcessQuerySlotList(E))
				{
					return false;
				}
				if (Parts.Count != count)
				{
					count = Parts.Count;
					if (i < count && Parts[i] != bodyPart)
					{
						i--;
					}
				}
			}
		}
		return true;
	}

	public bool ProcessExtrinsicValue(GetExtrinsicValueEvent E)
	{
		if (Equipped != null)
		{
			E.Value += Equipped.Value;
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			E.Value += Cybernetics.Value;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (!Parts[i].ProcessExtrinsicValue(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ProcessExtrinsicWeight(GetExtrinsicWeightEvent E)
	{
		if (Equipped != null)
		{
			E.Weight += Equipped.GetWeight();
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			E.Weight += Cybernetics.GetWeight();
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (!Parts[i].ProcessExtrinsicWeight(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static void GetCreatureFromSpec(string Blueprint, string Type, string Tag, string Base, Predicate<GameObjectBlueprint> Filter, ref GameObject Creature)
	{
		if (Blueprint != null)
		{
			Creature = GameObject.Create(Blueprint);
		}
		else if (Tag != null && Base != null)
		{
			if (Filter == null)
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag) && BP.DescendsFrom(Base));
			}
			else
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag) && BP.DescendsFrom(Base) && Filter(BP));
			}
		}
		else if (Tag != null)
		{
			if (Filter == null)
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag));
			}
			else
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag) && Filter(BP));
			}
		}
		else if (Base != null)
		{
			if (Filter == null)
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.DescendsFrom(Base));
			}
			else
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.DescendsFrom(Base) && Filter(BP));
			}
		}
		else if (Filter == null)
		{
			Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body"));
		}
		else
		{
			Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && Filter(BP));
		}
	}

	public static GameObject MakeSeveredBodyPart(string Blueprint = null, string Type = null, string Tag = null, string Base = null, Predicate<GameObjectBlueprint> Filter = null, GameObject PutIn = null)
	{
		GameObject Creature = null;
		int num = 0;
		while (++num < 10)
		{
			try
			{
				GetCreatureFromSpec(Blueprint, Type, Tag, Base, Filter, ref Creature);
				if (Creature == null)
				{
					return null;
				}
				Body body = Creature.Body;
				BodyPart bodyPart;
				if (Type == null)
				{
					SeverWork.Clear();
					foreach (BodyPart item in body.LoopParts())
					{
						if (item.IsSeverable())
						{
							SeverWork.Add(item);
						}
					}
					bodyPart = SeverWork.GetRandomElement();
				}
				else
				{
					bodyPart = body.GetFirstPart(Type);
				}
				if (bodyPart != null && bodyPart.IsSeverable())
				{
					GameObject gameObject = bodyPart.Dismember();
					if (gameObject != null)
					{
						PutIn?.FireEvent(Event.New("CommandTakeObject", "Object", gameObject, "EnergyCost", 0).SetSilent(Silent: true));
						return gameObject;
					}
				}
			}
			finally
			{
				Creature?.Obliterate();
			}
			if (Blueprint != null)
			{
				return null;
			}
		}
		return null;
	}

	public static void MakeSeveredBodyParts(int Number, string Blueprint = null, string Type = null, string Tag = null, string Base = null, Predicate<GameObjectBlueprint> Filter = null, GameObject PutIn = null, List<GameObject> Return = null)
	{
		GameObject Creature = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (++num2 < 10)
		{
			while (true)
			{
				GameObject gameObject = Creature;
				GetCreatureFromSpec(Blueprint, Type, Tag, Base, Filter, ref Creature);
				if (Creature == null)
				{
					break;
				}
				Body body = Creature.Body;
				BodyPart bodyPart;
				if (Type == null)
				{
					SeverWork.Clear();
					foreach (BodyPart item in body.LoopParts())
					{
						if (item.IsSeverable())
						{
							SeverWork.Add(item);
						}
					}
					bodyPart = SeverWork.GetRandomElement();
					if (Creature == gameObject)
					{
						if (num - SeverWork.Count >= Number / 2)
						{
							Creature.Obliterate();
							Creature = null;
							continue;
						}
					}
					else
					{
						num = SeverWork.Count;
					}
				}
				else
				{
					bodyPart = body.GetFirstPart(Type);
				}
				try
				{
					if (bodyPart != null && bodyPart.IsSeverable())
					{
						GameObject gameObject2 = bodyPart.Dismember();
						if (gameObject2 != null)
						{
							PutIn?.FireEvent(Event.New("CommandTakeObject", "Object", gameObject2, "EnergyCost", 0).SetSilent(Silent: true));
							Return?.Add(gameObject2);
							if (++num3 >= Number)
							{
								break;
							}
							continue;
						}
					}
				}
				finally
				{
					if (Blueprint == null)
					{
						Creature.Obliterate();
						Creature = null;
					}
				}
				goto IL_013f;
			}
			break;
			IL_013f:
			if (Blueprint != null)
			{
				break;
			}
		}
		Creature?.Obliterate();
	}

	public void TypeDump(StringBuilder SB)
	{
		SB.Append(Type).Append(' ').Append(VariantType)
			.Append(' ')
			.Append(Name)
			.Append('\n');
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part.TypeDump(SB);
		}
	}

	public void GetMobilityProvidingParts(List<BodyPart> Return)
	{
		if (Mobility > 0)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetMobilityProvidingParts(Return);
			}
		}
	}

	public void GetConcreteMobilityProvidingParts(List<BodyPart> Return)
	{
		if (Mobility > 0 && !Abstract)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetConcreteMobilityProvidingParts(Return);
			}
		}
	}

	public void GetContents(List<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Return.Contains(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetContents(Return);
			}
		}
	}

	public int GetPartDepth(BodyPart Part, int Depth)
	{
		if (Part == this)
		{
			if (!Contact && Depth == 1)
			{
				return 0;
			}
			return Depth;
		}
		if (Parts != null)
		{
			Depth++;
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				int partDepth = Parts[i].GetPartDepth(Part, Depth);
				if (partDepth != -1)
				{
					return partDepth;
				}
			}
		}
		return -1;
	}

	public int GetMaximumElectricalConductivityOfMobilityBodyParts()
	{
		int num = ((Mobility > 0) ? GetElectricalConductivity() : 0);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				int maximumElectricalConductivityOfMobilityBodyParts = Parts[i].GetMaximumElectricalConductivityOfMobilityBodyParts();
				if (maximumElectricalConductivityOfMobilityBodyParts > num)
				{
					num = maximumElectricalConductivityOfMobilityBodyParts;
				}
			}
		}
		return num;
	}

	public int GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(out GameObject CriticalObject)
	{
		CriticalObject = null;
		int num = ((Mobility > 0) ? GetEffectiveElectricalConductivity(out CriticalObject) : 0);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				int maximumEffectiveElectricalConductivityOfMobilityBodyParts = Parts[i].GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(out var CriticalObject2);
				if (maximumEffectiveElectricalConductivityOfMobilityBodyParts > num)
				{
					num = maximumEffectiveElectricalConductivityOfMobilityBodyParts;
					CriticalObject = CriticalObject2;
				}
			}
		}
		return num;
	}

	public int GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts()
	{
		GameObject CriticalObject;
		return GetMaximumEffectiveElectricalConductivityOfMobilityBodyParts(out CriticalObject);
	}

	public int GetElectricalConductivity()
	{
		return BodyPartCategory.GetElectricalConductivity(Category);
	}

	public int GetEffectiveElectricalConductivity(out GameObject CriticalObject)
	{
		CriticalObject = null;
		int num = BodyPartCategory.GetElectricalConductivity(Category);
		if (Equipped != null)
		{
			int electricalConductivity = Equipped.GetElectricalConductivity();
			if (electricalConductivity < num)
			{
				num = electricalConductivity;
				CriticalObject = Equipped;
			}
		}
		return num;
	}

	public int GetEffectiveElectricalConductivity()
	{
		GameObject CriticalObject;
		return GetEffectiveElectricalConductivity(out CriticalObject);
	}

	public int GetTotalElectricalConductivity()
	{
		int num = GetElectricalConductivity();
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				num += Parts[i].GetTotalElectricalConductivity();
			}
		}
		return num;
	}

	public void ModifyNameAndDescriptionRecursively(string NameMod, string DescMod)
	{
		Name = NameMod + "-" + Name;
		Description = DescMod + "-" + Description;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].ModifyNameAndDescriptionRecursively(NameMod, DescMod);
			}
		}
	}

	public BodyPart ReduceParts(Func<BodyPart, BodyPart, BodyPart> Reduction, BodyPart Result = null)
	{
		Result = Reduction(Result, this);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Result = Parts[i].ReduceParts(Reduction, Result);
			}
		}
		return Result;
	}

	public bool IsCategoryLive()
	{
		return BodyPartCategory.IsLiveCategory(Category);
	}

	public bool CanReceiveCyberneticImplant()
	{
		if (Extrinsic)
		{
			return false;
		}
		if (Category != 1)
		{
			return false;
		}
		return true;
	}

	public bool HasBlueprint(string Blueprint)
	{
		if (Blueprint.IsNullOrEmpty())
		{
			return false;
		}
		if (Equipped?.Blueprint == Blueprint)
		{
			return true;
		}
		if (Cybernetics?.Blueprint == Blueprint)
		{
			return true;
		}
		if (DefaultBehavior?.Blueprint == Blueprint)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].HasBlueprint(Blueprint))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart GetUnequippedPreferredBodyPartOrAlternate(string PreferredType, string AlternateType, ref BodyPart Alternate)
	{
		if (Equipped == null)
		{
			if (Type == PreferredType)
			{
				return this;
			}
			if (Type == AlternateType && Alternate == null)
			{
				Alternate = this;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart unequippedPreferredBodyPartOrAlternate = Parts[i].GetUnequippedPreferredBodyPartOrAlternate(PreferredType, AlternateType, ref Alternate);
				if (unequippedPreferredBodyPartOrAlternate != null)
				{
					return unequippedPreferredBodyPartOrAlternate;
				}
			}
		}
		return null;
	}

	[Obsolete("Use IDMatch(int)")]
	public bool idMatch(int testID)
	{
		return _ID == testID;
	}
}
