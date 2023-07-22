using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Player : KinematicBody2D
{
    [Export] public float ShurikenLifeTime = 1.0f;
    [Export] public Vector2 ActionForce = Vector2.Zero;
    [Export] public PackedScene ScythScene;
    [Export] public float ShootTime = .3f;
    public Cooldown ShootCooldown;

    [Export]
    public float speed = 200; // Speed at which the ninja moves

    [Export]
    public float friction = 6; // Friction applied when the ninja stops moving

    private Vector2 velocity; // Current velocity of the ninja    

    [Export]
    public float dashSpeed = 400; // Speed at which the ninja dashes

    [Export]
    public float dashDuration = 0.3f; // Duration of the dash in seconds

    [Export]
    public float dashTime = 0.5f; // Time in seconds between dashes
    public Cooldown DashCooldown;
    private Particles2D DashParticle;
    private CollisionShape2D AreaCollisionShape;
    private float dashTimer = 0; // Timer for the dash duration

    [Export] public int ScythCount = 2;
    [Export] public int GoldCoins;
    public int HarvestedCropsCount;
    public int HavestedWeedCount;
    public List<Perk> ActivePerks = new List<Perk>();
    [Export] public int ShurikenSize = 8;
    public float ThornTimer;
    private float ThornFactor;
    [Export] public float ThornTime = .5f;
    internal int BonusCropGold;
    public bool WeedStomper;
    public bool WeedKiller;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ShootCooldown = new Cooldown(ShootTime, this);
        DashCooldown = new Cooldown(dashTime, this);
        DashParticle = GetNode<Particles2D>("Dash");
        AreaCollisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionPressed("ui_accept") && ShootCooldown.Use())
        {
            for (var i = 0; i < ScythCount; i++)
            {
                var direction = (GetGlobalMousePosition() - GlobalPosition).Normalized();

                var scyth = ScythScene.Instance<Scyth>();
                scyth.LifeTime = ShurikenLifeTime;
                scyth.Size = ShurikenSize*4;
                scyth.GlobalPosition = GlobalPosition + direction.Rotated(Mathf.Pi / 2f) * (ScythCount - (i + 1)) * (ShurikenSize*4);
                GetTree().Root.AddChild(scyth);

                scyth.ApplyCentralImpulse(direction * ActionForce);

                scyth.OnCropHarvested += OnCropHarvested;
                scyth.OnWeedHarvested += OnWeedHarvested;
                scyth.OnAngryKilled += OnAngryKilled;

            }
        }

        // Get input from the player
        float horizontalInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        float verticalInput = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");

        bool moving = (horizontalInput != 0 || verticalInput != 0);

        // Calculate the new velocity based on the input
        if (Input.IsActionJustPressed("dash") && moving && DashCooldown.Use())
        {
            dashTimer = dashDuration;
            DashParticle.OneShot = true;
            DashParticle.Emitting = true;
            DashParticle.Restart();
        }

        GetNode<Sprite>("Sprite").FlipH = velocity.x < 0;

        var adjustedSpeed = speed;

        // Update the dash timer
        if (dashTimer > 0)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Stop();

            dashTimer -= delta;
            adjustedSpeed = speed + dashSpeed;
            var angle = 2f * Mathf.Pi / dashDuration * delta;
            GetNode<Sprite>("Sprite").Rotate(velocity.Normalized().x > 0 ? angle : -angle);
        }

        if (ThornTimer > 0f)
        {
            ThornTimer -= delta;
            adjustedSpeed *= ThornFactor;
        }

        if (moving && dashTimer <= 0)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Play("run");
        }
        else if (horizontalInput == 0 && verticalInput == 0 && dashTimer <= 0)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Stop();
            GetNode<Sprite>("Sprite").Rotation = 0f;
            GetNode<Sprite>("Sprite").Position = Vector2.Zero;
        }

        velocity.x = Mathf.Lerp(velocity.x, horizontalInput * adjustedSpeed, delta * friction);
        velocity.y = Mathf.Lerp(velocity.y, verticalInput * adjustedSpeed, delta * friction);

        // Move the ninja using the new velocity
        velocity = MoveAndSlide(velocity);
    }

    private void OnAngryKilled(object sender, EventArgs e)
    {
        if (sender is Type type)
        {
            // if (type == typeof(AngryEggplant))
            // {
            //     GoldCoins += 25;
            // }
            // if (type == typeof(AngryCorn))
            // {
            //     GoldCoins += 50;
            // }
            // if (type == typeof(AngryTomato))
            // {
            //     GoldCoins += 500;
            // }
            // if (type == typeof(AngryEggplant))
            // {
            //     GoldCoins += 1000;
            // }
        }
    }

    private void OnWeedHarvested(Tile tile)
    {
        HavestedWeedCount++;

        if (ActivePerks.Any(p => p.Id == "cut-weed-4"))
        {
            OnCropHarvested(tile);
            tile.ChangeGroup("Dirt", true);
        }
        else if (ActivePerks.Any(p => p.Id == "cut-weed-3"))
        {
            tile.ChangeGroup("Corn", true);
        }
        else if (ActivePerks.Any(p => p.Id == "cut-weed-2"))
        {
            tile.ChangeGroup("Grass", true);
        }
        else if (ActivePerks.Any(p => p.Id == "cut-weed-1"))
        {
            tile.ChangeGroup("Fertilized", true);
        }
    }

    private void OnCropHarvested(Tile tile)
    {
        HarvestedCropsCount++;

        var amount = 10 + BonusCropGold;

        tile.Harvest(amount);
        GoldCoins = GoldCoins + amount;

    }

    public void _on_Area2D_area_exited(Area2D area)
    {
        if (area is Tile tile)
        {
            if (dashTimer > 0)
            {
                if (area.IsInGroup("Dirt"))
                {
                    tile.ChangeGroup("Fertilized", true);
                }
                else if (WeedKiller && area.IsInGroup("Weed"))
                {
                    tile.Cut();
                    tile.ChangeGroup("Dirt", true);
                }
            }
            else if (area.IsInGroup("Weed") && WeedStomper)
            {
                tile.Cut();
                tile.ChangeGroup("Dirt", true);
            }
            else if (area.IsInGroup("Weed") && tile.Stage >= 0 && tile.Stage < 2)
            {
                ThornTimer = ThornTime;
                ThornFactor = .75f;
            }
            else if (area.IsInGroup("Weed") && tile.Stage >= 2 && tile.Stage < 4)
            {
                ThornTimer = ThornTime;
                ThornFactor = .5f;
            }
            else if (area.IsInGroup("Weed") && tile.Stage >= 4)
            {
                ThornTimer = ThornTime;
                ThornFactor = .25f;
            }
        }
    }

    internal void AddPerk(Perk perk)
    {
        ActivePerks.Add(perk);
    }

    internal void GrowSize()
    {
        ((CircleShape2D)AreaCollisionShape.Shape).Radius += (2f);
    }
}
