using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundles
{
    public enum SortOption
    {
        Asset,
        Bundle,
        Size,
        Message
    }

    public enum MyColumns
    {
        Asset,
        Bundle,
        Size,
        Message
    }

    public class AssetListTreeView : TreeView
    {
        List<BundleDataInfo> m_SourceBundles = new List<BundleDataInfo>();
        private BundleManagerControl m_Controller;
        List<UnityEngine.Object> m_EmptyObjectList = new List<UnityEngine.Object>();

        public AssetListTreeView(TreeViewState state, MultiColumnHeaderState mchs, BundleManagerControl ctrl) : base(state, new MultiColumnHeader(mchs))
        {
            m_Controller = ctrl;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            DefaultStyles.label.richText = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        SortOption[] m_SortOptions =
        {
            SortOption.Asset,
            SortOption.Bundle,
            SortOption.Size,
            SortOption.Message
        };


        protected override TreeViewItem BuildRoot()
        {
            var root = BundleModel.CreateAssetListTreeView(m_SourceBundles);
            return root;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };
            retVal[0].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[0].minWidth = 50;
            retVal[0].width = 100;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("BundleData", "BundleData name. 'auto' means asset was pulled in due to dependency");
            retVal[1].minWidth = 50;
            retVal[1].width = 100;
            retVal[1].maxWidth = 300;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Size", "Size on disk");
            retVal[2].minWidth = 30;
            retVal[2].width = 75;
            retVal[2].maxWidth = 100;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("!", "Errors, Warnings, or Info");
            retVal[3].minWidth = 16;
            retVal[3].width = 16;
            retVal[3].maxWidth = 16;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = true;
            retVal[3].autoResize = false;

            return retVal;
        }

        internal void SetSelectedBundles(IEnumerable<BundleDataInfo> bundles)
        {
            m_Controller.SetSelectedItems(null);
            m_SourceBundles = bundles.ToList();
            SetSelection(new List<int>());
            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as AssetTreeItem;
            if (item == null) return;
            var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.asset.fullAssetName);
            EditorGUIUtility.PingObject(o);
            Selection.activeObject = o;
        }

        protected override void KeyEvent()
        {
            if (m_SourceBundles.Count > 0 && Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                var selectedNodes = new List<AssetTreeItem>();
                foreach (var nodeID in GetSelection())
                {
                    selectedNodes.Add(FindItem(nodeID, rootItem) as AssetTreeItem);
                }

                RemoveAssets(selectedNodes);
            }
        }

        private void ReloadAndSelect(IList<int> hashCodes)
        {
            Reload();
            SetSelection(hashCodes);
            SelectionChanged(hashCodes);
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var selectedObjects = new List<Object>();
            List<AssetInfo> selectedAssets = new List<AssetInfo>();
            foreach (var id in selectedIds)
            {
                var assetItem = FindItem(id, rootItem) as AssetTreeItem;
                if (assetItem != null)
                {
                    Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.asset.fullAssetName);
                    selectedObjects.Add(o);
                    Selection.activeObject = o;
                    selectedAssets.Add(assetItem.asset);
                }
            }
            m_Controller.SetSelectedItems(selectedAssets);
            Selection.objects = selectedObjects.ToArray();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), args.item as AssetTreeItem, args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, AssetTreeItem item, int column, ref RowGUIArgs args)
        {
            if (item == null) return;
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            if (column != 3)
                GUI.color = item.itemColor;

            switch (column)
            {
                case 0:
                    {
                        var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                        DefaultGUI.Label(
                            new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height),
                            item.displayName,
                            args.selected,
                            args.focused);
                    }
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, item.asset.bundleName, args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, item.asset.GetSizeString(), args.selected, args.focused);
                    break;
                case 3:
                    //var icon = item.MessageIcon();
                    Texture2D icon = null;
                    if (icon != null)
                    {
                        var iconRect = new Rect(cellRect.x, cellRect.y, cellRect.height, cellRect.height);
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }
                    break;
            }
            GUI.color = oldColor;
        }

        #region 排序

        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByColumn();

            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);

            Repaint();
        }

        void SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            List<AssetTreeItem> assetList = new List<AssetTreeItem>();
            foreach (var item in rootItem.children)
            {
                assetList.Add(item as AssetTreeItem);
            }
            var orderedItems = InitialOrder(assetList, sortedColumns);

            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<AssetTreeItem> InitialOrder(IEnumerable<AssetTreeItem> myTypes, int[] columnList)
        {
            SortOption sortOption = m_SortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.Asset:
                    return myTypes.Order(l => l.displayName, ascending);
                case SortOption.Size:
                    return myTypes.Order(l => l.asset.fileSize, ascending);
                case SortOption.Message:
                //return myTypes.Order(l => l.HighestMessageLevel(), ascending);
                case SortOption.Bundle:
                default:
                    return myTypes.Order(l => l.asset.bundleName, ascending);
            }
        }

        #endregion

        #region Contexnt Menu

        protected override void ContextClickedItem(int id)
        {
            var selectedNode = new List<AssetTreeItem>();
            foreach (var nodeId in GetSelection())
                selectedNode.Add(FindItem(nodeId, rootItem) as AssetTreeItem);

            if (selectedNode.Count > 0)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("DeleteAsset assets from assetBundle"), false, RemoveAssets, selectedNode);
                menu.ShowAsContext();
            }
        }

        private void RemoveAssets(object userdata)
        {
            var selectedNodes = userdata as List<AssetTreeItem>;
            var assets = new List<AssetInfo>();
            foreach (var node in selectedNodes)
            {
                if (node.asset.bundleName != string.Empty)
                    assets.Add(node.asset);
            }
            BundleModel.MoveAssetToBundle(assets, string.Empty);
            foreach (var bundle in m_SourceBundles)
            {
                bundle.Refresh();
            }
            m_Controller.UpdateSelectedBundles(m_SourceBundles);
        }

        #endregion

        #region Drag

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (IsValidDragDrop(args))
            {
                if (args.performDrop)
                {
                    BundleModel.MoveAssetToBundle(DragAndDrop.paths, m_SourceBundles[0].m_Name);
                    foreach (var bundle in m_SourceBundles)
                    {
                        bundle.Refresh();
                    }
                    m_Controller.UpdateSelectedBundles(m_SourceBundles);
                }
                return DragAndDropVisualMode.Copy;//Move;
            }

            return DragAndDropVisualMode.Rejected;
        }

        private bool IsValidDragDrop(DragAndDropArgs args)
        {
            //can't drag onto none or >1 bundles
            if (m_SourceBundles.Count == 0 || m_SourceBundles.Count > 1)
                return false;

            //can't drag nothing
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
                return false;

            var data = m_SourceBundles[0] as BundleDataInfo;
            if (data == null)
                return false; // this should never happen.

            var thing = DragAndDrop.GetGenericData("AssetListTreeSource") as AssetListTreeView;
            if (thing != null)
                return false;

            if (data.IsEmpty())
                return true;

            if (data.isSceneBundle)
                return false;


            foreach (var assetPath in DragAndDrop.paths)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset))
                    return false;
            }

            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();

            DragAndDrop.objectReferences = m_EmptyObjectList.ToArray();
            var items = new List<AssetTreeItem>(args.draggedItemIDs.Select(x => FindItem(x, rootItem) as AssetTreeItem));
            DragAndDrop.paths = items.Select(a => a.asset.fullAssetName).ToArray();
            DragAndDrop.SetGenericData("AssetListTreeSource", this);
            DragAndDrop.StartDrag("AssetListTree");
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            args.draggedItemIDs = GetSelection();
            return true;
        }

        #endregion
    }

    public static class ExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}