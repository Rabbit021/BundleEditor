using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundles
{
    [System.Serializable]
    public class BundleManagerControl : ISubWindow
    {
        private EditorWindow m_parent = null;

        [SerializeField]
        private TreeViewState m_bundleTreeState;
        private BundleTreeView m_BundleTreeView;

        [SerializeField]
        private TreeViewState m_assetTreeState;
        private AssetListTreeView m_assetList;

        private Rect m_position;

        private Rect m_horizontalSplitterRect;
        bool m_resizingHorizontalSplitter = false;

        private float m_horizontalSplitterPercent;
        private float kSplitterWidth = 3;

        public BundleManagerControl()
        {
            m_horizontalSplitterPercent = 0.4f;
        }

        public void OnEnable(Rect pos, EditorWindow parent)
        {
            m_position = pos;
            m_parent = parent;

            m_horizontalSplitterRect = new Rect()
            {
                x = m_position.x + m_position.width * m_horizontalSplitterPercent,
                y = m_position.y,
                width = kSplitterWidth,
                height = m_position.height
            };
        }

        public void Update()
        {
        }

        public void OnGUI(Rect pos)
        {
            var rect = new Rect(0, 0, pos.width, 30);
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新", GUILayout.ExpandHeight(true)))
            {
                BundleModel.Refresh();
                m_BundleTreeView.Reload();
                m_assetList.Reload();
            }
            if (GUILayout.Button("保存", GUILayout.ExpandHeight(true)))
            {
                BundleModel.Save();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            m_position = new Rect(0, 30, pos.width, pos.height - 30);
            if (m_BundleTreeView == null)
            {
                if (m_assetTreeState == null)
                    m_assetTreeState = new TreeViewState();
                m_assetList = new AssetListTreeView(m_assetTreeState);
                m_assetList.Reload();

                if (m_bundleTreeState == null)
                    m_bundleTreeState = new TreeViewState();
                m_BundleTreeView = new BundleTreeView(m_bundleTreeState, this);
                m_BundleTreeView.Refresh();
                m_parent.Repaint();
            }

            HandleHorizontalResize();
            //BundleData Rect
            var bundleTreeRect = new Rect()
            {
                x = m_position.x,
                y = m_position.y,
                width = m_horizontalSplitterRect.x,
                height = m_position.height - kSplitterWidth
            };

            m_BundleTreeView.OnGUI(bundleTreeRect);

            // Asset Rect
            float panelLeft = m_horizontalSplitterRect.x + kSplitterWidth;
            float panelWidth = m_position.width * (1 - m_horizontalSplitterPercent);
            float panelHeight = m_position.height;

            var assetRect = new Rect()
            {
                x = panelLeft,
                y = m_position.y,
                width = panelWidth,
                height = panelHeight
            };
            m_assetList.OnGUI(assetRect);

            if (m_resizingHorizontalSplitter)
                m_parent.Repaint();
        }

        private void RefreshManual()
        {
        }

        public void ForceReloadData()
        {
            m_parent.Repaint();
        }

        private void HandleHorizontalResize()
        {
            m_horizontalSplitterRect.x = (int)(m_position.width * m_horizontalSplitterPercent);
            m_horizontalSplitterRect.height = m_position.height;

            EditorGUIUtility.AddCursorRect(m_horizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.mouseDown && m_horizontalSplitterRect.Contains(Event.current.mousePosition))
                m_resizingHorizontalSplitter = true;

            if (m_resizingHorizontalSplitter)
            {
                m_horizontalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / m_position.width, 0.1f, 0.9f);
                m_horizontalSplitterRect.x = (int)(m_position.width * m_horizontalSplitterPercent);
            }

            if (Event.current.type == EventType.MouseUp)
                m_resizingHorizontalSplitter = false;
        }
    }
}