using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Kobold;
using Qud.UI;
using UnityEngine;
using XRL;

public class AscendAnimation : MonoBehaviour
{
	public const int FADE_IN_DURATION = 18;

	public GameObject[] climber;

	public ParticleSystem[] system;

	public LineRenderer[] line;

	private ParticleSystem.Particle[][] Stars = new ParticleSystem.Particle[3][];

	public int MaxStars = 100;

	public float Speed = 10f;

	public float TotalTimer;

	public int Phase;

	public bool done;

	private static Color32[] colors = new Color32[7]
	{
		new Color32(byte.MaxValue, 0, 0, byte.MaxValue),
		new Color32(0, byte.MaxValue, 0, byte.MaxValue),
		new Color32(0, 0, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
		new Color32(0, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
	};

	private static CanvasGroup uiManager;

	private static GameObject _tileroot;

	private static GameObject _overlayroot;

	private static GameObject ImposterRoot;

	public bool auto;

	public bool begin;

	public float countdown;

	public float climbTimer;

	public static Color ParseColor(string Value)
	{
		Color value = default;
		if (!Value.IsNullOrEmpty())
		{
			if (Value[0] == '#')
			{
				UnityEngine.ColorUtility.TryParseHtmlString(Value, out value);
			}
			else
			{
				ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(Value[0], out value);
			}
		}
		return value;
	}

	public static async void Play(Action OnComplete, Renderable Renderable)
	{
		bool runInBackgroundSuspend = false;
		GameObject animation = null;
		try
		{
			await The.UiContext;
			runInBackgroundSuspend = Application.runInBackground;
			Application.runInBackground = true;
			SoundManager.StopMusic("music", Crossfade: true, 3f);
			GameManager.OverlayRootForceoff = true;
			uiManager = GameObject.Find("UI Manager")?.GetComponent<CanvasGroup>();
			_tileroot = GameObject.Find("_tileroot");
			_overlayroot = GameObject.Find("_overlayroot");
			ImposterRoot = GameObject.Find("ImposterRoot");
			if (uiManager != null)
			{
				uiManager.alpha = 0f;
			}
			_tileroot?.SetActive(value: false);
			_overlayroot?.SetActive(value: false);
			ImposterRoot?.SetActive(value: false);
			Camera.main.transform.localPosition = new Vector3(0f, 0f, 0f);
			animation = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/AscendAnimation")) as GameObject;
			animation.transform.localPosition = new Vector3(0f, 0f, 10f);
			AscendAnimation a = animation.GetComponent<AscendAnimation>();
			a.SetClimber(Renderable);
			a.Begin();
			while (!a.done)
			{
				await Task.Delay(100);
			}
		}
		finally
		{
			try
			{
				GameManager.OverlayRootForceoff = false;
				GameManager.Instance?.PopGameView();
				CombatJuiceManager.NoPause = false;
				if (uiManager != null)
				{
					uiManager.alpha = 1f;
				}
				_tileroot?.SetActive(value: true);
				_overlayroot?.SetActive(value: true);
				ImposterRoot?.SetActive(value: true);
				if (animation != null)
				{
					UnityEngine.Object.Destroy(animation);
				}
				The.Core.RenderBase();
				Application.runInBackground = runInBackgroundSuspend;
			}
			catch (Exception x)
			{
				MetricsManager.LogError("AscendAnimation::Complete", x);
			}
			OnComplete();
		}
	}

	public void Awake()
	{
		begin = false;
	}

	public void Begin()
	{
		done = false;
		auto = true;
		CombatJuiceManager.NoPause = true;
		FadeOutUI(2f);
		GameObject.Find("_tileroot")?.SetActive(value: false);
		GameObject.Find("_overlayroot")?.SetActive(value: false);
		Init();
		Phase = 1;
		Speed = 10f;
	}

	public void SetClimber(Renderable Renderable)
	{
		try
		{
			ex3DSprite2 ex3DSprite3 = climber[2]?.GetComponent<ex3DSprite2>();
			if (ex3DSprite3 != null)
			{
				if (SpriteManager.TryGetTextureInfo(Renderable.Tile, out var Info))
				{
					ex3DSprite3.textureInfo = Info;
				}
				ex3DSprite3.color = ConsoleLib.Console.ColorUtility.colorFromChar(Renderable.GetForegroundColor());
				ex3DSprite3.detailcolor = ConsoleLib.Console.ColorUtility.colorFromChar(Renderable.getDetailColor());
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("AscendAnimation::Begin::Golem Setup", x);
		}
	}

	public void FadeGroup(int group, float alpha)
	{
		Color32 startColor;
		for (int i = 0; i < MaxStars; i++)
		{
			startColor = Stars[group][i].startColor;
			Stars[group][i].startColor = new Color32(startColor.r, startColor.g, startColor.b, (byte)Mathf.Lerp(0f, 255f, alpha));
		}
		startColor = line[group].startColor;
		line[group].startColor = new Color((int)startColor.r, (int)startColor.g, (int)startColor.b, alpha);
		line[group].endColor = new Color((int)startColor.r, (int)startColor.g, (int)startColor.b, alpha);
		if (climber[group].GetComponent<SpriteRenderer>() != null)
		{
			startColor = climber[group].GetComponent<SpriteRenderer>().color;
			climber[group].GetComponent<SpriteRenderer>().color = new Color((int)startColor.r, (int)startColor.g, (int)startColor.b, alpha);
		}
		if (climber[group].GetComponent<ex3DSprite2>() != null)
		{
			startColor = climber[group].GetComponent<ex3DSprite2>().detailcolor;
			climber[group].GetComponent<ex3DSprite2>().detailcolor = new Color((int)startColor.r, (int)startColor.g, (int)startColor.b, alpha);
			startColor = climber[group].GetComponent<ex3DSprite2>().color;
			climber[group].GetComponent<ex3DSprite2>().color = new Color((int)startColor.r, (int)startColor.g, (int)startColor.b, alpha);
		}
	}

	public void Init()
	{
		for (int i = 0; i < 3; i++)
		{
			Stars[i] = new ParticleSystem.Particle[MaxStars];
			system[i].Stop();
			system[i].Clear();
			system[i].Emit(MaxStars);
			system[i].GetParticles(Stars[i]);
		}
		for (int j = 0; j < 3; j++)
		{
			for (int k = 0; k < MaxStars; k++)
			{
				Stars[j][k].startColor = new Color32(0, 0, 0, 0);
			}
			system[j].SetParticles(Stars[j]);
		}
		climber[0].transform.localPosition = new Vector3(0f, 10000f, 1f);
		for (int l = 0; l < 3; l++)
		{
			for (int m = 0; m < MaxStars; m++)
			{
				Stars[l][m].startColor = Color.white;
				Stars[l][m].position = new Vector3(UnityEngine.Random.Range(-Screen.width / 2, Screen.width / 2), UnityEngine.Random.Range(-Screen.height / 2, Screen.height / 2), 100f);
				float startSize = Mathf.Max(0.5f, UnityEngine.Random.Range(10f, 50f) / 10f - (float)l);
				Stars[l][m].startSize = startSize;
				Stars[l][m].startColor = colors.GetRandomElement();
				Stars[l][m].remainingLifetime = 1000f;
				Stars[l][m].startLifetime = 1000f;
			}
			system[l].SetParticles(Stars[l], MaxStars);
			system[l].Clear();
			system[l].Play();
		}
		FadeGroup(0, 0f);
		FadeGroup(1, 0f);
		FadeGroup(2, 0f);
	}

	private void LateUpdate()
	{
		int num = 20;
		int num2 = 25;
		int num3 = 45;
		int num4 = 55;
		Camera.main.transform.localPosition = new Vector3(0f, 0f, 0f);
		if (begin)
		{
			Phase = 0;
			begin = false;
			Begin();
		}
		TotalTimer += Time.deltaTime;
		if (Phase <= 0)
		{
			return;
		}
		if (Phase == 1)
		{
			if (TotalTimer >= (float)num)
			{
				TotalTimer = 0f;
				Phase = 2;
				SoundManager.PlayMusic("Music/Arrival", "music", Crossfade: false, 0f, 1f, () =>
				{
					Phase = 3;
					countdown = 0.5f;
				});
			}
			else
			{
				FadeGroup(0, TotalTimer / (float)num);
			}
		}
		if (Phase == 3)
		{
			countdown -= Time.deltaTime;
			if (countdown < 0f)
			{
				Phase = 4;
				Speed = 10000f;
			}
		}
		Speed -= Time.deltaTime * 2500f;
		if (Speed < 10f)
		{
			Speed = 10f;
			if (Phase == 4)
			{
				Phase = 5;
				climber[0].transform.localPosition = new Vector3(0f, 0f, 1f);
			}
		}
		if (Phase == 4)
		{
			climber[0].transform.localPosition = Vector3.Lerp(new Vector3(0f, 10000f, 1f), new Vector3(0f, 0f, 1f), 1f - Speed / 10000f);
		}
		else if (Phase == 5)
		{
			climbTimer += Time.deltaTime;
			if (climbTimer >= 1f)
			{
				climbTimer--;
				climber[0].transform.localPosition += new Vector3(0f, 1f, 0f);
			}
			if (TotalTimer > (float)num2)
			{
				climber[1].transform.localPosition = new Vector3(climber[1].transform.localPosition.x, -100f, 1f);
				StartCoroutine(FadeOut(0, 5f));
				StartCoroutine(FadeIn(1, 10f));
				Phase = 6;
			}
		}
		else if (Phase == 6)
		{
			climbTimer += Time.deltaTime;
			if (climbTimer >= 1f)
			{
				climbTimer--;
				climber[1].transform.localPosition += new Vector3(0f, 4f, 0f);
			}
			if (TotalTimer > (float)num3)
			{
				climber[2].transform.localPosition = new Vector3(climber[2].transform.localPosition.x, 0f, 1f);
				StartCoroutine(FadeOut(1, 5f));
				StartCoroutine(FadeIn(2, 10f));
				Phase = 7;
			}
		}
		else if (Phase == 7)
		{
			climbTimer += Time.deltaTime;
			if (climbTimer >= 1f)
			{
				climbTimer--;
				climber[2].transform.localPosition += new Vector3(0f, 16f, 0f);
				climber[2].transform.localScale = new Vector3(0f - climber[2].transform.localScale.x, climber[2].transform.localScale.y, climber[2].transform.localScale.z);
			}
			if (TotalTimer > (float)num4)
			{
				FadeToBlack.Fade(0f, 1f, 1f, Camera.main.backgroundColor);
				StartCoroutine(FadeInUI(5f));
				StartCoroutine(FadeOut(2, 5f));
				Phase = 8;
			}
		}
		else if (Phase == 8)
		{
			climbTimer += Time.deltaTime;
			if (climbTimer >= 2f)
			{
				climbTimer -= 2f;
				climber[2].transform.localPosition += new Vector3(0f, 16f, 0f);
				climber[2].transform.localScale = new Vector3(0f - climber[2].transform.localScale.x, climber[2].transform.localScale.y, climber[2].transform.localScale.z);
			}
		}
		for (int num5 = 0; num5 < 3; num5++)
		{
			ParticleSystem.SizeBySpeedModule sizeBySpeed = system[num5].sizeBySpeed;
			sizeBySpeed.yMultiplier = Mathf.Min(100f, Speed / 10f);
			for (int num6 = 0; num6 < MaxStars; num6++)
			{
				ParticleSystem.Particle[] array = Stars[num5];
				array[num6].position += new Vector3(0f, (0f - Speed - array[num6].startSize * 0.05f) * Time.deltaTime, 0f);
				if (array[num6].position.y < (float)(-Screen.height / 2))
				{
					array[num6].position = new Vector3(UnityEngine.Random.Range(-Screen.width / 2, Screen.width / 2), Screen.height / 2 + UnityEngine.Random.Range(0, 1000), array[num6].position.z);
				}
			}
			system[num5].SetParticles(Stars[num5], MaxStars);
		}
		if (TotalTimer > 60f)
		{
			The.Core.RenderBase();
			done = true;
		}
	}

	private IEnumerator FadeOut(int group, float t)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while ((float)sw.ElapsedMilliseconds < t * 1000f)
		{
			FadeGroup(group, 1f - (float)sw.ElapsedMilliseconds / (t * 1000f));
			yield return new WaitForEndOfFrame();
		}
		FadeGroup(group, 0f);
	}

	private IEnumerator FadeIn(int group, float t)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while ((float)sw.ElapsedMilliseconds < t * 1000f)
		{
			FadeGroup(group, (float)sw.ElapsedMilliseconds / (t * 1000f));
			yield return new WaitForEndOfFrame();
		}
		FadeGroup(group, 1f);
	}

	private IEnumerator FadeOutUI(float t)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while ((float)sw.ElapsedMilliseconds < t * 1000f)
		{
			if (uiManager != null)
			{
				uiManager.alpha = 1f - (float)sw.ElapsedMilliseconds / (t * 1000f);
			}
			yield return new WaitForEndOfFrame();
		}
		if (uiManager != null)
		{
			uiManager.alpha = 0f;
		}
	}

	private IEnumerator FadeInUI(float t)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while ((float)sw.ElapsedMilliseconds < t * 1000f)
		{
			if (uiManager != null)
			{
				uiManager.alpha = (float)sw.ElapsedMilliseconds / (t * 1000f);
			}
			yield return new WaitForEndOfFrame();
		}
		if (uiManager != null)
		{
			uiManager.alpha = 1f;
		}
	}
}
