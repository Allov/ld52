using Godot;
using System;

public class SplashScreen : Node2D
{
    private float SkipTimer = 2.0f;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        SkipTimer -= delta;
    }

    public override void _Input(InputEvent @event)
    {
        if (SkipTimer <= 0f && (@event is InputEventKey || @event is InputEventMouseButton))
        {
            GetTree().ChangeScene("Main.tscn");
        }
    }
}
