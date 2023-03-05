using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MatterHackers.Agg;
using MatterHackers.VectorMath;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
	public class ItemBounds
	{
		public double MinX { get; set; } = default;
		public double MaxX { get; set; } = default;
		public double MinY { get; set; } = default;
		public double MaxY { get; set; } = default;

		public double CenterX => (MaxX - MinX) / 2;
		public double CenterY => (MaxY - MinY) / 2;
		public Vector2 Center => new Vector2(CenterX, CenterY);

		public Vector2 Min => new Vector2(MinX, MinY);
		public Vector2 Max => new Vector2(MaxX, MaxY);

		public double Width => Math.Abs(MaxX - MinX);
		public double Height => Math.Abs(MaxY - MinY);

		public Vector2 Size => new Vector2(Width, Height);


		public ItemBounds() { }

		public ItemBounds(double minX, double minY, double maxX, double maxY)
		{
			CheckForSwap(ref minX, ref maxX);
			CheckForSwap(ref minY, ref maxY);

			MinX = minX;
			MaxX = maxX;
			MinY = minY;
			MaxY = maxY;
		}

		public ItemBounds(Vector2 min, Vector2 max) : this(min.X, min.Y, max.X, max.Y) { }

		public ItemBounds(RectangleDouble rectangle) : this(rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top) { }

		public double GetSquare()
		{
			return Width * Height;
		}

		public void Expand(double minX, double minY, double maxX, double maxY)
		{
			CheckForSwap(ref minX, ref maxX);
			CheckForSwap(ref minY, ref maxY);

			if (minX < MinX)
			{
				MinX = minX;
			}
			if (maxX > MaxX)
			{
				MaxX = maxX;
			}
			if (minY < MinY)
			{
				MinY = minY;
			}
			if (maxY > MaxY)
			{
				MaxY = maxY;
			}
		}

		public void Expand(ItemBounds rectange)
		{
			Expand(rectange.MinX, rectange.MinY, rectange.MaxX, rectange.MaxY);
		}

		public void Expand(RectangleDouble rectange)
		{
			Expand(-rectange.Left, -rectange.Bottom, rectange.Right, rectange.Top);
		}

		public void Expand(double x, double y)
		{
			if (x < MinX)
			{
				MinX = x;
			}
			else if (x > MaxX)
			{
				MaxX = x;
			}

			if (y < MinY)
			{
				MinY = y;
			}
			else if (y > MaxY)
			{
				MaxY = y;
			}
		}

		public void Expand(params Vector2[] vectors)
		{
			foreach (var vec in vectors)
			{
				Expand(vec.X, vec.Y);
			}
		}

		public void Expand(IEnumerable<Vector2> vectors)
		{
			foreach (var vec in vectors)
			{
				Expand(vec.X, vec.Y);
			}
		}

		private void CheckForSwap(ref double min, ref double max)
		{
			if (min > max)
			{
				double tmp = max;
				max = min;
				min = tmp;
			}
		}
	}
}
