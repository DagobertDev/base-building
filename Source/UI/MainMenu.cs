using System.Collections.Generic;
using System.Linq;
using BaseBuilding;
using Godot;
using Newtonsoft.Json;

public partial class MainMenu : Control
{
	private string _saveFile = "";

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed(InputActions.Escape))
		{
			GetTree().Root.SetInputAsHandled();

			ToggleVisibility();
		}
	}

	public void SaveGame()
	{
		var tileMap = GetNode<TileMap>("%Map");

		var tiles = new Vector2i[Main.MapSize * Main.MapSize];

		for (var x = 0; x < Main.MapSize; x++)
		{
			for (var y = 0; y < Main.MapSize; y++)
			{
				tiles[x + y * Main.MapSize] = tileMap.GetCellAtlasCoords(0, new Vector2i(x, y));
			}
		}

		var walls = tileMap.GetChildren().Cast<Sprite2D>()
			.Select(sprite => new WallData((Vector2i)sprite.Position, sprite.Texture.ResourcePath)).ToList();
		
		var saveData = new SaveData(Main.MapSize, Main.MapSize, tiles, walls);

		_saveFile = JsonConvert.SerializeObject(saveData);
	}

	public void LoadGame()
	{
		var tileMap = GetNode<TileMap>("%Map");

		foreach (var node in tileMap.GetChildren())
		{
			node.QueueFree();
		}

		var saveData = JsonConvert.DeserializeObject<SaveData>(_saveFile);

		if (saveData is null)
		{
			return;
		}

		for (var x = 0; x < saveData.MapWidth; x++)
		{
			for (var y = 0; y < saveData.MapHeight; y++)
			{
				tileMap.SetCell(0, new Vector2i(x, y), 1, saveData.Tiles[x + y * saveData.MapWidth]);
			}
		}

		foreach (var wallData in saveData.Walls)
		{
			tileMap.AddChild(new Sprite2D
			{
				Texture = GD.Load<Texture2D>(wallData.Texture),
				GlobalPosition = wallData.Position,
				ZIndex = 1,
			});
		}
	}

	public void ToggleVisibility()
	{
		Visible = !Visible;
	}

	private record SaveData(int MapWidth, int MapHeight, Vector2i[] Tiles, IEnumerable<WallData> Walls);
	private record WallData(Vector2i Position, string Texture);
}
