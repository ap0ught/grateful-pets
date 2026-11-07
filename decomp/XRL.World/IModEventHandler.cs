namespace XRL.World;

/// <summary>Contracts a class as capable of handling a custom event type.</summary>
/// <example>
/// <code>
/// public class ExampleHandler : IPart, IModEventHandler&lt;ExampleEvent&gt;
/// {
///
///     public override bool WantEvent(int ID, int Cascade)
///     {
///         return base.WantEvent(ID, Cascade)
///               || ID == ExampleEvent.ID
///             ;
///     }
///
///     public bool HandleEvent(ExampleEvent E)
///     {
///        E.Test = true;
///       return true;
///     }
///
/// }
/// </code>
/// </example>
public interface IModEventHandler<T> where T : MinEvent
{
	bool HandleEvent(T E)
	{
		return true;
	}
}
