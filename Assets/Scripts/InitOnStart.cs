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
            //TODO: Change The Name and GameID of Each cell
            transform.GetChild(0).gameObject.GetComponent<Text>().text = sj.GetScriptsResult().GetScript(idx).name;
            transform.gameObject.GetComponent<ScrollIndexCallback1>().gameID = sj.GetScriptsResult().GetScript(idx).id.ToString();
            transform.SendMessage("ScrollCellIndex", idx);
        }

        void Start()
        {
            //TODO: Get The Name and GameID from backend
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
                    sj = JsonMapper.ToObject<ScriptsJsonData>(webRequest.downloadHandler.text);
                    totalCount = sj.GetScriptsResult().GetNum();
                    CreateCells();
                }
            }
        }
    }
}