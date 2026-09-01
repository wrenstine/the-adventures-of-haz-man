using Godot;
using System;

[GlobalClass]
public partial class PlayerSpawnPoint : Node2D
{
	/// <summary>
	/// String key matches this scene transition with a spawn point in another scene
	/// </summary>
	[Export]
	public string key;

	/// <summary>
	/// If the player doesn't have a spawnpoint set, prefer this one
	/// Mostly for debug purposes
	/// </summary>
	[Export]
	public bool default_spawn;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("PlayerSpawns");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
