using System;
using Boo.Lang;
using UnityEngine;

[Serializable]
public class @params : MonoBehaviour
{
	[NonSerialized]
	public static string log = string.Empty;

	[NonSerialized]
	public static string VERSION = "Version test-110722t-2";

	[NonSerialized]
	public static bool DETAILED_LOG = true;

	[NonSerialized]
	public static bool DISPLAY_FPS;

	[NonSerialized]
	public static bool MANEKEN_USE = true;

	[NonSerialized]
	public static bool CONSOLE_VISIBLE;

	[NonSerialized]
	public static bool SOUND_ENABLED = true;

	[NonSerialized]
	public static Color[] last_hair_color;

	[NonSerialized]
	public static Color[] last_face_color;

	[NonSerialized]
	public static Hash scenarios_global_vars = new Hash();

	[NonSerialized]
	public static float opponent_updated_hp = -1f;

	[NonSerialized]
	public static float opponent_hp_stage = -1f;

	[NonSerialized]
	public static bool is_opponent_in_react_dmg = true;

	[NonSerialized]
	public static bool is_msg_finish_battle;

	[NonSerialized]
	public static string url_assets = "file:///j:/release/";

	[NonSerialized]
	public static bool SHOWGUIBATTLE;

	[NonSerialized]
	public static string LANGUAGE = "ru";

	[NonSerialized]
	public static float CHARACTERS_DISTANCE = 1.6f;

	[NonSerialized]
	public static float SCENARIOS_REACTWAIT = 0.5f;

	[NonSerialized]
	public static float GHOST_TIMEOUT = 1.5f;

	[NonSerialized]
	public static float FX_TIMEOUT = 15f;

	[NonSerialized]
	public static int ARMOR_MAXSET = 10;

	[NonSerialized]
	public static int CAMERA_MODE;

	[NonSerialized]
	public static int ASSETS_CACHE_SIZE = 157286400;

	[NonSerialized]
	public static bool ADDITIONAL_ANIMATIONS = true;

	[NonSerialized]
	public static bool LOAD_ARMOR = true;

	[NonSerialized]
	public static bool LOAD_LIGHT_ARENA;

	[NonSerialized]
	public static int REACT_DODGE = 1;

	[NonSerialized]
	public static int REACT_DMG = 2;

	[NonSerialized]
	public static int REACT_CRIT = 4;

	[NonSerialized]
	public static int REACT_DEATH = 8;

	[NonSerialized]
	public static int REACT_BLOCK = 16;

	[NonSerialized]
	public static int REACT_HEAL = 32;

	[NonSerialized]
	public static string SS_UNITY_LOG = "log";

	[NonSerialized]
	public static bool SS_SEND_FINISH_BATTLE = true;

	[NonSerialized]
	public static bool SS_FINISH_BATTLE_SHOW_STAT = true;

	[NonSerialized]
	public static bool LIGHTMODE;

	[NonSerialized]
	public static string UR_MOUSE_XY = "500";

	[NonSerialized]
	public static string UR_MOUSE_UP = "501";

	[NonSerialized]
	public static string UR_MOUSE_OVER = "502";

	[NonSerialized]
	public static string UR_MOUSE_OUT = "503";

	[NonSerialized]
	public static string UR_MOUSE_DOWN = "504";

	[NonSerialized]
	public static string UR_KEYDOWN = "505";

	[NonSerialized]
	public static string UR_KEYUP = "506";

	[NonSerialized]
	public static string UR_SHAKE = "510";

	[NonSerialized]
	public static string UR_READY = "520";

	[NonSerialized]
	public static string UR_LOAD_COMPLETE = "551";

	[NonSerialized]
	public static string UR_LOG = "560";

	[NonSerialized]
	public static string UR_ACTION = "701";

	[NonSerialized]
	public static string UR_FINISH_BATTLE = "703";

	[NonSerialized]
	public static string UR_FINISH_BATTLE_SKIP_STATISTIC = "706";

	[NonSerialized]
	public static string UR_PROGRESS = "800";

	[NonSerialized]
	public static string UR_DIALOG_HANDLER = "801";

	[NonSerialized]
	public static string UR_ART_INFO = "704";

	[NonSerialized]
	public static string UR_TOBUFFER = "714";

	[NonSerialized]
	public static string UR_LEAVE_BATTLE = "702";

	[NonSerialized]
	public static string UR_BACK_STAB = "705";

	[NonSerialized]
	public static string UR_FURY = "716";

	[NonSerialized]
	public static string US_GRAPHICS_QUALITY = "0";

