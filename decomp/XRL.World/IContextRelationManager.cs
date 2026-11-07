using XRL.World.Anatomy;

namespace XRL.World;

public interface IContextRelationManager
{
	bool RestoreContextRelation(GameObject Object, GameObject ObjectContext, Cell CellContext, BodyPart BodyPartContext, int Relation, bool Silent = true);
}
