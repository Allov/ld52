using Godot;
using System;

public class SceneTransition : CanvasLayer
{
    public async void ChangeScene(string target)
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Fade");
        await ToSignal(GetNode<AnimationPlayer>("AnimationPlayer"), "animation_finished");
        GetTree().CurrentScene.QueueFree();
        GetTree().ChangeScene(target);
        GetNode<AnimationPlayer>("AnimationPlayer").PlayBackwards("Fade");
        await ToSignal(GetNode<AnimationPlayer>("AnimationPlayer"), "animation_finished");
    }

}
