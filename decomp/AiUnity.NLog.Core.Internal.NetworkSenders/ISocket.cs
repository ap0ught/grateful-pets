using System.Net.Sockets;

namespace AiUnity.NLog.Core.Internal.NetworkSenders;

internal interface ISocket
{
	bool ConnectAsync(SocketAsyncEventArgs args);

	void Close();

	bool SendAsync(SocketAsyncEventArgs args);

	bool SendToAsync(SocketAsyncEventArgs args);
}
