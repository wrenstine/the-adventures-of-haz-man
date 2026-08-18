using Godot;
using System;

public partial class DamageBox : Area2D
{
    [Export]
    int damage = 0;
    [Export]
    bool one_shot = false;

    // shape data
    [Export]
    public float col_x = 20f;
    [Export]
    public float col_y = 20f;
    [Export]
    public NodePath col_shape_path = null;

    private bool _has_hit = false;

    [Signal]
    public delegate void HitEventHandler(HurtBox2d hurtbox);

    private CollisionShape2D collisionShape;

    public override void _Ready()
    {
        base._Ready();
        Monitoring = true;
        Monitorable = false;
        AreaEntered += OnAreaEntered;

        if (col_shape_path != null)
        {
            collisionShape = GetNode<CollisionShape2D>(col_shape_path);
            if (collisionShape != null)
            {
                if (col_x <= 0f)
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

    public void reset()
    {
        _has_hit = false;
    }

    private void OnAreaEntered(Area2D area)
    {
        //GD.Print("type hit:" + area.GetType().Name);
        if (area.GetType() == typeof(HurtBox2d))
        {
            HurtBox2d box = area as HurtBox2d;

            bool already_hit = one_shot & _has_hit;

            if (!(already_hit || box.is_invincible))
            {
                _has_hit = true;
                box.take_hit(this);
                EmitSignalHit(box);
            }
        }
    }
}
