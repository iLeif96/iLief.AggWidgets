
using System.Collections.Generic;

using MatterHackers.Agg.UI;

namespace iLeif.CustomAggWidgets.Widgets
{
	public class LayoutLayer : GuiWidget
	{
		private Dictionary<GuiWidget, int> _childrenToAttach = new Dictionary<GuiWidget, int>();
		public FlowDirection FlowDirection { get => _flowDirection; set { _flowDirection = value; Invalidate(); } }
		private FlowDirection _flowDirection;

		public LayoutLayer(FlowDirection flowDirection, int width, int height) : base(width, height)
		{
			_flowDirection = flowDirection;
		}

		public LayoutLayer(FlowDirection flowDirection)
		{
			_flowDirection = flowDirection;
		}

		public override void Initialize()
		{
			this.LayoutEngine = new LayoutEngineFlow(FlowDirection);
			PushChildrenToBase();
			SetBoundsToEncloseChildren();
			base.Initialize();
		}

		public new GuiWidget AddChild(GuiWidget widget, int indexInChildrenList = -1)
		{
			if (_flowDirection== FlowDirection.LeftToRight || _flowDirection == FlowDirection.RightToLeft)
			{
				widget.VAnchor = VAnchor;
			}
			else if (_flowDirection == FlowDirection.TopToBottom || _flowDirection== FlowDirection.BottomToTop)
			{
				widget.HAnchor = HAnchor;
			}

			_childrenToAttach.Add(widget, indexInChildrenList);

			return widget;
		}

		public new GuiWidget AddChild(GuiWidget widget, VAnchor vAnchor, HAnchor hAnchor, int indexInChildrenList = -1)
		{
			if (_flowDirection == FlowDirection.LeftToRight || _flowDirection == FlowDirection.RightToLeft)
			{
				widget.VAnchor = vAnchor;
			}
			else if (_flowDirection == FlowDirection.TopToBottom || _flowDirection == FlowDirection.BottomToTop)
			{
				widget.HAnchor = hAnchor;
			}

			_childrenToAttach.Add(widget, indexInChildrenList);

			return widget;
		}

		private void PushChildrenToBase()
		{
			using (LayoutLock())
			{
				foreach (var child in _childrenToAttach)
				{
					base.AddChild(child.Key, child.Value);
				}
			}
		}
	}
}
