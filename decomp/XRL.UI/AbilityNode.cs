using XRL.World.Parts;

namespace XRL.UI;

public class AbilityNode : ConsoleTreeNode<AbilityNode>
{
	public ActivatedAbilityEntry Ability;

	public AbilityNode(ActivatedAbilityEntry Ability = null, string Category = "", AbilityNode ParentNode = null)
		: base(Category, true, ParentNode)
	{
		this.Ability = Ability;
	}
}
