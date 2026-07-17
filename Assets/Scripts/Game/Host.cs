using System;
using System.Collections;
using System.IO;
using Assets.Plugins.GameCode.Data;
using SevenZip.Compression.LZMA;
using UnityEngine;

public class Host : MonoBehaviour
{
	public Rect ButtonRect = new Rect(0f, 0f, 300f, 120f);

	public string ButtonLabel = "--";

	public Rect TextRect = new Rect(0f, 0f, 300f, 120f);

	public string DebugText = "Debug text";

	private bool _runTest;

	private ServerDataSerializer _serializer;

	private bool _readyToSend;

	private string _langs;

	private ServerData _sd;

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 10f, 100f, 50f), SingletonT<ServerData>.I.Version.ToString());
		Color color = GUI.color;
		if (GUI.Button(ButtonRect, ButtonLabel))
		{
			Debug.Log(">>> " + ButtonLabel);
			if (_readyToSend)
			{
				_readyToSend = false;
				StartCoroutine(SendData());
			}
		}
		GUI.color = Color.yellow;
		GUI.Label(TextRect, DebugText);
		GUI.color = color;
	}

	private void RunTestGui()
	{
		Rect buttonRect = ButtonRect;
		buttonRect.y += buttonRect.height;
		if (!_runTest && GUI.Button(buttonRect, "Run test"))
		{
			_runTest = true;
			PassLanguages("ru");
			StartCoroutine(SendData());
		}
	}

	private void Awake()
	{
		_serializer = new ServerDataSerializer();
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private IEnumerator Start()
	{
		yield return null;
		Application.ExternalCall("GetLanguages");
		yield return null;
		DebugText = "Ready to send ...";
		_readyToSend = true;
	}

	private IEnumerator DebugRu()
	{
		string lang = "ru";
		ServerData serverdata = null;
		bool success = false;
		bool next = false;
		ActionD<ServerData> onSuccess = delegate(ServerData sd)
		{
			next = true;
			serverdata = sd;
			if (sd != null)
			{
				success = true;
			}
		};
		ActionD<string> onError = delegate(string err)
		{
			next = true;
			DebugText = (DebugText = "LANG:{0} STATUS: {1}".Fmt(lang, err));
			success = false;
		};
		DebugText = "getting {0} ...".Fmt(lang);
		ServerData.LoadRemoteData(this, lang, onSuccess, onError);
		while (!next)
		{
			yield return null;
		}
		if (success)
		{
			ServerDataData sdd = new ServerDataData(serverdata);
			using MemoryStream stream = new MemoryStream();
			_serializer.Serialize(stream, sdd);
			byte[] bin = stream.ToArray();
			byte[] cbin = SevenZipHelper.Compress(bin);
			string b64 = Convert.ToBase64String(cbin);
			Debug.Log("=====> {0} bytes".Fmt(cbin.Length));
		}
		yield return null;
	}

	private void Update()
	{
		ButtonLabel = ((!_readyToSend) ? "Wait..." : "SendData");
	}

	private void PassLanguages(string par)
	{
		DebugText = ((!string.IsNullOrEmpty(par)) ? par : "<empty>");
		_langs = par;
	}

	private IEnumerator SendData()
	{
		if (_langs == null)
		{
			_readyToSend = false;
			DebugText = "error";
			yield break;
		}
		string[] array = _langs.Split('|');
		foreach (string lang in array)
		{
			ServerData serverdata = null;
			bool success = false;
			bool next = false;
			string language = lang;
			ActionD<ServerData> onSuccess = delegate(ServerData sd)
			{
				next = true;
				serverdata = sd;
				if (sd != null)
				{
					success = true;
				}
			};
			ActionD<string> onError = delegate(string err)
			{
				next = true;
				DebugText = "LANG:{0} STATUS: {1}".Fmt(language, err);
				success = false;
			};
			DebugText = "getting {0} ...".Fmt(lang);
			ServerData.LoadRemoteData(this, language, onSuccess, onError);
			while (!next)
			{
				yield return null;
			}
			if (success)
			{
				ThisIsDataFromAdmin(serverdata);
				ServerDataData sdd = new ServerDataData(serverdata);
				using MemoryStream stream = new MemoryStream();
				_serializer.Serialize(stream, sdd);
				byte[] bin = stream.ToArray();
				byte[] cbin = SevenZipHelper.Compress(bin);
				string b64 = Convert.ToBase64String(cbin);
				if (!_runTest)
				{
					Application.ExternalCall("SetFilesData", lang, b64);
				}
				else
				{
					ProcessTestData(bin);
				}
			}
			yield return null;
		}
		Application.ExternalCall("SaveFiles");
		DebugText = "ok";
		_readyToSend = true;
	}

	private void ProcessTestData(byte[] bin)
	{
		try
		{
			using MemoryStream source = new MemoryStream(bin);
			ServerDataData rawData = _serializer.Deserialize(source, null, typeof(ServerDataData)) as ServerDataData;
			TestServerData(rawData);
		}
		finally
		{
			_runTest = false;
		}
	}

	private void ThisIsDataFromAdmin(ServerData sd)
	{
		_sd = sd;
	}

	private void TestServerData(ServerDataData rawData)
	{
		ServerData data = new ServerData();
		rawData.CopyTo(data);
	}
}
