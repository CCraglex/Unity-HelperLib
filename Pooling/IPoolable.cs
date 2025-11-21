using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Repos.Unity_HelperLib.Pooling
{
    public interface IPoolable
    {
        public GameObject GameObject { get; }
        public bool IsOccupied { get; set; }
        public Action<IPoolable> OnDeactivation  { get; set; }
        
        public void SetActivated(bool activated)
        {
            IsOccupied = activated;
            if (!activated)
                OnDeactivation?.Invoke(this);
            
            GameObject.SetActive(activated);
        }
    }
}