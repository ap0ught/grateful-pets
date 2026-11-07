namespace AiUnity.NLog.Core.Internal.NetworkSenders;

internal interface INetworkSenderFactory
{
	NetworkSender Create(string url, int maxQueueSize);
}
