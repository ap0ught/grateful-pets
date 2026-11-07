using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRL.Sound;
using XRL.UI;

public class CampfireSounds : MonoBehaviour
{
	public GameObject sources;

	public GameObject harmonicaSource;

	public GameObject extinguishSource;

	public Dictionary<string, float> originalMusicVolumes = new Dictionary<string, float>();

	public bool bPlaying;

	private float flourishTimer;

	private float harmonicaStart;

	private string[] flourishes = new string[5] { "Cooking_Chopping_1_N", "Cooking_Chopping_4_N", "Cooking_Chopping_6_N", "Cooking_Pan_1", "Cooking_Pan2" };

	public void Open()
	{
		bPlaying = true;
		if (Options.GetOption("OptionSound") != "Yes")
		{
			return;
		}
		extinguishSource.GetComponent<AudioSource>().pitch = Random.value * 0.1f - 0.05f + 1f;
		harmonicaSource.GetComponent<AudioSource>().pitch = Random.value * 0.1f - 0.05f + 1f;
		foreach (MusicSource value in SoundManager.MusicSources.Values)
		{
			originalMusicVolumes[value.Channel] = value.TargetVolume;
			value.TargetVolume = 0f;
		}
		flourishTimer = 0f;
		harmonicaStart = -0.75f;
		extinguishSource.SetActive(value: false);
		harmonicaSource.SetActive(value: false);
		StopCoroutine(Closeout());
		sources.SetActive(value: true);
	}

	public void Close()
	{
		bPlaying = false;
		extinguishSource.SetActive(value: true);
		extinguishSource.GetComponent<AudioSource>().Play();
		StartCoroutine(Closeout());
	}

	private IEnumerator Closeout()
	{
		yield return new WaitForSeconds(2f);
		foreach (MusicSource value2 in SoundManager.MusicSources.Values)
		{
			if (originalMusicVolumes.TryGetValue(value2.Channel, out var value))
			{
				value2.TargetVolume = value;
			}
		}
		sources.SetActive(value: false);
		bPlaying = false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!bPlaying)
		{
			return;
		}
		harmonicaStart += Time.deltaTime;
		if ((double)harmonicaStart > 0.25 && !harmonicaSource.activeSelf)
		{
			if (Random.value <= 0.1f)
			{
				harmonicaSource.SetActive(value: true);
			}
			harmonicaStart = 0f;
		}
		flourishTimer += Time.deltaTime;
		if (flourishTimer > 1f)
		{
			flourishTimer = 0f;
			if (Random.value <= 0.2f)
			{
				SoundManager.PlaySound(flourishes[Random.Range(0, flourishes.Length - 1)]);
			}
		}
	}
}
