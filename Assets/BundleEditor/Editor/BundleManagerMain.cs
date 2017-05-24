using UnityEditor;
using UnityEngine;

namespace AssetBundles
{
    public class BundleManagerMain : EditorWindow
    {
        private BundleManagerControl m_bundleControl;
        private float toolbarPadding = 5f;

        [MenuItem("AssetBundles/Browser")]
        static void ShowWindow()
        {
            GetWindow<BundleManagerMain>("AssetBundles");
        }

        private void OnEnable()
        {
            var subPos = GetSubWindowArea();
            if (m_bundleControl == null)
                m_bundleControl = new BundleManagerControl();
            m_bundleControl.OnEnable(subPos, this);
        }

        private void Update()
        {
            m_bundleControl.Update();
        }

        private void OnGUI()
        {
            m_bundleControl.OnGUI(GetSubWindowArea());
        }

        private Rect GetSubWindowArea()
        {
            Rect subPos = new Rect(0, toolbarPadding, position.width, position.height - toolbarPadding);
            return subPos;
        }
    }
}