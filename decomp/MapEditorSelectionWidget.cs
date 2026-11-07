using System;
using UnityEngine;

[ExecuteAlways]
public class MapEditorSelectionWidget : MonoBehaviour
{
	public GameObject Sprite;

	public float duration = 3f;

	private float t;

	private Vector2 tileSize = new Vector2(0.16f, 0.24f);

	private Vector2 borderPadding = new Vector2(0.04f, 0.04f);

	public Color Color1 = Color.green;

	public Color Color2 = new Color(0f, 1f, 0f, 0.5f);

	private void Update()
	{
		t += Time.deltaTime / duration;
		Sprite.GetComponent<SpriteRenderer>().color = Color.Lerp(Color1, Color2, Math.Abs(1f - t % 2f));
	}

	public void SetTileSize(int x, int y)
	{
		Vector2 vector = tileSize * new Vector2(x, y) + borderPadding;
		Sprite.GetComponent<SpriteRenderer>().size = vector;
		Vector2 vector2 = new Vector2(1f, -1f);
		Sprite.transform.localPosition = (vector - tileSize) / 2f * vector2 + borderPadding * new Vector2(-1f, 1f) / 2f;
	}
}
