using System;
using System.Collections.Generic;
using System.Linq;

public static class RandomHelpers
{
	static Random Randi = new Random();

	/// <summary>
	/// Gets a draw result. Great for 1 out of N chances.
	/// </summary>
	/// <param name="max">Max number to pick from.</param>
	/// <returns></returns>
	public static bool DrawResult(int max, Random random = null)
	{
		var first = (random ?? Randi).Next(max);
		var second = (random ?? Randi).Next(max);

		return first == second;
	}

	public static int NextInt(Random random = null)
	{
		return (random ?? Randi).Next();
	}

	public static double NextDouble(Random random = null)
	{
		return (random ?? Randi).NextDouble();
	}

	public static float NextFloat(Random random = null)
	{
		return (float)(random ?? Randi).NextDouble();
	}

	public static int RangeInt(int min = 0, int max = Int32.MaxValue, Random random = null)
	{
		return (random ?? Randi).Next(min, max);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <param name="tightness"></param>
	/// <param name="exp">Higher the value, lower the result</param>
	/// <param name="random"></param>
	/// <returns></returns>
	public static double DistributedRangeDouble(double min = 0, double max = Double.MaxValue, int tightness = 1, double exp = 0, Random random = null)
	{
		double total = 0.0;
		for (int i = 1; i <= tightness; i++)
		{
			total += Math.Pow((random ?? Randi).NextDouble(), exp);
		}

		return ((total / tightness) * (max - min)) + min;
	}

	public static double RangeDouble(double min = 0, double max = Double.MaxValue, Random random = null)
	{
		return (random ?? Randi).NextDouble() * (max - min) + min;
	}

	public static T[] PickN<T>(this T[] source, int count = 1, Random random = null)
	{
		var rnd = random ?? new Random();

		var items = source.ToArray();        

		// Shuffle the list using a Fisher-Yates shuffle
		for (int i = items.Length - 1; i > 0; i--)
		{
			int j = rnd.Next(i + 1);
			var temp = items[i];
			items[i] = items[j];
			items[j] = temp;
		}

		return items.Take(count).ToArray();
	}
}
