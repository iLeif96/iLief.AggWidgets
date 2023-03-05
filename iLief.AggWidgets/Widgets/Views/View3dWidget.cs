using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iLeif.Extensions.Logging;
using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.Csg.Transform;
using MatterHackers.DataConverters3D;
using MatterHackers.RayTracer;
using MatterHackers.RenderOpenGl;
using MatterHackers.RenderOpenGl.OpenGl;
using MatterHackers.VectorMath;

using Newtonsoft.Json.Linq;

namespace iLeif.CustomAggWidgets.Widgets.Views
{
    public class Settings3d
    {
        public event EventHandler? RenderTypeChanged;

        public RenderTypes RenderType { get => _renderType; set => SetRenderType(value); }
        private RenderTypes _renderType;

        public bool IsUseLightning { get; set; } = true;

        public LightingData LightingData { get; set; }
        public Vector3 Center { get; set; } = Vector3.Zero;

        public Settings3d()
        {
            LightingData = new LightingData()
            {
                AmbientLight = new float[] { 0.2f, 0.2f, 0.2f, 1.0f },
                DiffuseLight0 = new float[] { 0.7f, 0.7f, 0.7f, 1.0f },
                SpecularLight0 = new float[] { 0.5f, 0.5f, 0.5f, 1.0f },
                LightDirection0 = new float[] { -1, -1, 1, 0.0f },
                DiffuseLight1 = new float[] { 0.5f, 0.5f, 0.5f, 1.0f },
                SpecularLight1 = new float[] { 0.3f, 0.3f, 0.3f, 1.0f },
                LightDirection1 = new float[] { 1, 1, 1, 0.0f }
            };
        }

        private void SetRenderType(RenderTypes renderType)
        {
            if (RenderType != renderType)
            {
                _renderType = renderType;
                RenderTypeChanged?.Invoke(this, EventArgs.Empty);

            }
        }
    }

    public class View3dWidget : ViewBaseWidget, IView3d
    {
        public InteractiveScene Scene { get; } = new InteractiveScene();
        public WorldView World { get; } = new WorldView(0, 0);
        public Settings3d Settings { get; private set; }

        private TrackballTumbleWidget? _trackballTumbleWidget;


        public View3dWidget(ILogger logger) : base(true, logger)
        {
            Initialize();
        }

        public override void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Settings = new Settings3d();
            Settings.RenderType = RenderTypes.Shaded;
            Settings.RenderTypeChanged += OnRenderTypeChanged;

			DrawLayer.BackgroundColor = new Color(100, 150, 0);

            _trackballTumbleWidget = new TrackballTumbleWidget(World, DrawLayer);
            _trackballTumbleWidget.TransformState = MatterHackers.VectorMath.TrackBall.TrackBallTransformType.Rotation;
            _trackballTumbleWidget.AnchorAll();

			DrawLayer.AddChild(_trackballTumbleWidget, 0);

            base.Initialize();
        }

        public void AppendToScene(IObject3D item)
        {
            if (item == null)
            {
                return;
            }

            Scene.Children.Modify((children) =>
            {
                if (item.Mesh == null)
                {
                    children.AddRange(item.Children);
                    return;
                }

                children.Add(item);
            });

            ResetView();
			DrawLayer.Invalidate();
        }

        public override void OnDraw(Graphics2D graphics2D)
        {
			SuspendBaseDrawActionForNextDraw();
            base.OnDraw(graphics2D);

			GLHelper.SetGlContext(World, DrawLayer.TransformToScreenSpace(DrawLayer.LocalBounds), Settings.LightingData);
            ActionOnDraw?.Invoke(graphics2D);
            
            foreach (var object3D in Scene.Children)
            {
                DrawObject(object3D, false);
            }

            GLHelper.UnsetGlContext();
		}

        private void DrawObject(IObject3D object3D, bool parentSelected)
        {
            foreach (var item in object3D.VisibleMeshes())
            {
                bool isSelected = parentSelected ||
                    Scene.SelectedItem != null && (object3D == Scene.SelectedItem || Scene.SelectedItem.Children.Contains(object3D));

                GLHelper.Render(item.Mesh, item.WorldColor(), item.WorldMatrix(), Settings.RenderType);
            }
        }

        public override void ResetView()
        {
            _trackballTumbleWidget.ZeroVelocity();

            World.Reset();
            //World.Scale = 0.1;//WorldView.DefaultWorldScale;
            //World.Translate(-new Vector3(Settings.Center));

            base.ResetView();

            World.Rotate(Quaternion.FromEulerAngles(new Vector3(0, 0, MathHelper.Tau / 16)));
            World.Rotate(Quaternion.FromEulerAngles(new Vector3(MathHelper.Tau * .19, 0, 0)));
            World.Translate(-Scene.GetAxisAlignedBoundingBox().Center);
            //
            //Vector3 dir = (new Vector3(Settings.Center) - World.EyePosition).GetNormal();
            //double zDistance = WorldView.PerspectiveMaxZoomDist;
            //World.Translate(dir * (zDistance * 0.75));

            DrawLayer.Invalidate();
        }

		protected override void FitIntoView()
		{
			World.Fit(Scene, new RectangleDouble(0, 0, Widget.Width, Widget.Height));
		}

		public override void ClearScene()
        {
            Scene.Children.Clear();
            DrawLayer.Invalidate();
        }

        private void OnRenderTypeChanged(object sender, EventArgs eventArgs)
        {
            foreach (var renderTransfrom in Scene.VisibleMeshes())
            {
                renderTransfrom.Mesh.MarkAsChanged();
            }
        }
    }
}
