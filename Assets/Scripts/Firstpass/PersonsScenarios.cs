using System;
using System.Collections.Generic;
using System.IO;
using Scenarios.Parser;
using Scenarios.TestEvaluator;
using UnityEngine;

internal class PersonsScenarios : SingletonT<PersonsScenarios>
{
	internal class Context : TestEvaluator.Context
	{
		internal readonly Person Person;

		internal readonly Dictionary<string, bool> Randoms = new Dictionary<string, bool>();

		public readonly ReactE Reaction;

		public readonly Battle Battle;

		public readonly int Hp;

		public readonly DamageTypeE DamageType;

		public readonly AttackE AttackType;

		public override string Stacktrace => ((!(Person != null)) ? string.Empty : (Person.name + ":: ")) + base.Stacktrace;

		internal Person Opponent => (!Globals.Player.Equals(Person)) ? ((Person)Globals.Player) : ((Person)Globals.Enemy);

		internal Context(string id, string scenario, Battle battle, Person person, AttackE attackType, ReactE react, int hp, DamageTypeE damageType)
			: base(id, scenario)
		{
			Hp = hp;
			Person = person;
			Reaction = react;
			Battle = battle;
			DamageType = damageType;
			AttackType = attackType;
		}

		internal Context(string id, string scenario, Context context, Person person)
			: base(id, scenario)
		{
			_prevContext = context;
			Hp = context.Hp;
			Person = person;
			Reaction = context.Reaction;
			Battle = context.Battle;
			DamageType = context.DamageType;
			AttackType = context.AttackType;
		}
	}

	public KeyValuePair<int, int>[] FightPair = new KeyValuePair<int, int>[2]
	{
		default(KeyValuePair<int, int>),
		default(KeyValuePair<int, int>)
	};

	private TestEvaluator _globalsEvaluator;

	private Dictionary<string, TestEvaluator> _personsEvaluators = new Dictionary<string, TestEvaluator>();

	public PersonsScenarios()
	{
		_globalsEvaluator = LoadEvaluator_("characters/global.scenarios", null);
		Invs.Inv(_globalsEvaluator != null, "failed load characters/global.scenarios");
	}

	public TestEvaluator LoadEvaluator(string index)
	{
		return LoadEvaluator_("characters/" + index + "/" + index + ".scenarios", _globalsEvaluator);
	}

	private TestEvaluator LoadEvaluator_(string path, TestEvaluator parent)
	{
		TestEvaluator evaluator = null;
		if (_personsEvaluators.TryGetValue(path, out evaluator))
		{
			return evaluator;
		}
		evaluator = new TestEvaluator(path, parent);
		_personsEvaluators.Add(path, evaluator);
		FillEvaluator(evaluator);
		SingletonT<ResourcesManager>.I.LoadText(path, delegate(TextReader tr)
		{
			ScenariosScanner scenariosScanner = new ScenariosScanner();
			ScenariosParser scenariosParser = new ScenariosParser(evaluator);
			IEnumerator<Token> ts = scenariosScanner.Scan(tr);
			Script script = scenariosParser.Parse(ts);
			evaluator.AddScript(script);
		});
		return evaluator;
	}

	private static void AddCommand(TestEvaluator evaluator, string commandName, string methodName, params object[] defaults)
	{
		evaluator.AddCommand(commandName, Delegate.CreateDelegate(typeof(PersonsScenarios), null, methodName), defaults);
	}

