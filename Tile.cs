using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
    private Sprite WeedSprite3;
    private Sprite WeedSprite4;
    private List<Sprite> AllSprites = new List<Sprite>();
    public int Stage { get; private set; }
    private Sprite CurrentSprite;
    [Export] public bool Disabled;
    private Sprite DisabledSprite;
    private Sprite WaterSprite;

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
        WeedSprite3 = GetNode<Sprite>("Weed/Weed3");
        WeedSprite4 = GetNode<Sprite>("Weed/Weed4");
        DisabledSprite = GetNode<Sprite>("Disabled");
        WaterSprite = GetNode<Sprite>("Water");

        AllSprites.AddRange(new[] {
            WaterSprite,
            DisabledSprite,
            DirtSprite,
            FertilizedSprite,
            GrassSprite,
            CornSprite,
            WeedSprite,
            WeedSprite2,
            WeedSprite3,
            WeedSprite4,
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
        if ((new [] { "Disabled", "WaterTile"}).Contains(CurrentGroup) && Disabled) return;

        if (newGroup == "Dirt")
        {
            Stage = 0;
        }

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
        if (Disabled) return;

        Stage--;
        if (Stage <= 0)
        {
            Stage = 0;
            ChangeGroup("Dirt", true);
        }
        else
        {
            ModulateFromGroup(true);
        }

        GetNode<AudioStreamPlayer2D>("WeedDeath").Play();
    }

    public void Harvest()
    {
        if (Disabled) return;

        ChangeGroup("Dirt", true);
        GetNode<AudioStreamPlayer2D>("Harvest").Play();
    }


    public void ModulateFromGroup(bool doAnimation = false)
    {
        if (CurrentGroup == "Disabled" && Disabled) return;

        HideAllSprites();
        switch (CurrentGroup)
        {
            case "WaterTile":
                WaterSprite.Visible = true;
                CurrentSprite = WaterSprite;
                break;
            case "Disabled":
                DisabledSprite.Visible = true;
                CurrentSprite = DisabledSprite;
                break;
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
                WeedSprite3.Visible = Stage >= 4 && Stage < 6;
                WeedSprite4.Visible = Stage >= 6;
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
        tween.InterpolateProperty(CurrentSprite, "scale", new Vector2(.6f, .6f), Vector2.One * 4f, .2f, Tween.TransitionType.Linear, Tween.EaseType.In);
        tween.InterpolateProperty(CurrentSprite, "modulate:a", .7f, 1f, .1f, Tween.TransitionType.Sine, Tween.EaseType.In);
        AddChild(tween);
        tween.Start();

        await ToSignal(tween, "tween_all_completed");
        RemoveChild(tween);
        tween.QueueFree();
    }

    public void HarvestAnimation(int reward)
    {
        if (Disabled) return;

        GetNode<CanvasLayer>("CanvasLayer").Offset = GlobalPosition;
        GetNode<Label>("CanvasLayer/HarvestLabel").Text = $"+{reward}";
        GetNode<AnimationPlayer>("AnimationPlayer").Play("money");
    }

    public void _on_Tile_body_entered(Node node)
    {
        if (node is StaticBody2D body)
        {
            if (body.IsInGroup("Water"))
            {
                ChangeGroup("WaterTile");
            }
            else
            {
                ChangeGroup("Disabled");
            }

            Disabled = true;
        }
    }

    public void Grow()
    {
        if (Disabled) return;

        Stage++;
    }

    public void Stomp()
    {
        if (Disabled) return;

        Stage = RandomHelpers.RangeInt(1, 3);
        ChangeGroup("Weed", true);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //
    //  }
}