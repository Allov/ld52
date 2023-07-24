using Godot;
using System;

public class SceneTransition : CanvasLayer
{
    public async void ChangeScene(string target)
    {
        GetTree().CurrentScene.QueueFree();
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Fade");
        await ToSignal(GetNode<AnimationPlayer>("AnimationPlayer"), "animation_finished");
        GetTree().ChangeScene(target);
        GetNode<AnimationPlayer>("AnimationPlayer").PlayBackwards("Fade");

    }

}
