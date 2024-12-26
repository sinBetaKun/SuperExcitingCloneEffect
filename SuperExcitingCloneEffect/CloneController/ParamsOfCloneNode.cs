using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.CloneController
{
    internal class ParamsOfCloneNode
    {
        public bool Appear { get; set; }
        public BlendSECE BlendMode { get; set; }
        public Double3 Draw { get; set; }
        public double Opacity { get; set; }
        public double Scale { get; set; }
        public double Rotate { get; set; }
        public bool Mirror { get; set; }
        public Double2 Center { get; set; } = new();
        public bool KeepPlace { get; set; }
        public Double2 ExpXY { get; set; } = new();
        public string TagName { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty;
        public bool XYZDependent { get; set; }
        public bool ScaleDependent { get; set; }
        public bool OpacityDependent { get; set; }
        public bool RotateDependent { get; set; }
        public bool MirrorDependent { get; set; }
        public bool EffectXYZDependent { get; set; }
        public bool EffectScaleDependent { get; set; }
        public bool EffectOpacityDependent { get; set; }
        public bool EffectRotateDependent { get; set; }
        public bool EffectMirrorDependent { get; set; }
        public DependentMode EffectDependentMode { get; set; }
        public ImmutableList<IVideoEffect> Effects { get; set; }

        public ParamsOfCloneNode(IGraphicsDevicesAndContext devices, CloneBlock block, long length, long frame, int fps)
        {
            Appear = block.Appear;
            BlendMode = block.BlendMode;
            Draw = new(
                block.X.GetValue(frame, length, fps),
                block.Y.GetValue(frame, length, fps),
                block.Z.GetValue(frame, length, fps)
                );
            Opacity = block.Opacity.GetValue(frame, length, fps);
            Scale = block.Scale.GetValue(frame, length, fps);
            Rotate = block.Rotate.GetValue(frame, length, fps);
            Mirror = block.Mirror.GetValue(frame, length, fps) > 0.5;
            Center = new(
                block.Cnt_X.GetValue(frame, length, fps),
                block.Cnt_Y.GetValue(frame, length, fps)
                );
            KeepPlace = block.KeepPlace;
            ExpXY = new(
                block.Exp_X.GetValue(frame, length, fps),
                block.Exp_Y.GetValue(frame, length, fps)
                );
            TagName = block.TagName;
            Parent = block.Parent;
            XYZDependent = block.XYZDependent;
            ScaleDependent = block.ScaleDependent;
            OpacityDependent = block.OpacityDependent;
            RotateDependent = block.RotateDependent;
            MirrorDependent = block.MirrorDependent;
            EffectDependentMode = block.EffectDependentMode;
            EffectXYZDependent = block.EffectXYZDependent;
            EffectScaleDependent = block.EffectScaleDependent;
            EffectOpacityDependent = block.EffectOpacityDependent;
            EffectRotateDependent = block.EffectRotateDependent;
            EffectMirrorDependent = block.EffectMirrorDependent;
            Effects = block.Effects;
        }

        public bool Update(IGraphicsDevicesAndContext devices, CloneBlock block, long length, long frame, int fps)
        {
            var appear = block.Appear;
            var blendMode = block.BlendMode;
            var draw = new Double3(
                block.X.GetValue(frame, length, fps),
                block.Y.GetValue(frame, length, fps),
                block.Z.GetValue(frame, length, fps)
                );
            var opacity = block.Opacity.GetValue(frame, length, fps);
            var scale = block.Scale.GetValue(frame, length, fps);
            var rotate = block.Rotate.GetValue(frame, length, fps);
            var mirror = block.Mirror.GetValue(frame, length, fps) > 0.5;
            var center = new Double2(
                block.Cnt_X.GetValue(frame, length, fps),
                block.Cnt_Y.GetValue(frame, length, fps)
                );
            var keepPlace = block.KeepPlace;
            var expXY = new Double2(
                block.Exp_X.GetValue(frame, length, fps),
                block.Exp_Y.GetValue(frame, length, fps)
                );
            var tagName = block.TagName;
            var parent = block.Parent;
            var xyzDependent = block.XYZDependent;
            var scaleDependent = block.ScaleDependent;
            var opacityDependent = block.OpacityDependent;
            var rotateDependent = block.RotateDependent;
            var mirrorDependent = block.MirrorDependent;
            var effectXYZDependent = block.EffectXYZDependent;
            var effectScaleDependent = block.EffectScaleDependent;
            var effectRotateDependent = block.EffectRotateDependent;
            var effectOpacityDependent = block.EffectOpacityDependent;
            var effectMirrorDependent = block.EffectMirrorDependent;
            var effectDependentMode = block.EffectDependentMode;
            var effects = block.Effects;

            if (Appear != appear || BlendMode != blendMode || Draw != draw || Opacity != opacity || Rotate != rotate || Mirror != mirror
                || Center != center || KeepPlace != keepPlace || ExpXY != expXY || TagName != tagName || Parent != parent

                || XYZDependent != xyzDependent || ScaleDependent != scaleDependent || OpacityDependent != opacityDependent
                || RotateDependent != rotateDependent || MirrorDependent != mirrorDependent

                || EffectXYZDependent != effectXYZDependent || EffectScaleDependent != effectScaleDependent || EffectRotateDependent != effectRotateDependent
                || EffectOpacityDependent != effectOpacityDependent || EffectMirrorDependent != effectMirrorDependent || EffectDependentMode != effectDependentMode
                || effects.Count > 0 || effects.Count != Effects.Count)
            {
                Appear = appear;
                BlendMode = blendMode;
                Draw = draw;
                Opacity = opacity;
                Scale = scale;
                Rotate = rotate;
                Mirror = mirror;
                Center = center;
                KeepPlace = keepPlace;
                ExpXY = expXY;
                TagName = tagName;
                Parent = parent;
                XYZDependent = xyzDependent;
                ScaleDependent = scaleDependent;
                OpacityDependent = opacityDependent;
                RotateDependent = rotateDependent;
                MirrorDependent = mirrorDependent;
                EffectXYZDependent = effectXYZDependent;
                EffectScaleDependent = effectScaleDependent;
                EffectOpacityDependent = effectOpacityDependent;
                EffectMirrorDependent = effectMirrorDependent;
                EffectDependentMode = effectDependentMode;
                Effects = effects;
                return true;
            }
            return false;
        }
    }
}
