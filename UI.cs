using Godot;
using System;
using System.Linq;

public class UI : CanvasLayer
{
	private bool UserPaused;
	private bool TutorialActive;
	[Export] public NodePath PlayAreaNode;
	private Label MoneyLabel;
	private Label CropsLabel;
	private Label StatsLabel;
	private PlayArea PlayArea;
	private Tween Tween;
	private int OldCoinsCount;
	private float Horizontal;
	private bool Restart;
    private bool Restarting;

    static string GetOrdinalNumber(int number)
	{
		if (number >= 11 && number <= 13)
		{
			return "th"; // Special case for 11th, 12th, and 13th
		}
		else
		{
			int lastDigit = number % 10;
			switch (lastDigit)
			{
				case 1:
					return "st";
				case 2:
					return "nd";
				case 3:
					return "rd";
				default:
					return "th";
			}
		}
	}

	public override void _Ready()
	{
		MoneyLabel = GetNode<Label>("MoneyLabel");
		CropsLabel = GetNode<Label>("HarvestCounter");
		StatsLabel = GetNode<Label>("PlayerStats");
		PlayArea = GetNode<PlayArea>(PlayAreaNode);
		Tween = new Tween();
		AddChild(Tween);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (Restart)
		{
			RestartMap();
			return;
		}

		if (TutorialActive && (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("ui_accept")))
		{
			CloseTutorial();
		}
		else if (Input.IsActionJustPressed("ui_pause") && !GetTree().Paused)
		{
			Pause();
		}
		else if (Input.IsActionJustPressed("ui_pause") && UserPaused)
		{
			Resume();
		}

		UpdateUI(delta);
	}

	private void UpdateUI(float delta)
	{
		if (OldCoinsCount != PlayArea.Player.GoldCoins)
		{
			// MoneyLabel.Text = $"$ {PlayArea.Player.GoldCoins}";
			var grow = 1.3f;
			Tween.InterpolateProperty(MoneyLabel, "rect_scale", Vector2.One, Vector2.One * grow, .05f, Tween.TransitionType.Expo, Tween.EaseType.Out);
			Tween.InterpolateProperty(MoneyLabel, "rect_scale", Vector2.One * grow, Vector2.One, .05f, Tween.TransitionType.Linear, Tween.EaseType.Out, .1f);
			Tween.InterpolateMethod(this, nameof(UpdateMoneyLabel), OldCoinsCount, PlayArea.Player.GoldCoins, .5f);
			Tween.Start();

			OldCoinsCount = PlayArea.Player.GoldCoins;
		}

		GetNode<Label>("CalendarLabel").Text = $"{PlayArea.NumberOfDays}{GetOrdinalNumber(PlayArea.NumberOfDays)} day";
		CropsLabel.Text = $"C:{PlayArea.Player.HarvestedCropsCount.ToString("n0")}\nW:{PlayArea.Player.HavestedWeedCount.ToString("n0")}";

		StatsLabel.Text = $"SHOOT: {PlayArea.Player.ShootTime}s      DASH: {PlayArea.Player.DashCooldownTime}s    SIZE: {PlayArea.Player.Size}    STIM: {PlayArea.Player.ShurikenLifeTime}s";
		StatsLabel.Text += $"\n";
		StatsLabel.Text += $"SPEED: {PlayArea.Player.Speed}m/s    SHUR: {PlayArea.Player.ScythCount}       SSIZ: {PlayArea.Player.ShurikenSize}    THRN: {PlayArea.Player.ThornTimer}s";
	}

	private void UpdateMoneyLabel(int coins)
	{
		MoneyLabel.Text = $"{coins.ToString("n0")}$";
	}

	private void CloseTutorial()
	{
		TutorialActive = false;
		GetNode<Sprite>("Tutorial").Visible = false;
		GetNode<PanelContainer>("Paused").Visible = true;
	}

	private void Pause()
	{
        if (Input.GetMouseMode() == Input.MouseMode.Confined)
        {
            Input.SetMouseMode(Input.MouseMode.Hidden);
        }

		GetTree().Paused = true;
		UserPaused = true;
		GetNode<PanelContainer>("Paused").Visible = true;
		GetNode<AnimationPlayer>("Paused/AnimationPlayer").Play("fadein");
	}

	private void Resume()
	{
		GetTree().Paused = false;
		UserPaused = false;
		GetNode<PanelContainer>("Paused").Visible = false;
		GetNode<PanelContainer>("Paused").Modulate = Colors.Transparent;
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
		Restart = true;
	}

	public void RestartMap()
	{
        if (Restarting) return;

        Restarting = true;

		GetTree().Paused = false;
		var angries = GetTree()
			.GetNodesInGroup("AngryPlants");

		foreach (var angry in angries)
		{
            var a = angry as BaseAngry;
			a?.QueueFree();

            if (a == null) {
                GD.Print("angry is null on restart...");
            }
		}

		GetNode<SceneTransition>("/root/SceneTransition").ChangeScene("Main.tscn");
	}
}
