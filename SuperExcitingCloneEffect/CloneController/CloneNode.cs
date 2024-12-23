using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video.Effects;
using YukkuriMovieMaker.Player.Video;
using System.Numerics;

namespace SuperExcitingCloneEffect.CloneController
{
    public class CloneNode : IDisposable
    {
        readonly IGraphicsDevicesAndContext devices;
        readonly DisposeCollector disposer = new();
        readonly AffineTransform2D transform;
        readonly Crop cropEffect;
        readonly Transform3D renderEffect;
        readonly Opacity opacityEffect;
        public ID2D1Image? Output;
        ID2D1Image renderOutput;
        private bool disposedValue = false;

        ParamsOfCloneNode Params;

        public bool Appear => Params.Appear;
        public BlendSECE BlendMode => Params.BlendMode;
        public string TagName => Params.TagName;
        public string Parent => Params.Parent;

        public ImmutableList<CloneNode> ParentPath { get; set; } = [];

        public CloneNode(IGraphicsDevicesAndContext devices, CloneBlock block, long length, long frame, int fps)
        {
            this.devices = devices;
            transform = new AffineTransform2D(devices.DeviceContext);

            cropEffect = new Crop(devices.DeviceContext);
            disposer.Collect(cropEffect);

            renderEffect = new Transform3D(devices.DeviceContext);
            disposer.Collect(renderEffect);

            opacityEffect = new Opacity(devices.DeviceContext);
            disposer.Collect(opacityEffect);

            using (var image = transform.Output)
                cropEffect.SetInput(0, image, true);

            using (var image = cropEffect.Output)
                renderEffect.SetInput(0, image, true);

            using (var image = renderEffect.Output)
                opacityEffect.SetInput(0, image, true);

            renderOutput = renderEffect.Output;
            disposer.Collect(renderOutput);

            Output = opacityEffect.Output;
            disposer.Collect(Output);

            Params = new(block, length, frame, fps);
        }

        public bool UpdateParams(CloneBlock block, long length, long frame, int fps)
        {
            return Params.Update(block, length, frame, fps);
        }


        public bool UpdateOutput(CloneBlock block, long length, long frame, int fps)
        {
            return Params.Update(block, length, frame, fps);
        }

        void ClearEffectChain()
        {
            opacityEffect.SetInput(0, null, true);
            renderEffect.SetInput(0, null, true);
            cropEffect.SetInput(0, null, true);
            transform.SetInput(0, null, true);
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                ClearEffectChain();
                disposer.Dispose();
                GC.SuppressFinalize(this);
                disposedValue = true;
            }
        }

        #region SafeTransform3DHelper
        const float D3D11_FTOI_INSTRUCTION_MAX_INPUT = 2.1474836E+09f;
        const float D3D11_FTOI_INSTRUCTION_MIN_INPUT = -2.1474836E+09f;

        public void Apply(ID2D1DeviceContext deviceContext)
        {
            //transform3dエフェクトの出力画像1pxあたりの入力サイズが4096pxを超えるとエラーになる
            //エラー時には出力サイズがD3D11_FTOI_INSTRUCTION_MAX_INPUTになるため、cropエフェクトを使用し入力サイズを4096pxに制限する

            //一旦cropエフェクトの範囲を初期化する
            cropEffect.Rectangle = new Vector4(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
            var renderBounds = deviceContext.GetImageLocalBounds(renderOutput);
            if (renderBounds.Left == D3D11_FTOI_INSTRUCTION_MIN_INPUT
                || renderBounds.Top == D3D11_FTOI_INSTRUCTION_MIN_INPUT
                || renderBounds.Right == D3D11_FTOI_INSTRUCTION_MAX_INPUT
                || renderBounds.Bottom == D3D11_FTOI_INSTRUCTION_MAX_INPUT)
            {
                //エラーの場合にのみ入力サイズを制限する
                cropEffect.Rectangle = new Vector4(-2048, -2048, 2048, 2048);
            }
        }
        #endregion
    }

    namespace YukkuriMovieMaker.Player.Video.Effects
    {
        internal class DrawLazyEffectEffect(IGraphicsDevicesAndContext devices, Project.Effects.DrawLazyEffectEffect item) : VideoEffectProcessorBase(devices)
        {
            readonly IGraphicsDevicesAndContext devices = devices;
            AffineTransform2D zoomEffect;
            Crop cropEffect;
            Transform3D renderEffect;
            ColorMatrix opacityEffect;
            ID2D1Image renderOutput;

