using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetBundles
{
    [System.Serializable]
    public class AssetInfo
    {
        public bool isScene { get; set; }
        public long fileSize { get; set; }

        private string m_AssetName;
        private string m_DisplayName;
        private string m_BundleName;
        private HashSet<string> m_Parents;

        public AssetInfo(string assetName, string bundleName = "")
        {
            m_AssetName = assetName;
            m_BundleName = bundleName;
            isScene = false;
        }

        public string fullAssetName
        {
            get { return m_AssetName; }
            set
            {
                m_AssetName = value;
                m_DisplayName = System.IO.Path.GetFileNameWithoutExtension(m_AssetName);
                var fileinfo = new System.IO.FileInfo(m_AssetName);
                fileSize = fileinfo.Exists ? fileinfo.Length : 0;
            }
        }

        public string displayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }

        public string bundleName
        {
            get { return m_BundleName == "" ? "auto" : m_BundleName; }
            set { m_BundleName = value; }
        }

        public Color GetColor()
        {
            if (m_BundleName == "")
                return BundleModel.k_LightGray;
            else
                return Color.white;
        }

        public void AddParent(string name)
        {
            m_Parents.Add(name);
        }

        public void RemoveParent(string name)
        {
            m_Parents.Remove(name);
        }

        public string GetSizeString()
        {
            if (fileSize == 0)
                return "--";
            return EditorUtility.FormatBytes(fileSize);
        }

        private List<AssetInfo> m_Denpendences = null;

        public List<AssetInfo> GetDependencies()
        {
            if (m_Denpendences == null)
            {
                m_Denpendences = new List<AssetInfo>();
                if (AssetDatabase.IsValidFolder(m_AssetName))
                {
                }
                else
                {
                    foreach (var dep in AssetDatabase.GetDependencies(m_AssetName, true))
                    {
                        if (dep != m_AssetName)
                        {
                            // TODO
                        }
                    }
                }
            }
            return m_Denpendences;
        }
    }
}