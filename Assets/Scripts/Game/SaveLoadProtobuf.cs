using System;
using System.IO;
using Assets.Plugins.GameCode.Data;
using Common;
using SevenZip.Compression.LZMA;
using UnityEngine;

public class SaveLoadProtobuf : MonoBehaviour, ISaveLoad<PlayerState>, ISaveLoadData<ServerDataData>
{
	private static readonly PlayerStateSerializer _serializer = new PlayerStateSerializer();

	private readonly int MaxCount = 4;

	private readonly int Version = 1;

	private readonly ServerDataSerializer _sdSerializer = new ServerDataSerializer();

	public void Save(int index, PlayerState state)
	{
		state.Version = Version;
		using MemoryStream memoryStream = new MemoryStream();
		_serializer.Serialize(memoryStream, state);
		SaveToFile(index, memoryStream);
	}

	public void Clear(int index)
	{
		string persistentDataPath = Application.persistentDataPath;
		string path = Path.Combine(persistentDataPath, GetFileName(index));
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public PlayerState Load(int index)
	{
		byte[] array = LoadFromFile(index);
		PlayerState playerState = null;
		if (array != null)
		{
			using MemoryStream source = new MemoryStream(array);
			try
			{
				playerState = _serializer.Deserialize(source, null, typeof(PlayerState)) as PlayerState;
			}
			catch (Exception)
			{
				Debug.Log("FAILED LOAD LOAD SAVEGAME");
				return null;
			}
		}
		if (playerState != null)
		{
			Debug.Log($"[SAVE VERSION : {playerState.Version} LOADED] : ");
		}
		return playerState;
	}

	private string GetFileName(int index)
	{
		if (index > 0)
		{
			return "jug_savering" + index + ".jug";
		}
		return "jug_savering.jug";
	}

	private void SaveToFile(int index, MemoryStream stream)
	{
		byte[] inArray = stream.ToArray();
		string text = Convert.ToBase64String(inArray);
		string persistentDataPath = Application.persistentDataPath;
		Utils.WriteAllText(Path.Combine(persistentDataPath, GetFileName(index)), text);
	}

	private byte[] LoadFromFile(int index)
	{
		string persistentDataPath = Application.persistentDataPath;
		string path = Path.Combine(persistentDataPath, GetFileName(index));
		if (!File.Exists(path))
		{
			return null;
		}
		string s = null;
		using (StreamReader streamReader = new StreamReader(path))
		{
			s = streamReader.ReadToEnd();
		}
		return Convert.FromBase64String(s);
	}

	private static byte[] LoadFromFileLZMA(string fname)
	{
		string persistentDataPath = Application.persistentDataPath;
		byte[] result = null;
		try
		{
			byte[] inputBytes = Utils.ReadAllBytes(Path.Combine(persistentDataPath, fname));
			result = SevenZipHelper.Decompress(inputBytes);
		}
		catch (Exception message)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log(message);
			}
		}
		return result;
	}

	private static bool SaveToFileLZMA(string fname, MemoryStream stream)
	{
		try
		{
			byte[] inputBytes = stream.ToArray();
			byte[] bytes = SevenZipHelper.Compress(inputBytes);
			string persistentDataPath = Application.persistentDataPath;
			Utils.WriteAllBytes(Path.Combine(persistentDataPath, fname), bytes);
			return true;
		}
		catch (Exception message)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log(message);
			}
			return false;
		}
	}

	public bool SaveData(string path, ServerDataData data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		_sdSerializer.Serialize(memoryStream, data);
		return SaveToFileLZMA(path, memoryStream);
	}

	public ServerDataData LoadData(byte[] data)
	{
		byte[] array = null;
		try
		{
			array = SevenZipHelper.Decompress(data);
		}
		catch (Exception message)
		{
			if (Globals.IsDebugBuild && Globals.IsDebugBuild)
			{
				Debug.Log(message);
			}
		}
		if (array == null)
		{
			return null;
		}
		ServerDataData result = null;
		using MemoryStream source = new MemoryStream(array);
		try
		{
			result = _sdSerializer.Deserialize(source, null, typeof(ServerDataData)) as ServerDataData;
		}
		catch (Exception message2)
		{
			if (Globals.IsDebugBuild && Globals.IsDebugBuild)
			{
				Debug.Log(message2);
			}
		}
		return result;
	}
}