	private void FillEvaluator(TestEvaluator evaluator)
	{
		evaluator.AddCommand("ifweapon", delegate(Context context, int hands, int sword, int sword2h, int hammer, int glaive)
		{
			AnimationTypes currentWeaponType = context.Person.CurrentWeaponType;
			if (hands == 1 && (currentWeaponType == AnimationTypes.Hands || currentWeaponType == AnimationTypes.None))
			{
				return true;
			}
			if (sword == 1 && currentWeaponType == AnimationTypes.OneHanded)
			{
				return true;
			}
			if (sword2h == 1 && currentWeaponType == AnimationTypes.TwoHanded)
			{
				return true;
			}
			if (hammer == 1 && currentWeaponType == AnimationTypes.Hammer)
			{
				return true;
			}
			return (glaive == 1 && currentWeaponType == AnimationTypes.Glaive) ? true : false;
		}, null, 0, 0, 0, 0, 0);
		evaluator.AddCommand("ifnotweapon", delegate(Context context, int hands, int sword, int sword2h, int hammer, int glaive)
		{
			AnimationTypes currentWeaponType = context.Person.CurrentWeaponType;
			if (hands != 0 && (currentWeaponType == AnimationTypes.Hands || currentWeaponType == AnimationTypes.None))
			{
				return false;
			}
			if (sword != 0 && currentWeaponType == AnimationTypes.OneHanded)
			{
				return false;
			}
			if (sword2h != 0 && currentWeaponType == AnimationTypes.TwoHanded)
			{
				return false;
			}
			if (hammer != 0 && currentWeaponType == AnimationTypes.Hammer)
			{
				return false;
			}
			return (glaive == 0 || currentWeaponType != AnimationTypes.Glaive) ? true : false;
		}, null, 0, 0, 0, 0, 0);
		evaluator.AddCommand("ifnotreact", delegate(Context context, int block, int dodge, int dmg, int powerdmg, int death, int powerdeath)
		{
			ReactE reaction = context.Reaction;
			if (block != 0 && reaction.HasFlag(ReactE.Block))
			{
				return false;
			}
			if (dodge != 0 && reaction.HasFlag(ReactE.Dodge))
			{
				return false;
			}
			if (powerdeath != 0 && reaction.HasFlag(ReactE.Death) && reaction.HasFlag(ReactE.Critical) && !reaction.HasFlag(ReactE.Fatality))
			{
				return false;
			}
			if (death != 0 && reaction.HasFlag(ReactE.Death))
			{
				return false;
			}
			if (powerdmg != 0 && reaction.HasFlag(ReactE.Critical) && reaction.HasFlag(ReactE.Damage))
			{
				return false;
			}
			return (dmg == 0 || !reaction.HasFlag(ReactE.Damage)) ? true : false;
		}, null, 0, 0, 0, 0, 0, 0);
		evaluator.AddCommand("ifreact", delegate(Context context, int block, int dodge, int dmg, int powerdmg, int death, int powerdeath)
		{
			ReactE reaction = context.Reaction;
			if (block == 0 && reaction.HasFlag(ReactE.Block))
			{
				return false;
			}
			if (dodge == 0 && reaction.HasFlag(ReactE.Dodge))
			{
				return false;
			}
			if (powerdeath == 0 && reaction.HasFlag(ReactE.Death) && reaction.HasFlag(ReactE.Critical) && !reaction.HasFlag(ReactE.Fatality))
			{
				return false;
			}
			if (death == 0 && reaction.HasFlag(ReactE.Death))
			{
				return false;
			}
			if (powerdmg == 0 && reaction.HasFlag(ReactE.Critical) && reaction.HasFlag(ReactE.Damage))
			{
				return false;
			}
			return (dmg != 0 || !reaction.HasFlag(ReactE.Damage)) ? true : false;
		}, null, 0, 0, 0, 0, 0, 0);
		evaluator.AddCommand("random", delegate(Context context, int executeIfLess, string name, int ignore)
		{
			if (ignore != 1 && !context.Randoms.ContainsKey(name))
			{
				int num = UnityEngine.Random.Range(0, 99);
				if (num < executeIfLess)
				{
					if (name != "0")
					{
						context.Randoms.Add(name, value: true);
					}
					return true;
				}
			}
			return false;
		}, null, 0, "0", 0);
		evaluator.AddCommand("sound", delegate(Context context, string soundName, float delay, string posName, int isParent, float offsetX, float offsetY, float offsetZ)
		{
			SingletonT<SoundManager>.I.PlaySound(context.Person, soundName, delay, posName, isParent != 0, new Vector3(offsetX, offsetY, offsetZ));
		}, null, null, 0f, "bones", 0, 0f, 0f, 0f);
		evaluator.AddCommand("play", delegate(Context context, string animName, float wait)
		{
			float num = context.Person.PlayAnim(animName, wait);
			if (num > 0f)
			{
				context.WaitTime = num;
			}
		}, null, null, 0f);
		evaluator.AddCommand("enemyplay", delegate(Context context, string animName, float wait)
		{
			float num = context.Opponent.PlayAnim(animName, wait);
			if (num > 0f)
			{
				context.WaitTime = num;
			}
		}, null, null, 0f);
		evaluator.AddCommand("playpart", delegate(Context context, string animName, float speed, int start, int end)
		{
			float num = context.Person.PlayAnimPart(animName, speed, start, end);
			if (num > 0f)
			{
				context.WaitTime = num;
			}
		}, null, null, 0f, 0, 0);
		evaluator.AddCommand<Context>("endmovecam", delegate
		{
			if (!Globals.NoCameraMoveTo)
			{
				Globals.Battle.BattleCameraController.SendMessage("EndMoveTo");
			}
		}, new object[0]);
		evaluator.AddCommand<Context, string, string, float, float, float, float, float>("movecam", delegate
		{
			if (!Globals.NoCameraMoveTo && !(Globals.Player == null) && !(Globals.Enemy == null))
			{
				Utils.Log("PERSON SCENARIO MOVECAM");
				if (!(Globals.Player == null) && !(Globals.Enemy == null))
				{
				}
			}
		}, new object[8] { "center", "center", 1f, 1f, 0f, 0f, 0f, 0f });
		evaluator.AddCommand("scenario", delegate(Context context, string name, int dontWait)
		{
			if (name == "global_fatality_meat_part_2")
			{
				Globals.Battle.BattleCameraController.Shake();
			}
			return new Context(context.ScenarioName, name, context, context.Person);
		}, null, null, 0);
		evaluator.AddCommand("enemyscenario", (Context context, string name) => new Context(context.ScenarioName, name, context, context.Opponent)
		{
			Evaluator = context.Opponent.ScenariosEvaluator
		}, null, string.Empty);
		evaluator.AddCommand("enemyreact", delegate(Context context)
		{
			context.Opponent.react(context.AttackType, context.Reaction, context.DamageType, context.Hp, null, 0);
		}, null);
		evaluator.AddCommand("enemyreactfx", delegate(Context context)
		{
			context.Opponent.reactfx(context.AttackType, context.Reaction, context.DamageType, 0, 0);
		}, null);
		evaluator.AddCommand("reactfx", delegate(Context context)
		{
			context.Person.reactfx(context.AttackType, context.Reaction, context.DamageType, context.Hp, 0);
		}, null);
		evaluator.AddCommand("react", delegate(Context context, string dir, int wait)
		{
			context.WaitTime = context.Person.react(context.AttackType, context.Reaction, context.DamageType, context.Hp, dir, wait);
		}, null, null, 0);
		evaluator.AddCommand("wait", delegate(Context context, float wait)
		{
			context.WaitTime = wait;
		});
		evaluator.AddCommand("enemyfx", delegate(Context context, string id, string fxName, string posName, int isParent, float destroyTime)
		{
			context.Opponent.Fx(id, fxName, posName, isParent == 1, destroyTime, context.TopContext.ScenarioName);
		}, null, "0", "fx_default", "bones", 0, 0f);
		evaluator.AddCommand("hide", delegate(Context context)
		{
			context.Person.hide();
		});
		evaluator.AddCommand("enemyhide", delegate(Context context)
		{
			if (context.Opponent == Globals.Enemy && Globals.Battle.State == Battle.StateE.FatalityModeExecute)
			{
				Globals.Enemy.Die();
			}
			context.Opponent.hide();
		});
		evaluator.AddCommand("fx", delegate(Context context, string id, string fxName, string posName, int isParent, float destroyTime)
		{
			context.Person.Fx(id, fxName, posName, isParent == 1, destroyTime, context.TopContext.ScenarioName);
		}, null, "0", "fx_default", "bones", 0, 0f);
		evaluator.AddCommand<Context, string>("destroy", delegate
		{
		}, new object[2] { null, "bones" });
		evaluator.AddCommand("fxdestroy", delegate(Context context, string id, float delayTime)
		{
			context.Person.FxDestroy(id, delayTime);
		}, null, "fx_default", 0f);
		evaluator.AddCommand("fxpos", delegate(Context context, string groupName, float x, float y, float z, int absolute)
		{
			context.Person.FxPos(groupName, new Vector3(x, y, z), absolute == 1);
		}, null, "0", 0f, 0f, 0f, 0);
		evaluator.AddCommand("enemyfxpos", delegate(Context context, string groupName, float x, float y, float z, int absolute)
		{
			context.Opponent.FxPos(groupName, new Vector3(x, y, z), absolute == 1);
		}, null, "0", 0f, 0f, 0f, 0);
		evaluator.AddCommand("fxrot", delegate(Context context, string groupName, float x, float y, float z, int absolute)
		{
			context.Person.FxRot(groupName, new Vector3(x, y, z), absolute == 1);
		}, null, "0", 0f, 0f, 0f, 0);
		evaluator.AddCommand("enemyfxrot", delegate(Context context, string groupName, float x, float y, float z, int absolute)
		{
			context.Opponent.FxRot(groupName, new Vector3(x, y, z), absolute == 1);
		}, null, "0", 0f, 0f, 0f, 0);
		evaluator.AddCommand<Context, int, int, int>("vibration", delegate
		{
		}, new object[4] { null, 0, 0, 0 });
		evaluator.AddCommand("enemysize", delegate(Context context, float size, float time)
		{
			context.Opponent.ScaleForTime(size, time);
		}, null, 0f, 0f);
		evaluator.AddCommand("size", delegate(Context context, float size, float time)
		{
			context.Person.ScaleForTime(size, time);
		}, null, 0f, 0f);
		evaluator.AddCommand("moveto", delegate(Context context, float x, float y, float z, float time)
		{
			context.Person.MoveTo(new Vector3(x, y, z), time);
		}, null, 0f, 0f, 0f, 0f);
		evaluator.AddCommand("light", delegate(Context context, int on, float speed, float wait, float max)
		{
			if (on == 1)
			{
				Globals.Battle.LightOn(speed, wait, max);
			}
			else
			{
				Globals.Battle.LightOff(speed, wait, max);
			}
		}, null, 1, 0f, 0f, 1f);
		evaluator.AddCommand("step", delegate(Context context, int dir, int rotateDir, int animate, float stepSpeed)
		{
			context.Person.step(dir, rotateDir, animate, stepSpeed);
		}, null, 0, 0, 0, 0f);
	}
}
