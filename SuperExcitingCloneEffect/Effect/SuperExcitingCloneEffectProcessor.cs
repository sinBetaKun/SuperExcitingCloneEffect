using SuperExcitingCloneEffect.Properties;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace SuperExcitingCloneEffect.Effect
{
    internal class SuperExcitingCloneEffectProcessor : IVideoEffectProcessor
    {
        private readonly IGraphicsDevicesAndContext _devices;
        private readonly SuperExcitingCloneEffect _item;

        private ID2D1Image? _input;

        public ID2D1Image Output => _input ?? throw new ArgumentNullException(TextResource.Exception_CommandListIsNull);

        public SuperExcitingCloneEffectProcessor(IGraphicsDevicesAndContext devices, SuperExcitingCloneEffect item)
        {
            _devices = devices;
            _item = item;
        }

        public void SetInput(ID2D1Image? input)
        {
            _input = input;
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            return effectDescription.DrawDescription;
        }

        public void ClearInput()
        {
        }

        public void Dispose()
        {
        }
    }
}
