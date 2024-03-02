using System;
using Godot;

public interface IHealth
{
	int Health { get; set; }
}

public class BaseAngry : RigidBody2D, IHealth, IHittable
{
	[Export] public float StartingForce = 10f;
	public Cooldown EnrageCooldown;
	public float EnrageTime = 5000f;
	[Export] public int Health { get; set; } = 10;
	public bool Dead { get; private set; }
	[Export] public int StageMin { get; set; }
	[Export] public int StageMax { get; set; }

	[Export] public int GoldDrop;
	[Export] public PackedScene[] Drops;
	private bool DeadDead;
	private bool Spawned;
	[Export] public PackedScene DamageTextScene;
	private DamageText Text;
	private Cooldown NewTextCooldown;
	[Export] public float NewTextCooldownTime = .6f;
	private int TotalDamage;
	private int NumberOfConsecutiveHits;

	public override void _Process(float delta)
	{
		if (Dead)
		{
			if (GetNode<AnimationPlayer>("AnimationPlayer").CurrentAnimation != "death")
			{
				GetNode<AnimationPlayer>("AnimationPlayer").Play("death");
			}
			SpawnDrops();
		}

		if (DeadDead)
		{
			QueueFree();
		}

		GetNode<TextureProgress>("Health").Value = Health;
	}

	protected void _on_AnimationPlayer_animation_finished(string name)
	{
		if (Dead && name == "death")
		{
			DeadDead = true;
		}
	}

	private void SpawnDrops()
	{
		if (Spawned) return;

		while (!Spawned || RandomHelpers.DrawResult(5))
		{
			Spawned = true;
			for (var i = 0; i < Drops.Length; i++)
			{
				var drop = Drops[i].Instance<BaseDrop>();
				drop.GlobalPosition = GlobalPosition;
				GetParent().AddChild(drop);
			}
		}
	}

	public void _on_Area2D_area_entered(Area2D area)
	{
		if (area is Tile tile)
		{
			if (!area.IsInGroup("Weed"))
			{
				tile.Stomp(RandomHelpers.RangeInt(StageMin, StageMax));
			}
		}
	}

	public void _on_BaseAngry_body_entered(Node node)
	{
		if (node is StaticBody2D)
		{
			var tween = new Tween();
			var sprite = GetNode<Sprite>("Sprite");
			var v = new Vector2(4f * 1.1f, 4f * 1.1f);
			tween.InterpolateProperty(sprite, "scale", Vector2.One * 4f, v, .05f, Tween.TransitionType.Sine, Tween.EaseType.InOut);
			tween.InterpolateProperty(sprite, "scale", v, Vector2.One * 4f, .05f, Tween.TransitionType.Sine, Tween.EaseType.InOut, .05f);
			AddChild(tween);
			tween.Start();
		}
	}

	public override void _Ready()
	{
		base._Ready();

		float angle = (float)RandomHelpers.NextDouble() * Mathf.Pi * 2;

		ApplyCentralImpulse(Vector2.One.Rotated(angle) * StartingForce);
		GetNode<TextureProgress>("Health").MaxValue = Health;
		GetNode<TextureProgress>("Health").Value = Health;

		GetNode<Particles2D>("SpawnParticle").Emitting = true;

		NewTextCooldown = new Cooldown(NewTextCooldownTime, this);
	}

	public void Hit(bool pierce, int damage, bool crit)
	{
		if (Dead) return;

		GetNode<AnimationPlayer>("AnimationPlayer").Play("Hit");
		GetNode<AnimationPlayer>("AnimationPlayer").Queue("run");

		var hitString = $"Hit{RandomHelpers.RangeInt(1, 2)}";
		if (pierce)
		{
			hitString = "Hit3";
		}

		GetNode<AudioStreamPlayer2D>(hitString).PitchScale = (float)Math.Round((RandomHelpers.RangeDouble(.6D, .8D)), 1);
		GetNode<AudioStreamPlayer2D>(hitString).Play();

		SpawnDamageText(damage, crit);

	}

	private void SpawnDamageText(int damage, bool crit)
	{
		var isNew = Text == null || crit || NewTextCooldown.Use();
		Text = crit || isNew ? DamageTextScene.Instance<DamageText>() : Text ?? DamageTextScene.Instance<DamageText>();
		if (isNew)
		{
			CreateDamageText(crit);
			TotalDamage = 0;
			NumberOfConsecutiveHits = 0;
		}

		TotalDamage += damage;
		NumberOfConsecutiveHits++;
		// var counter = (!isNew ? "".PadLeft(NumberOfConsecutiveHits, ' ') : "");
		var counter = isNew ? "" : " ";
		if (RandomHelpers.DrawResult(2))
		{
			Text.TextLabel.Text = $"{TotalDamage}{counter}";
		}
		else
		{
			Text.TextLabel.Text = $"{counter}{TotalDamage}";
		}
	}

	private void CreateDamageText(bool crit)
	{
		Text.GlobalPosition = GlobalPosition;
		Text.Radius = (GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D).Radius;
		Text.Radius = crit ? Text.Radius * 1.5f : Text.Radius;
		Text.Animation = crit ? "Crit" : "Arc";

		var parent = GetParent<Node2D>();
		parent.AddChild(Text);
	}

	public void Kill()
	{
		Dead = true;
	}
}
