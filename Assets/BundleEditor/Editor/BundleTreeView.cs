using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Linq;

namespace AssetBundles
{
    public class BundleTreeView : TreeView
    {
        private BundleManagerControl m_Controller;
        private bool m_ContextOnItem = false;
        private List<UnityEngine.Object> m_EmptyObjectList = new List<UnityEngine.Object>();


        public BundleTreeView(TreeViewState state, BundleManagerControl ctrl) : base(state)
        {
            BundleModel.Refresh();
            m_Controller = ctrl;
            showBorder = true;
        }

        public BundleTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        protected override TreeViewItem BuildRoot()
        {
            return BundleModel.CreateAssetBundleTreeView();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item.icon == null)
                extraSpaceBeforeIconAndLabel = 16f;
            else
                extraSpaceBeforeIconAndLabel = 0f;
            base.RowGUI(args);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var selectedBundles = new List<BundleDataInfo>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem) as BundleTreeItem;
                selectedBundles.Add(item.BundleData);
            }
            m_Controller.UpdateSelectedBundles(selectedBundles);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item.displayName.Length > 0;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
            if (args.newName.Length > 0 && args.newName != args.originalName)
            {
                args.newName = args.newName.ToLower();
                args.acceptedRename = true;

                var renamedItem = FindItem(args.itemID, rootItem) as BundleTreeItem;
                args.acceptedRename = BundleModel.HandleBundleRename(renamedItem, args.newName);
                if (renamedItem != null) ReloadAndSelect(renamedItem.BundleData.nameHashCode, false);
            }
            else
            {
                args.acceptedRename = false;
            }
        }

        protected override void KeyEvent()
        {
            if (Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                var selectedNodes = new List<BundleTreeItem>();
                foreach (var nodeId in GetSelection())
                    selectedNodes.Add(FindItem(nodeId, rootItem) as BundleTreeItem);
                DeleteBundles(selectedNodes);
            }
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
        }

        protected override void ContextClicked()
        {
            if (m_ContextOnItem)
            {
                m_ContextOnItem = false;
                return;
            }
            var selectedNodes = new List<BundleTreeItem>();
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add New BundleData"), false, CreateNewAssetBundle, selectedNodes);
            menu.AddItem(new GUIContent("Add New Folder"), false, CreateFolder, selectedNodes);
            menu.AddItem(new GUIContent("Reload Data"), false, ForceReloadData, selectedNodes);
            menu.ShowAsContext();
        }

        protected override void ContextClickedItem(int id)
        {
            m_ContextOnItem = true;
            var selectedNodes = new List<BundleTreeItem>();
            foreach (var nodeId in GetSelection())
                selectedNodes.Add(FindItem(nodeId, rootItem) as BundleTreeItem);

            var menu = new GenericMenu();
            if (selectedNodes.Count == 1)
            {
                menu.AddItem(new GUIContent("Add New BundleData"), false, CreateNewAssetBundle, selectedNodes);
                menu.AddItem(new GUIContent("Add New Folder"), false, CreateFolder, selectedNodes);

                menu.AddItem(new GUIContent("Rename"), false, RenameBundle, selectedNodes);
                menu.AddItem(new GUIContent("Delete " + selectedNodes[0].displayName), false, DeleteBundles, selectedNodes);
            }
            else if (selectedNodes.Count > 1)
            {
                //    menu.AddItem(new GUIContent("Move duplicates shared by selected"), false, DedupeOverlappedBundles, selectedNodes);
                //    menu.AddItem(new GUIContent("Move duplicates existing in any selected"), false, DedupeAllBundles, selectedNodes);
                menu.AddItem(new GUIContent("Delete multiple bundles"), false, DeleteBundles, selectedNodes);
            }
            menu.ShowAsContext();
        }

        class DragAndDropData
        {
            public bool hasScene = false;
            public bool hasNonScene = false;
            public List<BundleDataInfo> draggedNodes;
            public BundleTreeItem targetNode;
            public DragAndDropArgs args;
            public string[] paths;

            public DragAndDropData(DragAndDropArgs a)
            {
                args = a;

                draggedNodes = DragAndDrop.GetGenericData("AssetBundles.BundleDataInfo") as List<BundleDataInfo>;
                targetNode = args.parentItem as BundleTreeItem;
                paths = DragAndDrop.paths;

                if (draggedNodes != null)
                {
                    foreach (var bundle in draggedNodes)
                    {
                        var dataBundle = bundle as BundleDataInfo;
                        if (dataBundle != null)
                        {
                            if (dataBundle.isSceneBundle)
                                hasScene = true;
                            else
                                hasNonScene = true;
                        }
                    }
                }
                else if (DragAndDrop.paths != null)
                {
                    foreach (var assetPath in DragAndDrop.paths)
                    {
                        if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset))
                            hasScene = true;
                        else
                            hasNonScene = true;
                    }
                }
            }
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var visualMode = DragAndDropVisualMode.None;
            var data = new DragAndDropData(args);

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                    visualMode = HandleDragAndDropUpon(data);
                    break;
                case DragAndDropPosition.BetweenItems:
                    visualMode = HandleDragAndDropBetween(data);
                    break;
                case DragAndDropPosition.OutsideItems:
                    if (data.draggedNodes != null)
                    {
                        visualMode = DragAndDropVisualMode.Copy;
                        if (data.args.performDrop)
                        {
                            // TODO ReParent
                            Reload();
                        }
                    }
                    else if (data.paths != null)
                    {
                        visualMode = DragAndDropVisualMode.Copy;
                        if (data.args.performDrop)
                        {
                            DragPathsToNewSpace(data.paths, null, data.hasScene);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return visualMode;
        }

        private void DragPathsToNewSpace(string[] paths, BundleDataInfo root, bool hasScene)
        {
            if (hasScene)
            {
                var hashCodes = new List<int>();
                foreach (var assetPath in paths)
                {
                    var newBundle = BundleModel.CreateEmptyBundle(root, System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower());
                    BundleModel.MoveAssetToBundle(assetPath, newBundle.m_Name);
                    hashCodes.Add(newBundle.nameHashCode);
                }
                ReloadAndSelect(hashCodes);
            }
            else
            {
                var newBundle = BundleModel.CreateEmptyBundle(root);
                BundleModel.MoveAssetToBundle(paths, newBundle.m_Name);
                ReloadAndSelect(newBundle.nameHashCode, true);
            }
        }

        private DragAndDropVisualMode HandleDragAndDropBetween(DragAndDropData data)
        {
            return DragAndDropVisualMode.Move;
        }

        private DragAndDropVisualMode HandleDragAndDropUpon(DragAndDropData data)
        {
            var visualMode = DragAndDropVisualMode.Copy;
            var targetDataBundle = data.targetNode.BundleData as BundleDataInfo;
            if (targetDataBundle != null)
            {
                if (targetDataBundle.isSceneBundle)
                    visualMode = DragAndDropVisualMode.Rejected;
                else
                {
                    if (data.hasScene && !targetDataBundle.IsEmpty())
                        visualMode = DragAndDropVisualMode.Rejected;
                    else
                    {
                        if (data.args.performDrop)
                        {
                            if (data.draggedNodes != null)
                            {
                                BundleModel.HandleBundleMerge(data.draggedNodes, targetDataBundle);
                                ReloadAndSelect(targetDataBundle.nameHashCode, false);
                            }
                            else if (data.paths != null)
                            {
                                BundleModel.MoveAssetToBundle(data.paths, targetDataBundle.m_Name);
                                ReloadAndSelect(targetDataBundle.nameHashCode, false);
                            }
                        }
                    }
                }
            }
            else
            {
                var root = data.targetNode.BundleData as BundleDataInfo;
            }
            return visualMode;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();

            var selectedBundles = new List<BundleDataInfo>();
            foreach (var id in args.draggedItemIDs)
            {
                var item = FindItem(id, rootItem) as BundleTreeItem;
                selectedBundles.Add(item.BundleData);
            }
            DragAndDrop.paths = null;
            DragAndDrop.objectReferences = m_EmptyObjectList.ToArray();
            DragAndDrop.SetGenericData("BundleDataInfo", selectedBundles);
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy; //Move;
            DragAndDrop.StartDrag("AssetBundleTree");
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            //return base.CanStartDrag(args);
            return true;
        }

        #region 编辑

        protected void CreateNewAssetBundle(object context)
        {
            BundleDataInfo info = null;
            var selectedNodes = context as List<BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                info = selectedNodes[0].BundleData as BundleDataInfo;
            }
            var newBundle = BundleModel.CreateEmptyBundle(info);
            ReloadAndSelect(newBundle.nameHashCode, true);
        }

        private void RenameBundle(object context)
        {
            var selectedNodes = context as List<BundleTreeItem>;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                BeginRename(FindItem(selectedNodes[0].BundleData.nameHashCode, rootItem), 0.1f);
            }
        }

        protected void ForceReloadData(object context)
        {
        }

        protected void CreateFolder(object context)
        {
        }

        protected void DeleteBundles(object context)
        {
            var selectedNodes = context as List<BundleTreeItem>;
            if (selectedNodes == null) return;
            BundleModel.HandleBundleDelete(selectedNodes.Select(x => x.BundleData));
            ReloadAndSelect(new List<int>());
        }

        private void ReloadAndSelect(int hashCode, bool rename)
        {
            var selection = new List<int>();
            selection.Add(hashCode);
            ReloadAndSelect(selection);
            if (rename)
            {
                BeginRename(FindItem(hashCode, rootItem), 0.25f);
            }
        }

        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes, TreeViewSelectionOptions.RevealAndFrame);
            SelectionChanged(hashCodes);
        }

        #endregion

        public void Refresh()
        {
            var selection = GetSelection();
            Reload();
            SelectionChanged(selection);
        }
    }
}