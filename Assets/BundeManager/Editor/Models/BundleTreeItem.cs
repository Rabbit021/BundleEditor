using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundles
{
    public class BundleTreeItem : TreeViewItem
    {
        private BundleDataInfo m_BundleData;

        public BundleDataInfo BundleData
        {
            get { return m_BundleData; }
        }

        public BundleTreeItem(BundleDataInfo dataInfo, int depth, Texture2D iconTexture) : base(dataInfo.nameHashCode, depth, dataInfo.m_Name)
        {
            m_BundleData = dataInfo;
            icon = iconTexture;
            children = new List<TreeViewItem>();
        }
    }
}