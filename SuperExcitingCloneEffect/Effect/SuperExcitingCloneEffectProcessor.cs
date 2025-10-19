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

        public ID2D1Image Output => _trans.Output;

        public SuperExcitingCloneEffectProcessor(IGraphicsDevicesAndContext devices, SuperExcitingCloneEffect item)
        {
            _devices = devices;
            _item = item;
            _empty = devices.DeviceContext.CreateEmptyBitmap();
            _trans = new(devices.DeviceContext);
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
            {
                DrawDescription desc = cd.UpdateOutput(effectDescription);

                if (_item.ListManager.SelectedManagedItems.Contains(cd.Value))
                {
                    ctrls.AddRange(desc.Controllers);
                }
            }

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

        private void UpdateDrawers()
        {
            List<(CloneGroupValue, List<CloneGroupValue>)> gts1 = [];
            List<(CloneGroupValue, List<CloneGroupValue>)> gts2 = [];
            List<CloneDrawer> drawers = [];

            foreach (IManagedItem mi in _item.ListManager.ManagedItems)
            {
                if (mi is CloneGroupValue gv)
                {
                    gts1.Add((gv, []));
                }
                else
                {
                    CloneValue cv = (CloneValue)mi;

                    if (_drawers.FirstOrDefault(d => d.Value == cv) is CloneDrawer cd1)
                    {
                        cd1.SetGroupChain([]);
                        drawers.Add(cd1);
                    }
                    else
                    {
                        CloneDrawer cd2 = new(_devices, cv);
                        cd2.SetInput(_input);
                        cd2.SetGroupChain([]);
                        drawers.Add(cd2);
                    }
                }
            }

            while (gts1.Count > 0)
            {
                foreach ((CloneGroupValue head, List<CloneGroupValue> list) in gts1)
                {
                    foreach (IManagedItem mi in head.Chirdren)
                    {
                        if (mi is CloneGroupValue gv)
                        {
                            gts2.Add((gv, [head, .. list]));
                        }
                        else
                        {
                            CloneValue cv = (CloneValue)mi;

                            if (_drawers.FirstOrDefault(d => d.Value == cv) is CloneDrawer cd1)
                            {
                                cd1.SetGroupChain([head, .. list]);
                                drawers.Add(cd1);
                            }
                            else
                            {
                                CloneDrawer cd2 = new(_devices, cv);
                                cd2.SetInput(_input);
                                cd2.SetGroupChain([head, .. list]);
                                drawers.Add(cd2);
                            }
                        }
                    }
                }

                gts1 = gts2;
                gts2 = [];
            }

            if (!_drawers.SequenceEqual(drawers))
            {
                List<CloneDrawer> toDispose = [.. _drawers.Where(d => !drawers.Contains(d))];

                foreach (CloneDrawer cd in toDispose)
                    cd.Dispose();

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
            _trans.Output.Dispose();
            _trans.Dispose();
            _commandList?.Dispose();
            _drawers.ForEach(d => d.Dispose());
            _empty.Dispose();
        }
    }
}
