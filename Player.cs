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

    private Vector2 Velocity; // Current velocity of the ninja

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

    [Export] public float AddedGrowRadius { get; set; }
    public float Size { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ShootCooldown = new Cooldown(ShootTime, this);
        DashCooldown = new Cooldown(dashTime, this);
        DashParticle = GetNode<Particles2D>("Dash");
        AreaCollisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        Size = ((CircleShape2D)AreaCollisionShape.Shape).Radius;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        // Get input from the player
        var horizontalInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        var verticalInput = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");

        var moving = (horizontalInput != 0 || verticalInput != 0);

        var horizontalAttack = Input.GetActionStrength("attack_right") - Input.GetActionStrength("attack_left");
        var verticalAttack = Input.GetActionStrength("attack_down") - Input.GetActionStrength("attack_up");

        var attackingController = (horizontalAttack != 0 || verticalAttack != 0);;


        var direction = (GetGlobalMousePosition() - GlobalPosition).Normalized();

        if (attackingController)
        {
            direction = new Vector2(horizontalAttack, verticalAttack).Normalized();
        }

        if ((Input.IsActionPressed("attack") || attackingController) && ShootCooldown.Use())
        {
            for (var i = 0; i < ScythCount; i++)
            {

                var scyth = ScythScene.Instance<Scyth>();
                scyth.LifeTime = ShurikenLifeTime;
                scyth.Size = ShurikenSize*4;
                scyth.GlobalPosition = GlobalPosition + direction.Rotated(Mathf.Pi / 2f) * (ScythCount - (i + 1)) * (ShurikenSize*4);
                if (ActivePerks.Any(p => (new [] {"piercing-1"}).Contains(p.Id)))
                {
                    scyth.PiercingLeft = 2;
                }
                else if (ActivePerks.Any(p => (new [] {"piercing-2"}).Contains(p.Id)))
                {
                    scyth.PiercingLeft = 10;
                }
                GetTree().Root.AddChild(scyth);

                scyth.ApplyCentralImpulse(direction * ActionForce);

                scyth.OnCropHarvested += OnCropHarvested;
                scyth.OnWeedHarvested += OnWeedHarvested;
                scyth.OnAngryKilled += OnAngryKilled;

            }
        }

        GetNode<Sprite>("Sprite3").LookAt(GlobalPosition + direction);

        // Calculate the new velocity based on the input
        if (Input.IsActionJustPressed("dash") && moving && DashCooldown.Use())
        {
            dashTimer = dashDuration;
            DashParticle.OneShot = true;
            DashParticle.Emitting = true;
            DashParticle.Restart();

            var areas = GetNode<Area2D>("Area2D").GetOverlappingAreas().Cast<Area2D>();
            foreach(var area in areas)
            {
                TileSoil(area);
            }
        }

        GetNode<Sprite>("Sprite").FlipH = Velocity.x < 0;

        var adjustedSpeed = speed;

        // Update the dash timer
        if (dashTimer > 0)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Stop();

            dashTimer -= delta;
            adjustedSpeed = speed + dashSpeed;
            var angle = 2f * Mathf.Pi / dashDuration * delta;
            GetNode<Sprite>("Sprite").Rotate(Velocity.Normalized().x > 0 ? angle : -angle);
        }

        if (ThornTimer > 0f)
        {
            ThornTimer -= delta;
            adjustedSpeed *= ThornFactor;
        }
        else
        {
            ThornTimer = 0f;
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

        Velocity.x = Mathf.Lerp(Velocity.x, horizontalInput * adjustedSpeed, delta * friction);
        Velocity.y = Mathf.Lerp(Velocity.y, verticalInput * adjustedSpeed, delta * friction);

        // Move the ninja using the new velocity
        Velocity = MoveAndSlide(Velocity);
    }

    private void OnAngryKilled(BaseAngry baseAngry)
    {
        GoldCoins += baseAngry.GoldDrop;
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

        tile.HarvestAnimation(amount);

        AwardGoldCoins(amount);
    }

    public void AwardGoldCoins(int amount)
    {
        GoldCoins = GoldCoins + amount;
    }

    public void _on_Area2D_area_entered(Area2D area)
    {
        TileSoil(area);
    }

    private void TileSoil(Area2D area)
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

    public void _on_PickUpArea_body_entered(Node node)
    {
        if (node is BaseDrop drop)
        {
            drop.PickUp();

            if (drop.Name == "ChestDrop")
            {
                GD.Print("Award Item");
            }
            else
            {
                AwardGoldCoins(drop.GoldCoinValue);
            }

        }
    }

    public void _on_AttrackArea_body_entered(Node node)
    {
        if (node is RigidBody2D body)
        {
            body.ApplyCentralImpulse((body.GlobalPosition - body.GlobalPosition).Normalized());
        }
    }

    public void AddPerk(Perk perk)
    {
        ActivePerks.Add(perk);
    }

    public void GrowSize()
    {
        Size = ((CircleShape2D)AreaCollisionShape.Shape).Radius += AddedGrowRadius;
    }
}
