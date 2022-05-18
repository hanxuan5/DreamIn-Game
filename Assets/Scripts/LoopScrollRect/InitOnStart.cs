using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace Demo
{
    [RequireComponent(typeof(UnityEngine.UI.LoopScrollRect))]
    [DisallowMultipleComponent]
    public class InitOnStart : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
    {
        public GameObject item;
        public int totalCount = -1;
        private Sprite[] resList;

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
            Image img = transform.GetComponent<Image>();
            img.sprite = resList[idx];
        }

        void Awake()
        {
            var ls = GetComponent<LoopScrollRect>();
            if (this.gameObject.name.Contains("Map"))
            {
                resList = Resources.LoadAll<Sprite>("Maps");
            }
            else if (this.gameObject.name.Contains("Object"))
            {
                resList = Resources.LoadAll<Sprite>("Objects");
            }
            
            int resCount = resList.Length;
            
            ls.prefabSource = this;
            ls.dataSource = this;
            ls.totalCount = resCount;
            ls.RefillCells();
        }
    }
}