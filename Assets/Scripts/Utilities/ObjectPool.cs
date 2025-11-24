using System.Collections.Generic;
using UnityEngine;

namespace Tenronis.Utilities
{
    /// <summary>
    /// 泛型對象池 - 用於導彈、子彈、特效等可重用對象
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool = new Queue<T>();
        private readonly HashSet<T> activeObjects = new HashSet<T>();
        
        public ObjectPool(T prefab, Transform parent, int initialSize = 10)
        {
            this.prefab = prefab;
            this.parent = parent;
            
            // 預先建立對象
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }
        
        private T CreateNewObject()
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            return obj;
        }
        
        public T Get()
        {
            T obj = pool.Count > 0 ? pool.Dequeue() : CreateNewObject();
            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }
        
        public void Return(T obj)
        {
            if (obj == null || !activeObjects.Contains(obj)) return;
            
            obj.gameObject.SetActive(false);
            activeObjects.Remove(obj);
            pool.Enqueue(obj);
        }
        
        public void ReturnAll()
        {
            var objectsToReturn = new List<T>(activeObjects);
            foreach (var obj in objectsToReturn)
            {
                Return(obj);
            }
        }
        
        public int ActiveCount => activeObjects.Count;
        public int PooledCount => pool.Count;
    }
}

