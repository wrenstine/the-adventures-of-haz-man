using Godot;
using System;

public partial class HealthPip : Control
{
	[Export]
	public bool is_on = true;
	[Export]
	NodePath pip_on_path = null;
	[Export]
	NodePath pip_off_path = null;

	private TextureRect pip_on = null;
	private TextureRect pip_off = null;

	public override void _Ready()
	{
		if (pip_on_path != null)
		{
			pip_on = GetNode<TextureRect>(pip_on_path);
		}
		if (pip_off_path != null)
		{
			pip_off = GetNode<TextureRect>(pip_off_path);
			pip_off.Hide();
		}
	}

	public void Toggle()
	{
		if (pip_off != null && pip_on != null)
		{
			is_on = !is_on;
			if (is_on)
			{
				pip_on.Show();
				pip_off.Hide();
			}
			else
			{
				pip_off.Show();
				pip_on.Hide();
			}
		}
	}
}
