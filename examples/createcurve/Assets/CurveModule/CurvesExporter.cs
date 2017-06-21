using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.IO;
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
        // Debug.Log(json);
        // Debug.Log(Application.streamingAssetsPath);
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
        DrawDefaultInspector();

        CurvesExporter obj = (CurvesExporter)target;
        if (GUILayout.Button("Save !"))
        {
            obj.Write();
        }
    }
}
