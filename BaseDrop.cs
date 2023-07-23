using Godot;
using System;

public class BaseDrop : RigidBody2D
{
    private bool PickedUpAndReadyToDie;
    private float DeadTimer;
    [Export] public int GoldCoinValue;

    [Export] public Vector2 StartingForce { get; private set; }
    public bool PickedUp { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        float angle = (float)RandomHelpers.NextDouble() * Mathf.Pi * 2;

        ApplyImpulse(new Vector2(RandomHelpers.NextFloat(), RandomHelpers.NextFloat()) * StartingForce, Vector2.One.Rotated(angle) * StartingForce);
        ApplyTorqueImpulse(StartingForce.x * RandomHelpers.RangeInt(-1, 1));

    }

    public void PickUp()
    {
        PickedUp = true;
    }

     // Called every frame. 'delta' is the elapsed time since the previous frame.
     public override void _Process(float delta)
     {
        if (PickedUp && !PickedUpAndReadyToDie)
        {
            GetNode<CanvasLayer>("CanvasLayer").Offset = GlobalPosition;
            GetNode<Label>("CanvasLayer/HarvestLabel").Text = $"+{GoldCoinValue}";
            GetNode<AnimationPlayer>("AnimationPlayer").Play("money");

            Visible = false;

            var sound = GetNode<AudioStreamPlayer2D>("PickUpSound");
            sound.PitchScale = RandomHelpers.RangeInt(1, 5);
            sound.Play();
            DeadTimer = .9f;
            PickedUpAndReadyToDie = true;

        }

        if (PickedUpAndReadyToDie && DeadTimer <= 0f)
        {
            QueueFree();
        }
        else if (PickedUpAndReadyToDie) 
        {
            DeadTimer -= delta;
        }
     }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        base._IntegrateForces(state);
        RotationDegrees = 0;
    }
}
