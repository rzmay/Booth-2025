using UnityEngine;

// For debugging
[RequireComponent(typeof(PrintReceipt))]
public class TriggerPrint : MonoBehaviour
{
  public string gunName;
  public Texture2D gunTex;

  private void Start()
  {
    PrintReceipt printReceipt = GetComponent<PrintReceipt>();

    // Set receipt controller stuff
    ReceiptController.SetStats(12, 6, 72, true);
    ReceiptController.SetScore(8000, 22);
    ReceiptController.SetGun(gunName, gunTex);


    printReceipt.Print();

  }
}
