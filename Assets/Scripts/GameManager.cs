using Godot;
using System;

// (fiend) made by referencing this a lot
// https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html
public partial class GameManager : Node
{
	public static Node current_scene { get; set; }
	// (fiend) In their tutorial they don't make this a variable like this, but it shouldn't... change?
	public static Viewport root { get; set; }

	// Current key used by scene transitions to know what spawnpoint to use.
	// Also just helpful to have
	public string curr_transition_key { get; set; }

	public static HazManPlayer player { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		root = GetTree().Root;
		// (fiend) Get the last child node, which apparently is the current scene
		// They do this in their tutorial, but it feels a bit... sketchy to me
		current_scene = root.GetChild(-1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public static GameManager GetManager() => root.GetNode<GameManager>("/root/GameManager");

	public void ChangeScene(string path, string key)
	{
		// (fiend) CallDeferred apparently waits for all the code from the prior scene is done running to load the next scene, to prevent crashes
		CallDeferred(MethodName.DeferredChangeScene, path, key);
	}

	public void ChangeScene(PackedScene scene, string key)
	{
		CallDeferred(MethodName.DeferredChangeScenePacked, scene, key);
	}

	// Load a new scene based on path and change the current scene to it
	public void DeferredChangeScene(string path, string key)
	{
		curr_transition_key = key;

		current_scene.Free();
		var nextScene = GD.Load<PackedScene>(path);
		current_scene = nextScene.Instantiate();
		root.AddChild(current_scene);
	}

	// Change to an already loaded PackedScene
	public void DeferredChangeScenePacked(PackedScene scene, string key)
	{
		curr_transition_key = key;

		current_scene.Free();
		current_scene = scene.Instantiate();
		root.AddChild(current_scene);
	}
}
