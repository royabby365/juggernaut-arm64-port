using UnityEngine;
using UnityEngine.SocialPlatforms;

public class UserProfileStub : IUserProfile
{
	private static int _idGenerator;

	public string id { get; private set; }

	public Texture2D image { get; private set; }

	public bool isFriend { get; private set; }

	public UserState state { get; private set; }

	public string userName { get; private set; }

	public UserProfileStub()
	{
		id = (++_idGenerator).ToString();
		image = Util.Resource<Texture2D>("__atlases/a_assorted_big_2_tex");
		isFriend = true;
		state = UserState.Online;
		userName = "user" + id;
	}
}
