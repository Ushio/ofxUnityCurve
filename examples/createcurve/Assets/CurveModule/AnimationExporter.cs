using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MiniJSON;
using System.IO;
using System;

public class AnimationExporter : MonoBehaviour {
    public List<string> PathsForRelativeAssets = new List<string>();

    void Start () {
		
	}
	
	void Update () {
		
	}

    [ContextMenu("Write Animation Curves")]
    public void Write()
    {
        var animation = GetComponent<Animation>();
        if(animation == null)
        {
            Debug.LogWarning("Animation Compornent is not found.");
            return;
        }
        var clip = animation.clip;
        if(clip == null)
        {
            foreach (AnimationState anim in animation)
            {
                clip = anim.clip;
            }
        }

        var bindings = AnimationUtility.GetCurveBindings(clip);


        Dictionary<string, object>[] dstCurves = new Dictionary<string, object>[bindings.Length];

        for (int i = 0; i < bindings.Length; ++i)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
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
            dstCurve["property"] = bindings[i].path + "/" + bindings[i].propertyName;

            dstCurves[i] = dstCurve;
        }

        var json = Json.Serialize(dstCurves);
        // Debug.Log(json);
        // Debug.Log(Application.streamingAssetsPath);
        var assetsPath = Path.Combine(Application.streamingAssetsPath, "..");
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        if(PathsForRelativeAssets != null)
        {
            foreach (var PathForRelativeAssets in PathsForRelativeAssets)
            {
                var dst = Path.Combine(assetsPath, PathForRelativeAssets);
                Debug.Log(string.Format("save to: {0}", Path.GetFullPath(dst)));
                File.WriteAllBytes(dst, bytes);
            }
        }
    }
}

[CustomEditor(typeof(AnimationExporter))]
public class AnimationExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationExporter obj = (AnimationExporter)target;
        if (GUILayout.Button("Add Save File"))
        {
            string file = EditorUtility.SaveFilePanel("Curve File", "", "curve.json", "json");

            if (file != null && file.Length > 1)
            {
                var fileURI = new System.Uri(file);
                var refURI = new Uri(Application.streamingAssetsPath);
                var relative = refURI.MakeRelativeUri(fileURI).ToString();

				Undo.RecordObject(obj, "PathsForRelativeAssets");

                if(obj.PathsForRelativeAssets == null)
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
    }
}
