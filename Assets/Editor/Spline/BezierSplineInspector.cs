using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor{
    private const int stepsPerCurve = 40;
    private const float directionScale = 0.5f;

    private BezierSpline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;
    private int selectedIndex = -1;
    private float handleSize = 0.04f;
    private float pickSize = 0.06f;

    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private void OnSceneGUI(){
        spline = (BezierSpline) target;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        var p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3) {
            var p1 = ShowPoint(i);
            var p2 = ShowPoint(i + 1);
            var p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }

        ShowDirections();



    }

    private void ShowDirections(){
        var steps = stepsPerCurve * spline.CurveCount;
        Handles.color = Color.blue;
        for (int i = 0; i <= steps; i++) {
            float t = i / (float) steps;
            var point = spline.GetPoint(t);
            var direction = spline.GetDirection(t);
            Handles.DrawLine(point, point + direction * directionScale);
        }
    }

    private Vector3 ShowPoint(int index){
        var point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        var size = HandleUtility.GetHandleSize(point);
        Handles.color = modeColors[(int) spline.GetControlPointMode(index)];
        if(index == 0)
        {
            size *= 2f;
        }
        if (Handles.Button(point, handleRotation, handleSize * size, pickSize * size, Handles.DotHandleCap)) {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index) {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Move point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }

        return point;
    }

    public override void OnInspectorGUI(){
//        DrawDefaultInspector();
        spline = (BezierSpline) target;
        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) {
            DrawSelectedPointInspector();
        }
        EditorGUI.BeginChangeCheck();
        var loop = EditorGUILayout.Toggle("Loop", spline.Loop);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Toggle Loop");
            EditorUtility.SetDirty(spline);
            spline.Loop = loop;
        }
        if (GUILayout.Button("Add Curve")) {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
     }

    private void DrawSelectedPointInspector(){
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        var point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }

        EditorGUI.BeginChangeCheck();
        var mode = (BezierControlPointMode) EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Change point mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }
}
