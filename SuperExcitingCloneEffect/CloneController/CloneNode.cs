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
using Newtonsoft.Json.Linq;

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
        private ID2D1Bitmap empty;
        ID2D1Image renderOutput;
        private bool disposedValue = false;
        private bool wasEmpty = true;
        private DrawDescription drawDescription = new(
            default, default, new Vector2(1f, 1f), default, Matrix4x4.Identity, InterpolationMode.Linear, 1.0, false, []);

        readonly ParamsOfCloneNode Params;

        public bool Appear => Params.Appear;
        public BlendSECE BlendMode => Params.BlendMode;
        public string TagName => Params.TagName;
        public string Parent => Params.Parent;

        public List<CloneNode> ParentPath { get; set; } = [];

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

            empty = devices.DeviceContext.CreateEmptyBitmap();
            disposer.Collect(empty);

            using (var image = transform.Output)
                cropEffect.SetInput(0, image, true);

            using (var image = cropEffect.Output)
                renderEffect.SetInput(0, image, true);

            using (var image = renderEffect.Output)
                opacityEffect.SetInput(0, image, true);

            renderOutput = renderEffect.Output;
            disposer.Collect(renderOutput);

            Output = empty;

            Params = new(devices, block, length, frame, fps);
        }

        public bool UpdateParams(IGraphicsDevicesAndContext devices, CloneBlock block, long length, long frame, int fps)
        {
            return Params.Update(devices, block, length, frame, fps);
        }

        public void UpdateOutput(ID2D1Image? input, TimelineItemSourceDescription timeLineItemSourceDescription)
        {
            if (input == null)
            {
                if (!wasEmpty)
                {
                    Output = empty;
                    wasEmpty = true;
                }

                return;
            }

            Double3 draw = new(), draw2 = new();
            Double2 trigon = new();
            double rotate = 0.0, scale = 1.0, opacity = 1.0, rotate2, scale2;
            bool xyzDependent, rotateDependent, scaleDependent, opacityDependent, mirrorDependent, mirror = false;
            xyzDependent = rotateDependent = scaleDependent = opacityDependent = mirrorDependent = true;
            foreach (var node in ParentPath)
            {
                rotate2 = node.Params.Rotate * Math.PI / 180.0;
                scale2 = node.Params.Scale / 100.0;

                if (xyzDependent)
                {
                    trigon.X = Math.Cos(rotate2);
                    trigon.Y = Math.Sin(rotate2);
                    draw.X *= (scaleDependent ? scale2 : 1) * ((node.Params.Mirror && mirrorDependent) ? -1 : 1);
                    draw.Y *= scaleDependent ? scale2 : 1;
                    draw.Z *= scaleDependent ? scale2 : 1;
                    draw2.X = trigon.X * draw.X + trigon.Y * draw.Y;
                    draw2.Y = trigon.Y * draw.X + trigon.X * draw.Y;
                    draw2.Z = draw.Z;

                    draw.X = draw2.X + (node.Params.Draw.X + (node.Params.KeepPlace ? node.Params.Center.X : 0.0));
                    draw.Y = draw2.Y + (node.Params.Draw.Y + (node.Params.KeepPlace ? node.Params.Center.Y : 0.0));
                    draw.Z = draw2.Z + node.Params.Draw.Z;

                    xyzDependent = node.Params.XYZDependent;
                }

                if (scaleDependent)
                {
                    scale *= scale2;
                    scaleDependent = node.Params.RotateDependent;
                }

                if (opacityDependent)
                {
                    opacity *= node.Params.Opacity / 100.0;
                    scaleDependent = node.Params.RotateDependent;
                }

                if (rotateDependent)
                {
                    rotate += node.Params.Rotate;
                    rotateDependent = node.Params.RotateDependent;
                }

                if (mirrorDependent)
                {
                    mirror ^= node.Params.Mirror;
                    mirrorDependent = node.Params.MirrorDependent;
                }
            }

            
            drawDescription = new DrawDescription(
                new Vector3((float)draw.X, (float)draw.Y, (float)draw.Z),
                default(Vector2),
                new Vector2((float)(scale * Params.ExpXY.X / 100.0), (float)(scale * Params.ExpXY.Y / 100.0)),
                new Vector3(0f, 0f, (float)rotate),
                Matrix4x4.Identity,
                InterpolationMode.Linear,
                opacity,
                mirror,
                ImmutableList.Create(default(ReadOnlySpan<VideoEffectController>))
                );
            UseProcessors(input, timeLineItemSourceDescription);
            Vector3 draw3 = drawDescription.Draw;
            Vector2 zoom = drawDescription.Zoom;
            Vector3 rotation = drawDescription.Rotation;
            Matrix4x4 camera = drawDescription.Camera;
            bool invert = drawDescription.Invert;
            Vector2 centerPoint = drawDescription.CenterPoint;
            AffineTransform2DInterpolationMode interPolationMode = drawDescription.ZoomInterpolationMode.ToTransform2D();
            Transform3DInterpolationMode interPolationMode2 = drawDescription.ZoomInterpolationMode.ToTransform3D();
            transform.InterPolationMode = interPolationMode;
            transform.TransformMatrix = Matrix3x2.CreateTranslation(-1 * new Vector2((float)Params.Center.X, (float)Params.Center.Y)) * Matrix3x2.CreateScale(zoom.X, zoom.Y);
            renderEffect.InterPolationMode = interPolationMode2;
            renderEffect.TransformMatrix = (
                invert ? Matrix4x4.CreateScale(-1f, 1f, 1f, new Vector3(centerPoint, 0f)) : Matrix4x4.Identity)
                * Matrix4x4.CreateRotationZ(MathF.PI * rotation.Z / 180f)
                * Matrix4x4.CreateRotationY(MathF.PI * (0f - rotation.Y) / 180f)
                * Matrix4x4.CreateRotationX(MathF.PI * (0f - rotation.X) / 180f)
                * Matrix4x4.CreateTranslation(draw3)
                * camera
                * new Matrix4x4(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, -0.001f, 0f, 0f, 0f, 1f);

            Apply(devices.DeviceContext);
            opacityEffect.Value = (float)drawDescription.Opacity;

            if (wasEmpty)
            {
                Output = opacityEffect.Output;
                wasEmpty = false;
            }
        }

        private void UseProcessors(ID2D1Image input, TimelineItemSourceDescription timeLineItemSourceDescription)
        {
            ID2D1Image input2 = input;
            foreach (var e_EP in Params.E_EPs)
            {
                if (e_EP.Item1.IsEnabled)
                {
                    IVideoEffectProcessor item = e_EP.Item2;
                    item.SetInput(input2);
                    EffectDescription effectDescription = new EffectDescription(timeLineItemSourceDescription, drawDescription, 0);
                    drawDescription = item.Update(effectDescription);
                    input2 = item.Output;
                }
            }

            transform.SetInput(0, input2, true);
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

        void Apply(ID2D1DeviceContext deviceContext)
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
}