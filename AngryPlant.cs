using Godot;
using System;

public class AngryPlant : RigidBody2D, IHealth

{
    [Export] public float StartingForce = 10f;
    [Export] public int Health { get; set; } = 10;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        float angle = (float)RandomHelpers.NextDouble() * Mathf.Pi * 2;

        ApplyCentralImpulse(Vector2.One.Rotated(angle) * StartingForce);
        GetNode<TextureProgress>("Health").MaxValue = Health;
        GetNode<TextureProgress>("Health").Value = Health;
    }

    public override void _Process(float delta)
    {
        GetNode<TextureProgress>("Health").Value = Health;
    }

    public void _on_Area2D_area_entered(Area2D area)
    {
        if (area is Tile tile)
        {
            if (!area.IsInGroup("Weed"))
            {
                tile.ChangeGroup("Weed", true);
            }
        }
    }

}

internal interface IHealth
{
    int Health { get; set; }
}