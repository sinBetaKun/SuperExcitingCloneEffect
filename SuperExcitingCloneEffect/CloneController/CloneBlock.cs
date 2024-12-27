using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.CloneController
{
    public class CloneBlock : ControlledParamsOfClone
    {
        [JsonIgnore]
        public bool Selected { get => selected; set => Set(ref selected, value); }
        bool selected = false;

        public CloneBlock()
        {
            
        }

        public CloneBlock(CloneBlock original)
        {
            CopyFrom(original);
        }
    }
}
