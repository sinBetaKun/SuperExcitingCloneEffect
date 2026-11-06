using SuperExcitingCloneEffect.Interfaces;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    internal record ItemTreeArgs(List<IManagedItem> List, IManagedItem? Selected, List<IManagedItem> AdditionalSelecteds)
    {
    }
}
