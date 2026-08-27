using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class HurtBox2d : Area2D
{
    [Export]
    public bool is_invincible = false;
    [Export]
    public float col_x = 20f;
    [Export]
    public float col_y = 20f;
    [Export]
    public NodePath col_shape_path = null;
    [Signal]
    public delegate void HitRecievedEventHandler(DamageBox hitbox);

    private CollisionShape2D collisionShape;

    public override void _Ready()
    {
        if (col_shape_path != null)
        {
            collisionShape = GetNode<CollisionShape2D>(col_shape_path);
            if (collisionShape != null)
            {
                if(col_x <= 0f)
                {
                    col_x = 20f;
                }
                if (col_y <= 0f)
                {
                    col_y = 20f;
                }
                RectangleShape2D replacement = new RectangleShape2D();
                replacement.Size = new Vector2(col_x, col_y);
                collisionShape.Shape = replacement;
               }
        }
    }

    public void take_hit(DamageBox hitbox)
    {
        if (!is_invincible)
        {
            EmitSignalHitRecieved(hitbox);
            //GD.Print("Took Hit");
            is_invincible = true;
        }
    }

    private void _on_haz_man_player_inv_timer_timeup()
    {
        is_invincible = false;
    }
    
}
