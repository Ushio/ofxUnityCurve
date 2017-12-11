using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.IO;
using System;
using UnityEditor;

public class CurvesExporter : MonoBehaviour {
    public List<string> PathsForRelativeAssets = new List<string>();

    [System.Serializable]
    public class CurveItem
    {
        public string name = "";
        public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    }
    public List<CurveItem> Curves = new List<CurveItem>();
	
    public float Evaluate(string name, float time)
    {
        for(int i = 0; i < Curves.Count; ++i)
        {
            if(Curves[i].name == name)
            {
                return Curves[i].curve.Evaluate(time);
            }
        }
        Debug.LogWarningFormat("CurvesExporter: {0} is not found.", name);
        return 0.0f;
    }

    [ContextMenu("Write Animation Curves")]
    public void Write()
    {
        Dictionary<string, object>[] dstCurves = new Dictionary<string, object>[Curves.Count];

        for (int i = 0; i < Curves.Count; ++i)
        {
            AnimationCurve curve = Curves[i].curve;
            var dstCurve = new Dictionary<string, object>();

            var keys = new Dictionary<string, float>[curve.keys.Length];
            for (int j = 0; j < curve.keys.Length; ++j)
            {
                var keyframe = new Dictionary<string, float>();
                keyframe["value"] = curve.keys[j].value;
                keyframe["time"] = curve.keys[j].time;
                keyframe["inTangent"] = curve.keys[j].inTangent;
                keyframe["outTangent"] = curve.keys[j].outTangent;
                keys[j] = keyframe;
            }
            dstCurve["keys"] = keys;
            dstCurve["property"] = Curves[i].name;

            dstCurves[i] = dstCurve;
        }

        var json = Json.Serialize(dstCurves);
        var assetsPath = Path.Combine(Application.streamingAssetsPath, "..");
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        foreach (var PathForRelativeAssets in PathsForRelativeAssets)
        {
            var dst = Path.Combine(assetsPath, PathForRelativeAssets);
            Debug.Log(string.Format("save to: {0}", Path.GetFullPath(dst)));
            File.WriteAllBytes(dst, bytes);
        }
    }
}
[CustomEditor(typeof(CurvesExporter))]
public class CurvesExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // DrawDefaultInspector();

        CurvesExporter obj = (CurvesExporter)target;
        Undo.RecordObject(obj, "CurvesExporter");

        SerializedProperty propPathsForRelativeAssets = serializedObject.FindProperty("PathsForRelativeAssets");
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Curves");
        for(int i = 0; i < obj.Curves.Count; ++i)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(string.Format("curve [{0}]", i));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("x"))
            {
                obj.Curves.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            obj.Curves[i].name = EditorGUILayout.TextField("key", obj.Curves[i].name);
            EditorGUILayout.CurveField(obj.Curves[i].curve);
            EditorGUILayout.EndVertical();

            // 操作
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = i != 0;
            if (GUILayout.Button("up"))
            {
                var tmp = obj.Curves[i];
                obj.Curves[i] = obj.Curves[i - 1];
                obj.Curves[i - 1] = tmp;

                serializedObject.Update();
            }
            GUI.enabled = true;
            GUI.enabled = i != obj.Curves.Count - 1;
            if (GUILayout.Button("down"))
            {
                var tmp = obj.Curves[i];
                obj.Curves[i] = obj.Curves[i + 1];
                obj.Curves[i + 1] = tmp;

                serializedObject.Update();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }
        GUILayout.EndVertical();
        if (GUILayout.Button("Add New Curve"))
        {
            var curve = new CurvesExporter.CurveItem();
            curve.name = "curve";
            curve.curve = AnimationCurve.Linear(0, 0, 1, 1);
            obj.Curves.Add(curve);
        }
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propPathsForRelativeAssets, true);

        if (GUILayout.Button("Add Save File"))
        {
            string file = EditorUtility.SaveFilePanel("Curve File", "", "curve.json", "json");

            if (file != null && file.Length > 1)
            {
                var fileURI = new System.Uri(file);
                var refURI = new Uri(Application.streamingAssetsPath);
                var relative = refURI.MakeRelativeUri(fileURI).ToString();

                if (obj.PathsForRelativeAssets == null)
                {
                    obj.PathsForRelativeAssets = new List<string>();
                }

                obj.PathsForRelativeAssets.Add(relative);
            }
        }
        if (GUILayout.Button("Save !"))
        {
            obj.Write();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