	[NonSerialized]
	public static string US_DISPLAY_FPS = "1";

	[NonSerialized]
	public static string US_UNITY_UNLOAD = "6";

	[NonSerialized]
	public static string US_UNITY_RESTART = "7";

	[NonSerialized]
	public static string US_QUIT_GAME = "8";

	[NonSerialized]
	public static string US_CAMERA_MODE = "10";

	[NonSerialized]
	public static string US_START_DRAG = "505";

	[NonSerialized]
	public static string US_GRAPHICS_MODE = "13";

	[NonSerialized]
	public static string US_LANGUAGE = "11";

	[NonSerialized]
	public static string US_ADDITION_ANIMATION = "14";

	[NonSerialized]
	public static string US_GAME_SPEED = "15";

	[NonSerialized]
	public static string US_LOAD_ARMOR = "16";

	[NonSerialized]
	public static string US_LIGHT_ARENA = "17";

	[NonSerialized]
	public static string US_DESTROY_ALL = "18";

	[NonSerialized]
	public static string US_ADD_EFF_ICON = "508";

	[NonSerialized]
	public static string US_REMOVE_EFF_ICON = "509";

	[NonSerialized]
	public static string US_GAME_URL = "5";

	[NonSerialized]
	public static string US_SOUND_VOLUME = "12";

	[NonSerialized]
	public static string US_LOAD_DEMO = "35";

	[NonSerialized]
	public static string US_LOAD_USERINFO = "40";

	[NonSerialized]
	public static string US_LOAD_ATELYE = "41";

	[NonSerialized]
	public static string US_CHANGE_ATELYE_PART = "42";

	[NonSerialized]
	public static string US_LOAD_BATTLE = "50";

	[NonSerialized]
	public static string US_LOAD_CHAR = "100";

	[NonSerialized]
	public static string US_CHAR_PARAMS = "101";

	[NonSerialized]
	public static string US_CHAR_SCENARIO = "150";

	[NonSerialized]
	public static string US_PET_SCENARIO = "155";

	[NonSerialized]
	public static string US_CHAR_HP = "151";

	[NonSerialized]
	public static string US_CHAR_RESURECT = "152";

	[NonSerialized]
	public static string US_FURY = "615";

	[NonSerialized]
	public static string US_ACTION = "601";

	[NonSerialized]
	public static string US_ACTION_NEXT = "613";

	[NonSerialized]
	public static string US_WAIT_OPPONENT = "602";

	[NonSerialized]
	public static string US_WAIT_BATTLE = "612";

	[NonSerialized]
	public static string US_FINISH_BATTLE = "603";

	[NonSerialized]
	public static string US_SHOW_MESSAGE = "604";

	[NonSerialized]
	public static string US_TIMEOUT = "605";

	[NonSerialized]
	public static string US_OPPONENT_TIMEOUT = "606";

	[NonSerialized]
	public static string US_DEFENSE_TIMEOUT = "607";

	[NonSerialized]
	public static string US_SHOW_DIALOG = "610";

	[NonSerialized]
	public static string US_SHOW_TEXT = "611";

	[NonSerialized]
	public static string US_BACK_STAB = "614";

	[NonSerialized]
	public static string US_DAMAGE = "666";

	[NonSerialized]
	public static string US_BG_END = "668";

	[NonSerialized]
	public static string US_UPDATE_HP = "650";

	[NonSerialized]
	public static string US_UPDATE_OPPONENT_HP = "651";

	[NonSerialized]
	public static string US_BERSERK = "616";

	[NonSerialized]
	public static string US_BAF = "617";

	[NonSerialized]
	public static string US_ANIMDATA = "618";

	[NonSerialized]
	public static string US_TRANSFORM = "619";

	[NonSerialized]
	public static string UCB_LOAD_CHAR = "300";

	[NonSerialized]
	public static string UCB_UNLOAD_CHAR = "301";

	[NonSerialized]
	public static string UCB_UPDATE_STATS = "302";

	[NonSerialized]
	public static string UCB_FINISH_BATTLE = "303";

	[NonSerialized]
	public static string UCB_DISPLAY_ACTION = "304";

	[NonSerialized]
	public static string UCB_HIDE_ACTION = "306";

	[NonSerialized]
	public static string UCB_WAIT_OPPONENT = "305";

	public static void is_opponent_in_react_dmg_set(string s, bool v)
	{
		is_opponent_in_react_dmg = v;
	}

	public static void opponent_updated_hp_set(string s, float v)
	{
		opponent_updated_hp = v;
	}

	public virtual void Main()
	{
	}
}
