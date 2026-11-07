using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SimpleTextOutline : MonoBehaviour
{
	public float pixelSize = 1f;

	public Color outlineColor = Color.black;

	public bool resolutionDependant;

	public int doubleResolution = 1024;

	private TextMesh textMesh;

	private MeshRenderer meshRenderer;

	private List<TextMesh> shadowmeshes = new List<TextMesh>();

	private List<MeshRenderer> renderers = new List<MeshRenderer>();

	private string lastString;

	private float lastAlpha = -1f;

	private void LateUpdate()
	{
		if (shadowmeshes.Count == 0)
		{
			this.textMesh = GetComponent<TextMesh>();
			this.meshRenderer = GetComponent<MeshRenderer>();
			for (int i = 0; i < 8; i++)
			{
				GameObject gameObject = new GameObject("outline " + i, typeof(TextMesh));
				gameObject.transform.parent = transform;
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
				component.material = new Material(this.meshRenderer.material);
				component.shadowCastingMode = ShadowCastingMode.Off;
				component.receiveShadows = false;
				component.sortingLayerID = this.meshRenderer.sortingLayerID;
				component.sortingLayerName = this.meshRenderer.sortingLayerName;
				shadowmeshes.Add(gameObject.GetComponent<TextMesh>());
				renderers.Add(component);
			}
		}
		if (lastString == this.textMesh.text)
		{
			if (lastAlpha == this.textMesh.color.a)
			{
				return;
			}
			foreach (TextMesh shadowmesh in shadowmeshes)
			{
				_ = shadowmesh;
			}
			lastAlpha = this.textMesh.color.a;
			return;
		}
		lastString = this.textMesh.text;
		lastAlpha = this.textMesh.color.a;
		Vector3 vector = Camera.main.WorldToScreenPoint(transform.position);
		outlineColor.a = this.textMesh.color.a * this.textMesh.color.a;
		for (int j = 0; j < shadowmeshes.Count; j++)
		{
			TextMesh textMesh = shadowmeshes[j];
			textMesh.color = outlineColor;
			textMesh.text = this.textMesh.text;
			textMesh.alignment = this.textMesh.alignment;
			textMesh.anchor = this.textMesh.anchor;
			textMesh.characterSize = this.textMesh.characterSize;
			textMesh.font = this.textMesh.font;
			textMesh.fontSize = this.textMesh.fontSize;
			textMesh.fontStyle = this.textMesh.fontStyle;
			textMesh.richText = this.textMesh.richText;
			textMesh.tabSize = this.textMesh.tabSize;
			textMesh.lineSpacing = this.textMesh.lineSpacing;
			textMesh.offsetZ = this.textMesh.offsetZ;
			bool flag = resolutionDependant && (Screen.width > doubleResolution || Screen.height > doubleResolution);
			Vector3 vector2 = GetOffset(j) * (flag ? (2f * pixelSize) : pixelSize);
			Vector3 position = Camera.main.ScreenToWorldPoint(vector + vector2);
			textMesh.transform.position = position;
			MeshRenderer meshRenderer = renderers[j];
			meshRenderer.sortingLayerID = this.meshRenderer.sortingLayerID;
			meshRenderer.sortingLayerName = this.meshRenderer.sortingLayerName;
		}
	}

	private Vector3 GetOffset(int i)
	{
		return (i % 8) switch
		{
			0 => new Vector3(0f, 1f, 1f), 
			1 => new Vector3(1f, 1f, 1f), 
			2 => new Vector3(1f, 0f, 1f), 
			3 => new Vector3(1f, -1f, 1f), 
			4 => new Vector3(0f, -1f, 1f), 
			5 => new Vector3(-1f, -1f, 1f), 
			6 => new Vector3(-1f, 0f, 1f), 
			7 => new Vector3(-1f, 1f, 1f), 
			_ => Vector3.zero, 
		};
	}
}
