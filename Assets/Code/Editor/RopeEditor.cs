using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rope)), CanEditMultipleObjects]
public class RopeEditor : Editor { 

    protected virtual void OnSceneGUI() {
        Rope rope = (Rope)target;


        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle((Vector2)rope.transform.position + rope.endPosEditor, Quaternion.identity);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(rope, "Moved start position");
            rope.endPosEditor = rope.transform.InverseTransformPoint(newTargetPosition);
        }
    }

}
