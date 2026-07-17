using UnityEngine;
using Yarx;

public class MainMenuSlot : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public int SlotNumber;

	public SpriteButton DeleteButton;

	public Sprite DiamondsBg;

	public Sprite DiamondIco;

	public Sprite GoldBg;

	public Sprite GoldIco;

	public Sprite LevelBg;

	public SpriteText NewGame;

	public SpriteText SaveNumber;

	public SpriteText Level;

	public SpriteText GoldCount;

	public SpriteText DiamondCount;

	public Sprite PersIco;

	public long SaveTime;

	private readonly Color _lightTint = new Color32(200, 200, 200, byte.MaxValue);

	private string _saveText;

	internal bool HasData;

	private void Awake()
	{
		DeleteButton.name += SlotNumber;
		DeleteButton.Init();
		Init(-15, 0);
	}

	private void OnEnable()
	{
		if (_subscriptions == null)
		{
			_subscriptions = new CompositeDisposable();
		}
	}

	private void OnDisable()
	{
		Utils.DisposeAndSetNull(ref _subscriptions);
	}

	private void Start()
	{
	}

	public void ShowHideInfo(bool show)
	{
		NewGame.ShowOrHide(!show);
		if (show)
		{
			DeleteButton.SetActive();
		}
		else
		{
			DeleteButton.SetInactive();
			SaveTime = 0L;
		}
		DeleteButton.ShowOrHide(show);
		DiamondsBg.ShowOrHide(show);
		DiamondIco.ShowOrHide(show);
		GoldBg.ShowOrHide(show);
		GoldIco.ShowOrHide(show);
		LevelBg.ShowOrHide(show);
		SaveNumber.ShowOrHide(show);
		GoldCount.ShowOrHide(show);
		DiamondCount.ShowOrHide(show);
		PersIco.ShowOrHide(show);
		Level.ShowOrHide(show);
		HasData = show;
	}

	public void SetInfo(long saveTime, string persIco, int lvl, int gold, int diamonds, string persname)
	{
		HasData = true;
		SaveTime = saveTime;
		ShowHideInfo(show: true);
		GoldCount.Text_ = gold.ToString();
		DiamondCount.Text_ = diamonds.ToString();
		Level.Text_ = lvl.ToString();
		PersIco.SpriteName_ = persIco;
		SaveNumber.Text_ = persname;
	}

	public override void Entered()
	{
		base.Entered();
		SetColor(_lightTint);
	}

	public override void Left()
	{
		base.Left();
		SetColor(Color.gray);
	}
}
