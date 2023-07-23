using Godot;
using System;

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

	public void _on_Area2D_area_entered(Area2D area)
	{
		if (area is Tile tile)
		{
			if (area.IsInGroup("Corn"))
			{
				tile.ChangeGroup("Dirt", true);
				tile.Harvest();
				OnCropHarvested?.Invoke(tile);
			}

			if (area.IsInGroup("Weed"))
			{
                GD.Print(tile.Stage);
                tile.Stage--;
                if (tile.Stage <= 0)
                {
                    tile.Stage = 0;
				    tile.ChangeGroup("Dirt", true);
                }
                else
                {
                    tile.ModulateFromGroup(true);
                }

				tile.Cut();
				OnWeedHarvested?.Invoke(tile);
			}
		}
	}

	public void _on_Area2D_body_entered(Node body)
	{
		if (body is IHittable hittable)
		{
			hittable.Hit();
		}

		if (body is BaseAngry angry)
		{
			angry.Health = angry.Health - 1;

			if (angry.Health <= 0)
			{
				OnAngryKilled?.Invoke(angry);
                angry.Kill();
				// body.QueueFree();
			}

			Dead = true;
		}
	}
}

