namespace XRL.World;

public static class QudGameObjectExtensions
{
	public static bool IsValid(this GameObject obj)
	{
		if (obj != null && obj.Physics != null)
		{
			return !obj.IsInGraveyard();
		}
		return false;
	}

	public static bool IsInvalid(this GameObject obj)
	{
		if (obj != null)
		{
			return !obj.IsValid();
		}
		return true;
	}
}
