using SuperExcitingCloneEffect.Interfaces;

namespace SuperExcitingCloneEffect.Classes
{
    internal class CloneTreeNode
    {
        public IManagedItem? Source;

        public List<CloneTreeNode> Children = [];

        //private CloneTreeNode? Parent = null;

        public CloneTreeNode(IEnumerable<IManagedItem> items)
        {
            Source = null;
            List<IManagedItem> list1 = [.. items];
            List<IManagedItem> list2 = [.. list1.Where(mi =>
            {
                if (mi.ParentIndex < 0 || mi.ParentIndex >= list1.Count)
                    return true;

                return false;
            })];

            foreach (IManagedItem mi in list2)
                mi.Depth = 0;

            Children = [.. list2.Select(item => new CloneTreeNode(item/*, this*/))];
            List<IManagedItem> list3 = [.. list1.Where(mi => !list2.Contains(mi))];
            List<CloneTreeNode> list4 = [.. Children.Where(tn => tn.Source is CloneGroupValue)];
            List<CloneTreeNode> list5 = [];
            int depth = 1;

            while (list3.Count > 0)
            {
                foreach (CloneTreeNode tn in list4)
                {
                    CloneGroupValue gv = (CloneGroupValue)tn.Source!;
                    List<IManagedItem> list6 = [.. list3.Where(mi => list1[mi.ParentIndex] == gv)];

                    foreach (IManagedItem mi in list6)
                        mi.Depth = depth;

                    foreach (IManagedItem mi in list6)
                        list3.Remove(mi);

                    tn.Children = [.. list6.Select(mi => new CloneTreeNode(mi/*, tn*/))];
                    list5.AddRange(tn.Children.Where(tn => tn.Source is CloneGroupValue));
                }

                list4 = list5;
                list5 = [];
                depth++;
            }
        }

        private CloneTreeNode(IManagedItem? source/*, CloneTreeNode? parent*/)
        {
            Source = source;
            //Parent = parent;
        }

        public CloneTreeNode GetCloneTree(CloneTreeNode? parent = null)
        {
            CloneTreeNode node;

            if (Source is CloneValue cv)
                node = new(new CloneValue(cv)/*, parent*/);
            else if (Source is CloneGroupValue gv)
                node = new(new CloneGroupValue(gv)/*, parent*/);
            else
                node = new((IManagedItem?)null/*, parent*/);

            node.Children = [.. Children.Select(tn1 => tn1.GetCloneTree(node))];

            return node;
        }

/*
        private int CalcIndex()
        {
            if (Parent is not CloneTreeNode parent)
                return -1;

            int index1 = parent.Children.IndexOf(this);
            int count1 = 0;

            for (int i = 0; i < index1; i++)
                count1 += parent.Children[i].CalcChildrenCount();

            index1 += count1;

            int index2 = parent.CalcIndex();

            if (index2 < 0)
                return index1;

            return index1 + index2 + 1;
        }

        private int CalcChildrenCount()
        {
            int count = 0;

            foreach (CloneTreeNode tn in Children)
                count += tn.CalcChildrenCount() + 1;

            return count;
        }
*/

        public CloneTreeNode GetVisibleNode(CloneTreeNode? parent = null)
        {
            CloneTreeNode ret = new(Source/*, parent*/);

            if ((Source is CloneGroupValue gv && gv.IsOpened) || Source is null)
                ret.Children = [.. Children.Select(tn => tn.GetVisibleNode(ret))];

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