            bool isFirstUpdate = true;
            Vector3 draw;
            Vector2 zoom;
            Vector3 rotation;
            Matrix4x4 camera;
            AffineTransform2DInterpolationMode interPolationMode2d;
            Transform3DInterpolationMode interPolationMode3d;
            float opacity;
            bool isInverted;
            Vector2 centerPoint;

            public override DrawDescription Update(EffectDescription effectDescription)
            {
                var draw = effectDescription.DrawDescription.Draw;
                var zoom = effectDescription.DrawDescription.Zoom;
                var rotation = effectDescription.DrawDescription.Rotation;
                var camera = effectDescription.DrawDescription.Camera;
                var opacity = (float)effectDescription.DrawDescription.Opacity;
                var isInverted = effectDescription.DrawDescription.Invert;
                var centerPoint = effectDescription.DrawDescription.CenterPoint;

                var interPolationMode2d = effectDescription.DrawDescription.ZoomInterpolationMode.ToTransform2D();
                var interPolationMode3d = effectDescription.DrawDescription.ZoomInterpolationMode.ToTransform3D();

                if (!item.IsXYZ)
                    draw = new Vector3();
                if (!item.IsZoom)
                    zoom = new Vector2(1, 1);
                if (!item.IsRotation)
                    rotation = new Vector3();
                if (!item.IsCamera)
                    camera = Matrix4x4.Identity;
                if (!item.IsOpacity)
                    opacity = 1;

                if (isFirstUpdate || this.zoom != zoom)
                    zoomEffect.TransformMatrix = Matrix3x2.CreateScale(zoom);
                if (isFirstUpdate || this.interPolationMode2d != interPolationMode2d)
                    zoomEffect.InterPolationMode = interPolationMode2d;
                if (isFirstUpdate || this.interPolationMode3d != interPolationMode3d)
                    renderEffect.InterPolationMode = interPolationMode3d;
                if (isFirstUpdate || this.rotation != rotation || this.draw != draw || this.camera != camera || this.isInverted != isInverted || this.centerPoint != centerPoint)
                {
                    renderEffect.TransformMatrix =
                        (isInverted ? Matrix4x4.CreateScale(-1, 1, 1, new Vector3(centerPoint, 0)) : Matrix4x4.Identity)
                        * Matrix4x4.CreateRotationZ(MathF.PI * rotation.Z / 180f)
                        * Matrix4x4.CreateRotationY(MathF.PI * -rotation.Y / 180f)
                        * Matrix4x4.CreateRotationX(MathF.PI * -rotation.X / 180f)
                        * Matrix4x4.CreateTranslation(draw)
                        * camera
                        * new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, -1 / 1000f, 0, 0, 0, 1);
                }

                Apply(devices.DeviceContext, cropEffect, renderOutput);

                if (isFirstUpdate || this.opacity != opacity)
                {
                    opacityEffect.Matrix = new Matrix5x4()
                    {
                        M11 = 1,
                        M12 = 0,
                        M13 = 0,
                        M14 = 0,

                        M21 = 0,
                        M22 = 1,
                        M23 = 0,
                        M24 = 0,

                        M31 = 0,
                        M32 = 0,
                        M33 = 1,
                        M34 = 0,

                        M41 = 0,
                        M42 = 0,
                        M43 = 0,
                        M44 = opacity,

                        M51 = 0,
                        M52 = 0,
                        M53 = 0,
                        M54 = 0,
                    };
                }

                isFirstUpdate = false;
                this.draw = draw;
                this.zoom = zoom;
                this.rotation = rotation;
                this.camera = camera;
                this.interPolationMode2d = interPolationMode2d;
                this.interPolationMode3d = interPolationMode3d;
                this.opacity = opacity;
                this.isInverted = isInverted;
                this.centerPoint = centerPoint;

                var desc = effectDescription.DrawDescription;
                return desc with
                {
                    Draw = item.IsXYZ ? new() : desc.Draw,
                    Zoom = item.IsZoom ? new(1, 1) : desc.Zoom,
                    Rotation = item.IsRotation ? new() : desc.Rotation,
                    Camera = item.IsCamera ? Matrix4x4.Identity : desc.Camera,
                    Opacity = item.IsOpacity ? 1 : desc.Opacity,
                    Invert = !item.IsInvert && desc.Invert,
                };
            }
        }
    }
}
