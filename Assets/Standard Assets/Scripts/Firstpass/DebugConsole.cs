using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DebugConsole : MonoBehaviour, IDebugCommandExecutioner, IDebugEchoListner, IDebugCommandHost
{
	private enum StateE
	{
		Closed,
		Opening,
		Opened,
		Closing,
		IntelliSense
	}

	private class CommandInfo
	{
		public string command;

		public string description;

		public Action<IDebugCommandHost, string, IList<string>> callback;

		public CommandInfo(string command, string description, Action<IDebugCommandHost, string, IList<string>> callback)
		{
			this.command = command;
			this.description = description;
			this.callback = callback;
		}
	}

	private const int MAX_COMMAND_HISTORY = 32;

	private const int MAX_LINE_COUNT = 1024;

	private const int TEXT_FIELD_WIDTH = 32;

	private const float INTELLISENSE_WINDOW_WIDTH = 300f;

	private const float INTELLISENSE_WINDOW_HEIGHT = 300f;

	private const float INTELLISENSE_BUTTON_HEIGHT = 32f;

	private StateE _state;

	private float _stateTransition;

	private List<IDebugEchoListner> _listeners = new List<IDebugEchoListner>();

	private Stack<IDebugCommandExecutioner> _executioners = new Stack<IDebugCommandExecutioner>();

	private Dictionary<string, CommandInfo> _commandTable = new Dictionary<string, CommandInfo>();

	private List<string> _commandHistory = new List<string>();

	private int _commandHistoryIndex;

	private Queue<string> _lines = new Queue<string>();

	private string _commandLine = string.Empty;

	private StringBuilder _stringBuilder = new StringBuilder();

	private Vector2 _scrollPosLines;

	private Vector2 _scrollPosIntelliSense;

	private bool _needAutoScroll;

	private int _prevCommandLineLenght;

	private List<string> _intelliSenseCommands = new List<string>();

	private bool _intellisenseCommandWasClicked;

	private bool _intelliSenseEscape;

	private int _intelliSenseSelectedIndex;

	public bool Focused => _state != StateE.Closed;

	private void Start()
	{
		RegisterEchoListner(new UnityConsoleEchoListener());
		RegisterCommand("help", "Show Command helps", delegate
		{
			int num2 = 0;
			foreach (CommandInfo value in _commandTable.Values)
			{
				num2 = Math.Max(num2, value.command.Length);
			}
			string format = $"{{0,-{num2}}}    {{1}}";
			foreach (CommandInfo value2 in _commandTable.Values)
			{
				Echo(string.Format(format, value2.command, value2.description));
			}
		});
		RegisterCommand("cls", "Clear Screen", delegate
		{
			_lines.Clear();
		});
		RegisterCommand("echo", "Display Messages", delegate(IDebugCommandHost host, string command, IList<string> args)
		{
			Echo(command.Substring(5));
		});
		for (int num = 0; num < 50; num++)
		{
			RegisterCommand("echo" + num, "Display Messages", delegate(IDebugCommandHost host, string command, IList<string> args)
			{
				Echo(command.Substring(5));
			});
		}
	}

	private void OnGUI()
	{
		UpdateInput();
		if (_state == StateE.Closed)
		{
			return;
		}
		float num = Screen.width;
		float num2 = Screen.height;
		float num3 = num2 * 0.1f;
		float num4 = num * 0.1f;
		Rect position = new Rect
		{
			x = (int)num4,
			y = (int)num3 + 32,
			width = (int)(num * 0.8f),
			height = (int)((float)Screen.height * 0.8f - 32f)
		};
		GUI.matrix = Matrix4x4.TRS(new Vector3(0f, (0f - position.height) * (1f - _stateTransition), 0f), Quaternion.identity, Vector3.one);
		_commandLine = GUI.TextField(new Rect(num4, num3, position.width, 32f), _commandLine);
		if (GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) is TextEditor textEditor && _intellisenseCommandWasClicked)
		{
			textEditor.MoveCursorToPosition(new Vector2(10000f, 10000f));
			_intellisenseCommandWasClicked = false;
		}
		_stringBuilder.Remove(0, _stringBuilder.Length);
		foreach (string line in _lines)
		{
			_stringBuilder.AppendLine(line);
		}
		GUI.Box(position, GUIContent.none);
		GUIContent content = new GUIContent(_stringBuilder.ToString());
		float height = GUI.skin.label.CalcHeight(content, position.width);
		Rect rect = new Rect(position.x, position.y, position.width, height);
		if (_needAutoScroll)
		{
			if (rect.height > position.height)
			{
				_scrollPosLines.y = rect.height - position.height;
			}
			_needAutoScroll = false;
		}
		_scrollPosLines = GUI.BeginScrollView(position, _scrollPosLines, rect);
		GUI.Label(rect, content);
		GUI.EndScrollView(handleScrollWheel: true);
		float num5 = num2 * 0.8f / 3f;
		int num6 = 0;
		if (GUI.Button(new Rect(num * 0.9f, num3 + (float)num6 * num5, num4, num5), "X") && _state == StateE.Opened)
		{
			Hide();
		}
		num6++;
		if (GUI.Button(new Rect(num * 0.9f, num3 + (float)num6 * num5, num4, num5), "Prev") && _state == StateE.Opened)
		{
			UpHistory();
		}
		num6++;
		if (GUI.Button(new Rect(num * 0.9f, num3 + (float)num6 * num5, num4, num5), "Next") && _state == StateE.Opened)
		{
			DownHistory();
		}
		if (GUI.Button(new Rect(0f, num3, num4, num2 * 0.8f), "Exec") && _state == StateE.Opened)
		{
			ExecuteCommand(_commandLine);
			_commandLine = string.Empty;
		}
		DrawIntelliSense();
		GUI.Label(new Rect(0f, 0f, 500f, 100f), _state.ToString() + " " + _prevCommandLineLenght);
		_prevCommandLineLenght = _commandLine.Length;
	}

	private void DrawIntelliSense()
	{
		if (_state != StateE.IntelliSense)
		{
			return;
		}
		if (_prevCommandLineLenght != _commandLine.Length)
		{
			if (_commandLine.Length > 0)
			{
				_intelliSenseCommands = (from item in _commandTable
					where item.Key.StartsWith(_commandLine)
					orderby item.Key
					select item.Key).ToList();
			}
			else if (_intelliSenseCommands != null)
			{
				_intelliSenseCommands.Clear();
			}
		}
		if (_commandLine.Length <= 0 || _intelliSenseCommands == null || _intelliSenseCommands.Count <= 0)
		{
			return;
		}
		float num = 32f * (float)_intelliSenseCommands.Count;
		Rect position = new Rect((float)Screen.width * 0.1f, (float)Screen.height * 0.1f + 32f, 300f, Mathf.Min(num, 300f));
		float num2 = 32f * (float)_intelliSenseSelectedIndex;
		_scrollPosIntelliSense = GUI.BeginScrollView(position, _scrollPosIntelliSense, new Rect(0f, 0f, 300f, num));
		Color contentColor = GUI.contentColor;
		Color contentColor2 = new Color(0f, 1f, 0f);
		for (int num3 = 0; num3 < _intelliSenseCommands.Count; num3++)
		{
			string text = _intelliSenseCommands[num3] + " ";
			if (num3 == _intelliSenseSelectedIndex)
			{
				GUI.contentColor = contentColor2;
			}
			if (GUI.Button(new Rect(0f, (float)num3 * 32f, 300f, 32f), text))
			{
				_commandLine = text;
				_intellisenseCommandWasClicked = true;
				_state = StateE.Opened;
			}
			if (num3 == _intelliSenseSelectedIndex)
			{
				GUI.contentColor = contentColor;
			}
		}
		GUI.EndScrollView(handleScrollWheel: true);
	}

	private void UpdateInput()
	{
		switch (_state)
		{
		case StateE.Closed:
			if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
			{
				Show();
			}
			break;
		case StateE.Opening:
			break;
		case StateE.Opened:
			if (Event.current != null && Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == KeyCode.Escape)
				{
					Hide();
				}
				else if (Event.current.keyCode == KeyCode.Return)
				{
					ExecuteCommand(_commandLine);
					_commandLine = string.Empty;
				}
				else if (Event.current.keyCode == KeyCode.UpArrow)
				{
					UpHistory();
				}
				else if (Event.current.keyCode == KeyCode.DownArrow)
				{
					DownHistory();
				}
			}
			break;
		case StateE.IntelliSense:
			if (Event.current == null || Event.current.type != EventType.KeyDown)
			{
				break;
			}
			if (Event.current.keyCode == KeyCode.Escape)
			{
				_state = StateE.Opened;
				_intelliSenseEscape = true;
			}
			else if (Event.current.keyCode == KeyCode.Return)
			{
				if (_intelliSenseCommands.Count > 0)
				{
					_commandLine = _intelliSenseCommands[_intelliSenseSelectedIndex] + " ";
					_intellisenseCommandWasClicked = true;
					_state = StateE.Opened;
				}
			}
			else if (Event.current.keyCode == KeyCode.UpArrow)
			{
				if (_intelliSenseSelectedIndex > 0)
				{
					_intelliSenseSelectedIndex--;
				}
			}
			else if (Event.current.keyCode == KeyCode.DownArrow && _intelliSenseSelectedIndex < _intelliSenseCommands.Count - 1)
			{
				_intelliSenseSelectedIndex++;
			}
			break;
		case StateE.Closing:
			break;
		}
	}

	private void UnfocusControls()
	{
		GUI.SetNextControlName("dummy");
		GUI.Button(new Rect(0f, 0f, 0f, 0f), GUIContent.none);
		GUI.FocusControl("dummy");
	}

	private void DownHistory()
	{
		if (_commandHistory.Count > 0)
		{
			_commandHistoryIndex = Math.Min(_commandHistory.Count - 1, _commandHistoryIndex + 1);
			_commandLine = _commandHistory[_commandHistoryIndex];
		}
	}

	private void UpHistory()
	{
		if (_commandHistory.Count > 0)
		{
			_commandHistoryIndex = Math.Max(0, _commandHistoryIndex - 1);
			_commandLine = _commandHistory[_commandHistoryIndex];
		}
	}

	private void Update()
	{
		switch (_state)
		{
		case StateE.Closed:
		{
			if (!Input.multiTouchEnabled || Input.touchCount < 4)
			{
				break;
			}
			int num = 0;
			Touch[] touches = Input.touches;
			for (int i = 0; i < touches.Length; i++)
			{
				Touch touch = touches[i];
				if (Mathf.Abs(touch.deltaPosition.y) > Mathf.Abs(touch.deltaPosition.x) && touch.deltaPosition.y < 0f)
				{
					num++;
				}
			}
			if (num == Input.touchCount)
			{
				Show();
			}
			break;
		}
		case StateE.Opening:
			_stateTransition += Time.time * 0.01f;
			if (_stateTransition > 1f)
			{
				_stateTransition = 1f;
				_state = StateE.Opened;
			}
			break;
		case StateE.Opened:
			if (_commandLine.Length == 0)
			{
				_intelliSenseEscape = false;
				_intelliSenseSelectedIndex = 0;
				_state = StateE.IntelliSense;
			}
			break;
		case StateE.Closing:
			_stateTransition -= Time.time * 0.01f;
			if (_stateTransition < 0f)
			{
				_stateTransition = 0f;
				_state = StateE.Closed;
			}
			break;
		}
	}

	public void Show()
	{
		if (_state == StateE.Closed)
		{
			_stateTransition = 0f;
			_state = StateE.Opening;
		}
	}

	public void Hide()
	{
		if (_state == StateE.Opened)
		{
			_stateTransition = 1f;
			_state = StateE.Closing;
		}
	}

	public void RegisterCommand(string command, string description, Action<IDebugCommandHost, string, IList<string>> callback)
	{
		string key = command.ToLower();
		if (_commandTable.ContainsKey(key))
		{
			throw new InvalidOperationException($"Command \"{command}\" is already registered.");
		}
		_commandTable.Add(key, new CommandInfo(command, description, callback));
	}

	public void UnregisterCommand(string command)
	{
		string key = command.ToLower();
		if (!_commandTable.ContainsKey(key))
		{
			throw new InvalidOperationException($"Command \"{command}\" is not registered.");
		}
		_commandTable.Remove(command);
	}

	public void ExecuteCommand(string command)
	{
		if (_executioners.Count != 0)
		{
			_executioners.Peek().ExecuteCommand(command);
			return;
		}
		char[] array = new char[1] { ' ' };
		Echo(">" + command);
		command = command.TrimStart(array);
		List<string> list = new List<string>(command.Split(array));
		string text = list[0];
		list.RemoveAt(0);
		if (_commandTable.TryGetValue(text.ToLower(), out var value))
		{
			try
			{
				value.callback(this, command, list);
				_needAutoScroll = true;
			}
			catch (Exception ex)
			{
				EchoError("Unhandled Exception occurred");
				string[] array2 = ex.Message.Split('\n');
				string[] array3 = array2;
				foreach (string text2 in array3)
				{
					EchoError(text2);
				}
			}
		}
		else
		{
			Echo("Unknown Command");
		}
		_commandHistory.Add(command);
		while (_commandHistory.Count > 32)
		{
			_commandHistory.RemoveAt(0);
		}
		_commandHistoryIndex = _commandHistory.Count;
	}

	public void Echo(string text)
	{
		Echo(DebugCommandMessage.Standard, text);
	}

	public void Echo(DebugCommandMessage messageType, string text)
	{
		_lines.Enqueue(text);
		while (_lines.Count >= 1024)
		{
			_lines.Dequeue();
		}
		foreach (IDebugEchoListner listener in _listeners)
		{
			listener.Echo(messageType, text);
		}
	}

	public void EchoWarning(string text)
	{
		Echo(DebugCommandMessage.Warning, text);
	}

	public void EchoError(string text)
	{
		Echo(DebugCommandMessage.Error, text);
	}

	public void RegisterEchoListner(IDebugEchoListner listner)
	{
		_listeners.Add(listner);
	}

	public void UnregisterEchoListner(IDebugEchoListner listner)
	{
		_listeners.Remove(listner);
	}

	public void PushExecutioner(IDebugCommandExecutioner executioner)
	{
		_executioners.Push(executioner);
	}

	public void PopExecutioner()
	{
		_executioners.Pop();
	}
}
