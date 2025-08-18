using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Simulation2D))]
public class Simulation2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
    }
}
