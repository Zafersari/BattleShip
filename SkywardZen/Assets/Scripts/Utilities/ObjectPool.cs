using UnityEngine;
using System.Collections.Generic;

namespace SkywardZen.Level
{
    /// <summary>
    /// Generic object pool implementation for optimal performance on mobile devices.
    /// Reduces GC pressure by reusing GameObjects instead of instantiating/destroying.
    /// </summary>
    public class ObjectPool
    {
        private GameObject prefab;
        private Transform parent;
        private Queue<GameObject> availableObjects;
        private List<GameObject> allObjects;
        private int poolSize;

        public ObjectPool(GameObject prefab, int initialSize, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.poolSize = initialSize;

            availableObjects = new Queue<GameObject>();
            allObjects = new List<GameObject>();

            // Pre-instantiate pool objects
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private GameObject CreateNewObject()
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            availableObjects.Enqueue(obj);
            allObjects.Add(obj);
            return obj;
        }

        public GameObject GetObject()
        {
            GameObject obj;

            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else
            {
                // Pool exhausted - create new object dynamically
                Debug.LogWarning($"Object pool exhausted for {prefab.name}. Consider increasing pool size.");
                obj = CreateNewObject();
                availableObjects.Dequeue(); // Remove it immediately since we're using it
            }

            obj.SetActive(true);
            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);

            if (!availableObjects.Contains(obj))
            {
                availableObjects.Enqueue(obj);
            }
        }

        public void ReturnAll()
        {
            foreach (var obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    ReturnObject(obj);
                }
            }
        }

        public int AvailableCount => availableObjects.Count;
        public int TotalCount => allObjects.Count;
    }
}
