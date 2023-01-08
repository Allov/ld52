using Godot;
using System;

public class SoundSettings : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AudioServer.SetBusVolumeDb(0, -80f);    
        GetNode<AudioStreamPlayer>("Plop").Play();    
    }

    public void _on_HSlider_value_changed(float value)
    {
        AudioServer.SetBusVolumeDb(0, Mathf.Log(value) * 20f);
        GetNode<AudioStreamPlayer>("Plop").Play();
    }

    public void _on_Button_pressed()
    {
        GetTree().ChangeScene("SplashScreen.tscn");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
