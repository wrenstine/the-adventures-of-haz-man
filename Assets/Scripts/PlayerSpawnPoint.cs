using Godot;
using System;

[GlobalClass]
public partial class PlayerSpawnPoint : Node2D
{
	/// <summary>
	/// String key matches this scene transition with a spawn point in another scene
	/// </summary>
	[Export(PropertyHint.TypeString)]
	private string key;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
