using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.BundeManager
{
    public class BundleLoader : MonoBehaviour
    {
        public string BaseDownLoadUrl { get; set; }

        public void LoadAssetBundle(string assetbundle, Action<AssetBundle> callback)
        {
            var url = Path.Combine(BaseDownLoadUrl, assetbundle);
            StartCoroutine(DownLoadAssetBundle(url, callback));
        }

        public IEnumerator DownLoadAssetBundle(string url, Action<AssetBundle> callback)
        {
            using (var www = UnityWebRequest.GetTexture(url))
            {
                AssetBundle assetbundle = null;
                yield return www.Send();
                if (!www.isError && www.responseCode != 404)
                    assetbundle = DownloadHandlerAssetBundle.GetContent(www);
                if (callback != null)
                    callback.Invoke(assetbundle);
            }
        }
    }
}