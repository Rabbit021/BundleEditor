using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace AssetBundles
{
    public class AssetListTreeView : TreeView
    {
        public AssetListTreeView(TreeViewState state) : base(state)
        {
            showBorder = true;
            Reload();
        }

        public AssetListTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>
            {
                new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
                new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
                new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
                new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
                new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
                new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
                new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
                new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
                new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
            };

            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;
        }
    }
}