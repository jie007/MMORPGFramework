using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameObjectPool : MonoBehaviour
    {
        public GameObject Prefab;
        public int InitialPoolSize = 25;
        public bool AutoExpandPool = true;

        private Queue<GameObject> pool = new Queue<GameObject>();

        public void Awake()
        {
            GeneratePoolItems();
        }

        private void GeneratePoolItems()
        {
            for (int i = 0; i < InitialPoolSize; i++)
            {
                GenerateNewPoolItem();
            }
        }

        private void GenerateNewPoolItem()
        {
            Return(GameObject.Instantiate(Prefab));
        }

        public void Return(GameObject go)
        {
            go.SetActive(false);
            pool.Enqueue(go);
        }

        public GameObject Get()
        {
            if (pool.Count <= 0)
            {
                if (!AutoExpandPool)
                {
                    return null;
                }

                GeneratePoolItems();
            }

            var go = pool.Dequeue();
            go.SetActive(true);
            return go;
        }
    }
}
