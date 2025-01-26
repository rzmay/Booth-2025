using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StandardGun))]
public class StandardGunEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    StandardGun gun = (StandardGun)target;
    float dpsPerDistance = (gun.damage * gun.bulletSpeed) / gun.rate;

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("DPS per Distance:", dpsPerDistance.ToString("F2"));
  }
}
