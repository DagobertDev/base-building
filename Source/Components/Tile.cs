using Godot;

namespace BaseBuilding.Components;

public readonly record struct Tile(Vector2I Value)
{
	public static implicit operator Vector2I(Tile position)
	{
		return position.Value;
	}

	public static implicit operator Tile(Vector2I vector)
	{
		return new Tile(vector);
	}
}
