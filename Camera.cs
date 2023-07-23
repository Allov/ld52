using Godot;
using System;

public class Camera : Camera2D
{
    private float ShakingTimer;
    [Export] public float ShakeMaxTime = 0.3f;
    [Export] public float ShakeTimeScale = 0.5f;
    [Export] public float ShakeAmount = 1.0f;
    [Export] public Vector2 ZoomOutScale = Vector2.One;
    [Export] public Vector2 ZoomInScale = new Vector2(0.8f, 0.8f);

    private float ShakeAmountFactor;
    private float ShakeMaxTimeFactor;
    private float ShakeTimeScaleFactor;

    private RandomNumberGenerator Rand = new RandomNumberGenerator();

    private Color BgColor;
    private bool Shaking;

    public override void _Ready()
    {
        BgColor = (Color)ProjectSettings.GetSetting("rendering/environment/default_clear_color");
    }

    public void Shake(float amountFactor = 1.0f, float timeScaleFactor = 1.0f, float maxTimeFactor = 1.0f)
    {
        ShakingTimer = 0f;
        Shaking = true;
        ShakeAmountFactor = amountFactor;
        ShakeMaxTimeFactor = maxTimeFactor;
        ShakeTimeScaleFactor = timeScaleFactor;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        //0e222e
        // ProjectSettings.SetSetting("rendering/environment/default_clear_color", bgColor.LinearInterpolate(ColorHelpers.FromHexString("#000000"), 0.1f));

        // if (BgColor != Colors.Black)
        // {
        // 	BgColor = BgColor.LinearInterpolate(Colors.Black, 0.001f);
        // }
        // else
        // {
        // 	BgColor =
        // }
        //VisualServer.SetDefaultClearColor(Colors.DarkOrange);

        // var direction = Player.GetLookingDirection();

        // var desiredPosition = Player.GlobalPosition + (direction * LookAheadDistance);
        // GlobalPosition = GlobalPosition.LinearInterpolate(desiredPosition, SmoothingSpeed * delta);

        var desiredZoom = ZoomOutScale;

        // if (Player.InCombat)
        // {
        //     desiredZoom = desiredZoom * 1.2f;
        // }

        Zoom = Zoom.LinearInterpolate(desiredZoom, SmoothingSpeed * delta);

        if (Shaking)
        {
            ShakingTimer += delta;
            if (ShakingTimer < (ShakeMaxTime * ShakeMaxTimeFactor))
            {
                var x = Rand.RandfRange(-1.0f, 1.0f) * (ShakeAmount * ShakeAmountFactor);
                var y = Rand.RandfRange(-1.0f, 1.0f) * (ShakeAmount * ShakeAmountFactor);
                // Engine.TimeScale = ShakeTimeScale * ShakeTimeScaleFactor;

                Offset = new Vector2(x, y);
            }
            else
            {
                // Engine.TimeScale = 1f;
                Shaking = false;
                Offset = Vector2.Zero;
                ShakingTimer = 0f;
            }
        }
    }
}
