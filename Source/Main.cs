using System;
using Godot;

namespace BaseBuilding;

public partial class Main : Node
{
	public const int MapSize = 128;

	public override void _Ready()
	{
		var tileMap = GetNode<TileMap>("%Map");

		var random = new Random();

		for (var x = 0; x < MapSize; x++)
		{
			for (var y = 0; y < MapSize; y++)
			{
				var tile = random.Next(2);
				tileMap.SetCell(0, new Vector2i(x, y), 1, new Vector2i(tile, 0));
			}
		}
	}
}
