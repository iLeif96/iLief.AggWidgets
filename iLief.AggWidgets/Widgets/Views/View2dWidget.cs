using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using iLeif.Extensions.Logging;

using MatterHackers.Agg;
using MatterHackers.Agg.VertexSource;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
	public class View2dWidget : ViewBaseWidget, IView2d
	{
		public ItemBounds TotalBounds { get; protected set; } = new();
		public bool IsNeedToDrawGrid { get; set; } = true;
		public bool IsUseAnimation { get => _tumbleWidget.IsUseAnimation; set => _tumbleWidget.IsUseAnimation = value; }

		protected HashSet<Item2d> _renderItems = new();
		protected HashSet<Item2d> _debugRenderItems = new();

		protected AffineTumbleWidget _tumbleWidget;

		protected GridDrawer _gridDrawer;

		public View2dWidget(ILogger logger) : base(true, logger)
		{
			_tumbleWidget = new AffineTumbleWidget(_logger);
			_gridDrawer = new GridDrawer(_logger, _tumbleWidget, 100);
			IsUseAnimation = false;
		}

		public override void Initialize()
		{
			if (Initialized)
			{
				return;
			}

			DrawLayer.SizeChanged += (sender, ev) => _tumbleWidget.SetScreenBounds(DrawLayer.LocalBounds);

			//DrawLayer.BackgroundColor = new Color(0, 100, 0);
			DrawLayer.BackgroundColor = Color.Transparent;
			DrawLayer.AddChild(_tumbleWidget);
			base.Initialize();

			Initialized = true;
		}


		public override void OnDraw(Graphics2D graphics2D)
		{

			if (ActionOnDraw == null && !_renderItems.Any() && !IsNeedToDrawGrid)
			{
				return;
			}

			string statusString = "";

			if (IsNeedToDrawGrid)
			{
				statusString += $"Cell size: {Math.Round(_gridDrawer.CellSize, 2)};";
			}

			_tumbleWidget.SetScreenBounds(DrawLayer.LocalBounds);
			var affine = _tumbleWidget.GetWorldAffine();
			statusString += $" {Math.Round(affine.GetScale(), 2)}x;";
			StatusBar.WriteLine(statusString);
			//graphics2D.PushTransform();
			graphics2D.SetTransform(affine);

			if (IsNeedToDrawGrid)
			{
				_gridDrawer.Draw(graphics2D);
			}

			base.OnDraw(graphics2D);

			RenderFromStore(graphics2D, _renderItems);

#if DEBUG
			RenderFromStore(graphics2D, _debugRenderItems);
#endif


			//graphics2D.FillRectangle(100, 100, 400, 400, Color.LightBlue);

			graphics2D.PopTransform();
		}

		private void RenderFromStore(Graphics2D graphics2D, IEnumerable<Item2d> items)
		{
			foreach (var renderItem in items)
			{
				if (renderItem.Verteces != null)
				{
					//TODO: sometimes null vertexes
					try
					{
						if (renderItem.IsPolygons)
						{
							graphics2D.Render(renderItem.Verteces, renderItem.Color);
						}

						if (renderItem.DrawStroke == true)
						{
							//double scaledWidth = renderItem.StrokeWidth / affine.GetScale();

							graphics2D.Render(new Stroke(renderItem.Verteces, renderItem.StrokeWidth), renderItem.StrokeColor);
						}
					}
					catch
					{
						_logger.Error("Can`t render vertexes");
					}
				}
			}
		}
		public override void ResetView()
		{
			_tumbleWidget.SetWorldBaseToWorldCenter();
			_tumbleWidget.ResetWorld();
			base.ResetView();
		}

		public override void ClearScene()
		{
			_renderItems.Clear();
			ResolveBounds();

			base.ClearScene();
		}

		protected override void FitIntoView()
		{
			//TODO: need to compute all AABB of drawing objects and fit them into view
			//For example: TotalBounds = new RectangeBounds(100, 100, 400, 400);

			_tumbleWidget.FitIntoView(TotalBounds);
		}

		public void AddDebugRenderItem(Item2d item)
		{
			if (item == null || double.IsInfinity(item.Bounds.Width) || double.IsNaN(item.Bounds.Width))
			{
				return;
			}

			TotalBounds.Expand(item.Bounds);
			_debugRenderItems.Add(item);
		}

		public void ClearDebugRenderItems()
		{
			_debugRenderItems.Clear();
		}

		public void AddRenderItem(Item2d item)
		{
			if (item == null || double.IsInfinity(item.Bounds.Width) || double.IsNaN(item.Bounds.Width))
			{
				return;
			}

			TotalBounds.Expand(item.Bounds);
			_renderItems.Add(item);
		}

		public void AddRenderItems(IEnumerable<Item2d> items)
		{
			foreach (var item in items)
			{
				if (item == null || double.IsInfinity(item.Bounds.Width) || double.IsNaN(item.Bounds.Width))
				{
					return;
				}

				TotalBounds.Expand(item.Bounds);
				_renderItems.Add(item);
			}
		}

		public void RemoveRenderItem(Item2d item, bool autoResolveBounds = true)
		{
			_renderItems.Remove(item);

			if (autoResolveBounds)
			{
				ResolveBounds();
			}
		}

		public void RemoveRenderItems(List<Item2d> list, bool autoResolveBounds = true)
		{
			foreach (var item in list)
			{
				_renderItems.Remove(item);
			}

			if (autoResolveBounds)
			{
				ResolveBounds();
			}
		}

		public void ResolveBounds(bool hard = false)
		{
			TotalBounds = new ItemBounds();

			if (_renderItems.Any() == false)
			{
				return;
			}

			if (hard)
			{
				foreach (var item in _renderItems)
				{
					item.ResolveBounds();
					TotalBounds.Expand(item.Bounds);
				}

				return;
			}

			foreach (var item in _renderItems)
			{
				TotalBounds.Expand(item.Bounds);
			}
		}
	}
}
