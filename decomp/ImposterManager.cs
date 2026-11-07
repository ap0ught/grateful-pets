using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL;
using XRL.Core;
using XRL.UI;
using XRL.World;

public static class ImposterManager
{
	private static Queue<QudScreenBufferExtra> extraPool = new Queue<QudScreenBufferExtra>();

	public static long currentImposterFrame = 0L;

	public static QudScreenBufferExtra getNewExtra()
	{
		lock (extraPool)
		{
			while (extraPool.Count > 0)
			{
				QudScreenBufferExtra qudScreenBufferExtra = extraPool.Dequeue();
				if (qudScreenBufferExtra != null)
				{
					return qudScreenBufferExtra;
				}
			}
			for (int i = 0; i < 100; i++)
			{
				extraPool.Enqueue(new QudScreenBufferExtra());
			}
		}
		return new QudScreenBufferExtra();
	}

	public static void freeExtra(QudScreenBufferExtra extra)
	{
		try
		{
			lock (extraPool)
			{
				extra.Clear();
				extraPool.Enqueue(extra);
			}
		}
		catch (Exception x)
		{
			extraPool = new Queue<QudScreenBufferExtra>();
			MetricsManager.LogException("ImposterManager::freeExtra", x);
		}
	}

	public static QudScreenBufferExtra getImposterUpdateFrame(ScreenBuffer Buffer)
	{
		if (Globals.RenderMode != RenderModeType.Text && !Options.DisableImposters)
		{
			currentImposterFrame++;
		}
		QudScreenBufferExtra newExtra = getNewExtra();
		Cell cell = The.Player?.CurrentCell;
		if (cell != null)
		{
			newExtra.playerPosition = cell.Pos2D;
		}
		return newExtra;
	}
}
