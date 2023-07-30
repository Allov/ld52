using Godot;
using System;

public class RotatingSprite : Sprite
{
    [Export] public float RotationSpeed = 100f;
    public override void _Ready()
    {

    }

    public override void _Process(float delta)
    {
        Rotation += RotationSpeed * delta;
    }
}
