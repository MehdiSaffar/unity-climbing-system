using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(Line))]
public class LineInspector : Editor {
    /// <summary>
    /// Color of the line
    /// </summary>
    [SerializeField] private Color lineColor = Color.white;

    private void OnSceneGUI(){
        var line = target as Line;
        if (line == null) {
            Debug.Log($"{nameof(target)} is null in {nameof(LineInspector)}");
            return;
        }

        var handleTransform = line.transform;
        var handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
        var first = handleTransform.TransformPoint(line.first);
        var second = handleTransform.TransformPoint(line.second);

        Handles.color = lineColor;
        Handles.DrawLine(first, second);
        EditorGUI.BeginChangeCheck();
        first = Handles.DoPositionHandle(first, handleRotation);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(line, "Move point");
            EditorUtility.SetDirty(line);
            line.first = handleTransform.InverseTransformPoint(first);
        }
        EditorGUI.BeginChangeCheck();
        second = Handles.DoPositionHandle(second, handleRotation);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(line, "Move point");
            EditorUtility.SetDirty(line);
            line.second = handleTransform.InverseTransformPoint(second);
        }
    }
}

