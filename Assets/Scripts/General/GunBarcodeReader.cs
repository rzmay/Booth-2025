using UnityEngine;
using MediaProjection.Models;

[RequireComponent(typeof(GunManager))]
public class GunBarcodeReader : MonoBehaviour
{
    [SerializeField]
    private bool _allowSwitching = true;

    private GunManager _gunManager;

    private bool set;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gunManager = GetComponent<GunManager>();
    }

    public void ProcessResult(BarcodeReadingResult[] results)
    {
        if (set && !_allowSwitching) return;

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
                    set = true;
                    return;
                }
            }
        }
    }
}
