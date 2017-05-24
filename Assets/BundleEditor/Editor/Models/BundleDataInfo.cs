using System.Collections.Generic;
using UnityEditor;
using System;

namespace AssetBundles
{
    [System.Serializable]
    public class BundleDataInfo
    {
        public string m_Name;

        public List<AssetInfo> m_ConcreteAssets;
        public List<AssetInfo> m_DependentAssets;

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
            // 
        }
    }
}