using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using iLeif.Extensions.Logging;
using iLeif.Extensions.Numbers;

using MatterHackers.Agg;
using MatterHackers.Agg.Transform;
using MatterHackers.Agg.VertexSource;
using MatterHackers.VectorMath;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
	public class GridDrawer
	{
		private class GridDrawInputs
		{
			public double XOffset;
			public double YOffset;
			public int XSteps;
			public int YSteps;

			public Vector2 Min;
			public Vector2 Max;

			public double ScaledStep;

			public bool IsFirstMainX;
			internal bool IsFirstMainY;
		}

		public Color ColorAxis { get; set; } = new Color(50, 50, 50, 0.7);
		public Color MainLine { get; set; } = new Color(10, 10, 10, 0.2);
		public Color SubLine { get; set; } = new Color(60, 60, 60, 0.2);

		public double CellSize { get; private set; }

		public int GridStep { get; set; }

		private ILogger _logger;
		private AffineTumbleWidget _tumbleWidget;

		private VertexStorage? _mains;
		private VertexStorage? _subs;

		public GridDrawer(ILogger logger, AffineTumbleWidget tumbleWidget, int gridStep = 50)
		{
			_logger = logger;
			GridStep = gridStep;
			CellSize = GridStep;
			_tumbleWidget = tumbleWidget;
		}

		public void Draw(Graphics2D g2D)
		{
			Affine affine = _tumbleWidget.GetWorldAffine();
			Vector2 center = new Vector2(0, 0);

			double width = _tumbleWidget.Width;
			double height = _tumbleWidget.Height;

			double pixel = 1 / affine.GetScale();

			GridDrawInputs dIn = ResolveDrawingInputs(center, width, height, affine);
			CellSize = dIn.ScaledStep;

			Vector2 min = dIn.Min;
			Vector2 max = dIn.Max;

			_mains = new VertexStorage();
			_subs = new VertexStorage();

			//verteces.Ad

			Color one, two;
			if (dIn.IsFirstMainX)
			{
				one = MainLine;
				two = SubLine;
			}
			else
			{
				one = SubLine;
				two = MainLine;
			}

			for (int i = 0; i <= dIn.XSteps; i++)
			{
				double next = dIn.XOffset + (min.X + dIn.ScaledStep * i);

				if (i % 2 == 0)
				{

					AllocateToVertexes(new Vector2(next, min.Y), new Vector2(next, max.Y), one);
				}
				else
				{
					AllocateToVertexes(new Vector2(next, min.Y), new Vector2(next, max.Y), two);
				}
			}

			if (dIn.IsFirstMainY)
			{
				one = MainLine;
				two = SubLine;
			}
			else
			{
				one = SubLine;
				two = MainLine;
			}

			for (int i = 0; i <= dIn.YSteps; i++)
			{
				double next = dIn.YOffset + (min.Y + dIn.ScaledStep * i);

				if (i % 2 == 0)
				{
					AllocateToVertexes(new Vector2(min.X, next), new Vector2(max.X, next), one);
				}
				else
				{
					AllocateToVertexes(new Vector2(min.X, next), new Vector2(max.X, next), two);
				}
			}

			if (_mains?.Count > 0)
			{
				g2D.Render(new Stroke(_mains, pixel), MainLine);
				//g2D.Render(_mains, MainLine);
			}

			if (_subs?.Count > 0)
			{
				g2D.Render(new Stroke(_subs, pixel), SubLine);
				//g2D.Render(_subs, SubLine);
			}

			g2D.Line(new Vector2(center.X, min.Y), new Vector2(center.X, max.Y), ColorAxis, pixel);
			g2D.Line(new Vector2(min.X, center.Y), new Vector2(max.X, center.Y), ColorAxis, pixel);

			//g2D.Line(new Vector2(500, center.Y), new Vector2(-1000, center.Y), Color.Black, pixel);
		}

		private GridDrawInputs ResolveDrawingInputs(Vector2 center, double width, double height, Affine affine)
		{
			Affine inverted = affine;
			inverted.invert();

			Vector2 viewMin = inverted.Transform(_tumbleWidget.TransformToScreenSpace(new Vector2(0, 0)));
			Vector2 viewMax = inverted.Transform(_tumbleWidget.TransformToScreenSpace(new Vector2(width, height)));

			double zoom = affine.GetScale();

			double scaledStep = GridStep;
			if (zoom >= 1.5)
			{
				double nearQuad = Math.Ceiling(Math.Log2(zoom)) ; //Find nearest pow
				scaledStep = GridStep / Math.Pow(2, nearQuad);
				//scaledStep = GridStep / (Math.Floor(zoom));

				//_logger.Info($"div: {Math.Round(zoom, 3)}, " + $"nearQuad: {Math.Round(nearQuad, 3)}, " +
				//	$"Cells: {(int)(height / scaledStep / zoom)} Step: {Math.Round(scaledStep, 3)}, " + $"Cell size:  {Math.Round(scaledStep * zoom)}");
			}
			else if (zoom <= 0.5)
			{
				double scaleRelation = 1 / zoom;
				double nearQuad = Math.Floor(Math.Log2(scaleRelation)); //Find nearest pow
				scaledStep = GridStep * Math.Pow(2, nearQuad);

				//_logger.Info($"div: {Math.Round(scaleRelation, 3)}, " + $"nearQuad: {Math.Round(nearQuad, 3)}, " +
				//	$"Cells: {(int)(height / scaledStep / zoom)} Step: {Math.Round(scaledStep, 3)}, " + $"Cell size:  {Math.Round(scaledStep * zoom)}");

			}
			//(Math.Floor(scaleRelation)
			int xSteps = (int)((width / scaledStep) / zoom);
			int ySteps = (int)((height / scaledStep) / zoom);

			Vector2 tCenterToTMim = center - viewMin;

			var xOffset = tCenterToTMim.X % (scaledStep);
			var yOffset = tCenterToTMim.Y % (scaledStep);

			var isFirstMainX = Math.Floor(tCenterToTMim.X / scaledStep) % 2 == 0;
			var isFirstMainY = Math.Floor(tCenterToTMim.Y / scaledStep) % 2 == 0;

			GridDrawInputs drawInputs = new GridDrawInputs
			{
				Min = viewMin,
				Max = viewMax,
				XSteps = xSteps,
				YSteps = ySteps,
				XOffset = xOffset,
				YOffset = yOffset,
				ScaledStep = scaledStep,
				IsFirstMainX = isFirstMainX,
				IsFirstMainY = isFirstMainY
			};

			return drawInputs;
		}

		private void AllocateToVertexes(Vector2 start, Vector2 end, Color color)
		{
			if (color == MainLine)
			{
				_mains!.MoveTo(start);
				_mains!.LineTo(end);
			}

			if (color == SubLine)
			{
				_subs!.MoveTo(start);
				_subs!.LineTo(end);
			}
		}
	}
}
