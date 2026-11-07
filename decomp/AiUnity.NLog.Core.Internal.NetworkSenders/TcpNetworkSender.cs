using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal.NetworkSenders;

internal class TcpNetworkSender : NetworkSender
{
	internal class MySocketAsyncEventArgs : SocketAsyncEventArgs
	{
		public void RaiseCompleted()
		{
			OnCompleted(this);
		}
	}

	private readonly Queue<SocketAsyncEventArgs> pendingRequests = new Queue<SocketAsyncEventArgs>();

	private ISocket socket;

	private Exception pendingError;

	private bool asyncOperationInProgress;

	private AsyncContinuation closeContinuation;

	private AsyncContinuation flushContinuation;

	internal AddressFamily AddressFamily { get; set; }

	internal int MaxQueueSize { get; set; }

	public TcpNetworkSender(string url, AddressFamily addressFamily)
		: base(url)
	{
		AddressFamily = addressFamily;
	}

	protected internal virtual ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
	{
		return new SocketProxy(addressFamily, socketType, protocolType);
	}

	protected override void DoInitialize()
	{
		MySocketAsyncEventArgs e = new MySocketAsyncEventArgs();
		e.RemoteEndPoint = ParseEndpointAddress(new Uri(Address), AddressFamily);
		e.Completed += SocketOperationCompleted;
		e.UserToken = null;
		socket = CreateSocket(e.RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		asyncOperationInProgress = true;
		if (!socket.ConnectAsync(e))
		{
			SocketOperationCompleted(socket, e);
		}
	}

	protected override void DoClose(AsyncContinuation continuation)
	{
		lock (this)
		{
			if (asyncOperationInProgress)
			{
				closeContinuation = continuation;
			}
			else
			{
				CloseSocket(continuation);
			}
		}
	}

	protected override void DoFlush(AsyncContinuation continuation)
	{
		lock (this)
		{
			if (!asyncOperationInProgress && pendingRequests.Count == 0)
			{
				continuation(null);
			}
			else
			{
				flushContinuation = continuation;
			}
		}
	}

	protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
	{
		MySocketAsyncEventArgs e = new MySocketAsyncEventArgs();
		e.SetBuffer(bytes, offset, length);
		e.UserToken = asyncContinuation;
		e.Completed += SocketOperationCompleted;
		lock (this)
		{
			if (MaxQueueSize != 0 && pendingRequests.Count >= MaxQueueSize)
			{
				pendingRequests.Dequeue()?.Dispose();
			}
			pendingRequests.Enqueue(e);
		}
		ProcessNextQueuedItem();
	}

	private void CloseSocket(AsyncContinuation continuation)
	{
		try
		{
			ISocket socket = this.socket;
			this.socket = null;
			socket?.Close();
			continuation(null);
		}
		catch (Exception exception)
		{
			if (exception.MustBeRethrown())
			{
				throw;
			}
			continuation(exception);
		}
	}

	private void SocketOperationCompleted(object sender, SocketAsyncEventArgs e)
	{
		lock (this)
		{
			asyncOperationInProgress = false;
			AsyncContinuation asyncContinuation = e.UserToken as AsyncContinuation;
			if (e.SocketError != SocketError.Success)
			{
				pendingError = new IOException("Error: " + e.SocketError);
			}
			e.Dispose();
			asyncContinuation?.Invoke(pendingError);
		}
		ProcessNextQueuedItem();
	}

	private void ProcessNextQueuedItem()
	{
		lock (this)
		{
			if (asyncOperationInProgress)
			{
				return;
			}
			if (pendingError != null)
			{
				while (pendingRequests.Count != 0)
				{
					SocketAsyncEventArgs e = pendingRequests.Dequeue();
					AsyncContinuation asyncContinuation = (AsyncContinuation)e.UserToken;
					e.Dispose();
					asyncContinuation(pendingError);
				}
			}
			if (pendingRequests.Count == 0)
			{
				AsyncContinuation asyncContinuation2 = flushContinuation;
				if (asyncContinuation2 != null)
				{
					flushContinuation = null;
					asyncContinuation2(pendingError);
				}
				AsyncContinuation asyncContinuation3 = closeContinuation;
				if (asyncContinuation3 != null)
				{
					closeContinuation = null;
					CloseSocket(asyncContinuation3);
				}
			}
			else
			{
				SocketAsyncEventArgs e = pendingRequests.Dequeue();
				asyncOperationInProgress = true;
				if (!socket.SendAsync(e))
				{
					SocketOperationCompleted(socket, e);
				}
			}
		}
	}

	public override void CheckSocket()
	{
		if (socket == null)
		{
			DoInitialize();
		}
	}
}
