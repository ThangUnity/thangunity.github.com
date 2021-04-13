using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AR
{

    public class DataAR : MonoBehaviour
    {
        [SerializeField] string[] link;
        [SerializeField] List<GameObject> listObj;
        bool isLoadDone=false;
        bool isLoading;
        #region public
        public static DataAR _Instance;
        public GameObject GetObj(int i)
        {
            return listObj[i];
        }
        public int GetCountListObj { get { return listObj.Count; } }
        public bool GetLoadDone { get { return isLoadDone; } }
        #endregion
        // Start is called before the first frame update

        private void Awake()
        {
            if (_Instance != null)
            {
                if (_Instance == this) Destroy(this.gameObject);
            }
            else
            {
                _Instance = this;
            }
            DontDestroyOnLoad(this);
        }
        private void Start()
        {
           StartCoroutine (CheckAndDownload());
        }
        IEnumerator CheckAndDownload()
        {
            for (int i = 0; i < link.Length; i++)
            {
                yield return new WaitWhile(() => isLoading);
                StartCoroutine(DownloadAndCacheAssetBundle(link[i],0));
            }
            print("day" + link.Length + " / " + (listObj.Count+1).ToString());
            if((link.Length)== listObj.Count+1)
            {
                isLoadDone = true;
            }
            yield return null;
        }
        
     
        // Update is called once per frame
        void Update()
        {

        }
        IEnumerator DownloadAndCacheAssetBundle(string assetBundlePath,int version)
        {
            //UIManager._Instance.SetActiveLoading();
            isLoading = true;

            // Use the default cache here
            Caching.currentCacheForWriting = Caching.defaultCache;



            // Download actual content bundle
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundlePath, (uint)version, 0);
            yield return request.SendWebRequest();

            // This one is crucial for debugging. Code 200 means downloaded from web and Code 0 means loaded from cache.
            Debug.Log(request.responseCode);

            // Download the actual content bundle
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);

            // Load 
            var loadAsset = bundle.LoadAllAssetsAsync();
            yield return loadAsset;

            GameObject  newObj = Instantiate(loadAsset.asset) as GameObject;
            newObj.transform.SetParent(this.transform);
            //RefreshShaders(newObj);
            newObj.SetActive(false);
            newObj.name = System.IO.Path.GetFileName(assetBundlePath);
            listObj.Add(newObj);
            // Don't forget to unload the bundle
            bundle.Unload(false);
            //UIManager._Instance.SetHideLoading();
            isLoading = false;
           
        }
        private void RefreshShaders(GameObject go)
        {
            // find all renderers for this GO (parent + Children)
            Renderer[] rens = go.GetComponentsInChildren<Renderer>();

            // for each material in each renderer, reattach the same shader if you can find it.
            for (int i = 0; i < rens.Length; i++)
            {
                foreach (Material mat in rens[i].materials)
                {
                    var shd = Shader.Find(mat.shader.name);
                    if (null == shd)
                    {
                        Debug.Log("Cannot refresh the shader on GameObject:" + go.name + " shader name: " + mat.shader.name + ". Applying standard shader. If not okay, add this shader to Resources folder.");
                        shd = Shader.Find("Standard");
                    }
                    mat.shader = shd;
                }
            }
        }
    }
}
