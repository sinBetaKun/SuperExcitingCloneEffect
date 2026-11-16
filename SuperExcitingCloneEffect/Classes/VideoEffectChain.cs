using SuperExcitingCloneEffect.Interfaces;
using System.Xml.Linq;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace SuperExcitingCloneEffect.Classes
{
    internal class VideoEffectChain : IDisposable
    {
        private readonly IGraphicsDevicesAndContext _devices;
        private readonly AffineTransform2D transform;
        private ID2D1Image? _input;
        private List<VideoEffectChainNode> _nodes = [];

        public ID2D1Image Output;

        public VideoEffectChain(IGraphicsDevicesAndContext devices)
        {
            _devices = devices;
            transform = new AffineTransform2D(devices.DeviceContext);
            Output = transform.Output;
        }

        private void UpdateNodes(List<IManagedItem> items)
        {
            IManagedItem[] mi_a0 = [.. _nodes.Select(x => x.Item)];
            var disposedIndex = from item in mi_a0
                                where !items.Exists(x => x == item)
                                select mi_a0.IndexOf(item) into i
                                orderby i descending
                                select i;

            foreach (int index in disposedIndex)
            {
                VideoEffectChainNode node = _nodes[index];
                node.Dispose();
                _nodes.RemoveAt(index);
            }

            IManagedItem[] keeped = [.. _nodes.Select(x => x.Item)];
            List<VideoEffectChainNode> newNodes = new(items.Count);
            foreach (IManagedItem item in items)
            {
                int index = keeped.IndexOf(item);
                newNodes.Add(index < 0 ? new VideoEffectChainNode(_devices, item) : _nodes[index]);
            }

            _nodes.Clear();
            _nodes.AddRange(newNodes);
        }

        public void SetInputAndItems(ID2D1Image? input, List<IManagedItem> items)
        {
            _input = input;
            UpdateNodes(items);
        }

        public void ClearChain()
        {
            _nodes.ForEach(x => x.Dispose());
            _nodes.Clear();
            transform.SetInput(0, _input, true);
        }

        public DrawDescription UpdateOutputAndDescription(TimelineItemSourceDescription timelineSourceDescription, DrawDescription drawDescription)
        {
            if (_input == null)
            {
                throw new InvalidOperationException("[VideoEffectChain] input is null.");
            }

            FrameAndLength fl = new(timelineSourceDescription);
            fl.Frame -= _nodes.Sum(x => x.Item.Offset);
            DrawDescription desc = drawDescription;
            ID2D1Image? image = _input;

            foreach (VideoEffectChainNode node in _nodes)
            {
                node.SetInputAndUpdateChain(image);
                desc = node.UpdateOutputAndDescription(timelineSourceDescription, desc, new FrameAndLength(fl));
                image = node.Output;
                fl.Frame += node.Item.Offset;
            }

            transform.SetInput(0, image, true);
            return desc;
        }

        public void Dispose()
        {
            transform.SetInput(0, null, true);
            Output.Dispose();
            transform.Dispose();
            _nodes.ForEach(x => x.Dispose());
        }
    }
}
