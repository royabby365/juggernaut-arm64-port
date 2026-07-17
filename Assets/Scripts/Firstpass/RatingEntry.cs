using UnityEngine;

public class RatingEntry : MonoBehaviour
{
	public SpriteText Place;

	public Renderer IconRenderer;

	public SpriteText Name;

	public SpriteText Scores;

	public Sprite PlaceBg;

	private static Texture _defaultAvatar;

	private void Awake()
	{
		if (_defaultAvatar == null)
		{
			_defaultAvatar = IconRenderer.sharedMaterial.mainTexture;
		}
	}

	internal void SetScoresInfo(int place, SocialAspect.ScoresInfo info)
	{
		Utils.LogForce("SetScoresInfo {0} {1} {2}".Fmt(place, info.UserName, info.Score));
		Place.Text_ = place.ToString();
		IconRenderer.material.mainTexture = ((!(info.Image != null)) ? _defaultAvatar : info.Image);
		Name.Text_ = info.UserName;
		Scores.Text_ = info.Score.ToString();
		if (info.IsMe)
		{
			Name.NamedColorE_ = FontManager.ColorE.CompareRed;
			Scores.NamedColorE_ = FontManager.ColorE.CompareRed;
			PlaceBg.SpriteName_ = "main_menu_lvl";
		}
		else
		{
			Name.NamedColorE_ = FontManager.ColorE.ButtonGold;
			Scores.NamedColorE_ = FontManager.ColorE.ButtonGold;
			PlaceBg.SpriteName_ = "main_menu_lvl_sepia";
		}
	}
}
