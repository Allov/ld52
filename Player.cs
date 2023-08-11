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

    [Export] public float Speed = 200; // Speed at which the ninja moves

    [Export] public float Friction = 6; // Friction applied when the ninja stops moving

    private Vector2 Velocity; // Current velocity of the ninja

    [Export] public float DashingSpeed = 400; // Speed at which the ninja dashes

    [Export] public float DashDuration = 0.3f; // Duration of the dash in seconds

    [Export]
    public float DashCooldownTime = 0.5f; // Time in seconds between dashes
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
    [Export] public int PierceCount { get; set; }
    [Export] public bool Disabled { get; set; }
    [Export] public bool AutoShoot = false;
    private Cooldown FanOfShurikensCooldown;
    [Export] public bool ConeOfShurikens;

    [Export] public int ShurikenDamage { get; set; } = 1;

    [Export] public int HomingCount { get; set; }
    [Export] public float ShurikenSpeedModifier { get; set; } = 1f;
    [Export] public bool FanOfShurikens { get; set; }
    [Export] public float FanOfShurikensCooldownTime { get; set; } = 2f;
    [Export] public int FanOfShurikensCount { get; set; } = 8;
    [Export] public int ConeOfShurikensCount { get; set; } = 4;
    [Export] public bool GapOfShurikens { get; set; }
    [Export] public int GapOfShurikensCount { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ShootCooldown = new Cooldown(ShootTime, this);
        DashCooldown = new Cooldown(DashCooldownTime, this);
        FanOfShurikensCooldown = new Cooldown(FanOfShurikensCooldownTime, this);
        DashParticle = GetNode<Particles2D>("Dash");
        AreaCollisionShape = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        Size = ((CircleShape2D)AreaCollisionShape.Shape).Radius;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        if (Disabled) return;

        if (Input.IsActionJustPressed("toggle_auto_shoot"))
        {
            AutoShoot = !AutoShoot;
        }

        // Get input from the player
        var horizontalInput = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        var verticalInput = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");

        var moving = (horizontalInput != 0 || verticalInput != 0);

        var horizontalAttack = Input.GetActionStrength("attack_right") - Input.GetActionStrength("attack_left");
        var verticalAttack = Input.GetActionStrength("attack_down") - Input.GetActionStrength("attack_up");

        var attackingController = (horizontalAttack != 0 || verticalAttack != 0); ;


        var direction = (GetGlobalMousePosition() - GlobalPosition).Normalized();

        if (attackingController)
        {
            direction = new Vector2(horizontalAttack, verticalAttack).Normalized();
        }

        if ((AutoShoot || Input.IsActionPressed("attack") || attackingController) && ShootCooldown.Use())
        {
            for (var i = 0; i < ScythCount; i++)
            {
                ThrowShuriken(direction, i);
            }
        }

        GetNode<Sprite>("Sprite3").LookAt(GlobalPosition + direction);

        // Calculate the new velocity based on the input
        if (Input.IsActionPressed("dash") && moving && DashCooldown.Use())
        {
            PerformFanOfShurikens();
            PerformConeOfShurikens(direction);
            PerformGapOfShurikens(direction);

            dashTimer = DashDuration;
            DashParticle.OneShot = true;
            DashParticle.Emitting = true;
            DashParticle.Restart();

            var areas = GetNode<Area2D>("Area2D").GetOverlappingAreas().Cast<Area2D>();
            foreach (var area in areas)
            {
                TileSoil(area);
            }
        }

        GetNode<Sprite>("Sprite").FlipH = Velocity.x < 0;

        var adjustedSpeed = Speed;

        // Update the dash timer
        if (dashTimer > 0)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Stop();

            dashTimer -= delta;
            adjustedSpeed = Speed + DashingSpeed;
            var angle = 2f * Mathf.Pi / DashDuration * delta;
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

        var hv = new Vector2(horizontalInput, verticalInput).Normalized();

        Velocity.x = Mathf.Lerp(Velocity.x, hv.x * adjustedSpeed, delta * Friction);
        Velocity.y = Mathf.Lerp(Velocity.y, hv.y * adjustedSpeed, delta * Friction);

        // Move the ninja using the new velocity
        Velocity = MoveAndSlide(Velocity);
    }

    private void PerformConeOfShurikens(Vector2 direction)
    {
        if (ConeOfShurikens)
        {
            float coneAngleRadians = Mathf.Deg2Rad(180);
            float coneHalfAngleRadians = coneAngleRadians / 2;

            for (var i = 0; i < ConeOfShurikensCount; i++)
            {
                float angle = coneHalfAngleRadians - i * coneAngleRadians / (ConeOfShurikensCount - 1);
                var offset = direction;

                var shuriken = ThrowShuriken(offset.Rotated(angle), i);
                shuriken.Modulate = Colors.Orange;
            }
        }
    }

    private void PerformGapOfShurikens(Vector2 direction)
    {
        if (GapOfShurikens)
        {
            float gapAngleRadians = Mathf.Deg2Rad(180);

            for (var i = 0; i < GapOfShurikensCount; i++)
            {
                float angle = i * (Mathf.Pi * 2 + gapAngleRadians * (GapOfShurikensCount - 1)) / GapOfShurikensCount;
                var offset = direction;

                var shuriken = ThrowShuriken(offset.Rotated(angle), i);
                shuriken.Modulate = Colors.Red;
            }
        }
    }

    public void PerformFanOfShurikens()
    {
        if (FanOfShurikens)
        {
            float angleIncrement = Mathf.Pi * 2 / 8;
            for (var i = 0; i < FanOfShurikensCount; i++)
            {
                var angle = i * angleIncrement;
                var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var shuriken = ThrowShuriken(offset.Normalized(), i);
                shuriken.Modulate = Colors.Blue;
            }
        }
    }

    private Scyth ThrowShuriken(Vector2 direction, int i)
    {
        var scyth = ScythScene.Instance<Scyth>();
        scyth.LifeTime = ShurikenLifeTime;
        scyth.Size = ShurikenSize * 4;
        scyth.Damage = ShurikenDamage;

        var scythRotation = ((direction.Rotated(Mathf.Pi / 2f)));
        scyth.GlobalPosition = (GlobalPosition) + (scythRotation * i * ShurikenSize * 4) - ((ScythCount * scythRotation * ShurikenSize * 4) * .5f);
        scyth.PiercingLeft = PierceCount;
        scyth.HomingLeft = HomingCount;

        scyth.OnCropHarvested += OnCropHarvested;
        scyth.OnWeedHarvested += OnWeedHarvested;
        scyth.OnAngryKilled += OnAngryKilled;

        GetParent().AddChild(scyth);

        scyth.ApplyCentralImpulse(direction * ActionForce * ShurikenSpeedModifier);

        return scyth;
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
            body.ApplyCentralImpulse((body.GlobalPosition - body.GlobalPosition).Normalized() * 20f);
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
