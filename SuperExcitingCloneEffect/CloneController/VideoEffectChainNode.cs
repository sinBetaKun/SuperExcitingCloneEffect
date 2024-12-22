using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.CloneController
{
    internal class VideoEffectChainNode
    {
        List<(IVideoEffect, IVideoEffectProcessor)> Chain = [];
        readonly AffineTransform2D transform;
        readonly ID2D1Bitmap empty;
        bool wasEmpty = false;
        public ID2D1Image Output;

        public VideoEffectChainNode(IGraphicsDevicesAndContext devices)
        {
            transform = new AffineTransform2D(devices.DeviceContext);
            empty = devices.DeviceContext.CreateEmptyBitmap();
            Output = transform.Output;
        }

        public void Update(VideoEffectChainNode original)
        {
        }

    }
}
