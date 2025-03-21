using MediaProjection.Models;
using UnityEngine;

[RequireComponent(typeof(GunManager))]
public class GunBarcodeReader : MonoBehaviour
{
    private GunManager _gunManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gunManager = GetComponent<GunManager>();
    }

    public void ProcessResult(BarcodeReadingResult[] results)
    {
        foreach (var result in results)
        {
            Debug.Log($"[GunBarcodeReader] Got barcode result: {result.Text}");

            // Try to read gun data in format "MIB:n" where "n" is the index of the gun
            string[] data = result.Text.Split(':');
            if (data[0] == "MIB")
            {
                bool success = int.TryParse(data[1], out int gun);
                if (success)
                {
                    _gunManager.SetGun(gun);
                    return;
                }
            }
        }
    }

    public static void SetBarcodeReadingActive(bool active)
    {
        // TODO: Implement
    }
}
