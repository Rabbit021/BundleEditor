using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundles
{
    public class AssetTreeItem : TreeViewItem
    {
        private AssetInfo m_asset;
        private Color m_color = new Color(0, 0, 0, 0);

        public Color itemColor
        {
            get
            {
                if (m_color.a == 0.0f)
                {
                    m_color = m_asset.GetColor();
                }
                return m_color;
            }
            set { m_color = value; }
        }

        public AssetInfo asset
        {
            get { return m_asset; }
        }

        public AssetTreeItem() : base(-1, -1)
        {
        }

        public AssetTreeItem(AssetInfo a) : base(a.nameHashCode, 0, a.displayName)
        {
            m_asset = a;
            icon = AssetDatabase.GetCachedIcon(a.fullAssetName) as Texture2D;
        }

        public bool ContainsChild(AssetInfo asset)
        {
            bool contains = false;
            if (children == null)
                return contains;

            foreach (var child in children)
            {
                if ((child as AssetTreeItem).asset.fullAssetName == asset.fullAssetName)
                {
                    contains = true;
                    break;
                }
            }

            return contains;
        }
    }
}