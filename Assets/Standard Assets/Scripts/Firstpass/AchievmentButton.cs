using System;
using System.Runtime.CompilerServices;

public class AchievmentButton : NewGuiButtonMk1NoInit
{
	[method: MethodImpl((MethodImplOptions)32)]
	public event Action Click;

	public override void Entered()
	{
		base.Entered();
		if (this.Click != null)
		{
			this.Click();
		}
	}
}
