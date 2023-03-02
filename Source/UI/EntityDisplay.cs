using BaseBuilding;
using BaseBuilding.Components;
using DefaultEcs;
using Godot;

public partial class EntityDisplay : Label
{
	private readonly EntityMultiMap<Tile> _entitiesByTile;
	private Entity _selectedEntity;

	public EntityDisplay()
	{
		_entitiesByTile = Main.World.GetEntities().AsMultiMap<Tile>();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed(InputActions.MouseclickLeft))
		{
			GetTree().Root.SetInputAsHandled();
			var tileMap = GetNode<TileMap>("%Map");
			var position = tileMap.GetLocalMousePosition();
			var tile = tileMap.LocalToMap(position);

			if (!_entitiesByTile.TryGetEntities(tile, out var entities))
			{
				Text = null;
				return;
			}

			_selectedEntity = entities[0];
		}
		else if (_selectedEntity.IsAlive && @event.IsActionPressed(InputActions.MouseclickRight))
		{
			GetTree().Root.SetInputAsHandled();
			_selectedEntity = default;
		}
	}

	public override void _Process(double delta)
	{
		if (!_selectedEntity.IsAlive)
		{
			Text = null;
			return;
		}

		Text = GetStringForEntity(_selectedEntity);
	}

	private string GetStringForEntity(Entity entity)
	{
		return entity.Get<Sprite2D>().Name;
	}
}
