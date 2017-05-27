using UnityEditor;
using UnityEngine;
using System;

namespace AssetBundles
{
    [Serializable]
    public class BundleBuildControl : ISubWindow
    {
        const string k_BuildPrefPrefix = "ABBBuild:";

        [SerializeField]
        private bool m_AdvanceSettings;

        private GUIContent[] m_CompressOption = new[]
        {
            new GUIContent("No Compression"),
            new GUIContent("Standard Compression (LZMA)"),
            new GUIContent("Chunk Based Compression (LZ4)")
        };

        private Vector2 m_ScrollPosition;
        private GUIContent m_BuildTargetContent;
        private BuildTarget m_BuildTarget;
        private string m_OutputPath;
        private bool m_UserDefaultPath = true;

        public BundleBuildControl()
        {
            m_AdvanceSettings = false;
        }

        public void OnEnable(Rect pos, EditorWindow parent)
        {
            m_BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            m_BuildTargetContent = new GUIContent("Build Target", "Choose target platform to build for");
        }

        public void OnGUI(Rect pos)
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            EditorGUILayout.Space();
            GUILayout.BeginVertical();
            // 选择平台
            var target = (BuildTarget)EditorGUILayout.EnumPopup(m_BuildTargetContent, m_BuildTarget);
            if (target != m_BuildTarget)
            {
                m_BuildTarget = target;
                EditorPrefs.SetInt(k_BuildPrefPrefix + "BuildTarget", (int)m_BuildTarget);
                if (m_UserDefaultPath)
                {
                    m_OutputPath = "AssetBundles/";
                    m_OutputPath += m_BuildTarget.ToString();
                    EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
                }

            }

            EditorGUILayout.Space();
            GUILayout.BeginVertical();
            // 路径
            var newPath = EditorGUILayout.TextField("Out Path", m_OutputPath);
            if (newPath != m_OutputPath)
            {
                m_UserDefaultPath = false;
                m_OutputPath = newPath;
                EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
            }


            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        public void Update()
        {
        }

        public void ForceReloadData()
        {
        }


        public class ToggleData
        {
        }
    }
}