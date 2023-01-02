using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

namespace BaseBuilding.Assets;

public class BuildingCollection
{
	// TODO: Load buildings dynamically
	private readonly IReadOnlyCollection<Building> _buildings = ImmutableArray.Create(
		Create("Remove", "res://Assets/remove_building.png", "res://Assets/remove_building.png"),
		Create("Wall", "res://Assets/wall.png", "res://Assets/wall_outline.png")
	);

	public Building Remove => GetBuilding("Remove");

	public Building GetBuilding(string id)
	{
		var x = _buildings;
		return _buildings.Single(building => building.Name.Equals(id, StringComparison.InvariantCultureIgnoreCase));
	}

	private static Building Create(string name, string texture, string ghostTexture)
	{
		return new Building(name, GD.Load<Texture2D>(texture), GD.Load<Texture2D>(ghostTexture));
	}
}
