using UnityEditor;

namespace AudioSystem
{
    [CustomEditor(typeof(SoundData))]
    public class SoundDataEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw everything except the 3D-specific ones first
            EditorGUILayout.PropertyField(serializedObject.FindProperty("audioClip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("audioMixerGroup"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frequentSound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pitch"));
        
            SerializedProperty spatialBlend = serializedObject.FindProperty("spatialBlend");
            EditorGUILayout.PropertyField(spatialBlend);

            // Simple hide/show logic
            if (spatialBlend.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rolloffMode"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}