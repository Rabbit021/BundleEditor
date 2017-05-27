using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundles
{
    public class BundleModel
    {
        private const string k_NewBundleName = "newbundle";
        private const string savePath = "Assets/BundeManager/Editor/save.json";
        public static Color k_LightGray = Color.gray * 1.5f;

        private static BundleDataInfo m_RootLevelDataInfo = new BundleDataInfo("root");
        private static List<BundleDataInfo> m_BundleList = new List<BundleDataInfo>();
        private static Dictionary<string, AssetInfo> m_GlobalAssetList = new Dictionary<string, AssetInfo>();

        private static Dictionary<string, HashSet<string>> m_DependencyTracker = new Dictionary<string, HashSet<string>>();

        public static BundleDataInfo CreateEmptyBundle(string newName = null)
        {
            var bundle = new BundleDataInfo(GetUniqueName(newName));
            m_BundleList.Add(bundle);
            Save();
            return bundle;
        }

        public static BundleDataInfo CreateEmptyBundle(BundleDataInfo info = null, string newName = null)
        {
            var name = newName;
            return CreateEmptyBundle(GetUniqueName(name));
        }

        public static BundleTreeItem CreateAssetBundleTreeView()
        {
            var root = new BundleTreeItem(m_RootLevelDataInfo, -1, null);
            foreach (var itr in m_BundleList)
            {
                var child = new BundleTreeItem(itr, 0, null);
                root.AddChild(child);
            }
            return root;
        }

        public static AssetTreeItem CreateAssetListTreeView(List<BundleDataInfo> selectes)
        {
            var root = new AssetTreeItem();
            root.children = new List<TreeViewItem>();

            if (selectes != null)
            {
                foreach (var itr in selectes)
                {
                    if (itr == null) continue;
                    itr.AddAssetsToNode(root);
                }
            }
            return root;
        }

        public static void HandleBundleDelete(IEnumerable<BundleDataInfo> assetBundleInfos)
        {
            foreach (var itr in assetBundleInfos)
                m_BundleList.Remove(itr);
            Save();
        }

        public static bool HandleBundleRename(BundleTreeItem item, string newName)
        {
            bool result = item.BundleData.HandleRename(newName);
            return result;
        }

        public static string GetUniqueName(string suggestedName)
        {
            suggestedName = string.IsNullOrEmpty(suggestedName) ? k_NewBundleName : suggestedName;
            string name = suggestedName;
            int index = 1;
            bool foundExisting = m_BundleList.FirstOrDefault(x => x.m_Name == name) != null;
            while (foundExisting)
            {
                name = suggestedName + index;
                index++;
                foundExisting = m_BundleList.FirstOrDefault(x => x.m_Name == name) != null;
            }
            return name;
        }

        public static BundleDataInfo GetBundleDataInfo(string name)
        {
            return m_BundleList.FirstOrDefault(x => x.m_Name == name);
        }

        public static void Save()
        {
            var obj = new BundleFile() { Bundles = m_BundleList };
            var str = JsonUtility.ToJson(obj);
            File.WriteAllText(savePath, str);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RefreshList()
        {
            foreach (var itr in m_BundleList)
                itr.Refresh();
        }

        public static void Reload()
        {
            if (!File.Exists(savePath))
            {
                m_BundleList = new List<BundleDataInfo>();
                return;
            }
            var json = File.ReadAllText(savePath);
            var file = JsonUtility.FromJson<BundleFile>(json);
            m_BundleList = file.Bundles;
        }

        public static void HandleBundleMerge(List<BundleDataInfo> draggedNodes, BundleDataInfo targetDataBundle)
        {
        }

        public static void MoveAssetToBundle(string[] assetPaths, string bundleName)
        {
            foreach (var itr in assetPaths)
                MoveAssetToBundle(itr, bundleName);
        }

        public static void MoveAssetToBundle(string assetPath, string bundleName)
        {
            var asset = new AssetInfo(assetPath, bundleName);
            MoveAssetToBundle(asset, bundleName);
        }

        public static void MoveAssetToBundle(List<AssetInfo> assets, string bundleName)
        {
            foreach (var asset in assets)
                MoveAssetToBundle(asset, bundleName);
        }

        private static void MoveAssetToBundle(AssetInfo asset, string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                // TOOD 删除
                var bundle = GetBundleDataInfo(asset.bundleName);
                if (bundle != null)
                    bundle.DeleteAsset(asset);
            }
            else
            {
                var bundle = GetBundleDataInfo(bundleName);
                if (bundle != null)
                    bundle.AddAssets(asset);
            }
        }

        public static AssetInfo CreateAsset(string name, string bundleName)
        {
            if (ValidateAsset(name))
            {
                return CreateAsset(name, bundleName, null);
            }
            return null;
        }

        private static AssetInfo CreateAsset(string name, string bundleName, AssetInfo parent)
        {
            if (bundleName != string.Empty)
            {
                return new AssetInfo(name, bundleName);
            }
            else
            {
                AssetInfo info = null;
                if (!m_GlobalAssetList.TryGetValue(name, out info))
                {
                    info = new AssetInfo(name, string.Empty);
                    m_GlobalAssetList.Add(name, info);
                }
                info.AddParent(parent.displayName);
                return info;
            }
        }

        public static bool ValidateAsset(string name)
        {
            if (!name.StartsWith("Assets/"))
                return false;
            string ext = System.IO.Path.GetExtension(name);
            if (ext == ".dll" || ext == ".cs" || ext == ".meta" || ext == ".js" || ext == ".boo")
                return false;
            return true;
        }

        public static void Analysize()
        {
            Debug.ClearDeveloperConsole();
        }

        public static void BuildAssetBundles(string outpath, BuildAssetBundleOptions options, BuildTarget target)
        {
            // TODO
            var lst = new AssetBundleBuild[m_BundleList.Count];
            for (var i = 0; i < m_BundleList.Count; i++)
            {
                var itr = m_BundleList[i];
                var build = new AssetBundleBuild();
                build.assetBundleName = itr.m_Name;
                build.assetNames = itr.GetConcreteAssets().ToArray();
                lst[i] = build;
            }
            BuildPipeline.BuildAssetBundles(outpath, lst, options, target);
        }
    }

    public class BundleFile
    {
        public List<BundleDataInfo> Bundles;
    }
}