using System.Linq.Expressions;

namespace Sheeter;

public class BlueprintValueElement : BlueprintElement
{
	public object Value;

	public bool Output;

	public ExpressionType Operator = ExpressionType.Equal;
}
