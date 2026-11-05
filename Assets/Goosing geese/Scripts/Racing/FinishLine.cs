using System;
using UnityEngine;

namespace GoosingGeese.Racing
{
    [RequireComponent(typeof(BoxCollider))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "FinishLine")]
    public class FinishLine : MonoBehaviour
    {
        public event Action<Racer> Crossed;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Racer racer = other.GetComponentInParent<Racer>();

            if (racer != null)
                Crossed?.Invoke(racer);
        }
    }
}
