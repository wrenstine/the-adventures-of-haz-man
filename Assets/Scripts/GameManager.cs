using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

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

	// Tiny optimization to just keep this stored so we don't need to run GD.Load everytime we need this.
	// Aka on every scene transition.
	private readonly PackedScene player_scene = GD.Load<PackedScene>("res://Assets/Scenes/haz_man_player.tscn");

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		root = GetTree().Root;
		// (fiend) Get the last child node, which apparently is the current scene
		// They do this in their tutorial, but it feels a bit... sketchy to me
		current_scene = root.GetChild(-1);

		// I really hope that by the time we get to this await we're not already past this signal.
		await ToSignal(current_scene, "ready");
		TrySpawnPlayer();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public static GameManager GetManager() => root.GetNode<GameManager>("/root/GameManager");

	public void ChangeScene(string path, string key)
	{
		SceneTransitionStartSceneExit();
		// (fiend) CallDeferred apparently waits for all the code from the prior scene is done running to load the next scene, to prevent crashes
		CallDeferred(MethodName.DeferredChangeScene, path, key);
	}

	public void ChangeScene(PackedScene scene, string key)
	{
		SceneTransitionStartSceneExit();
		CallDeferred(MethodName.DeferredChangeScenePacked, scene, key);
	}

	// Load a new scene based on path and change the current scene to it
	private void DeferredChangeScene(string path, string key)
	{
		current_scene.Free();
		ChangeSceneInternal(GD.Load<PackedScene>(path), key);
	}

	// Change to an already loaded PackedScene
	private void DeferredChangeScenePacked(PackedScene scene, string key)
	{
		current_scene.Free();
		ChangeSceneInternal(scene, key);
	}

	// (fiend) Tbh when I first made this function it also contained basically all of TrySpawnPlayer
	// Now that it doesn't I wonder if its even necessary 
	/// <summary>
	/// Internal func to do some things shared between the 2 change scene funcs
	/// </summary>
	private void ChangeSceneInternal(PackedScene scene, string key)
	{
		curr_transition_key = key;
		current_scene = scene.Instantiate();
		root.AddChild(current_scene);

		TrySpawnPlayer();
		SceneTransitionStartSceneEnter();
	}

	private void TrySpawnPlayer()
	{
		if (IsPlayerValid())
		{
			GD.PushWarning("Attempted to spawn player but GameManager player variable is still valid!");
			return;
		}

		HazManPlayer playerInstance = (HazManPlayer)root.FindChild("HazManPlayer");
		if (playerInstance != null && IsInstanceValid(playerInstance))
		{
			// Using a pre-existing HazMan should prevent scenes that already have the actual player SCENE pre-placed from breaking.
			player = playerInstance;
			GD.Print("HazMan already exists in scene, using them instead of spawning a new one.");
			return;
		}

		if (!GetTree().HasGroup("PlayerSpawns"))
		{
			// (fiend) This is an error because at this point if HazMan is pre-spawned we shouldn't be able to be here
			// FIXME: ^ This actually happens though lmao how
			GD.PushError("Couldn't find any spawn points in the scene we're trying to load!");
			return;
		}

		// (fiend) In theory not everything in this array has to be a PlayerSpawnPoint,
		// but I think if something else gets added to this group we have a bigger problem.
		var spawnPoints = GetTree().GetNodesInGroup("PlayerSpawns");

		PlayerSpawnPoint currSpawn = null;
		// In case we can't find a matching key name, we try and find a default spawn point to use instead
		PlayerSpawnPoint defaultSpawn = null;
		// Ideally this cast should be unnecessary, but maybe worthwhile for safety, just in case.
		foreach (PlayerSpawnPoint spawn in spawnPoints.Cast<PlayerSpawnPoint>())
		{
			if (curr_transition_key != null && spawn.key == curr_transition_key)
			{
				if (currSpawn == null)
				{
					currSpawn = spawn;
				}
				else
				{
					GD.PushWarning($"Found player spawn \'{spawn.Name}\' with matching key \'{spawn.key}\' but we already set currSpawn to \'{currSpawn.Name}\'!! Ignoring!");
				}
			}

			if (spawn.default_spawn)
			{
				if (defaultSpawn == null)
				{
					defaultSpawn = spawn;
				}
				else
				{
					GD.PushWarning($"Found player spawn \'{spawn.Name}\' marked as default, but we already set defaultSpawn to \'{defaultSpawn.Name}\'!! Ignoring!");
				}
			}
		}


		if (currSpawn == null)
		{
			if (defaultSpawn == null)
			{
				if (curr_transition_key == null)
				{
					// (fiend) If our scene transition has no key AND there's no default spawn, FUCKING PANIC and just grab the first one in the list.
					// Maybe really bad if this isn't actually a spawn point? (as mentioned above)
					// Or maybe its (relatively) fine since we probably just use the position of the node anyway.
					defaultSpawn = (PlayerSpawnPoint)spawnPoints[0];
				}
				else
				{
					GD.PushError("Couldn't find a spawn with a matching key or a default spawn?? HazMan's ass is not spawning!");
					return;
				}
			}
			SpawnPlayer(defaultSpawn);
		}
		else
		{
			SpawnPlayer(currSpawn);
		}
	}

	// (fiend) TODO: should this be private or public?
	/// <summary>
	/// Function to spawn HazMan at a specific spawn point
	/// </summary>
	/// <param name="spawn"></param>
	private void SpawnPlayer(PlayerSpawnPoint spawn)
	{
		player = (HazManPlayer)player_scene.Instantiate();
		current_scene.AddChild(player);
		player.Position = spawn.Position;
	}

	/// <summary>
	/// Quick function to check if the player instance is valid.
	/// NOTE: false could mean that the instance is invalid OR that the variable itself is null!
	/// Be sure that you're not trying to use a null pointer beceause it returned false!
	/// </summary>
	/// <returns>Is player instance valid</returns>
	public static bool IsPlayerValid()
	{
		if (player != null)
		{
			return IsInstanceValid(player);
		}
		return false;
	}

	/// <summary>
	/// Function that waits for the global player variable to send its ready signal.
	/// Returns immediately if already ready.
	/// </summary>
	// (fiend) FIXME: I feel like this should be static, but apparently the ToSignal call doesn't work then?
	public async Task WaitForPlayerReady()
	{
		if (!IsPlayerValid())
		{
			// player could be null, so wait for it to not be null first
			await Task.Run(() => { while (player == null) ; });

		}

		if (player.IsNodeReady())
		{
			// If player is already ready, just return.
			return;
		}

		// Now wait for it to be ready
		await ToSignal(player, "ready");
	}

	public async void SceneTransitionStartSceneExit()
	{
		GD.Print("Scene Exit Start");
		GetTree().Paused = true;
		// TODO: Replace this with a fade out animation
		await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
		GD.Print("Scene Exit End");
	}

	public async void SceneTransitionStartSceneEnter()
	{
		GD.Print("Scene Enter Start");
		// TODO: Replace this with a fade in animation
		await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
		GetTree().Paused = false;
		GD.Print("Scene Enter End");
	}
}
