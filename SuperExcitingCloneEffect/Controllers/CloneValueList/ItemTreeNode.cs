using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    internal class ItemTreeNode(IManagedItem item, sbyte selected)
    {
        /// <summary>
        /// このノードが対応するクローン
        /// </summary>
        public readonly IManagedItem Item = item;

        /// <summary>
        /// 親ノード
        /// </summary>
        public ItemTreeNode? Parent = null;

        /// <summary>
        /// 子ノード
        /// </summary>
        public readonly List<ItemTreeNode> Children = [];

        /// <summary>
        /// 選択状態。最初に選択されたものなら1, 追加選択なら2, 未選択なら0
        /// </summary>
        public readonly sbyte Selected = selected;

        /// <summary>
        /// 構造内からパーツに対応するノードを探し出す。
        /// </summary>
        /// <param name="item">このパーツに対応するノードを探す。</param>
        /// <returns>見つけたノード</returns>
        public ItemTreeNode? Find(IManagedItem item)
        {
            // 子ノードのいずれかが該当するか確かめる。
            if (Children.FirstOrDefault(x => x.Item == item) is ItemTreeNode child)
            {
                return child;
            }

            // 子ノードの子孫を探索する。
            for (int i = 0; i < Children.Count; i++)
            {
                // 見つけた場合
                if (Children[i].Find(item) is ItemTreeNode ret)
                    return ret;
            }

            // 見つからなかった場合
            return null;
        }

        /// <summary>
        /// 木構造をパーツリストに戻す。
        /// </summary>
        /// <param name="nodes">リストにしたい木構造</param>
        /// <returns>木構造をリスト化したものと選択された状態のパーツ</returns>
        public static ItemTreeArgs GetList(IEnumerable<ItemTreeNode> nodes)
        {
            List<IManagedItem> list = [];
            List<IManagedItem> additionalSelecteds = [];
            IManagedItem? selected = null;

            foreach (ItemTreeNode node in nodes)
            {
                IManagedItem item = node.Item;
                item.ParentIndex = -1;
                item.Depth = 0;
                list.Add(item);

                switch (node.Selected)
                {
                    case 1:
                        selected = item;
                        break;

                    case 2:
                        additionalSelecteds.Add(item);
                        break;
                }

                ItemTreeArgs descendants = GetList(node.Children);

                foreach (IManagedItem descendant in descendants.List)
                {
                    descendant.Depth++;
                    descendant.ParentIndex += list.Count;
                }

                list.AddRange(descendants.List);
                additionalSelecteds.AddRange(descendants.AdditionalSelecteds);

                if (descendants.Selected is not null)
                    selected = descendants.Selected;
            }

            return new ItemTreeArgs(list, selected, additionalSelecteds);
        }

        /// <summary>
        /// 木構造のうち表示するパーツのリストを作る。
        /// </summary>
        /// <param name="nodes">リストにしたい木構造</param>
        /// <returns>木構造をリスト化したもの</returns>
        public static List<IManagedItem> GetVisibleList(IEnumerable<ItemTreeNode> nodes)
        {
            // 返すリスト
            List<IManagedItem> mi_l0 = [];

            // 渡されたノードを1つずつ
            foreach (ItemTreeNode n_0 in nodes)
            {
                // ノードに対応するクローン/グループ
                IManagedItem mi_0 = n_0.Item;
                mi_l0.Add(mi_0);

                if (mi_0 is CloneGroupValue gv && gv.IsOpened)
                    mi_l0.AddRange(GetVisibleList(n_0.Children)); // 子孫をリストにしたもの
            }

            return mi_l0;
        }

        /// <summary>
        /// このノードの隣にノード群を挿入することを試す。
        /// このノードの子要素になるか、ならずに同階層に差し込まれる。
        /// 自身が始祖である場合はここでは挿入処理ができないので拒否。
        /// </summary>
        /// <param name="list">挿入するノード群</param>
        /// <returns>挿入に失敗した場合、真。</returns>
        public bool InsertToHere(List<ItemTreeNode> nodes)
        {
            if (Item is CloneGroupValue group && Children.Count == 0)
            {
                Children.AddRange(nodes);

                foreach (ItemTreeNode node in nodes)
                    node.Parent = this;

                return false;
            }

            if (Parent is null)
                return true;

            if (Parent.Children.Last() == this)
            {
                Parent.Children.AddRange(nodes);
            }
            else
            {
                int i = Parent.Children.IndexOf(this);
                Parent.Children.InsertRange(i + 1, nodes);
            }

            return false;
        }

        /// <summary>
        /// パーツリストを木構造に変換する。
        /// </summary>
        /// <param name="args">変換したいリスト情報</param>
        /// <returns>リストを木構造に変換したもの</returns>
        public static List<ItemTreeNode> GetTree(ItemTreeArgs args)
        {
            List<ItemTreeNode> ret = [];
            List<ItemTreeNode> descendants = [];

            foreach (IManagedItem item in args.List)
            {
                sbyte selected;

                if (args.Selected == item)
                    selected = 1;
                else if (args.AdditionalSelecteds.Contains(item))
                    selected = 2;
                else
                    selected = 0;

                ItemTreeNode node = new(item, selected);
                ret.Add(node);
                int i = item.ParentIndex;

                if (i >= 0)
                {
                    ret[i].Children.Add(node);
                    descendants.Add(node);
                    node.Parent = ret[i];
                }
            }

            foreach (ItemTreeNode descendant in descendants)
                ret.Remove(descendant);

            return ret;
        }
    }
}
