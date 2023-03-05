using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using iLeif.CustomAggWidgets.Widgets.Interfaces;
using iLeif.Extensions.Animations;
using iLeif.Extensions.Logging;
using iLeif.Extensions.Numbers;

using MatterHackers.Agg;
using MatterHackers.Agg.Transform;
using MatterHackers.Agg.UI;
using MatterHackers.VectorMath;

namespace iLeif.CustomAggWidgets.Widgets.Views
{

	public class AffineTumbleWidget : GuiWidget, IReinitializable
	{
		public event EventHandler? ReinitializationBegin;
		public event EventHandler? ReinitializationEnd;
		public bool IsUseAnimation { get; set; } = true;

		private Affine _worldCenter = Affine.NewIdentity();
		private Affine _worldBase = Affine.NewIdentity();
		private Affine _transform = Affine.NewIdentity();
		private Affine _world = Affine.NewIdentity();

		private Vector2? _mouseDownInitPosition = null;

		private LimitStack<Vector2> _lastMousePositions = new LimitStack<Vector2>() { Limit = 5 };

		private bool _isAnyChanges = false;

		private Animator _animation;

		private ILogger _logger;

		private long _timeFromLastMove = 0;
		private long _timeForIdle = 100;

		private bool _togglerScale = true;

		private ItemBounds? _screenBounds = null;
		private bool _isWorldCenterInScreenCenter = true;

		public AffineTumbleWidget(ILogger logger)
		{
			AnchorAll();
			SetScreenBounds(this.BoundsRelativeToParent);
			SetWorldBaseToWorldCenter();
			_world = _worldBase;

			_lastMousePositions.Push(new Vector2());
			_logger = logger;
			_animation = new Animator(_logger);
			_transform = Affine.NewIdentity();
		}

		public override void OnMouseDown(MouseEventArgs mouseEvent)
		{
			base.OnMouseDown(mouseEvent);

			if (mouseEvent.Button == MouseButtons.Left)
			{
				_mouseDownInitPosition = mouseEvent.Position;
			}
		}

		public override void OnMouseMove(MouseEventArgs mouseEvent)
		{
			base.OnMouseMove(mouseEvent);

			_transform = Affine.NewIdentity();

			if (_mouseDownInitPosition != null)
			{
				if (_animation.InProgress)
				{
					_animation.Stop();
				}

				Vector2 deltaPosition = (mouseEvent.Position - _lastMousePositions.Iterate().Last());

				OnTranslate(deltaPosition);
			}

			_timeFromLastMove = UiThread.CurrentTimerMs;
			_lastMousePositions.Push(mouseEvent.Position);
		}

		public override void OnMouseUp(MouseEventArgs mouseEvent)
		{
			base.OnMouseUp(mouseEvent);

			if (IsUseAnimation)
			{
				TranslateAnimation();
			}

			_mouseDownInitPosition = null;
		}

		public override void OnMouseWheel(MouseEventArgs mouseEvent)
		{
			base.OnMouseWheel(mouseEvent);

			double zoomDiff = 0.1;
			double scale = mouseEvent.WheelDelta > 0 ? 1 + zoomDiff : 1 - zoomDiff;
			ZoomToPosition(scale, TransformToScreenSpace(mouseEvent.Position));
		}

		private void TranslateAnimation()
		{
			var deltaTime = UiThread.CurrentTimerMs - _timeFromLastMove;
			if (deltaTime < _timeForIdle)
			{
				var pArr = _lastMousePositions.Iterate().ToList();
				Vector2 deltaPosition = (pArr[pArr.Count - 1] - pArr[pArr.Count - 2]);
				double speed = deltaPosition.Length / deltaTime;
				deltaPosition.Normalize();

				_animation.DoTransit(speed, (nextSpeed) =>
				{
					Vector2 dir = deltaPosition;
					OnTranslate(dir * nextSpeed);
					return true;
				}, new AnimationLimits(1000, 300, DecelerationType.Time), "Translate");
			}
		}

		private void Zoom(double scale)
		{
			Affine world = _world;
			Vector2 savedWorldPosition = new Vector2(world.tx, world.ty);

			Affine scalingAffine = Affine.NewScaling(scale);
			world *= scalingAffine;

			Vector2 newWorldPosition = new Vector2(world.tx, world.ty);

			Vector2 delta = savedWorldPosition - newWorldPosition;

			Affine translation = Affine.NewTranslation(delta);
			world *= translation;

			_world = world;
			Invalidate();
		}
		private void ZoomToPosition(double scale, Vector2 position)
		{
			Affine world = _world;

			world.translate(-position.X, -position.Y);
			world.scale(scale);
			world.translate(position.X, position.Y);

			_world = world;
			Invalidate();
		}

		public Affine GetWorldAffine()
		{
			return _world;
		}

		public void SetScreenBounds(RectangleDouble bounds)
		{
			_screenBounds = new ItemBounds(bounds);
		}

		public void SetWorldBaseToWorldCenter()
		{
			if (_isWorldCenterInScreenCenter && _screenBounds != null)
			{
				var center = TransformToScreenSpace(_screenBounds.Center);
				_worldCenter = Affine.NewTranslation(center);
			}

			_worldBase = _worldCenter;
		}

		public void ResetWorld()
		{
			_animation?.Stop();
			_transform = Affine.NewIdentity();
			_world = _worldBase;
			_isAnyChanges = true;
		}

		public void FitIntoView(ItemBounds bounds)
		{
			if (bounds is null || bounds.GetSquare().IsLessTolerance())
			{
				return;
			}

			if (_screenBounds == null)
			{
				_screenBounds = new ItemBounds(LocalBounds);
			}

			var word = Affine.NewIdentity();
			var min = TransformToScreenSpace(-bounds.Min);
			word.translate(min.X, min.Y);

			var center = TransformToScreenSpace(bounds.Center);
			word.translate(_worldCenter.tx - center.X, _worldCenter.ty - center.Y);


			Vector2 boundsSize = bounds.Size;
			Vector2 screenSize = _screenBounds.Size;

			Vector2 diff = boundsSize - screenSize;

			double paddingZoom = 0.65;

			_worldBase = word;
			ResetWorld();

			double scale = Math.Min((screenSize.Y / boundsSize.Y), (screenSize.X / boundsSize.X));
			scale *= paddingZoom;
			ZoomToPosition(scale, new Vector2(_worldCenter.tx, _worldCenter.ty));
		}

		private void ComputeTransform(bool isSilence = false)
		{
			if (!_isAnyChanges || _transform.is_identity())
			{
				_isAnyChanges = false;
				return;
			}

			_world *= _transform;
			_isAnyChanges = false;

			if (isSilence == false)
			{
				Invalidate();
			}
		}

		private void OnTranslate(Vector2 moveVector)
		{
			_transform = Affine.NewTranslation(moveVector);
			_isAnyChanges = true;
			ComputeTransform();
		}

		public void Reinitializing()
		{
			ReinitializationBegin?.Invoke(this, EventArgs.Empty);
			_animation.Stop();
			ResetWorld();
			_animation = new Animator(_logger);
			ReinitializationEnd?.Invoke(this, EventArgs.Empty);
		}
	}
}
