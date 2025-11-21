using System.Collections.Generic;
using Repos.Unity_HelperLib.Generics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Repos.Unity_HelperLib.Pooling
{
    public class GameObjectPool<T> : Initable<T,Transform> where T : MonoBehaviour , IPoolable
    {
        private Transform _parentObj;
        private List<IPoolable> _objects;
        private Queue<IPoolable> _availableQueue;
        
        private T _prefab;
        
        public void Init(T obj,Transform parentObj)
        {
            _availableQueue =  new Queue<IPoolable>();
            
            _parentObj = parentObj;
            _prefab = obj;
            Pool();
        }
        
        public void Pool(int amount = 64)
        {
            if (_parentObj == null)
            {
                Debug.LogError("without a parent for pooled objects cant pool objects");
                return;
            }
                
            if(_objects  == null)
                _objects = new List<IPoolable>(amount);
            else
                _objects.Capacity = amount;
            
            var oldCount = _objects.Count;
            
            for (var i = oldCount; i < amount; i++)
            {
                var o = Object.Instantiate(_prefab, _parentObj);
                IPoolable p = o;
                p.OnDeactivation += Release;
                
                _objects.Add(p);
                _availableQueue.Enqueue(p);
                
                o.gameObject.SetActive(false);
            }
        }

        public void SetPoolSize(int value)
        {
            var trueSize = GetUpperPowerOfTwo(value);
            Pool(trueSize);
        }

        private IPoolable GetNext()
            => _availableQueue.Count > 0 ? _availableQueue.Dequeue() : null;
        
        public GameObject Next(bool expandIfNotEnough = false)
        {
            var poolable = GetNext();
            if (poolable != null)
            {
                poolable.SetActivated(true);
                return poolable.GameObject;
            }
            
            if (expandIfNotEnough)
            { 
                Pool(GetUpperPowerOfTwo(_objects.Count));
                var next = GetNext();
                return next?.GameObject;
            }
            
            Debug.Log("Not enough objects in the pool! Returning null."); 
            return null;
        }

        private void Release(IPoolable p)
            => _availableQueue.Enqueue(p);

        public void ClearPool()
        {
            foreach (var o in _objects)
            {
                o.SetActivated(false);
                Object.Destroy(o.GameObject);
            }
            
            _objects.Clear();
            _availableQueue.Clear();
        }
        
        private int GetUpperPowerOfTwo(int value)
        {
            if (value < 1) return 1;

            // If already a power of two, return it
            if ((value & (value - 1)) == 0) return value;

            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;
            return value;
        }
    }
}