using Godot;

public class Trail : Line2D
{
    [Export] public int MaxLength = 30;
    [Export] public bool Enabled = true;

    public override void _Ready()
    {
    }

    public override void _Process(float delta)
    {
        GlobalPosition = Vector2.Zero;
        GlobalRotation = 0;

        if (Enabled)
        {
            var point = GetParent<Node2D>().GlobalPosition;
            if (Points.Length > 1)
            {
                var lastPoint = Points[Points.Length - 2];
                var direction  = point - lastPoint;
                var distance = direction.Length();
                direction = direction.Rotated(Mathf.Pi / 2f) * (RandomHelpers.DrawResult(2) ? -1 : 1);
                point = point + (direction.Normalized() * 2f);
            }

            AddPoint(point);
        }

        if (Points.Length > 0 && (Points.Length > MaxLength || !Enabled))
        {
            RemovePoint(0);
        }
    }
}
