using System.Collections.Immutable;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.Classes
{
    internal class VideoEffectChain : IDisposable
    {
        private readonly IGraphicsDevicesAndContext _devices;
        private readonly AffineTransform2D transform;
        private ID2D1Image? _input;
        private List<(IVideoEffect effect, IVideoEffectProcessor processor)> _chain = [];
        private TimelineItemSourceDescription? _lastTimelineSourceDescription;
        private DrawDescription? _lastDrawDescription;

        public ID2D1Image Output;

        public VideoEffectChain(IGraphicsDevicesAndContext devices)
        {
            _devices = devices;
            transform = new AffineTransform2D(devices.DeviceContext);
            Output = transform.Output;
        }

        private void UpdateChain(ImmutableList<IVideoEffect> effects)
        {
            var disposedIndex = from tuple in _chain
                                where !effects.Contains(tuple.effect)
                                select _chain.IndexOf(tuple) into i
                                orderby i descending
                                select i;
            foreach (int index in disposedIndex)
            {
                IVideoEffectProcessor processor = _chain[index].processor;
                processor.ClearInput();
                processor.Dispose();
                _chain.RemoveAt(index);
            }

            List<IVideoEffect> keeped = _chain.Select((e_ep) => e_ep.effect).ToList();
            List<(IVideoEffect effect, IVideoEffectProcessor processor)> newChain = new(effects.Count);
            foreach (IVideoEffect effect in effects)
            {
                int index = keeped.IndexOf(effect);
                newChain.Add(index < 0 ? (effect, effect.CreateVideoEffect(_devices)) : _chain[index]);
            }

            _chain = newChain;
        }

        public void SetInputAndEffects(ID2D1Image? input, ImmutableList<IVideoEffect> effects)
        {
            _input = input;
            if (effects.Count > 0)
            {
                UpdateChain(effects);

                if (_lastTimelineSourceDescription is not null && _lastDrawDescription is not null)
                {
                    UpdateOutputAndDescription(_lastTimelineSourceDescription, _lastDrawDescription);
                    return;
                }
            }

            transform.SetInput(0, input, true);
        }

        public void ClearChain()
        {
            foreach (var (_, processor) in _chain)
            {
                processor.ClearInput();
                processor.Dispose();
            }
            _chain.Clear();
            transform.SetInput(0, _input, true);
        }

        public void ClearInput()
        {
            _input = null;
            transform.SetInput(0, null, true);
            foreach (var (_, processor) in _chain)
            {
                processor.ClearInput();
                processor.Dispose();
            }
            _chain.Clear();
        }

        public DrawDescription UpdateOutputAndDescription(TimelineItemSourceDescription timelineSourceDescription, DrawDescription drawDescription)
        {
            _lastTimelineSourceDescription = timelineSourceDescription;
            _lastDrawDescription = drawDescription;

            if (_input == null)
            {
                throw new InvalidOperationException("[VideoEffectChain] input is null.");
            }

            FrameAndLength fl = new(timelineSourceDescription);
            DrawDescription desc = new(
                drawDescription.Draw,
                drawDescription.CenterPoint,
                drawDescription.Zoom,
                drawDescription.Rotation,
                drawDescription.Camera,
                drawDescription.ZoomInterpolationMode,
                drawDescription.Opacity,
                drawDescription.Invert,
                drawDescription.Controllers
                );

            ID2D1Image? image = _input;
            foreach (var (effect, processor) in _chain)
            {
                if (effect.IsEnabled)
                {
                    IVideoEffectProcessor item = processor;
                    item.SetInput(image);
                    TimelineItemSourceDescription timeLineItemSourceDescription
                        = new(timelineSourceDescription, fl.Frame, fl.Length, 0);
                    EffectDescription effectDescription = new(timeLineItemSourceDescription, desc, 0, 1, 0, 1);
                    desc = item.Update(effectDescription);
                    image = item.Output;
                }
            }

            transform.SetInput(0, image, true);
            return desc;
        }

        public void Dispose()
        {
            transform.SetInput(0, null, true);
            Output.Dispose();
            transform.Dispose();

            _chain.ForEach(i =>
            {
                i.processor.ClearInput();
                i.processor.Dispose();
            });
        }
    }
}
