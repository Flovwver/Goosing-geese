using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
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
