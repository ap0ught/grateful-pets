namespace XRL.World;

public interface IThrownWeaponFlexPhaseProvider
{
	void ThrownWeaponFlexPhaseStart(GameObject Weapon);

	void ThrownWeaponFlexPhaseEnd(GameObject Weapon);

	bool ThrownWeaponFlexPhaseIsActive(GameObject Weapon);

	bool ThrownWeaponFlexPhaseTraversal(GameObject Actor, GameObject WillHit, GameObject Target, GameObject Weapon, int Phase, Cell FromCell, Cell ToCell, out bool RecheckHit, out bool RecheckPhase, bool HasDynamicTargets = false);
}
