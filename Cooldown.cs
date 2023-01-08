using System;
using Godot;
public class Cooldown : Godot.Object
{
    private Timer Timer;

    public Node Parent { get; private set; }
    public float Time { get; private set; }
    public bool OnCooldown { get; private set; }
    public event EventHandler OnCooldownReady;

    public Cooldown(float time, Node parent)
    {
        Parent = parent;
        Time = time;
        Timer = new Timer();
        Parent.AddChild(Timer);
        Reset(time);
    }

    public void Reset(float time)
    {
        Timer.WaitTime = time;
        Timer.Connect("timeout", this, nameof(OnTimeout));
        OnTimeout();
    }

    private void OnTimeout()
    {
        OnCooldown = false;
        Timer.Stop();
        OnCooldownReady?.Invoke(this, EventArgs.Empty);
    }

    public float TimeLeft()
    {
        return Timer.TimeLeft;
    }

    public bool Use()
    {
        if (OnCooldown) return false;

        Timer.Start();
        OnCooldown = true;
        return OnCooldown;
    }
}