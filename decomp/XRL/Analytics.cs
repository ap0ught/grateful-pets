using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using LitJson;

namespace XRL;

public static class Analytics
{
	public static bool Enabled = false;

	private static Dictionary<string, string> Message = new Dictionary<string, string>();

	public static void Log(string Key, string Value)
	{
		Message.Clear();
		Message.Add(Key, Value);
		Log(Message);
	}

	public static void Log(Dictionary<string, string> Message)
	{
		if (!Enabled)
		{
			return;
		}
		string text = "3f2566437044cda60c96a1b9e31e6fcd";
		string text2 = "4857668fc230220314b315d9ad790c34b46ff065";
		string text3 = "http://api.gameanalytics.com/1";
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		string text4 = "";
		NetworkInterface[] array = allNetworkInterfaces;
		for (int i = 0; i < array.Length; i++)
		{
			PhysicalAddress physicalAddress = array[i].GetPhysicalAddress();
			if (physicalAddress.ToString() != "" && text4 == "")
			{
				byte[] bytes = Encoding.UTF8.GetBytes(physicalAddress.ToString());
				text4 = BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(bytes)).Replace("-", "");
			}
		}
		string text5 = "design";
		string text6 = JsonMapper.ToJson(Message);
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] bytes2 = Encoding.Default.GetBytes(text6 + text2);
		byte[] array2 = mD5CryptoServiceProvider.ComputeHash(bytes2);
		string text7 = "";
		byte[] array3 = array2;
		foreach (byte b in array3)
		{
			text7 += $"{b:x2}";
		}
		byte[] bytes3 = Encoding.ASCII.GetBytes(text6);
		WebRequest webRequest = WebRequest.Create(text3 + "/" + text + "/" + text5);
		webRequest.Headers.Add("Authorization", text7);
		webRequest.Method = "POST";
		webRequest.ContentLength = bytes3.Length;
		webRequest.ContentType = "application/x-www-form-urlencoded";
		Stream requestStream = webRequest.GetRequestStream();
		requestStream.Write(bytes3, 0, bytes3.Length);
		requestStream.Close();
	}
}
