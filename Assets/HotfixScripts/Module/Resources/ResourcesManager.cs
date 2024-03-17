using System;
using System.Collections.Generic;
using UnityEngine;
namespace ResourcesManager
{
    public class ResourcesManager : ModuleSingleton<ResourcesManager>, IModule
    {
        private AssetBundleManifest assetBundleManifest;
        public static ResMode ResMode = ResMode.LocalAB;
        private Dictionary<string, AssetBundleInfo> _loadAssetBundles = new Dictionary<string, AssetBundleInfo>();
        //当前正在加载中的AB包
        private Dictionary<string, Action<AssetBundle>> m_nowLodingList = new Dictionary<string, Action<AssetBundle>>();



        public void OnCreate(object createParam)
        {
            //if(ResMode!= ResMode.Default)
            //{
            //    var manifestName = PathUtil.ABRootPath.Trim('/');
            //    AssetBundle manifestBundle = LoadAssetBundle(manifestName);
            //    if(manifestBundle != null)
            //    {
            //        assetBundleManifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            //
            //        manifestBundle.Unload(false);
            //        manifestBundle = null;
            //        _loadAssetBundles.Remove(manifestName.ToLower());
            //    }
            //    else
            //    {
            //        Debug.LogWarning("AssetBundleManifest 不存在");
            //    }
            //}
        }

        public void OnUpdate()
        {

        }



        public void LoadAssetAsync<T>(string abName, string assetName, Action<T> action) where T : UnityEngine.Object
        {
            LoadAssetBundleAsync(abName, delegate (AssetBundle resAB)
            {
                var res = resAB.LoadAssetAsync<T>(assetName);
                res.completed += delegate (AsyncOperation operation)
                {
                    action?.Invoke((operation as AssetBundleRequest).asset as T);
                };
            });
        }


        private void LoadAssetBundleAsync(string abName, Action<AssetBundle> action = null)
        {
            abName = abName.ToLower();
            AssetBundleInfo info = null;
            if (_loadAssetBundles.TryGetValue(abName, out info))
            {
                info.m_ReferencedCount++;
                string[] dependices = assetBundleManifest.GetAllDependencies(abName);
                foreach (var depend in dependices)
                {
                    _loadAssetBundles[depend].m_ReferencedCount++;
                }
                action?.Invoke(info.m_AssetBundle);
            }
            else
            {
                if (m_nowLodingList.ContainsKey(abName))
                {
                    if (action != null)
                        m_nowLodingList[abName] += action;
                    m_nowLodingList[abName] += delegate
                    {
                        _loadAssetBundles[abName].m_ReferencedCount++;
                        string[] dependencies = assetBundleManifest.GetDirectDependencies(abName);
                        foreach (var dep in dependencies)
                        {
                            _loadAssetBundles[dep].m_ReferencedCount++;
                        }
                    };
                }
                else
                {
                    if (action != null)
                    {
                        m_nowLodingList.Add(abName, action);
                    }
                    var loadPath = PathUtil.DataPath + PathUtil.ABRootPath + abName;
                    AssetBundleCreateRequest abRequest = AssetBundle.LoadFromFileAsync(loadPath);
                    abRequest.completed += LoadAssetBundleCallBack;
                }
            }
        }
        private void LoadAssetBundleCallBack(AsyncOperation obj)
        {
            var abRequest = obj as AssetBundleCreateRequest;
            if (abRequest == null)
            {
                Debug.LogError(abRequest.assetBundle.name + " : 不存在");
            }
            else
            {
                var abName = abRequest.assetBundle.name;
                Debug.Log(abName + " : 加载成功");
                var resAB = abRequest.assetBundle;
                var info = new AssetBundleInfo(resAB);
                _loadAssetBundles.Add(abName, info);
                if (m_nowLodingList.ContainsKey(abName))
                {
                    var actionList = m_nowLodingList[abName].GetInvocationList();
                    m_nowLodingList.Remove(abName);

                    foreach (Action<AssetBundle> a in actionList)
                    {
                        a?.Invoke(info.m_AssetBundle);
                    }
                }

                if (assetBundleManifest != null)
                {
                    string[] dependencies = assetBundleManifest.GetDirectDependencies(abName);

                    foreach (string dependency in dependencies)
                    {
                        if (_loadAssetBundles.ContainsKey(dependency))
                        {
                            _loadAssetBundles[dependency].m_ReferencedCount++;
                        }
                        else
                        {
                            LoadAssetBundleAsync(dependency, null);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="abName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string abName, string assetName) where T : UnityEngine.Object
        {
            T obj = null;
            var AB = LoadAssetBundle(abName);
            obj = AB.LoadAsset<T>(assetName);
            return obj;
        }
        /// <summary>
        /// 同步加载AB包
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        private AssetBundle LoadAssetBundle(string abName)
        {
            abName = abName.ToLower();
            AssetBundleInfo abinfo = null;
            if (_loadAssetBundles.TryGetValue(abName, out abinfo))
            {
                //如果被加载了，就让引用计数++
                abinfo.m_ReferencedCount++;
            }
            else
            {
                var loadPath = PathUtil.DataPath + PathUtil.ABRootPath + abName;
                var AB = AssetBundle.LoadFromFile(loadPath);
                if (AB == null)
                {
                    Debug.LogError(AB.name + ": 不存在");
                }
                else
                {
                    abinfo = new AssetBundleInfo(AB);
                    _loadAssetBundles.Add(abName, abinfo);
                }
            }
            if (assetBundleManifest != null)
            {
                string[] des = assetBundleManifest.GetAllDependencies(abName);
                //循环加载所有直接或间接引用的AB包
                foreach (var de in des)
                {
                    LoadAssetBundle(de);
                }
            }
            return abinfo.m_AssetBundle;

        }
        /// <summary>
        /// 卸载资源包
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough">是否连带资源一起卸载</param>
        public void UnLoadAssetBundle(string abName, bool isThorough = false)
        {
            abName.ToLower();
            if (_loadAssetBundles.ContainsKey(abName))
            {
                //每次有不用的资源就卸载一次，当所有资源都不用了以后会卸载AB包
                _loadAssetBundles[abName].m_ReferencedCount--;
                if (_loadAssetBundles[abName].m_ReferencedCount <= 0)
                {
                    _loadAssetBundles[abName].m_AssetBundle.Unload(isThorough);
                }
                string[] deps = assetBundleManifest.GetAllDependencies(abName);
                foreach (var de in deps)
                {
                    UnLoadAssetBundle(de, isThorough);
                }
            }
            else
            {
                Debug.LogError(abName + "：不存在");
            }
        }
    }
    public class AssetBundleInfo
    {
        public AssetBundle m_AssetBundle;       //AB包资源
        public int m_ReferencedCount;           //引用计数

        public AssetBundleInfo(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }
    public enum ResMode
    {
        Default = 0,          //默认从编辑器加载
        LocalAB = 1,          //读取打包路径下的AB包
        Online = 2,           //从网络下载AB包到本地读取
    }
}