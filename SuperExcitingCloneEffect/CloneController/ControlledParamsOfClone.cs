﻿using Newtonsoft.Json;
using SuperExcitingCloneEffect.Infomations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.CloneController
{
    public class ControlledParamsOfClone : SuperExcitingCloneDialog
    {
        public bool Appear { get => appear; set => Set(ref appear, value); }
        bool appear = true;

        [Display(GroupName = "ブロック情報", Name = "タグ")]
        [TextEditor(AcceptsReturn = true)]
        public string TagName { get => tagName; set => Set(ref tagName, value); }
        string tagName = string.Empty;

        [Display(GroupName = "ブロック情報", Name = "親")]
        [TextEditor(AcceptsReturn = true)]
        public string Parent { get => parent; set => Set(ref parent, value); }
        string parent = string.Empty;

        /// <summary>
        /// 描画時に現在の値を取得する際は GetBusNum を呼び出す。
        /// ここから直々に GetValue すると、Listbox の一要素の UI に変化が起きなくなってしまう。
        /// </summary>
        [Display(GroupName = "ブロック情報", Name = "バス")]
        [AnimationSlider("F0", "", -50, 50)]
        public Animation BusNum { get; } = new Animation(0, -1000, 1000);
        /*
                /// <summary>
                /// Listbox の一要素の UI を変化させて、ユーザに見やすくしたいんじゃ。
                /// </summary>
                [JsonIgnore]
                public int BusNumView { get => busNumView; set => Set(ref busNumView, value); }
                int busNumView = 0;

                /// <summary>
                /// BusNum の現在の値を取得する際はこちらを呼び出す
                /// </summary>
                public int GetBusNum(long frame, long length, int fps)
                {
                    BusNumView = (int)BusNum.GetValue(frame, length, fps);
                    return BusNumView;
                }*/

        [Display(GroupName = "ブロック情報", Name = "備考")]
        [TextEditor(AcceptsReturn = true)]
        public string Comment { get => comment; set => Set(ref comment, value); }
        string comment = string.Empty;

        [Display(GroupName = "描画", Name = "X")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation X { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "描画", Name = "Y")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Y { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "描画", Name = "Z")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Z { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "描画", Name = "不透明度")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation Opacity { get; } = new Animation(100, 0, 100);

        [Display(GroupName = "描画", Name = "拡大率")]
        [AnimationSlider("F1", "%", 0, 200)]
        public Animation Scale { get; } = new Animation(100, 0, 5000);

        [Display(GroupName = "描画", Name = "回転角")]
        [AnimationSlider("F1", "°", -360, 360)]
        public Animation Rotate { get; } = new Animation(0, -36000, 36000, 360);

        [Display(GroupName = "描画", Name = "左右反転")]
        [AnimationSlider("F0", "", 0, 1)]
        public Animation Mirror { get; } = new Animation(0, 0, 1);

        [Display(GroupName = "描画", Name = "合成モード")]
        [EnumComboBox]
        public BlendSECE BlendMode { get => blendMode; set { Set(ref blendMode, value); } }

        BlendSECE blendMode = BlendSECE.SourceOver;

        [Display(GroupName = "値の依存", Name = "X/Y/Z", Description = "X/Y/Z")]
        [ToggleSlider]
        public bool XYZDependent { get => xyzDependent; set => Set(ref xyzDependent, value); }
        bool xyzDependent = true;

        [Display(GroupName = "値の依存", Name = "拡大率", Description = "拡大率")]
        [ToggleSlider]
        public bool ScaleDependent { get => scaleDependent; set => Set(ref scaleDependent, value); }
        bool scaleDependent = true;

        [Display(GroupName = "値の依存", Name = "不透明度", Description = "不透明度")]
        [ToggleSlider]
        public bool OpacityDependent { get => opacityDependent; set => Set(ref opacityDependent, value); }
        bool opacityDependent = true;

        [Display(GroupName = "値の依存", Name = "回転角", Description = "回転角")]
        [ToggleSlider]
        public bool RotateDependent { get => rotateDependent; set => Set(ref rotateDependent, value); }
        bool rotateDependent = true;

        [Display(GroupName = "値の依存", Name = "左右反転", Description = "左右反転")]
        [ToggleSlider]
        public bool MirrorDependent { get => mirrorDependent; set => Set(ref mirrorDependent, value); }
        bool mirrorDependent = true;

        [Display(GroupName = "中心位置", Name = "X")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Cnt_X { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "中心位置", Name = "Y")]
        [AnimationSlider("F1", "px", -500, 500)]
        public Animation Cnt_Y { get; } = new Animation(0, -10000, 10000);

        [Display(GroupName = "中心位置", Name = "位置を保持")]
        [ToggleSlider]
        public bool KeepPlace { get => keepPlace; set => Set(ref keepPlace, value); }
        bool keepPlace = false;

        [Display(GroupName = "サブ拡大率", Name = "横方向")]
        [AnimationSlider("F1", "%", 0, 200)]
        public Animation Exp_X { get; } = new Animation(100, 0, 5000);

        [Display(GroupName = "サブ拡大率", Name = "縦方向")]
        [AnimationSlider("F1", "%", 0, 200)]
        public Animation Exp_Y { get; } = new Animation(100, 0, 5000);

        [Display(GroupName = "クローン個別エフェクトの依存", Name = "X/Y/Z", Description = "X/Y/Z")]
        [ToggleSlider]
        public bool EffectXYZDependent { get => effectXYZDependent; set => Set(ref effectXYZDependent, value); }
        bool effectXYZDependent = true;

        [Display(GroupName = "クローン個別エフェクトの依存", Name = "拡大率", Description = "拡大率")]
        [ToggleSlider]
        public bool EffectZoomDependent { get => effectZoomDependent; set => Set(ref effectZoomDependent, value); }
        bool effectZoomDependent = true;

        [Display(GroupName = "クローン個別エフェクトの依存", Name = "不透明度", Description = "不透明度")]
        [ToggleSlider]
        public bool EffectOpacityDependent { get => effectOpacityDependent; set => Set(ref effectOpacityDependent, value); }
        bool effectOpacityDependent = true;

        [Display(GroupName = "クローン個別エフェクトの依存", Name = "回転角", Description = "回転角")]
        [ToggleSlider]
        public bool EffectRotateDependent { get => effectRotateDependent; set => Set(ref effectRotateDependent, value); }
        bool effectRotateDependent = true;

        [Display(GroupName = "クローン個別エフェクトの依存", Name = "左右反転", Description = "左右反転")]
        [ToggleSlider]
        public bool EffectMirrorDependent { get => effectMirrorDependent; set => Set(ref effectMirrorDependent, value); }
        bool effectMirrorDependent = true;

        [Display(GroupName = "クローン個別エフェクトの依存", Name = "カメラ", Description = "カメラ")]
        [ToggleSlider]
        public bool EffectCameraDependent { get => effectCameraDependent; set => Set(ref effectCameraDependent, value); }
        bool effectCameraDependent = true;

        [Display(GroupName = "クローン個別エフェクトの依存", Name = "非遅延エフェクト", Description = "非遅延エフェクト")]
        [ToggleSlider]
        public bool EffectUnlazyDependent { get => effectUnlazyDependent; set => Set(ref effectUnlazyDependent, value); }
        bool effectUnlazyDependent = true;

        [Display(GroupName = "クローン個別エフェクト", Name = "")]
        [VideoEffectSelector(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ImmutableList<IVideoEffect> Effects { get => effects; set => Set(ref effects, value); }
        ImmutableList<IVideoEffect> effects = [];


        public void CopyFrom(ControlledParamsOfClone original)
        {
            Appear = original.Appear;
            BusNum.CopyFrom(original.BusNum);
            //BusNumView = original.BusNumView;
            TagName = original.TagName;
            Parent = original.Parent;
            Comment = original.Comment;
            X.CopyFrom(original.X);
            Y.CopyFrom(original.Y);
            Z.CopyFrom(original.Z);
            Opacity.CopyFrom(original.Opacity);
            Scale.CopyFrom(original.Scale);
            Rotate.CopyFrom(original.Rotate);
            Mirror.CopyFrom(original.Mirror);
            BlendMode = original.BlendMode;
            Cnt_X.CopyFrom(original.Cnt_X);
            Cnt_Y.CopyFrom(original.Cnt_Y);
            KeepPlace = original.KeepPlace;
            Exp_X.CopyFrom(original.Exp_X);
            Exp_Y.CopyFrom(original.Exp_Y);
            XYZDependent = original.XYZDependent;
            ScaleDependent = original.ScaleDependent;
            OpacityDependent = original.OpacityDependent;
            RotateDependent = original.RotateDependent;
            MirrorDependent = original.MirrorDependent;
            EffectXYZDependent = original.EffectXYZDependent;
            EffectZoomDependent = original.EffectZoomDependent;
            EffectOpacityDependent = original.EffectOpacityDependent;
            EffectRotateDependent = original.EffectRotateDependent;
            EffectMirrorDependent = original.EffectMirrorDependent;
            EffectCameraDependent = original.EffectCameraDependent;
            EffectUnlazyDependent = original.EffectUnlazyDependent;
            try
            {
                string effectsStr = JsonConvert.SerializeObject(original.Effects, Newtonsoft.Json.Formatting.Indented, GetJsonSetting);
                if (JsonConvert.DeserializeObject<IVideoEffect[]>(effectsStr, GetJsonSetting) is IVideoEffect[] effects)
                {
                    Effects = [.. effects];
                }
                else
                {
                    throw new Exception("Jsonで変換できないエフェクトを検知");
                }
            }
            catch (Exception ex)
            {
                Effects = [];
                ShowWarning(ex.Message);
            }
        }

        public static JsonSerializerSettings GetJsonSetting =>
            new()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

        protected override IEnumerable<IAnimatable> GetAnimatables() => [BusNum, X, Y, Z, Opacity, Scale, Rotate, Mirror, Cnt_X, Cnt_Y, Exp_X, Exp_Y, .. Effects];
    }
}
