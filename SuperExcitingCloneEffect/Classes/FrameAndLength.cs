using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace SuperExcitingCloneEffect.Classes
{
    internal class FrameAndLength
    {
        public int Frame;
        public int Length;

        public FrameAndLength(TimelineItemSourceDescription description)
        {
            Frame = description.ItemPosition.Frame;
            Length = description.ItemDuration.Frame;
        }

        public void CopyFrom(FrameAndLength origin)
        {
            Frame = origin.Frame;
            Length = origin.Length;
        }

        public double GetValue(Animation animation, int fps)
            => animation.GetValue(Frame, Length, fps);
    }
}
