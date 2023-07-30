using Godot;
using System;
using System.Linq;

public delegate void TileEventHandler(Tile tile);
public delegate void AngryKilledHandler(BaseAngry angry);
public class Scyth : RigidBody2D
{
    private CollisionShape2D BodyCollisionShape;

    public event TileEventHandler OnCropHarvested;
    public event TileEventHandler OnWeedHarvested;
    public event AngryKilledHandler OnAngryKilled;
    [Export] public int Size = 8;
    [Export] public int StartingSize = 8;
    private CollisionShape2D AreaCollisionShape;
    private Sprite ShadowSprite;
    private bool Dead;
    public float LifeTime;
    [Export] public int PiercingLeft = 1;
    private bool Hit;
    private BaseAngry HitAngry;
    [Export] public int HomingLeft { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        BodyCollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        ((CircleShape2D)BodyCollisionShape.Shape).Radius = Size / 2;

        GetNode<Sprite>("Sprite").Scale = Vector2.One * ((float)Size / (float)StartingSize);

        AreaCollisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        ((CircleShape2D)AreaCollisionShape.Shape).Radius = Size / 2;

        ShadowSprite = GetNode<Sprite>("Sprite2");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        ShadowSprite.Rotation = Rotation;
    }

    public override void _PhysicsProcess(float delta)
    {
        LifeTime -= delta;
        if (Dead || LifeTime <= 0f)
        {
            QueueFree();
        }
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        if (HomingLeft > 0 && Hit)
        {

            var angries = GetTree().GetNodesInGroup("AngryPlants").Cast<BaseAngry>();

            var nearest = float.MaxValue;
            BaseAngry nearestAngry = null;
            foreach (var an in angries)
            {
                if (an != HitAngry)
                {
                    var distance = HitAngry.GlobalPosition.DistanceSquaredTo(an.GlobalPosition);
                    if (distance < nearest)
                    {
                        nearestAngry = an;
                        nearest = distance;
                    }
                }
            }
            if (nearestAngry != null)
            {
                LinearVelocity = Vector2.Zero;
                ApplyCentralImpulse((nearestAngry.GlobalPosition - GlobalPosition).Normalized() * 8f);
            }

            Hit = false;
            HomingLeft--;
        }
    }

    public void _on_Area2D_area_entered(Area2D area)
    {
        if (area is Tile tile)
        {
            if (area.IsInGroup("Corn"))
            {
                tile.Harvest();
                OnCropHarvested?.Invoke(tile);
            }

            if (area.IsInGroup("Weed"))
            {
                tile.Cut();
                OnWeedHarvested?.Invoke(tile);
            }
        }
    }

    public void _on_Area2D_body_entered(Node node)
    {
        if (node is BaseAngry angry)
        {
            angry.Health = angry.Health - 1;
            PiercingLeft--;

            Hit = true;
            HitAngry = angry;

            if (angry.Health <= 0)
            {
                Hit = false;
                HitAngry = null;
                OnAngryKilled?.Invoke(angry);
                angry.Kill();
            }

            if (PiercingLeft < 0)
            {
                Hit = false;
                Dead = true;
            }

            if (node is IHittable hittable)
            {
                hittable.Hit(Hit);
            }
        }

        if (node is StaticBody2D body)
        {
            GetNode<AudioStreamPlayer2D>("Thunk").Play();
        }
    }
}

