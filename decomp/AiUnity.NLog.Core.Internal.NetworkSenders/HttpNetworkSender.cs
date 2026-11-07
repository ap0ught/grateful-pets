using System;
using System.IO;
using System.Net;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal.NetworkSenders;

internal class HttpNetworkSender : NetworkSender
{
	public HttpNetworkSender(string url)
		: base(url)
	{
	}

	protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
	{
		WebRequest webRequest = WebRequest.Create(new Uri(Address));
		webRequest.Method = "POST";
		AsyncCallback onResponse = (IAsyncResult r) =>
		{
			try
			{
				using (webRequest.EndGetResponse(r))
				{
				}
				asyncContinuation(null);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}
				asyncContinuation(exception);
			}
		};
		AsyncCallback callback = (IAsyncResult r) =>
		{
			try
			{
				using (Stream stream = webRequest.EndGetRequestStream(r))
				{
					stream.Write(bytes, offset, length);
				}
				webRequest.BeginGetResponse(onResponse, null);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}
				asyncContinuation(exception);
			}
		};
		webRequest.BeginGetRequestStream(callback, null);
	}
}
