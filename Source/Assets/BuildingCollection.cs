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
		new Building("Remove", GD.Load<Texture2D>("res://Assets/remove_building.png")),
		new Building("Wall", GD.Load<Texture2D>("res://Assets/wall.png"))
	);

	public Building Remove => GetBuilding("Remove");

	public Building GetBuilding(string id)
	{
		var x = _buildings;
		return _buildings.Single(building => building.Name.Equals(id, StringComparison.InvariantCultureIgnoreCase));
	}
}
