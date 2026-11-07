using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
[GeneratePoolingPartial]
[GenerateSerializationPartial]
public class BurnToAshesIfOrganic : IPart
{
	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool BurnToAshesIfOrganicPool = new IPartPool();

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => BurnToAshesIfOrganicPool;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ParentObject.Physics != null && (ParentObject.Physics.LastDamagedByType == "Fire" || ParentObject.Physics.LastDamagedByType == "Light") && ParentObject.IsOrganic && !ParentObject.HasPart<Metal>() && !ParentObject.HasPart<Crysteel>() && !ParentObject.HasPart<Zetachrome>() && ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
		{
			IInventory dropInventory = ParentObject.GetDropInventory();
			if (dropInventory != null)
			{
				GameObject gameObject = GameObject.Create("Ashes");
				DoCarryOvers(ParentObject, gameObject);
				dropInventory.AddObjectToInventory(gameObject, null, Silent: false, NoStack: false, FlushTransient: true, null, E);
				DroppedEvent.Send(ParentObject, gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void DoCarryOvers(GameObject From, GameObject To)
	{
		if (From.HasProperty("StoredByPlayer") || From.HasProperty("FromStoredByPlayer"))
		{
			To.SetIntProperty("FromStoredByPlayer", 1);
		}
		Temporary.CarryOver(From, To);
		Phase.carryOver(From, To);
	}
}
