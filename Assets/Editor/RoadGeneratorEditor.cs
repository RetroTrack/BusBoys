#if UNITY_EDITOR
using BusBoys.Assets.Scripts.Environment.Generation;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralRoadGraphGenerator))]
public class RoadGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        ProceduralRoadGraphGenerator generator = (ProceduralRoadGraphGenerator)target;

        if (GUILayout.Button("Generate Roads", GUILayout.Height(30)))
        {
            generator.Generate();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Reset + New Seed", GUILayout.Height(30)))
        {
            generator.NewSeedAndGenerate();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Roads", GUILayout.Height(24)))
        {
            generator.ClearGenerated();
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif