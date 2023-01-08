using Godot;
using System;
using System.Collections.Generic;

public class Tile : Area2D
{
    internal int PositionIndex;
    internal int X;
    internal int Y;
    private string CurrentGroup = "";
    private Sprite DirtSprite;
    private Sprite FertilizedSprite;
    private Sprite GrassSprite;
    private Sprite CornSprite;
    private Sprite WeedSprite;
    private Sprite WeedSprite2;
    private List<Sprite> AllSprites = new List<Sprite>();
    internal int Stage;
    private Sprite CurrentSprite;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DirtSprite = GetNode<Sprite>("Dirt");
        FertilizedSprite = GetNode<Sprite>("Fertilized");
        GrassSprite = GetNode<Sprite>("Grass");
        CornSprite = GetNode<Sprite>("Corn");
        WeedSprite = GetNode<Sprite>("Weed");
        WeedSprite2 = GetNode<Sprite>("Weed/Weed2");

        AllSprites.AddRange(new[] {
            DirtSprite,
            FertilizedSprite,
            GrassSprite,
            CornSprite,
            WeedSprite
        });

        HideAllSprites();
    }

    private void HideAllSprites()
    {
        foreach (var sprite in AllSprites)
        {
            sprite.Visible = false;
        }
    }

    internal void ChangeGroup(string newGroup, bool doAnimation = false)
    {
        if (CurrentGroup != "")
        {
            RemoveFromGroup(CurrentGroup);
        }
        else
        {
            CurrentGroup = newGroup;
        }

        AddToGroup(newGroup);
        CurrentGroup = newGroup;

        ModulateFromGroup(doAnimation);
    }

    public void Cut()
    {
        GetNode<AudioStreamPlayer2D>("WeedDeath").Play();
    }

    public void Harvest()
    {
        GetNode<AudioStreamPlayer2D>("Harvest").Play();
    }


    public void ModulateFromGroup(bool doAnimation = false)
    {
        HideAllSprites();
        switch (CurrentGroup)
        {
            case "Dirt":
                DirtSprite.Visible = true;
                CurrentSprite = DirtSprite;
                break;
            case "Fertilized":
                FertilizedSprite.Visible = true;
                CurrentSprite = FertilizedSprite;
                break;
            case "Grass":
                GrassSprite.Visible = true;
                CurrentSprite = GrassSprite;
                break;
            case "Corn":
                CornSprite.Visible = true;
                CurrentSprite = CornSprite;
                break;
            case "Weed":
                WeedSprite.Visible = Stage >= 0;
                WeedSprite2.Visible = Stage >= 2 && Stage < 4;
                CurrentSprite = WeedSprite;
                break;
            default:
                break;
        }

        if (doAnimation)
        {
            DoAnimation();
        }
    }

    private async void DoAnimation()
    {
        var tween = new Tween();
        tween.InterpolateProperty(CurrentSprite, "scale", new Vector2(.6f, .6f), Vector2.One, .2f, Tween.TransitionType.Linear, Tween.EaseType.In);
        tween.InterpolateProperty(CurrentSprite, "modulate:a", .4f, 1f, .2f, Tween.TransitionType.Sine, Tween.EaseType.In);
        AddChild(tween);
        tween.Start();

        await ToSignal(tween, "tween_all_completed");
        RemoveChild(tween);
        tween.QueueFree();
    }

    internal void Harvest(int reward)
    {
        GetNode<Label>("HarvestLabel").Text = $"+{reward}";
		GetNode<AnimationPlayer>("AnimationPlayer").Play("money");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
