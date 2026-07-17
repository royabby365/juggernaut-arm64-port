using System.Collections.Generic;
using ProtoBuf;
using Yarx.Collections;

namespace Assets.Plugins.GameCode.Data;

[ProtoContract]
public class StorylineDialogProxy
{
	[ProtoMember(1)]
	public int Id;

	[ProtoMember(2)]
	public string Title;

	[ProtoMember(3)]
	public int LocationBot01;

	[ProtoMember(4)]
	public int LocationBot02;

	[ProtoMember(5)]
	public int LocationBot03;

	[ProtoMember(6)]
	public List<ServerData.DialogPhrase> Dialogs { get; set; }

	public static implicit operator ServerData.StorylineDialog(StorylineDialogProxy data)
	{
		ServerData.StorylineDialog storylineDialog = new ServerData.StorylineDialog();
		storylineDialog.Id = data.Id;
		storylineDialog.Title = data.Title;
		storylineDialog.LocationBot = Tuple.Create(data.LocationBot01, data.LocationBot02, data.LocationBot03);
		storylineDialog.Dialogs = data.Dialogs;
		return storylineDialog;
	}

	public static implicit operator StorylineDialogProxy(ServerData.StorylineDialog data)
	{
		StorylineDialogProxy storylineDialogProxy = new StorylineDialogProxy();
		storylineDialogProxy.Id = data.Id;
		storylineDialogProxy.Title = data.Title;
		storylineDialogProxy.LocationBot01 = data.LocationBot.Item1;
		storylineDialogProxy.LocationBot02 = data.LocationBot.Item2;
		storylineDialogProxy.LocationBot03 = data.LocationBot.Item3;
		storylineDialogProxy.Dialogs = data.Dialogs;
		return storylineDialogProxy;
	}
}
