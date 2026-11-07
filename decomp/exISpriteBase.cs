using UnityEngine;

public interface exISpriteBase : IMonoBehaviour
{
	bool customSize { get; set; }

	float width { get; set; }

	float height { get; set; }

	Anchor anchor { get; set; }

	Color color { get; set; }

	Vector2 offset { get; set; }

	Vector2 shear { get; set; }

	Shader shader { get; set; }

	exUpdateFlags updateFlags { get; set; }

	int vertexCount { get; }

	int indexCount { get; }

	Material material { get; }

	bool visible { get; }

	Matrix4x4 cachedWorldMatrix { get; }

	void UpdateMaterial();

	float GetScaleX(Space _space);

	float GetScaleY(Space _space);
}
