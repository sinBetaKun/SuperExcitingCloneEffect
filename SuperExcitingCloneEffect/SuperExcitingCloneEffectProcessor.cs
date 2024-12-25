using SuperExcitingCloneEffect.CloneController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace SuperExcitingCloneEffect
{
    internal class SuperExcitingCloneEffectProcessor : IVideoEffectProcessor
    {
        readonly SuperExcitingCloneEffect item;
        readonly AffineTransform2D transformEffect;
        IGraphicsDevicesAndContext devices;
        ID2D1CommandList? commandList = null;
        ID2D1Image? input;
        readonly ID2D1Bitmap empty;

        bool isFirst = true;
        public ID2D1Image Output { get; }

        private List<CloneNode> CloneNodes = [];
        private int numOfClones = 0;

        public SuperExcitingCloneEffectProcessor(IGraphicsDevicesAndContext devices, SuperExcitingCloneEffect item)
        {
            this.item = item;

            this.devices = devices;
            transformEffect = new AffineTransform2D(devices.DeviceContext);//Outputのインスタンスを固定するために、間にエフェクトを挟む
            Output = transformEffect.Output;//EffectからgetしたOutputは必ずDisposeする必要がある。Effect側では開放されない。
            empty = devices.DeviceContext.CreateEmptyBitmap();
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            if (updateClonesValues(frame, length, fps))
            {
                commandList?.Dispose();

                UpdateParentPaths();

                UpdateOutputs(effectDescription);

                SetCommandList();
            }

            isFirst = false;

            return effectDescription.DrawDescription;
        }

        private void UpdateParentPaths()
        {
            List<CloneNode> independent = [];
            List<CloneNode> parents = [];
            List<CloneNode> children = [];

            foreach (var cloneNode in CloneNodes)
            {
                cloneNode.ParentPath = [cloneNode];
                if (cloneNode.TagName == cloneNode.Parent) independent.Add(cloneNode);
                else if (cloneNode.Parent == string.Empty) parents.Add(cloneNode);
                else children.Add(cloneNode);
                /*if (cloneNode.Parent == string.Empty)
                {
                    if (cloneNode.TagName == string.Empty || cloneNode.TagName == cloneNode.Parent) independent.Add(cloneNode);
                    else parents.Add(cloneNode);
                }
                else
                {
                    children.Add(cloneNode);
                }*/
            }
            int numOfChildren = children.Count;


            while (numOfChildren > 0)
            {
                for (int i = 0; i < children.Count;)
                {
                    var matched = from x in parents
                                  where (x.TagName == children[i].Parent)
                                  select x;

                    if (matched.Any())
                    {
                        children[i].ParentPath.AddRange(matched.First().ParentPath);
                        parents.Add(children[i]);
                        children.RemoveAt(i);
                    }
                    else i++;
                }

                if (numOfChildren == children.Count) break;
                numOfChildren = children.Count;
            }
        }

        private void SetCommandList()
        {
            if(numOfClones == 0)
            {
                transformEffect.SetInput(0, empty, true);
                return;
            }
            commandList = devices.DeviceContext.CreateCommandList();
            var dc = devices.DeviceContext;
            dc.Target = commandList;
            dc.BeginDraw();
            dc.Clear(null);

            for (int i = 0; i < numOfClones; i++)
            {
                if (CloneNodes[i].Appear)
                {
                    if (CloneNodes[i].Output is ID2D1Image output)
                    {
                        // var vec2 = CloneNodes[i].Shift;
                        switch (CloneNodes[i].BlendMode)
                        {
                            case BlendSECE.SourceOver:
                                dc.DrawImage(output, compositeMode: CompositeMode.SourceOver);
                                break;

                            case BlendSECE.Plus:
                                dc.DrawImage(output, compositeMode: CompositeMode.Plus);
                                break;

                            case BlendSECE.DestinationOver:
                                dc.DrawImage(output, compositeMode: CompositeMode.DestinationOver);
                                break;

                            case BlendSECE.DestinationOut:
                                dc.DrawImage(output, compositeMode: CompositeMode.DestinationOut);
                                break;

                            case BlendSECE.SourceAtop:
                                dc.DrawImage(output, compositeMode: CompositeMode.SourceAtop);
                                break;

                            case BlendSECE.XOR:
                                dc.DrawImage(output, compositeMode: CompositeMode.Xor);
                                break;

                            case BlendSECE.MaskInverseErt:
                                dc.DrawImage(output, compositeMode: CompositeMode.MaskInverseErt);
                                break;

                            case BlendSECE.Multiply:
                                dc.BlendImage(output, BlendMode.Multiply, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Screen:
                                dc.BlendImage(output, BlendMode.Screen, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Darken:
                                dc.BlendImage(output, BlendMode.Darken, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Lighten:
                                dc.BlendImage(output, BlendMode.Lighten, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Dissolve:
                                dc.BlendImage(output, BlendMode.Dissolve, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.ColorBurn:
                                dc.BlendImage(output, BlendMode.ColorBurn, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.LinearBurn:
                                dc.BlendImage(output, BlendMode.LinearBurn, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.DarkerColor:
                                dc.BlendImage(output, BlendMode.DarkerColor, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.LighterColor:
                                dc.BlendImage(output, BlendMode.LighterColor, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.ColorDodge:
                                dc.BlendImage(output, BlendMode.ColorDodge, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.LinearDodge:
                                dc.BlendImage(output, BlendMode.LinearDodge, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Overlay:
                                dc.BlendImage(output, BlendMode.Overlay, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.SoftLight:
                                dc.BlendImage(output, BlendMode.SoftLight, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.HardLight:
                                dc.BlendImage(output, BlendMode.HardLight, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.VividLight:
                                dc.BlendImage(output, BlendMode.VividLight, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.LinearLight:
                                dc.BlendImage(output, BlendMode.LinearLight, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.PinLight:
                                dc.BlendImage(output, BlendMode.PinLight, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.HardMix:
                                dc.BlendImage(output, BlendMode.HardMix, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Difference:
                                dc.BlendImage(output, BlendMode.Difference, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Exclusion:
                                dc.BlendImage(output, BlendMode.Exclusion, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Hue:
                                dc.BlendImage(output, BlendMode.Hue, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Saturation:
                                dc.BlendImage(output, BlendMode.Saturation, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Color:
                                dc.BlendImage(output, BlendMode.Color, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Luminosity:
                                dc.BlendImage(output, BlendMode.Luminosity, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Subtract:
                                dc.BlendImage(output, BlendMode.Subtract, null, null, InterpolationMode.MultiSampleLinear);
                                break;

                            case BlendSECE.Division:
                                dc.BlendImage(output, BlendMode.Division, null, null, InterpolationMode.MultiSampleLinear);
                                break;
                        }

                    }
                }
            }

            dc.EndDraw();
            commandList.Close();//CommandListはEndDraw()の後に必ずClose()を呼んで閉じる必要がある
            transformEffect.SetInput(0, commandList, true);
        }

        private bool updateClonesValues(long frame, long length, int fps)
        {
            bool isOld = false;

            if (numOfClones != item.Clones.Count || isFirst)
            {
                isOld = true;
                numOfClones = item.Clones.Count;
                RemoveNodes(0);
                for (int i = 0; i < numOfClones; i++)
                    CloneNodes.Add(new CloneNode(devices, item.Clones[i], length, frame, fps));
            }
            else
            {
                for (int i = 0; i < numOfClones; i++)
                {
                    if (CloneNodes[i].UpdateParams(devices, item.Clones[i], length, frame, fps))
                    {
                        isOld = true;
                    }
                }
            }
            return isOld;
        }

        private void UpdateOutputs(TimelineItemSourceDescription? timeLineItemSourceDescription)
        {
            if (timeLineItemSourceDescription is null)
            {
                return;
            }
            var sortedCloneNodes = CloneNodes.OrderBy(node => node.ParentPath.Count);
            foreach (var cloneNode in sortedCloneNodes)
                cloneNode.UpdateOutput(input, timeLineItemSourceDescription);

        }

        public void ClearInput()
        {
            input = null;
            transformEffect.SetInput(0, null, true);
            commandList?.Dispose();//前回のUpdateで作成したCommandListを破棄する
            RemoveNodes(0);
        }

        public void SetInput(ID2D1Image? input)
        {
            commandList?.Dispose();

            this.input = input;

            numOfClones = 0;

            RemoveNodes(0);

            UpdateParentPaths();

            UpdateOutputs(null);

            SetCommandList();
        }

        public void RemoveNodes(int count)
        {
            while (CloneNodes.Count > count)
            {
                CloneNodes[count].Dispose();
                CloneNodes.RemoveAt(count);
            }
        }

        public void Dispose()
        {
            commandList?.Dispose();//最後のUpdateで作成したCommandListを破棄
            transformEffect.SetInput(0, null, true);//EffectのInputは必ずnullに戻す。
            transformEffect.Dispose();
            RemoveNodes(0);
            Output.Dispose();//EffectからgetしたOutputは必ずDisposeする必要がある。Effect側では開放されない。
        }

    }
}
