using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using Blend = YukkuriMovieMaker.Project.Blend;

namespace SuperExcitingCloneEffect.Effect
{
    internal class SuperExcitingCloneEffectProcessor : IVideoEffectProcessor
    {
        private readonly IGraphicsDevicesAndContext _devices;
        private readonly SuperExcitingCloneEffect _item;
        
        private ID2D1Image? _input;
        private readonly ID2D1Image _empty;
        private List<CloneDrawer> _drawers = [];
        private ID2D1CommandList? _commandList;
        private readonly AffineTransform2D _trans;
        private readonly ID2D1Image _output;

        public ID2D1Image Output => _output;

        public SuperExcitingCloneEffectProcessor(IGraphicsDevicesAndContext devices, SuperExcitingCloneEffect item)
        {
            _devices = devices;
            _item = item;
            _empty = devices.DeviceContext.CreateEmptyBitmap();
            _trans = new(devices.DeviceContext);
            _output = _trans.Output;
        }

        public void SetInput(ID2D1Image? input)
        {
            _input = input;
            _drawers.ForEach(d => d.SetInput(input));
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            UpdateDrawers();
            List<VideoEffectController> ctrls = [];

            foreach (var cd in _drawers)
                cd.UpdateOutput(effectDescription);

            List<CloneDrawer> sortedDrawers = [.. _drawers];
            sortedDrawers.Sort((a, b) => a.M43.CompareTo(b.M43));

            _commandList?.Dispose();
            _commandList = _devices.DeviceContext.CreateCommandList();

            var dc = _devices.DeviceContext;
            dc.Target = _commandList;
            dc.BeginDraw();
            dc.Clear(null);

            foreach (CloneDrawer cd in sortedDrawers)
            {
                if (!cd.GetHide())
                {
                    switch (cd.Value.Blend)
                    {
                        case Blend.Normal:
                            dc.DrawImage(cd.Output, compositeMode: CompositeMode.SourceOver);
                            break;

                        case Blend.Add:
                            dc.DrawImage(cd.Output, compositeMode: CompositeMode.Plus);
                            break;

                        case Blend.DestinationOver:
                            dc.DrawImage(cd.Output, compositeMode: CompositeMode.DestinationOver);
                            break;

                        case Blend.DestinationOut:
                            dc.DrawImage(cd.Output, compositeMode: CompositeMode.DestinationOut);
                            break;

                        case Blend.SourceAtop:
                            dc.DrawImage(cd.Output, compositeMode: CompositeMode.SourceAtop);
                            break;

                        case Blend.XOR:
                            dc.DrawImage(cd.Output, compositeMode: CompositeMode.Xor);
                            break;

                        case Blend.MaskInvert:
                            dc.DrawImage(cd.Output, compositeMode: CompositeMode.MaskInverseErt);
                            break;

                        case Blend.Multiply:
                            dc.BlendImage(cd.Output, BlendMode.Multiply, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Screen:
                            dc.BlendImage(cd.Output, BlendMode.Screen, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Darker:
                            dc.BlendImage(cd.Output, BlendMode.Darken, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Lighter:
                            dc.BlendImage(cd.Output, BlendMode.Lighten, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Dissolve:
                            dc.BlendImage(cd.Output, BlendMode.Dissolve, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.ColorBurn:
                            dc.BlendImage(cd.Output, BlendMode.ColorBurn, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.LinearBurn:
                            dc.BlendImage(cd.Output, BlendMode.LinearBurn, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.DarkerColor:
                            dc.BlendImage(cd.Output, BlendMode.DarkerColor, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.LighterColor:
                            dc.BlendImage(cd.Output, BlendMode.LighterColor, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.ColorDodge:
                            dc.BlendImage(cd.Output, BlendMode.ColorDodge, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.LinearDodge:
                            dc.BlendImage(cd.Output, BlendMode.LinearDodge, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Overlay:
                            dc.BlendImage(cd.Output, BlendMode.Overlay, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.SoftLight:
                            dc.BlendImage(cd.Output, BlendMode.SoftLight, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.HardLight:
                            dc.BlendImage(cd.Output, BlendMode.HardLight, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.VividLight:
                            dc.BlendImage(cd.Output, BlendMode.VividLight, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.LinearLight:
                            dc.BlendImage(cd.Output, BlendMode.LinearLight, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.PinLight:
                            dc.BlendImage(cd.Output, BlendMode.PinLight, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.HardMix:
                            dc.BlendImage(cd.Output, BlendMode.HardMix, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Difference:
                            dc.BlendImage(cd.Output, BlendMode.Difference, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Exclusion:
                            dc.BlendImage(cd.Output, BlendMode.Exclusion, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Hue:
                            dc.BlendImage(cd.Output, BlendMode.Hue, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Saturation:
                            dc.BlendImage(cd.Output, BlendMode.Saturation, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Color:
                            dc.BlendImage(cd.Output, BlendMode.Color, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Luminosity:
                            dc.BlendImage(cd.Output, BlendMode.Luminosity, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Subtract:
                            dc.BlendImage(cd.Output, BlendMode.Subtract, null, null, InterpolationMode.MultiSampleLinear);
                            break;

                        case Blend.Division:
                            dc.BlendImage(cd.Output, BlendMode.Division, null, null, InterpolationMode.MultiSampleLinear);
                            break;
                    }
                }
            }

            dc.EndDraw();
            dc.Target = null;
            _commandList.Close();

            _trans.SetInput(0, _commandList, true);

            return effectDescription.DrawDescription with
            {
                Controllers = [.. ctrls]
            };
        }

        private List<IManagedItem> GetAncestors(IManagedItem mi1)
        {
            List<IManagedItem> list1 = [];
            IManagedItem mi2 = mi1;

            while (mi2.ParentIndex > -1)
            {
                mi2 = _item.ManagedItems[mi2.ParentIndex];
                list1.Add(mi2);
            }

            return list1;
        }

        private void UpdateDrawers()
        {
            List<CloneDrawer> drawers = [];

            foreach (IManagedItem mi in _item.ManagedItems)
            {
                if (mi is CloneValue cv)
                { 
                    if (_drawers.FirstOrDefault(d => d.Value == cv) is CloneDrawer cd1)
                    {
                        drawers.Add(cd1);
                    }
                    else
                    {
                        CloneDrawer cd2 = new(_devices, cv);
                        cd2.SetInput(_input);;
                        drawers.Add(cd2);
                    }
                }
            }

            if (!_drawers.SequenceEqual(drawers))
            {
                List<CloneDrawer> toDispose = [.. _drawers.Where(d => !drawers.Contains(d))];

                foreach (CloneDrawer cd in toDispose)
                    cd.Dispose();

                foreach (CloneDrawer cd in drawers)
                    cd.SetAncestor(GetAncestors(cd.Value));

                _drawers = drawers;
            }
        }

        public void ClearInput()
        {
            _input = null;
            _drawers.ForEach(d => d.SetInput(null));
        }

        public void Dispose()
        {
            _trans.SetInput(0, null, true);
            _output.Dispose();
            _trans.Dispose();
            _commandList?.Dispose();
            _drawers.ForEach(d => d.Dispose());
            _empty.Dispose();
        }
    }
}
