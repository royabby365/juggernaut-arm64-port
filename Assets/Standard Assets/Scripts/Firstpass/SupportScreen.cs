using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class SupportScreen : MonoBehaviour
{
	private static string PROD_SUPPORT_URL = Globals.MainServerUrl + "support.php";

	private static string DEV_SUPPORT_URL = "http://jugger.projects.nikelgames.com/dump.php";

	private static string EMAIL_REGEX = "[A-Z0-9a-z._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,4}";

	private CompositeDisposable _subscriptions;

	public Transform Placeholder;

	public GUIStyle TextStyle;

	public GUIStyle EditStyle;

	public Rect GuiRect;

	private bool _showing;

	private string _email;

	private string _subject;

	private string _message;

	private string _emailLabel;

	private string _subjectLabel;

	private string _errorLabel;

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<string>.AddListener(Globals.MsgGuiButtonPressed, ProcessButtons));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToBefore, OnMsgGuiSwitchToBefore));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
		if (@new == GuiRoot.GuiType.SupportPopup)
		{
			ResetFormData();
			_showing = true;
		}
	}

	private void ResetFormData()
	{
		_emailLabel = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.supportXCodeEMail);
		_subjectLabel = SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.supportXCodeTheme);
		_errorLabel = string.Empty;
		_message = string.Empty;
		_email = string.Empty;
		_subject = string.Empty;
	}

	private void Start()
	{
		ResetFormData();
		GuiRect.x = (float)(Screen.width / 2) - GuiRect.width / 2f;
		GuiRect.y = (float)(Screen.height / 2) - GuiRect.height / 2f + 100f;
	}

	private void ProcessButtons(string buttonName)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.SupportPopup)
		{
			return;
		}
		switch (buttonName)
		{
		case "button_options_support_send":
			_errorLabel = ValidateForm();
			if (!string.IsNullOrEmpty(_errorLabel))
			{
				Debug.Log($"Support form is not valid: {_errorLabel}");
				return;
			}
			SendRequest();
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Options);
			break;
		case "button_options_support_cancel":
			HudMk1.Instance.ChangeGuiTo(GuiRoot.GuiType.Options);
			break;
		}
		_showing = false;
	}

	private string ValidateForm()
	{
		if (string.IsNullOrEmpty(_email))
		{
			return SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.supportXCodeEnterEmail);
		}
		if (!_email.IsMatch(EMAIL_REGEX))
		{
			return SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.supportXCodeInvEmail);
		}
		if (string.IsNullOrEmpty(_subject))
		{
			return SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.supportXCodeEnterSubject);
		}
		if (string.IsNullOrEmpty(_message))
		{
			return SingletonT<ServerData>.I.GetPhrase(ServerData.PhrasesE.supportXCodeEnterDesc);
		}
		return string.Empty;
	}

	private string SafeString(string s)
	{
		return (!string.IsNullOrEmpty(s)) ? s : string.Empty;
	}

	private void SendRequest()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("akey", SafeString(UniqueData.BundleIdentifier));
		dictionary.Add("uid", SafeString(UniqueData.GetCurrentSlotUUID()));
		dictionary.Add("s", SafeString(UniqueData.SlotId));
		dictionary.Add("did", string.Empty);
		dictionary.Add("maddr", SafeString(UniqueData.MacAdressGlobalUniqueIdentifier));
		dictionary.Add("appv", $"{SafeString(UniqueData.GameVersion)}:{UniqueData.BuildVersion}");
		dictionary.Add("ndt", SafeString(SystemInfo.deviceModel));
		dictionary.Add("osv", SafeString(SystemInfo.operatingSystem));
		dictionary.Add("vc", SafeString(UnityApi.GetPackageVersionCode().ToString()));
		dictionary.Add("umail", _email);
		dictionary.Add("subj", _subject);
		dictionary.Add("body", _message);
		string pROD_SUPPORT_URL = PROD_SUPPORT_URL;
		Utils.Log("Posting support request to: ", pROD_SUPPORT_URL);
		string text = string.Empty;
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			string text2 = text;
			text = text2 + item.Key + " => " + item.Value + "\n";
		}
		Utils.Log("Posting support request params:\n", text);
		StartCoroutine(Utils.WWWLoadForm(pROD_SUPPORT_URL, 100f, delegate
		{
			Utils.Log("Support request sent successfully", string.Empty);
		}, delegate(string _, string __)
		{
			Utils.Log("Failed to send support request: ", __);
		}, dictionary));
	}

	private void OnGUI()
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.SupportPopup && _showing)
		{
			GUI.skin.textField.font = EditStyle.font;
			GUI.skin.textArea.font = EditStyle.font;
			GUILayout.BeginArea(GuiRect);
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label(_emailLabel, TextStyle, GUILayout.Width(100f));
			_email = GUILayout.TextField(_email);
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			GUILayout.Label(_subjectLabel, TextStyle, GUILayout.Width(100f));
			_subject = GUILayout.TextField(_subject);
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			_message = GUILayout.TextArea(_message, GUILayout.Height(100f));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(_errorLabel, TextStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
