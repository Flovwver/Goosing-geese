using System;
using System.Threading.Tasks;
using GoosingGeese.Presentation;
using UnityEngine;

public static class AppearanceMaterialTests
{
    public static async Task<string> Main()
    {
        GameObject sample = null;
        Material owned = null;

        try
        {
            sample = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sample.transform.position = Vector3.down * 1000f;
            var renderer = sample.GetComponent<Renderer>();
            Material original = renderer.sharedMaterial;
            Color originalColor = original.color;
            var appearance = sample.AddComponent<RacerAppearance>();
            appearance.ShowOriginal(Color.blue);
            owned = renderer.sharedMaterial;

            if (owned == original || original.color != originalColor)
                throw new Exception("Tint must own a separate material without changing the shared asset.");

            appearance.ShowOriginal(Color.green);

            if (renderer.sharedMaterial != owned || owned.color != Color.green)
                throw new Exception("Repeated tint changes must reuse the owned material.");

            UnityEngine.Object.Destroy(sample);
            await Task.Delay(300);

            if (sample != null || owned != null || original == null)
                throw new Exception("Deleting the appearance must release its material while preserving the shared asset.");

            return "PASS: isolated tint, material reuse and owned material cleanup.";
        }
        finally
        {
            if (sample != null)
                UnityEngine.Object.Destroy(sample);

            if (owned != null)
                UnityEngine.Object.Destroy(owned);
        }
    }
}
