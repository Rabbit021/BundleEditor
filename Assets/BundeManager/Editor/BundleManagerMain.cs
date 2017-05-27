using System;
using UnityEditor;
using UnityEngine;

namespace AssetBundles
{
    public class BundleManagerMain : EditorWindow
    {
        private BundleManagerControl m_bundleManager;
        private BundleBuildControl m_bundleBuild;
        private float toolbarPadding = 5f;
        private Mode m_Mode = Mode.Browser;

        const float k_ToolbarPadding = 15;
        const float k_MenubarPadding = 32;

        [MenuItem("AssetBundles/Browser")]
        static void ShowWindow()
        {
            GetWindow<BundleManagerMain>("AssetBundles");
        }

        private void OnEnable()
        {
            var subPos = GetSubWindowArea();
            if (m_bundleManager == null)
                m_bundleManager = new BundleManagerControl();
            m_bundleManager.OnEnable(subPos, this);

            if (m_bundleBuild == null)
                m_bundleBuild = new BundleBuildControl();
            m_bundleBuild.OnEnable(subPos, this);
        }

        private void Update()
        {
            switch (m_Mode)
            {
                case Mode.Builder:
                    m_bundleBuild.Update();
                    break;
                case Mode.Browser:
                    m_bundleManager.Update();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnGUI()
        {
            ModeToggle();
            switch (m_Mode)
            {
                case Mode.Builder:
                    m_bundleBuild.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Browser:
                    m_bundleManager.OnGUI(GetSubWindowArea());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ModeToggle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(k_ToolbarPadding);
            float toolbarWidth = position.width - k_ToolbarPadding * 4;
            string[] labels = new string[2] { "Browser", "Build" };
            m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, labels, "LargeButton", GUILayout.Width(toolbarWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private Rect GetSubWindowArea()
        {
            Rect subPos = new Rect(0, toolbarPadding + k_MenubarPadding, position.width, position.height - toolbarPadding - k_MenubarPadding);
            return subPos;
        }
    }

    public enum Mode : int
    {
        Browser = 0,
        Builder = 1
    }
}