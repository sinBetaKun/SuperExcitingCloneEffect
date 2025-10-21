using SuperExcitingCloneEffect.Interfaces;

namespace SuperExcitingCloneEffect.Classes
{
    internal class CloneTreeNode
    {
        public IManagedItem? Source;

        public List<CloneTreeNode> Children = [];

        public CloneTreeNode(IEnumerable<IManagedItem> items)
        {
            Source = null;
            List<IManagedItem> list1 = [.. items];
            List<IManagedItem> list2 = [.. list1.Where(mi =>
            {
                if (mi.Parent is null)
                    return true;

                return !list1.Contains(mi.Parent);
            })];

            foreach (IManagedItem mi in list2)
                mi.Depth = 0;

            Children = [.. list2.Select(item => new CloneTreeNode(item))];
            List<IManagedItem> list3 = [.. list1.Where(mi => !list2.Contains(mi))];
            List<CloneTreeNode> list4 = [.. Children.Where(tn => tn.Source is CloneGroupValue)];
            List<CloneTreeNode> list5 = [];
            int depth = 1;

            while (list3.Count > 0)
            {
                foreach (CloneTreeNode tn in list4)
                {
                    CloneGroupValue gv = (CloneGroupValue)tn.Source!;
                    List<IManagedItem> list6 = [.. list3.Where(mi => mi.Parent == gv)];

                    foreach (IManagedItem mi in list6)
                        mi.Depth = depth;

                    foreach (IManagedItem mi in list6)
                        list3.Remove(mi);

                    tn.Children = [.. list6.Select(mi => new CloneTreeNode(mi))];
                    list5.AddRange(tn.Children.Where(tn => tn.Source is CloneGroupValue));
                }

                list4 = list5;
                list5 = [];
                depth++;
            }
        }

        private CloneTreeNode(IManagedItem? source)
        {
            Source = source;
        }

        public CloneTreeNode GetCloneTree()
        {
            CloneTreeNode node;

            if (Source is CloneValue cv)
                node = new(new CloneValue(cv));
            else if (Source is CloneGroupValue gv)
                node = new(new CloneGroupValue(gv));
            else
                node = new((IManagedItem?)null);

            node.Children = [.. Children.Select(tn1 => 
            {
                CloneTreeNode tn2 = tn1.GetCloneTree();
                tn2.Children.ForEach(child => {
                    if (child.Source is IManagedItem mi){
                        mi.Parent = tn2.Source as CloneGroupValue;
                    }
                });
                return tn2;
            })];

            return node;
        }

        public CloneTreeNode GetVisibleNode()
        {
            CloneTreeNode ret = new(Source);

            if ((Source is CloneGroupValue gv && gv.IsOpened) || Source is null)
                ret.Children = [.. Children.Select(tn => tn.GetVisibleNode())];

            return ret;
        }

        public List<IManagedItem> ToList()
        {
            List<IManagedItem> list = [];

            if (Source is IManagedItem mi)
                list.Add(mi);

            foreach (CloneTreeNode child in Children)
            {
                list.AddRange(child.ToList());
            }

            return list;
        }

        public CloneTreeNode? FindNode(IManagedItem mi)
        {
            if (Source == mi)
                return this;

            foreach (CloneTreeNode child in Children)
                if (child.FindNode(mi) is CloneTreeNode tn)
                    return tn;

            return null;
        }
    }
}
