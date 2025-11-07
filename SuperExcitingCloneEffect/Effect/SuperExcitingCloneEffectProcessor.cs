using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

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
                    dc.DrawImage(cd.Output, compositeMode: CompositeMode.SourceOver);
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
