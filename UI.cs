using Godot;
using System;

public class UI : CanvasLayer
{
    private bool UserPaused;
    private bool TutorialActive;

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
        if (TutorialActive && (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("ui_accept")))
        {
            TutorialActive = false;
            GetNode<Sprite>("Tutorial").Visible = false;
            GetNode<PanelContainer>("Paused").Visible = true;
        }
        else if (Input.IsActionJustPressed("ui_cancel") && !GetTree().Paused)
        {
            GetTree().Paused = true;
            UserPaused = true;
            GetNode<PanelContainer>("Paused").Visible = true;
        }
        else if (Input.IsActionJustPressed("ui_cancel") && UserPaused)
        {
            Resume();
        }
    }

    private void Resume()
    {
        GetTree().Paused = false;
        UserPaused = false;
        GetNode<PanelContainer>("Paused").Visible = false;
    }

    public void _on_HowToPlay_pressed()
    {
        GetNode<PanelContainer>("Paused").Visible = false;
        GetNode<Sprite>("Tutorial").Visible = true;
        TutorialActive = true;
    }

    public void _on_Quit_pressed()
    {
        GetTree().Quit();
    }
    public void _on_Resume_pressed()
    {
        Resume();        
    }
    public void _on_Restart_pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeScene("SplashScreen.tscn");
    }
}
