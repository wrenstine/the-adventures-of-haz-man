using Godot;
using System;

public partial class Fist : Node2D
{
    [Export]
    int Damage = 0;
    [Export]
    NodePath life_path = null;
    [Export]
    NodePath hitbox_path = null;

    Area2D hitbox = null;
    Timer lifetime = null;
    bool kill_me = false;

    public override void _Ready()
    {
        base._Ready();
        lifetime = GetNode<Timer>(life_path);
        hitbox = GetNode<Area2D>(hitbox_path);
        Hide();
    }

    public void _punch()
    {
        if (lifetime != null)
        {
            lifetime.Start();
            Show();
        }
    }

    private void OnTimerTimeout()
    {
        //QueueFree();
        Hide();
    }

    
}
