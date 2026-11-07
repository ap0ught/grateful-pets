using System;
using System.Collections.Generic;
using UnityEngine;

namespace QupKit;

public class ScheduleComponent : MonoBehaviour
{
	public List<ScheduleEntry> Entries = new List<ScheduleEntry>();

	private void LateUpdate()
	{
		if (Entries.Count == 0)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		List<ScheduleEntry> list = null;
		for (int i = 0; i < Entries.Count; i++)
		{
			Entries[i].Time -= Time.deltaTime;
			if (!(Entries[i].Time <= 0f))
			{
				continue;
			}
			if (list == null)
			{
				list = new List<ScheduleEntry>();
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].ScheduledTime > Entries[i].ScheduledTime)
				{
					list.Insert(j, Entries[i]);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(Entries[i]);
			}
		}
		if (list != null)
		{
			for (int k = 0; k < list.Count; k++)
			{
				list[k].Execute();
				Entries.Remove(list[k]);
			}
		}
	}

	public void After(float T, Action A)
	{
		Entries.Add(new ScheduleEntry(T, A, Time.time + T));
	}
}
