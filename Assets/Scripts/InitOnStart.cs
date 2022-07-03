using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;

namespace Demo
{
    [RequireComponent(typeof(UnityEngine.UI.LoopScrollRect))]
    [DisallowMultipleComponent]
    public class InitOnStart : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
    {
        public GameObject item;
        public int totalCount = -1;

        private ScriptsJsonData sj;

        // Implement your own Cache Pool here. The following is just for example.
        Stack<Transform> pool = new Stack<Transform>();
        public GameObject GetObject(int index)
        {
            if (pool.Count == 0)
            {
                return Instantiate(item);
            }
            Transform candidate = pool.Pop();
            candidate.gameObject.SetActive(true);
            return candidate.gameObject;
        }

        public void ReturnObject(Transform trans)
        {
            // Use `DestroyImmediate` here if you don't need Pool
            trans.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            trans.gameObject.SetActive(false);
            trans.SetParent(transform, false);
            pool.Push(trans);
        }

        public void ProvideData(Transform transform, int idx)
        {
            string[] subs = sj.GetScriptsResult().GetScript(idx).name.Split(',');
            if (idx > 37) //Delete after
            {
                transform.GetChild(0).gameObject.GetComponent<Text>().text = subs[0] + "(" + subs[1] + " players)";
                transform.gameObject.GetComponent<ScrollIndexCallback1>().gameID = sj.GetScriptsResult().GetScript(idx).id.ToString();
                transform.gameObject.GetComponent<ScrollIndexCallback1>().numOfPlayer = int.Parse(subs[1]);
            }
            transform.SendMessage("ScrollCellIndex", idx);
        }

        void Start()
        {
            StartCoroutine(GetNameAndID());
        }

        void CreateCells()
        {
            var ls = GetComponent<LoopScrollRect>();
            ls.prefabSource = this;
            ls.dataSource = this;
            ls.totalCount = totalCount;
            ls.RefillCells();
        }

        IEnumerator GetNameAndID()
        {
            string url = "https://api.dreamin.land/game_name/";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(webRequest.error + "\n");
                }
                else
                {
                    //Save a copy to local
                    string savePath = "Assets/JsonData/ScriptData.json";
                    File.WriteAllText(savePath, webRequest.downloadHandler.text);

                    sj = JsonMapper.ToObject<ScriptsJsonData>(webRequest.downloadHandler.text);
                    totalCount = sj.GetScriptsResult().GetNum();
                    CreateCells();
                }
            }
        }
    }
}