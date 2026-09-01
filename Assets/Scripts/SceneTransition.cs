using Godot;
using System;
using static GameManager;

[GlobalClass]
public partial class SceneTransition : Area2D
{
	[Export(PropertyHint.File)]
	private string scene_path;

	/// <summary>
	/// String key matches this scene transition with a spawn point in another scene
	/// </summary>
	[Export]
	private string key;

	// (fiend) make the scene transition not active for the first bit to prevent players spawning into it from transitioning (:flushed:)
	private bool active = false;
	// How long to wait before either becoming active or accepting that the player has spawned into the zone
	private double safety_timer = 0.15;

	private bool player_inside = false;


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		// Only wait for the player if they don't already exist
		await GetManager().WaitForPlayerReady();

		if (!OverlapsBody(player))
		{
			GD.Print("Not Overlapping");
			//active = true;
		}
		else
		{
			GD.Print("Overlapping");
		}

		// If the player's spawn point is inside this box, make sure not to activate.
		//Godot.Collections.Array<Node2D> overlapping = GetOverlappingBodies();

		// TODO: Maybe check if the specific spawnpoint is going to be used or 
		//foreach (var item in overlapping)
		//{
		//	if ( item.IsClass("PlayerSpawnPoint") )
		//	{
		//		player_inside = true;
		//		break;
		//	}
		//}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if (!active && safety_timer > 0)
		//{
		//	safety_timer -= delta;
		//}
		//else
		//{
		//	active = true;
		//}

	}

	/// <summary>
	/// Triggered on "BodyEntered" signal
	/// </summary>
	/// <param name="body"></param>
	private void OnBodyEntered(Node2D body)
	{
		GD.Print("OnBodyEntered");
		if (active && /*!player_inside && */ body.GetType() == typeof(HazManPlayer) )
		{
			player_inside = true;
			GetManager().ChangeScene(scene_path, key);
		}
	}

	/// <summary>
	/// Triggered on "BodyExited" signal
	/// </summary>
	/// <param name="body"></param>
	private void OnBodyExited(Node2D body)
	{
		GD.Print("OnBodyExited");
		if (!active && body.GetType() == typeof(HazManPlayer))
		{
			active = true;
		}
	}
}
