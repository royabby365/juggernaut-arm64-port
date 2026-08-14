using System;
using System.Runtime.CompilerServices;

public class PauseButton : NewGuiButtonMk1
{
	private bool _click;

	[method: MethodImpl((MethodImplOptions)32)]
	public event Action<PauseButton> Click;

	public override void Released()
	{
		if (this.Click != null && HudMk1.Instance != null)
		{
			if (HudMk1.Instance.IsGuiStable)
			{
				this.Click(this);
			}
			else
			{
				_click = true;
			}
		}
	}

	private void LateUpdate()
	{
		if (_click && HudMk1.Instance != null && HudMk1.Instance.IsGuiStable)
		{
			_click = false;
			this.Click(this);
		}
	}
}
