using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;

namespace AssetBundles
{
    [System.Serializable]
    public class BundleDataInfo
    {
        public string m_Name;
        public List<AssetInfo> m_ConcreteAssets;
        public List<AssetInfo> m_DependentAssets;
        public HashSet<string> m_BundleDependencies;

        public int m_ConcreteCounter;
        public int m_DependentCounter;
        public bool m_IsSceneBundle;

        public bool isSceneBundle
        {
            get { return m_IsSceneBundle; }
            set { m_IsSceneBundle = value; }
        }

        public long m_TotalSize;

        public int nameHashCode
        {
            get { return m_Name.GetHashCode(); }
        }

        public BundleDataInfo(string name)
        {
            m_ConcreteAssets = new List<AssetInfo>();
            m_DependentAssets = new List<AssetInfo>();
            m_BundleDependencies = new HashSet<string>();
            m_ConcreteCounter = 0;
            m_DependentCounter = 0;
            m_Name = name;
        }

        public bool IsEmpty()
        {
            return (m_ConcreteAssets.Count == 0);
        }

        public bool HandleRename(string newName)
        {
            m_Name = newName;
            return true;
        }

        public bool HandleDelete()
        {
            return true;
        }

        public string TotalSize()
        {
            if (m_TotalSize == 0)
                return "--";
            return EditorUtility.FormatBytes(m_TotalSize);
        }

        public BundleTreeItem CreateTreeView(int depth)
        {
            if (isSceneBundle)
                return new BundleTreeItem(this, depth, null);
            else
                return new BundleTreeItem(this, depth, null);
        }

        public void Refresh()
        {
            m_DependentAssets.Clear();
            foreach (var itr in m_ConcreteAssets)
            {
                m_DependentAssets.AddRange(itr.GetDependencies());
            }
            BundleModel.Save();
        }

        public List<string> GetBundleDependencies()
        {
            return m_DependentAssets.Select(x => x.fullAssetName).ToList();
        }

        public List<string> GetConcreteAssets()
        {
            return m_ConcreteAssets.Select(x => x.fullAssetName).ToList();
        }

        public void AddAssetsToNode(AssetTreeItem root)
        {
            if (root == null) return;
            foreach (var itr in m_ConcreteAssets)
                root.AddChild(new AssetTreeItem(itr));

            foreach (var itr in m_DependentAssets)
            {
                if (!root.ContainsChild(itr))
                    root.AddChild(new AssetTreeItem(itr));
            }
        }

        public void AddAssets(AssetInfo asset)
        {
            m_ConcreteAssets.Add(asset);
            Refresh();
        }

        public void DeleteAsset(AssetInfo asset)
        {
            m_ConcreteAssets.Remove(asset);
            Refresh();
        }
    }
}