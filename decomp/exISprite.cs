using UnityEngine;

public interface exISprite : exISpriteBase, IMonoBehaviour
{
	exTextureInfo textureInfo { get; set; }

	bool useTextureOffset { get; set; }

	exSpriteType spriteType { get; set; }

	Vector2 tiledSpacing { get; set; }

	bool borderOnly { get; set; }

	bool customBorderSize { get; set; }

	float leftBorderSize { get; set; }

	float rightBorderSize { get; set; }

	float topBorderSize { get; set; }

	float bottomBorderSize { get; set; }

	void UpdateBufferSize();
}
