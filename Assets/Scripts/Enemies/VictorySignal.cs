using UnityEngine;

public class VictorySignal
{
  void OnDestroy()
  {
    // Change track
    MusicManager.PlayTrack("win", 1f);

    // Set to empty phase
    SpawnSequencer.SetPhase(2);

    // Set menu to victory
    MenuController.SetMenu(3);
  }
}
