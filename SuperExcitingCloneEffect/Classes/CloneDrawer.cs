using SuperExcitingCloneEffect.Interfaces;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using Transform3D = Vortice.Direct2D1.Effects.Transform3D;

namespace SuperExcitingCloneEffect.Classes
{
    internal class CloneDrawer : IDisposable
    {
        private readonly IGraphicsDevicesAndContext _devices;
        private ID2D1Image? _input;
        private readonly DisposeCollector _disposer = new();
        private readonly VideoEffectChain _effectChain;
        private readonly List<IManagedItem> _ancestor = []; // _ancestor[0]が直近の親
        public CloneValue Value { get; init; }

        private readonly AffineTransform2D _transform;
        private readonly Crop _cropEffect;
        private readonly Transform3D _renderEffect;
        private readonly Opacity _opacityEffect;
        private readonly ID2D1Image _renderOutput;

        public float M43 { get; private set; }

        public ID2D1Image Output;

        public CloneDrawer(IGraphicsDevicesAndContext devices, CloneValue value)
        {
            _devices = devices;
            _disposer = new();
            Value = value;

            _effectChain = new(devices);
            _disposer.Collect(_effectChain);

            _transform = new AffineTransform2D(devices.DeviceContext);
            _disposer.Collect(_transform);

            _cropEffect = new Crop(devices.DeviceContext);
            _disposer.Collect(_cropEffect);

            _renderEffect = new Transform3D(devices.DeviceContext);
            _disposer.Collect(_renderEffect);

            _opacityEffect = new Opacity(devices.DeviceContext);
            _disposer.Collect(_opacityEffect);

            using (var image = _effectChain.Output)
                _transform.SetInput(0, image, true);

            using (var image = _transform.Output)
                _cropEffect.SetInput(0, image, true);

            using (var image = _cropEffect.Output)
                _renderEffect.SetInput(0, image, true);

            using (var image = _renderEffect.Output)
                _opacityEffect.SetInput(0, image, true);

            _renderOutput = _renderEffect.Output;
            _disposer.Collect(_renderOutput);

            Output = _opacityEffect.Output;
            _disposer.Collect(Output);
        }

        public void SetInput(ID2D1Image? input)
        {
            _input = input;
        }

        public void SetAncestor(IEnumerable<IManagedItem> list)
        {
            _ancestor.Clear();
            _ancestor.AddRange(list);
        }

        public bool GetHide()
        {
            if (Value.Hide)
                return true;

            return _ancestor.Any(mi => mi.Hide);
        }

        public void UpdateOutput(EffectDescription effectDescription)
        {
            List<IVideoEffect> effects = [.. Value.Effects];

            foreach (IManagedItem mi in _ancestor)
                effects.AddRange(mi.Effects);

            _effectChain.SetInputAndEffects(_input, [.. effects]);

            DrawDescription drawDescription = new(
                new Vector3(),
                new Vector2(),
                new Vector2(1, 1),
                new Vector3(),
                Matrix4x4.Identity,
                InterpolationMode.Linear,
                1.0,
                false,
                []);

            drawDescription = _effectChain.UpdateOutputAndDescription(effectDescription, drawDescription);

            Vector2 centerPoint = drawDescription.CenterPoint;
            AffineTransform2DInterpolationMode interPolationMode = drawDescription.ZoomInterpolationMode.ToTransform2D();
            Transform3DInterpolationMode interPolationMode2 = drawDescription.ZoomInterpolationMode.ToTransform3D();
            _transform.InterPolationMode = interPolationMode;
            _transform.TransformMatrix =
                Matrix3x2.CreateTranslation(-1 * drawDescription.CenterPoint)
                * Matrix3x2.CreateScale(drawDescription.Zoom);
            _renderEffect.InterPolationMode = interPolationMode2;
            _renderEffect.TransformMatrix = (
                drawDescription.Invert ? Matrix4x4.CreateScale(-1f, 1f, 1f, new Vector3(centerPoint, 0f)) : Matrix4x4.Identity)
                * Matrix4x4.CreateRotationZ(MathF.PI * drawDescription.Rotation.Z / 180f)
                * Matrix4x4.CreateRotationY(MathF.PI * -drawDescription.Rotation.Y / 180f)
                * Matrix4x4.CreateRotationX(MathF.PI * -drawDescription.Rotation.X / 180f)
                * Matrix4x4.CreateTranslation(drawDescription.Draw)
                * drawDescription.Camera
                * new Matrix4x4(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, -0.001f, 0f, 0f, 0f, 1f);

            M43 = _renderEffect.TransformMatrix.M43;
            Apply();
            _opacityEffect.Value = (float)drawDescription.Opacity;
        }

        private void ClearEffectChain()
        {
            _transform.SetInput(0, null, true);
            _cropEffect.SetInput(0, null, true);
            _renderEffect.SetInput(0, null, true);
            _opacityEffect.SetInput(0, null, true);
        }

        public void Dispose()
        {
            ClearEffectChain();
            _disposer.Dispose();
        }

        #region SafeTransform3DHelper
        const float D3D11_FTOI_INSTRUCTION_MAX_INPUT = 2.1474836E+09f;
        const float D3D11_FTOI_INSTRUCTION_MIN_INPUT = -2.1474836E+09f;

        void Apply()
        {
            //transform3dエフェクトの出力画像1pxあたりの入力サイズが4096pxを超えるとエラーになる
            //エラー時には出力サイズがD3D11_FTOI_INSTRUCTION_MAX_INPUTになるため、cropエフェクトを使用し入力サイズを4096pxに制限する

            //一旦cropエフェクトの範囲を初期化する
            _cropEffect.Rectangle = new Vector4(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
            var renderBounds = _devices.DeviceContext.GetImageLocalBounds(_renderOutput);
            if (renderBounds.Left == D3D11_FTOI_INSTRUCTION_MIN_INPUT
                || renderBounds.Top == D3D11_FTOI_INSTRUCTION_MIN_INPUT
                || renderBounds.Right == D3D11_FTOI_INSTRUCTION_MAX_INPUT
                || renderBounds.Bottom == D3D11_FTOI_INSTRUCTION_MAX_INPUT)
            {
                //エラーの場合にのみ入力サイズを制限する
                _cropEffect.Rectangle = new Vector4(-2048, -2048, 2048, 2048);
            }
        }
        #endregion
    }
}
