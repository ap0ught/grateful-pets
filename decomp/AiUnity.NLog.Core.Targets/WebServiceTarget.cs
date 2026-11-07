using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using AiUnity.Common.Attributes;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("WebService")]
[Preserve]
public sealed class WebServiceTarget : MethodCallTargetBase
{
	private const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

	private const string Soap12EnvelopeNamespace = "http://www.w3.org/2003/05/soap-envelope";

	[Display("URL", "Web service URL.", false, 0)]
	public Uri Url { get; set; }

	public string MethodName { get; set; }

	public string Namespace { get; set; }

	[DefaultValue("Post")]
	public WebServiceProtocol Protocol { get; set; }

	public Encoding Encoding { get; set; }

	public WebServiceTarget()
	{
		Protocol = WebServiceProtocol.HttpPost;
		Encoding = Encoding.UTF8;
	}

	protected override void DoInvoke(object[] parameters)
	{
		throw new NotImplementedException();
	}

	protected override void DoInvoke(object[] parameters, AsyncContinuation continuation)
	{
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
		byte[] postPayload = null;
		switch (Protocol)
		{
		case WebServiceProtocol.Soap11:
			postPayload = PrepareSoap11Request(request, parameters);
			break;
		case WebServiceProtocol.Soap12:
			postPayload = PrepareSoap12Request(request, parameters);
			break;
		case WebServiceProtocol.HttpGet:
			postPayload = PrepareGetRequest(request, parameters);
			break;
		case WebServiceProtocol.HttpPost:
			postPayload = PreparePostRequest(request, parameters);
			break;
		}
		AsyncContinuation sendContinuation = (Exception ex) =>
		{
			if (ex != null)
			{
				continuation(ex);
			}
			else
			{
				request.BeginGetResponse((IAsyncResult r) =>
				{
					try
					{
						using (request.EndGetResponse(r))
						{
						}
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
				}, null);
			}
		};
		if (postPayload != null && postPayload.Length != 0)
		{
			request.BeginGetRequestStream((IAsyncResult r) =>
			{
				try
				{
					using (Stream stream = request.EndGetRequestStream(r))
					{
						stream.Write(postPayload, 0, postPayload.Length);
					}
					sendContinuation(null);
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
					{
						throw;
					}
					continuation(exception);
				}
			}, null);
		}
		else
		{
			sendContinuation(null);
		}
	}

	private byte[] PrepareSoap11Request(HttpWebRequest request, object[] parameters)
	{
		request.Method = "POST";
		request.ContentType = "text/xml; charset=" + Encoding.WebName;
		if (Namespace.EndsWith("/", StringComparison.Ordinal))
		{
			request.Headers["SOAPAction"] = Namespace + MethodName;
		}
		else
		{
			request.Headers["SOAPAction"] = Namespace + "/" + MethodName;
		}
		using MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
		{
			Encoding = Encoding
		});
		xmlWriter.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
		xmlWriter.WriteStartElement("Body", "http://schemas.xmlsoap.org/soap/envelope/");
		xmlWriter.WriteStartElement(MethodName, Namespace);
		int num = 0;
		foreach (MethodCallParameter parameter in Parameters)
		{
			xmlWriter.WriteElementString(parameter.Name, Convert.ToString(parameters[num]));
			num++;
		}
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		return memoryStream.ToArray();
	}

	private byte[] PrepareSoap12Request(HttpWebRequest request, object[] parameterValues)
	{
		request.Method = "POST";
		request.ContentType = "text/xml; charset=" + Encoding.WebName;
		using MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
		{
			Encoding = Encoding
		});
		xmlWriter.WriteStartElement("soap12", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
		xmlWriter.WriteStartElement("Body", "http://www.w3.org/2003/05/soap-envelope");
		xmlWriter.WriteStartElement(MethodName, Namespace);
		int num = 0;
		foreach (MethodCallParameter parameter in Parameters)
		{
			xmlWriter.WriteElementString(parameter.Name, Convert.ToString(parameterValues[num]));
			num++;
		}
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		return memoryStream.ToArray();
	}

	private byte[] PreparePostRequest(HttpWebRequest request, object[] parameterValues)
	{
		request.Method = "POST";
		return PrepareHttpRequest(request, parameterValues);
	}

	private byte[] PrepareGetRequest(HttpWebRequest request, object[] parameterValues)
	{
		request.Method = "GET";
		return PrepareHttpRequest(request, parameterValues);
	}

	private byte[] PrepareHttpRequest(HttpWebRequest request, object[] parameterValues)
	{
		request.ContentType = "application/x-www-form-urlencoded; charset=" + Encoding.WebName;
		string value = string.Empty;
		using MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding);
		streamWriter.Write(string.Empty);
		int num = 0;
		foreach (MethodCallParameter parameter in Parameters)
		{
			streamWriter.Write(value);
			streamWriter.Write(parameter.Name);
			streamWriter.Write("=");
			streamWriter.Write(UrlHelper.UrlEncode(Convert.ToString(parameterValues[num]), spaceAsPlus: true));
			value = "&";
			num++;
		}
		streamWriter.Flush();
		return memoryStream.ToArray();
	}
}
