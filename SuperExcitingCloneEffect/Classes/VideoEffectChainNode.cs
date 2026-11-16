using SuperExcitingCloneEffect.Interfaces;
using System.Collections.Immutable;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.Classes
{
    internal class VideoEffectChainNode : IDisposable
    {
        private readonly IGraphicsDevicesAndContext _devices;
        private readonly AffineTransform2D transform;
        private readonly List<(IVideoEffect effect, IVideoEffectProcessor processor)> _chain = [];
        private ID2D1Image? _input;
        private ID2D1Bitmap1 _empty;
        private bool _isEmpty = false;

        public readonly IManagedItem Item;
        public ID2D1Image Output;

        public VideoEffectChainNode(IGraphicsDevicesAndContext devices, IManagedItem item)
        {
            _devices = devices;
            Item = item;
            transform = new AffineTransform2D(devices.DeviceContext);
            Output = transform.Output;
            _empty = devices.DeviceContext.CreateEmptyBitmap();
        }

        private void UpdateChain()
        {
            ImmutableList<IVideoEffect> effects = Item.Effects;
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

            IVideoEffect[] keeped = [.. _chain.Select(e_ep => e_ep.effect)];
            List<(IVideoEffect effect, IVideoEffectProcessor processor)> newChain = new(effects.Count);

            foreach (IVideoEffect effect in effects)
            {
                int index = keeped.IndexOf(effect);
                newChain.Add(index < 0 ? (effect, effect.CreateVideoEffect(_devices)) : _chain[index]);
            }

            _chain.Clear();
            _chain.AddRange(newChain);
        }

        public void SetInputAndUpdateChain(ID2D1Image? input)
        {
            _input = input;
            UpdateChain();
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

        public DrawDescription UpdateOutputAndDescription(TimelineItemSourceDescription timelineSourceDescription, DrawDescription drawDescription, FrameAndLength fl)
        {
            if (_input == null)
            {
                throw new InvalidOperationException("[VideoEffectChain] input is null.");
            }

            if (fl.Frame < 0)
            {
                if (!_isEmpty)
                {
                    Output = _empty;
                    _isEmpty = true;
                }

                return drawDescription;
            }
            else
            {
                if (_isEmpty)
                {
                    Output = transform.Output;
                    _isEmpty = false;
                }
            }

            TimelineItemSourceDescription timeLineItemSourceDescription
                = new(timelineSourceDescription, fl.Frame, fl.Length, 0);
            DrawDescription desc = drawDescription;
            ID2D1Image? image = _input;

            foreach (var (effect, processor) in _chain)
            {
                if (effect.IsEnabled)
                {
                    IVideoEffectProcessor item = processor;
                    item.SetInput(image);
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

            _empty.Dispose();
        }
    }
}
