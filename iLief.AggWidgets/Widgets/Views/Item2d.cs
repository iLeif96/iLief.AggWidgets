using MatterHackers.Agg;
using MatterHackers.Agg.VertexSource;
using MatterHackers.VectorMath;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
	public class Item2d 
	{
		public string Name { get; set; } = nameof(Item2d);
		public IVertexSource? Verteces { get; set; }
		public IColorType Color { get; set; } = new Color(10, 10, 10);
		public Vector2? ShiftPosition { get; set; }
		public ItemBounds Bounds { get; set; }
		public bool IsPolygons { get; set; } = true;
		public int StrokeWidth { get; set; } = 20;

		public bool DrawStroke
		{
			get => _drawStroke ?? !IsPolygons;
			set => _drawStroke = value;
		}
		private bool? _drawStroke = null;

		public IColorType StrokeColor
		{
			get => _strokeColor ?? Color.AdjustLightness(0.5);
			set => _strokeColor = value;
		}
		private IColorType? _strokeColor;

		public Item2d(IVertexSource vertexes, IColorType color)
		{
			Verteces = vertexes;
			Color = color;

			Bounds = new ItemBounds(vertexes.GetBounds());
		}

		public Item2d(IVertexSource vertexes, ItemBounds bounds, IColorType color)
		{
			Verteces = vertexes;
			Bounds = bounds;
			Color = color;
		}

		public Item2d(ItemBounds bounds)
		{
			Bounds = bounds;
		}

		public void ResolveBounds()
		{
			if (Verteces != null)
			{
				Bounds = new ItemBounds(Verteces.GetBounds());
			}
		}
	}
}
