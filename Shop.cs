using Godot;
using System;
using System.Linq;

public class Shop : PanelContainer
{
	private const int NumberOfBuyableItems = 3;
	[Export] public NodePath PlayerNodePath;
	public Player Player;

	[Export] public NodePath PlayAreaNodePath;
	public PlayArea PlayArea;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Player = GetNode<Player>(PlayerNodePath);
		PlayArea = GetNode<PlayArea>(PlayAreaNodePath);
	}


	public void OpenShop()
	{
		GetTree().Paused = true;
		Visible = true;

		RollPerks(false);
	}

	public void Buy(int index)
	{
		var boughtPerk = BuyablePerks[index];
		boughtPerk.Effect(this, boughtPerk);
		Player.GoldCoins = Player.GoldCoins - boughtPerk.Cost;
		Player.AddPerk(boughtPerk);

		if (boughtPerk.Unique)
		{
			Perks = Perks.Where(p => p != boughtPerk).ToArray();
		}

		CloseAndContinue();
	}

	private void CloseAndContinue()
	{
		GetTree().Paused = false;
		Visible = false;
	}

	private void RollPerks(bool spend)
	{
		if (spend)
		{
			Player.GoldCoins -= 50;
		}
		// pick 3 random perks
		BuyablePerks = Perks
			.Where(perk => perk.CanSpawn(this))
			.ToArray()
			.PickN(NumberOfBuyableItems);

		for (var i = 0; i < BuyablePerks.Length; i++)
		{
			var path = "ShopContainer/Items/Item" + i;

			GetNode<Label>(path + "/Name").Text = BuyablePerks[i].Name;
			GetNode<Button>(path + "/Buy").Text = $"$ {BuyablePerks[i].Cost}";
			GetNode<Button>(path + "/Buy").Disabled = Player.GoldCoins < BuyablePerks[i].Cost;
		}

		GetNode<Button>("ShopContainer/HBoxContainer/RerollItems").Disabled = Player.GoldCoins < 50;

	}

	public void _on_Continue_pressed()
	{
		CloseAndContinue();
	}

	public void _on_Buy_Item1_pressed()
	{
		Buy(0);
	}
	public void _on_Buy_Item2_pressed()
	{
		Buy(1);
	}
	public void _on_Buy_Item3_pressed()
	{
		Buy(2);
	}

	public void _on_RerollItems_pressed()
	{
		RollPerks(true);
	}

	public Perk[] Perks = new Perk[] {
		new Perk
		{
			Name = "More Shurikens (+1)",
			Description = "You throw one more shuriken.",
			Cost = 100,
			Effect = (shop, perk) => {
				shop.Player.ScythCount++;
				perk.Cost = perk.Cost + (perk.Cost * perk.Tier);
				perk.Tier++;

			}
		},
		new Perk
		{
			Name = "Longer Shurikens (+1s)",
			Description = "Your shurikens last longer.",
			Cost = 100,
			Effect = (shop, perk) => {
				shop.Player.ShurikenLifeTime = shop.Player.ShurikenLifeTime + 1f;
				perk.Cost = perk.Cost + (perk.Cost * perk.Tier);
				perk.Tier++;
			}

		},
		new Perk
		{
			Name = "Wider Shurikens (+25%)",
			Description = "Your shurikens are wider",
			Cost = 100,
			Effect = (shop, perk) => {
				shop.Player.ShurikenSize = (int)((float)shop.Player.ShurikenSize * 1.25f);
				perk.Cost = perk.Cost + (perk.Cost * perk.Tier);
				perk.Tier++;
			}

		},
		new Perk
		{
			Name = "Wider Planting (+2m)",
			Description = "You have a wider planting area when dashing.",
			Cost = 100,
			Effect = (shop, perk) => {
				shop.Player.GrowSize();
				perk.Cost = perk.Cost + (perk.Cost * perk.Tier);
				perk.Tier++;
			}

		},
		new Perk
		{
			Id = "weed-killer",
			Name = "Weed Killer",
			Description = "Dashing onto weed removes it.",
			Cost = 500,
			Unique = true,
			Effect = (shop, perk) => {
				shop.Player.WeedKiller = true;
			}

		},
		new Perk
		{
			Id = "weed-stomper",
			Name = "Weed Stomper",
			Description = "Running on weed removes it.",
			Cost = 1500,
			CanSpawn = (shop) => shop.Player.ActivePerks.Any(perk => perk.Id == "weed-killer"),
			Unique = true,
			Effect = (shop, perk) => {
				shop.Player.WeedStomper = true;
			}
		},
		new Perk
		{
			Name = "Speed Increase (+10%)",
			Description = "You run faster.",
			Cost = 100,
			Effect = (shop, perk) => {
				shop.Player.speed = shop.Player.speed * 1.1f;
				perk.Cost = perk.Cost + (perk.Cost * perk.Tier);
				perk.Tier++;
			}

		},
		new Perk
		{
			Name = "Dash Longer (+10%)",
			Description = "Your dash effect last longer.",
			Cost = 100,
			Unique = true,
			Effect = (shop, perk) => {
				shop.Player.dashDuration = shop.Player.dashDuration * 1.10f;
				perk.Cost = perk.Cost + (perk.Cost * perk.Tier);
				perk.Tier++;
			}
		},
		// new Perk
		// {
		//     Id = "farmer",
		//     Name = "Farmer (+1 crop stage)",
		//     Cost = 500,
		//     Effect = (shop) => {
		//         // todo
		//     }
		// },
		new Perk
		{
			Id = "Crop Gold",
			Name = "Crop Gold (+5 gold per)",
			Description = "Crops yield more gold.",
			Cost = 100,
			Effect = (shop, perk) => {
				shop.Player.BonusCropGold += 5;
				perk.Cost = perk.Cost + (perk.Cost * perk.Tier);
				perk.Tier++;
			}
		},

		new Perk
		{
			Id = "cut-weed-1",
			Name = "Cut Weed is Fertilized",
			Cost = 5000,
			Unique = true,
			Effect = (shop, perk) => {
				// noop
			}
		},
		new Perk
		{
			Id = "cut-weed-2",
			Name = "Cut Weed is Fertilized x2",
			Cost = 5000,
			Unique = true,
			CanSpawn = (shop) => shop.Player.ActivePerks.Any( perk => perk.Id == "cut-weed-1" ),
			Effect = (shop, perk) => {
				// noop
			}

		},
		new Perk
		{
			Id = "cut-weed-3",
			Name = "Cut Weed becomes corn",
			Cost = 10000,
			CanSpawn = (shop) => shop.Player.ActivePerks.Any( perk => perk.Id == "cut-weed-2" ),
			Unique = true,
			Effect = (shop, perk) => {
				// noop
			}

		},
		new Perk
		{
			Id = "cut-weed-4",
			Name = "Cut Weed becomes gold",
			CanSpawn = (shop) => shop.Player.ActivePerks.Any( perk => perk.Id == "cut-weed-3" ),
			Cost = 20000,
			Unique = true,
			Effect = (shop, perk) => {
				// noop
			}

		},
	};
	private Perk[] BuyablePerks;
}


public class Perk
{
	public int Tier = 1;
	public string Id;
	public string Name;
	public int Cost;
	public Action<Shop, Perk> Effect;
	public bool Unique;
	public Func<Shop, bool> CanSpawn = (shop) => true;
	public string Description;
}
