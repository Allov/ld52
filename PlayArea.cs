using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayArea : Node2D
{
    private const int NextHarvestCountModifier = 50;
    [Export] public int StartingNextHarvestCount = 100;
    [Export] public float PercentDanger = .8f;
    [Export] public float PercentEndGame = .9f;
    [Export] public int Width = 24;
    [Export] public int Height = 12;
    [Export] public int TileSize = 64;
    [Export] public PackedScene TileScene;
    public Cooldown GrowCooldown { get; private set; }
    [Export] public float GrowTime = 5f;

    public Dictionary<int, Tile> Field = new Dictionary<int, Tile>();
    private List<Tile> GrassTiles;
    private List<Tile> FertilizedTiles;
    private List<Tile> DirtTiles;
    private List<Tile> CornTiles;
    private List<Tile> WeedTiles;
    private List<Tile> DisabledTiles;
    private List<Tile> WaterTiles;
    public int FieldCount { get; private set; }

    public int CurrentDay { get; private set; }
    [Export] public NodePath DaysLabelNodePath;
    public Label DaysLabel;
    public Player Player;
    [Export] public float SpawnFactor = 2f / 75f; // about 33% of the field would spawn as weed at day 60
    [Export] public int MaxNumberOfDays = 60;

    [Export] public NodePath PlayerNodePath;
    [Export] public NodePath ShopNodePath;
    public Shop Shop;

    [Export] public PackedScene AngryPlantScene;
    [Export] public PackedScene AngryCornScene;
    private int MadnessLevel;
    [Export] public PackedScene AngryTomatoScene;
    [Export] public PackedScene AngryEggplantScene;
    private bool Day15Done;
    private bool TriggerEndOfGame;
    [Export] public NodePath GameOverNodePath;
    public PanelContainer GameOverPanel;
    private bool UserPaused;
    [Export] public int SpawnFrequency = 4;
    [Export] public bool LockMouseToWindow = true;
    [Export] public NodePath CameraNodePath;
    public Camera Camera;
    [Export] public NodePath AngriesNodePath;
    public YSort Angries;
    [Export] public PackedScene[] Maps;
    [Export] public bool OpenShopToday;
    [Export] public float ShopCooldownTime = 10f;
    private int NextHarvestShopCount = 50;

    [Export] public bool SpawnAngriesToday { get; set; }

    public float WeedProgress { get; private set; }
    [Export] public Curve DayLengthCurve { get; set; }
    [Export] public Curve AngryHealthCurve { get; set; }
    [Export] public int StartingDay { get; set; }
    [Export] public Cooldown ShopCooldown { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var map = Maps[RandomHelpers.RangeInt(0, Maps.Length)].Instance<Node2D>();
        AddChild(map);

        DaysLabel = GetNode<Label>(DaysLabelNodePath);
        Player = GetNode<Player>(PlayerNodePath);
        Shop = GetNode<Shop>(ShopNodePath);
        GameOverPanel = GetNode<PanelContainer>(GameOverNodePath);
        Camera = GetNode<Camera>(CameraNodePath);
        Angries = GetNode<YSort>(AngriesNodePath);

        var tween = new Tween();
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                // if (RandomHelpers.DrawResult(10)) continue;
                var tile = TileScene.Instance<Tile>();
                tile.GlobalPosition = new Vector2((float)x * TileSize, (float)y * TileSize);
                // tile.Scale = Vector2.Zero;

                // tween.InterpolateProperty(tile, "scale", Vector2.Zero, Vector2.One * 4f, .9f, Tween.TransitionType.Linear, Tween.EaseType.In, (float)(x + y) * .032f);
                AddChild(tile);

                // if (RandomHelpers.DrawResult(2))
                // {
                // 	tile.ChangeGroup("Fertilized");
                // }
                if (RandomHelpers.DrawResult(60))
                {
                    tile.ChangeGroup("Corn");
                }
                // else if ((RandomHelpers.DrawResult(50)))
                // {
                // 	tile.ChangeGroup("Grass");
                // }
                else
                {
                    tile.ChangeGroup("Dirt");
                }
                // else if (RandomHelpers.DrawResult(3))
                // {
                //     tile.Modulate = Color.ColorN("Blue", 1);
                // }

                tile.PositionIndex = Width * y + x;
                tile.X = x;
                tile.Y = y;

                Field.Add(tile.PositionIndex, tile);


				// GD.Print(tile.X, tile.Y, tile.PositionIndex);

            }
        }

        AddChild(tween);
        tween.Start();

        CurrentDay = StartingDay + 1;

        GrowTime = GrowTime * (DayLengthCurve.Interpolate((float)(CurrentDay) / (float)MaxNumberOfDays));
        GrowCooldown = new Cooldown(GrowTime, this);
        GrowCooldown.Use();
        ShopCooldown = new Cooldown(ShopCooldownTime, this);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        if (LockMouseToWindow)
        {
            Input.MouseMode = Input.MouseModeEnum.Confined | Input.MouseModeEnum.Hidden;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Hidden;
        }

        if (Input.IsActionJustPressed("DEBUG_spawn_angries"))
        {
            SpawnAngryPlant();
            Camera.Shake(1.4f, 2f);
        }

        if (TriggerEndOfGame)
        {
            Player.Disabled = true;
            GameOverPanel.Visible = true;
            return;
        }

        UpdateTileGroupCount();

        if (Player.HarvestedCropsCount >= NextHarvestShopCount)
        {
            NextHarvestShopCount += StartingNextHarvestCount;
            StartingNextHarvestCount += NextHarvestCountModifier;
            Shop.OpenShop();
        }


        if (GrowCooldown.Use())
        {
            CurrentDay++;

            GrowCrops();
            GrowWeeds();

            SpawnWeeds();

            SpawnAngriesToday = (CurrentDay) % 5 == 0;

            // if (CurrentDay > 0 && (CurrentDay) % SpawnFrequency == 0)

            GrowTime = GrowCooldown.Time * (DayLengthCurve.Interpolate((float)(CurrentDay) / (float)MaxNumberOfDays));
            GrowCooldown.Reset(GrowTime);
            GrowCooldown.Use();
        }

        if (SpawnAngriesToday && GrowCooldown.TimeLeft() < GrowTime * .5f)
        {
            SpawnAngriesToday = false;
            SpawnAngries();
        }

        if (OpenShopToday && GrowCooldown.TimeLeft() < GrowTime * .25f)
        {
            OpenShopToday = false;
            Shop.OpenShop();
        }

        TriggerEndOfGame = WeedTiles.Count >=  FieldCount * PercentEndGame;
    }

    private void UpdateTileGroupCount()
    {
        GrassTiles = GetTree().GetNodesInGroup("Grass").Cast<Tile>().ToList();
        FertilizedTiles = GetTree().GetNodesInGroup("Fertilized").Cast<Tile>().ToList();
        DirtTiles = GetTree().GetNodesInGroup("Dirt").Cast<Tile>().ToList();
        CornTiles = GetTree().GetNodesInGroup("Corn").Cast<Tile>().ToList();
        WeedTiles = GetTree().GetNodesInGroup("Weed").Cast<Tile>().ToList();
        DisabledTiles = GetTree().GetNodesInGroup("Disabled").Cast<Tile>().ToList();
        WaterTiles = GetTree().GetNodesInGroup("WaterTile").Cast<Tile>().ToList();

        FieldCount = Field.Count - WaterTiles.Count - DisabledTiles.Count;
        WeedProgress = (float)WeedTiles.Count / (float)Field.Count;
    }

    private void SpawnAngries()
    {
        var weedRatio = (float)WeedTiles.Count / (float)Field.Count;
        if (CurrentDay == StartingDay + 5)
        {
            SpawnAngryPlant();
            Camera.Shake(1.4f, 2f);
        }

        if (CurrentDay == StartingDay + 10)
        {
            for (var i = 0; i < 3; i++)
            {
                SpawnAngryPlant();
            }

            Camera.Shake(1.4f, 2f);
        }

        if (!Day15Done && (CurrentDay == StartingDay + 15 || weedRatio > .7f))
        {
            Day15Done = true;
            for (var i = 0; i < 5; i++)
            {
                SpawnAngryPlant();
            }

            SpawnAngryCorn();
            Camera.Shake(2f, 3f);
        }

        else if (CurrentDay >= StartingDay + 20 && CurrentDay % 5 == 0)
        {
            MadnessLevel++;
            for (var i = 0; i < Math.Min(MadnessLevel, 3); i++)
            {
                SpawnAngryPlant();
            }

            for (var i = 0; i < Math.Max(MadnessLevel, 2); i++)
            {
                SpawnAngryCorn();
            }

            if (MadnessLevel >= 3)
            {
                SpawnAngryTomato();
            }

            if (MadnessLevel >= 5)
            {
                SpawnAngryEggplant();
            }

            if (MadnessLevel >= 6)
            {
                SpawnAngryTomato();
            }

            Camera.Shake(2f, 3f);
        }
    }

    private static Vector2 GetRandomPositionOnPlayArea()
    {
        return new Vector2(RandomHelpers.RangeInt(400, 1200), RandomHelpers.RangeInt(200, 600));
    }

    private void SpawnAngryAtRandomPosition(BaseAngry angry)
    {
        SpawnAngryAt(angry, GetRandomPositionOnPlayArea());
    }
    private void SpawnAngryAt(BaseAngry angry, Vector2 position)
    {
        angry.Health = (int)((float)angry.Health * (1f + AngryHealthCurve.Interpolate((float)CurrentDay / (float)MaxNumberOfDays)));
        angry.GlobalPosition = position;
        Angries.AddChild(angry);
    }


    private void SpawnAngryPlant()
    {
        var angryPlant = AngryPlantScene.Instance<BaseAngry>();
        SpawnAngryAtRandomPosition(angryPlant);
    }

    private void SpawnAngryTomato()
    {
        var angryTomato = AngryTomatoScene.Instance<BaseAngry>();
        SpawnAngryAtRandomPosition(angryTomato);
    }

    private void SpawnAngryEggplant()
    {
        var angryEggplant = AngryEggplantScene.Instance<BaseAngry>();
        SpawnAngryAtRandomPosition(angryEggplant);
    }

    private void SpawnAngryCorn()
    {
        var angryCorn = AngryCornScene.Instance<BaseAngry>();
        SpawnAngryAtRandomPosition(angryCorn);
    }

    private void SpawnAngryPlantAt(Vector2 position)
    {
        var angryPlant = AngryPlantScene.Instance<BaseAngry>();
        SpawnAngryAt(angryPlant, position);
    }

    private void SpawnAngryTomatoAt(Vector2 position)
    {
        var angryTomato = AngryTomatoScene.Instance<BaseAngry>();
        SpawnAngryAt(angryTomato, position);
    }

    private void SpawnAngryEggplantAt(Vector2 position)
    {
        var angryEggplant = AngryEggplantScene.Instance<BaseAngry>();
        SpawnAngryAt(angryEggplant, position);
    }

    private void SpawnAngryCornAt(Vector2 position)
    {
        var angryCorn = AngryCornScene.Instance<BaseAngry>();
        SpawnAngryAt(angryCorn, position);
    }


    private void SpawnWeeds()
    {
        var numberOfSpawns = (int)(SpawnFactor * ((float)CurrentDay * (float)CurrentDay) + 1f);

        for (var i = 0; i < numberOfSpawns; i++)
        {
            if (DirtTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, DirtTiles.Count);
                DirtTiles[randomIndex].ChangeGroup("Weed");
            }
            else if (FertilizedTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, FertilizedTiles.Count);
                FertilizedTiles[randomIndex].ChangeGroup("Weed");
            }
            else if (GrassTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, GrassTiles.Count);
                GrassTiles[randomIndex].ChangeGroup("Weed");
            }
            else if (CornTiles.Count > 0)
            {
                var randomIndex = RandomHelpers.RangeInt(0, CornTiles.Count);
                CornTiles[randomIndex].ChangeGroup("Weed");
            }
        }

        UpdateTileGroupCount();
    }

    private void GrowCrops()
    {
        foreach (var tile in GrassTiles)
        {
            if (RandomHelpers.DrawResult(3))
            {
                tile.ChangeGroup("Corn", true);
            }
        }

        foreach (var tile in FertilizedTiles)
        {
            if (RandomHelpers.DrawResult(3))
            {
                tile.ChangeGroup("Grass", true);
            }
        }
    }

    private void GrowWeeds()
    {
        var neighbors = new int[4];
        var numberOfAngriesSpawned = 0;
        foreach (var tile in WeedTiles)
        {
            // tile.Grow();

            if (RandomHelpers.DrawResult(10))
            {
                tile.Grow();
            }

            neighbors[0] = Width * (tile.Y - 1) + tile.X;
            neighbors[1] = Width * (tile.Y + 1) + tile.X;
            neighbors[2] = Width * (tile.Y) + tile.X + 1;
            neighbors[3] = Width * (tile.Y) + tile.X - 1;

            for (var i = 0; i < 4; i++)
            {
                if (RandomHelpers.DrawResult(4) && neighbors[i] >= 0 && neighbors[i] < Field.Count)
                {
                    Field[neighbors[i]].ChangeGroup("Weed", true);
                }
            }

            tile.ModulateFromGroup();

            if (tile.Stage >= 7)
            {
                SpawnAngryEggplantAt(tile.GlobalPosition);
                tile.ChangeGroup("Dirt");
            }
            else if (tile.Stage >= 5)
            {
                SpawnAngryCornAt(tile.GlobalPosition);
            }
            else if (tile.Stage >= 3 && numberOfAngriesSpawned < 2)
            {
                numberOfAngriesSpawned++;
                SpawnAngryPlantAt(tile.GlobalPosition);
            }
        }
    }

    public bool InDanger()
    {
        return WeedProgress > PercentEndGame * PercentDanger;
    }
}

