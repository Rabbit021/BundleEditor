using UnityEditor;
using UnityEngine;

namespace AssetBundles
{
    public interface ISubWindow
    {
        void OnEnable(Rect pos, EditorWindow parent);
        void OnGUI(Rect pos);
        void Update();
        void ForceReloadData();
    }
}