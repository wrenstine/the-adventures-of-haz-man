using Godot;
using System;
using System.ComponentModel;

enum haz_state
{
	IDLE,
	TALKING,
	CHARGING,
	PUNCHING,
	MOBILE,
	TRAPPED,
	STALL,
	DEAD
}
public partial class HazManPlayer : CharacterBody2D
{
	Vector2 tile_size = new Vector2(16,16);
	Tween sprite_node_pos_tween = null;

	int HP = 3;
	haz_state state = haz_state.IDLE;

	// Node References
	[Export]
	NodePath up_path = null;
	[Export]
	NodePath down_path = null;
	[Export]
	NodePath left_path = null;
	[Export]
	NodePath right_path = null;
	[Export]
	NodePath anim_pos_path = null;
	[Export]
	NodePath punch_path = null;
	[Export]
	NodePath timer_path = null;

	[Signal]
	public delegate void InvTimerTimeupEventHandler();

	private RayCast2D up = null;
	private RayCast2D down = null;
	private RayCast2D left = null;
	private RayCast2D right = null;
	private Node2D anim_pos = null;
	private Fist punch = null;

	bool all_right = true;
	bool punched = false;

	public override void _Ready()
	{
		base._Ready();
		up = GetNode<RayCast2D>(up_path);
		if (up == null)
			all_right = false;
		down = GetNode<RayCast2D>(down_path);
		if (down == null)
			all_right = false;
		left = GetNode<RayCast2D>(left_path);
		if (left == null)
			all_right = false;
		right = GetNode<RayCast2D>(right_path);
		if (right == null)
			all_right = false;
		anim_pos = GetNode<Node2D>(anim_pos_path);
		if (anim_pos == null)
			all_right = false;
		punch = GetNode<Fist>(punch_path);
		if (punch == null)
		{
			all_right = false;
		}
		else
		{
			Timer timer = GetNode<Timer>(timer_path);
			timer.Timeout += OnTimerTimeout;
		}
		state = haz_state.STALL;
        Timer s_timer = (Timer)FindChild("StallTimer");
		s_timer.Start();
		GD.Print(state);
        //sprite_node_pos_tween = GetTree().CreateTween();
    }

	public override void _PhysicsProcess(double delta)
	{
		// direction vector
		Vector2 dir = new Vector2(0, 0);

		// Inputs and stuff
		if (state != haz_state.PUNCHING)
			state = haz_state.IDLE;
		if (all_right && ((sprite_node_pos_tween == null) || !sprite_node_pos_tween.IsRunning()) && (state == haz_state.IDLE || state == haz_state.MOBILE || state == haz_state.CHARGING))
		{
			if (Input.IsActionPressed("space"))
			{
				if (state != haz_state.CHARGING)
				{
					state = haz_state.CHARGING;

				}
			}
			else if (all_right)
			{
				//state = haz_state.IDLE;

				if (Input.IsActionPressed("up") && !up.IsColliding())
				{
					state = haz_state.MOBILE;
					dir = new Vector2(0, -1);
				}
				if (Input.IsActionPressed("down") && !down.IsColliding())
				{
					state = haz_state.MOBILE;
					dir = new Vector2(0, 1);
				}
				if (Input.IsActionPressed("left") && !left.IsColliding())
				{
					state = haz_state.MOBILE;
					dir = new Vector2(-1, 0);
				}
				if (Input.IsActionPressed("right") && !right.IsColliding())
				{
					state = haz_state.MOBILE;
					dir = new Vector2(1, 0);
				}
				if (Input.IsActionJustReleased("space"))
				{
					// play animation idk
					state = haz_state.PUNCHING;
				}
			}
		}
		// ======================================================================================== //
		// THIS IS TO STOP CLIPPING BETWEEN SCENE TRANSITIONS	// FIEND FEEL FREE TO EDIT THIS PART
		else if (state == haz_state.STALL) {
			Timer timer = (Timer)FindChild("StallTimer");
            if (Input.IsActionJustPressed("up") && !up.IsColliding())
            {
				EndTimerEarly(timer);
                state = haz_state.MOBILE;
                dir = new Vector2(0, -1);
            }
            if (Input.IsActionJustPressed("down") && !down.IsColliding())
            {
                EndTimerEarly(timer);
                state = haz_state.MOBILE;
                dir = new Vector2(0, 1);
            }
            if (Input.IsActionJustPressed("left") && !left.IsColliding())
            {
                EndTimerEarly(timer);
                state = haz_state.MOBILE;
                dir = new Vector2(-1, 0);
            }
            if (Input.IsActionJustPressed("right") && !right.IsColliding())
            {
                EndTimerEarly(timer);
                state = haz_state.MOBILE;
                dir = new Vector2(1, 0);
            }
        }

			// state machine and processing
			//GD.Print(state);
			switch (state)
			{
				case haz_state.IDLE:

					break;
				case haz_state.TALKING:

					break;
				case haz_state.PUNCHING:
					if (!punched)
						_punch(dir);
					punched = true;
					break;
				case haz_state.MOBILE:
					_move(dir);
					break;
				case haz_state.DEAD:
					//lol you are dead

					break;
				case haz_state.STALL: 
						// you are stalled until timer ends

					break;
				case haz_state.TRAPPED:
					// get trapped mron no moving

					break; 
				default:

					break;
			}

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="dir"></param>
	private void _punch(Vector2 dir)
	{
		// fuuuuuck why can't the position vector fucking workkkk
		punch.GlobalPosition = GlobalPosition;
		punch.GlobalPosition += ((dir) * tile_size);
		punch._punch();
		GD.Print("Punch");
		// wait for punch to end idk
		//state = haz_state.IDLE;
		
	}

	/// <summary>
	/// This is called when the punch object is done existing, and allows haz-man to move and punch again.
	/// </summary>
	private void _end_punch()
	{
		state = haz_state.IDLE;
		punched = false;
	}

	private void _move(Vector2 dir)
	{
		GlobalPosition += dir * tile_size;
		anim_pos.GlobalPosition -= dir * tile_size;

		if (sprite_node_pos_tween != null)
			sprite_node_pos_tween.Kill();
		sprite_node_pos_tween = CreateTween();
		sprite_node_pos_tween.SetProcessMode(Tween.TweenProcessMode.Physics);
		sprite_node_pos_tween.TweenProperty(anim_pos, "global_position", GlobalPosition, 0.18).SetTrans(Tween.TransitionType.Sine);
	}

	private void OnTimerTimeout()
	{
		_end_punch();
	}

	private void _on_hurt_box_2d_hit_recieved(DamageBox damage)
	{
        //GD.Print("INJURY");

        // calculate damage

        Timer timer = (Timer)FindChild("InvTimer");
		timer.Start();

        GD.Print("Current Health: " + HP);
	}

    private void _on_stall_timer_timeout()
	{
		GD.Print("unstalled");
	}

	private void EndTimerEarly(Timer timer)
	{
		timer.Stop();
	}

    private void OnInvTimerTimeout()
	{
        EmitSignalInvTimerTimeup();
	}

}
