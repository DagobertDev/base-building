using BaseBuilding;
using BaseBuilding.Components;
using DefaultEcs;
using Godot;

public partial class EntityDisplay : Label
{
	private readonly EntityMultiMap<Tile> _entitiesByTile;

	public EntityDisplay()
	{
		_entitiesByTile = Main.World.GetEntities().AsMultiMap<Tile>();
	}

	public override void _Process(double delta)
	{
		var tileMap = GetNode<TileMap>("%Map");
		var position = tileMap.GetLocalMousePosition();
		var tile = tileMap.LocalToMap(position);

		if (!_entitiesByTile.TryGetEntities(tile, out var entities))
		{
			Text = null;
			return;
		}

		var entity = entities[0];
		Text = GetStringForEntity(entity);
	}

	private string GetStringForEntity(Entity entity)
	{
		return entity.Get<Sprite2D>().Name;
	}
}
