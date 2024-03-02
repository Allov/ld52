using Godot;
using System;

public class DamageText : Node2D
{
    private AnimationPlayer AnimationPlayer;

    public Label TextLabel { get; private set; }
    public float Radius { get; set; }
    public float Speed { get; set; } = 1.0f;
    public string Animation { get; set; } = "Arc";

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TextLabel = GetNode<Label>("TextLabel");
        Position = GlobalPosition + RandomHelpers.GetRandomPositionInCircle(Radius * 1.5f);

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationPlayer.Play(Animation);
        AnimationPlayer.PlaybackSpeed = Speed;

        const float center = (Mathf.Pi / 8f) / 2f;
        var rotation = RandomHelpers.GetRandomAngle(-center, +center);

        var tween = new Tween();
        tween.InterpolateProperty(this, "position", Position, Position - Vector2.Up.Rotated(Rotation) * RandomHelpers.RangeInt(25, 50), .8f / Speed, Tween.TransitionType.Back, Tween.EaseType.InOut);
        tween.InterpolateProperty(this, "rotation", 0f, rotation, .8f);
        AddChild(tween);
        tween.Start();


    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//
//  }
}
