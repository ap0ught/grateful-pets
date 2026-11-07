namespace Sheeter;

public class RowBlueprint
{
	[Field(Key = "Name")]
	public string Blueprint;

	[Property(Key = "CachedDisplayNameStripped")]
	public string DisplayName;

	[Tag]
	public string Creature;

	[Tag]
	public string Shield;

	[Tag]
	[Tag(Key = "ShowMeleeWeaponStats")]
	public string MeleeWeapon;

	[Tag(Key = "Gender", Value = "nonspecific")]
	public string Nonspecific;

	[IntProperty(Value = 1)]
	public string Bleeds;

	[Tag]
	[Part]
	public string Armor;

	[Tag(Key = "SemanticFood")]
	[Part]
	public string Food;

	[Tag]
	public string NaturalGear;
}
