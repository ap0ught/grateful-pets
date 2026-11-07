namespace XRL.World.Parts;

public interface ILightSource
{
	int GetRadius();

	bool IsActive();

	bool IsDarkvision();
}
