using XRL.UI.Framework;
using XRL.World.Parts.Mutation;

namespace Qud.UI;

public class CharacterMutationLineData : PooledFrameworkDataElement<CharacterMutationLineData>
{
	public BaseMutation mutation;

	public CharacterMutationLineData set(BaseMutation mutation)
	{
		this.mutation = mutation;
		return this;
	}

	public override void free()
	{
		mutation = null;
		base.free();
	}
}
