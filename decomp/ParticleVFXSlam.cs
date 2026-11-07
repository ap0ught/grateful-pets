using System;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;

[ExecuteAlways]
public class ParticleVFXSlam : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public bool reset = true;

	public int count;

	public float delay;

	public float duration = 0.05f;

	public float delaycount;

	public ex3DSprite2 mask;

	public ex3DSprite2 spriteRenderer;

	public ParticleSystem system;

	public Vector3 start = new Vector3(0f, 0f, 0f);

	public Vector3 end = new Vector3(6f, 1f, 0f);

	public Color color;

	public Color detail;

	private float t;

	public void configure(string configurationString)
	{
		string[] array = configurationString.Split(';');
		exTextureInfo textureInfo = SpriteManager.GetTextureInfo(array[0]);
		this.color = ConsoleLib.Console.ColorUtility.ColorFromString(array[1]);
		detail = ConsoleLib.Console.ColorUtility.ColorFromString(array[2]);
		start = GameManager.Instance.GetCellCenter(Convert.ToInt32(array[3].Split(',')[0]), Convert.ToInt32(array[3].Split(',')[1]), 1);
		end = GameManager.Instance.GetCellCenter(Convert.ToInt32(array[4].Split(',')[0]), Convert.ToInt32(array[4].Split(',')[1]), 1);
		Color color = ConsoleLib.Console.ColorUtility.ColorFromString("k");
		mask.color = color;
		mask.detailcolor = color;
		mask.backcolor = color;
		spriteRenderer.textureInfo = textureInfo;
		spriteRenderer.detailcolor = detail;
		spriteRenderer.color = this.color;
		spriteRenderer.backcolor = color;
		spriteRenderer.transform.localPosition = new Vector3(0f, 0f, -1f);
		spriteRenderer.gameObject.SetActive(value: true);
		gameObject.transform.position = start;
		reset = true;
	}

	private void Awake()
	{
		reset = true;
	}

	private void Update()
	{
		if (reset)
		{
			system.Stop();
			system.Clear();
			reset = false;
			count = 0;
			system.Play();
			system.Emit(count);
			t = 0f;
			delaycount = delay;
		}
		if (!(t < duration))
		{
			return;
		}
		if (delaycount > 0f)
		{
			bool flag = false;
			if (delaycount == delay)
			{
				flag = true;
			}
			delaycount -= Time.deltaTime;
			if (!flag)
			{
				return;
			}
		}
		else
		{
			t += Time.deltaTime;
		}
		gameObject.transform.position = Vector3.Lerp(start, end, t / duration);
		spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 360f, t / duration));
		mask.transform.position = end;
	}
}
