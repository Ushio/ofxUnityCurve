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

    public AnimationClip ExportedAnimationClip
    {
        get
        {
            var animation = GetComponent<Animation>();
            if(animation == null)
            {
                return null;
            }
            var clip = animation.clip;
            if (clip == null)
            {
                foreach (AnimationState anim in animation)
                {
                    clip = anim.clip;
                }
            }
            return clip;
        }
    }

    [ContextMenu("Write Animation Curves")]
    public void Write()
    {
        var clip = this.ExportedAnimationClip;
        if (clip == null)
        {
            Debug.LogWarning("Animation Clip not found.");
            return;
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
        AnimationExporter obj = (AnimationExporter)target;
        Undo.RecordObject(obj, "AnimationExporter");

        if (obj.GetComponent<Animation>() != null)
        {
            if (GUILayout.Button("Edit Animation"))
            {
                EditorApplication.ExecuteMenuItem("Window/Animation");
            }
        }
        else
        {
            if (GUILayout.Button("Create Animation"))
            {
                Undo.AddComponent<Animation>(obj.gameObject);
            }
        }

        // 保存するキーを表示する
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Export Keys");
        var clip = obj.ExportedAnimationClip;
        var bindings = AnimationUtility.GetCurveBindings(clip);

        for (int i = 0; i < bindings.Length; ++i)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
            string key = bindings[i].path + "/" + bindings[i].propertyName;
            EditorGUILayout.TextField(key);
        }
        GUILayout.EndVertical();

        DrawDefaultInspector();
        
        if (GUILayout.Button("Add Save File"))
        {
            string file = EditorUtility.SaveFilePanel("Curve File", "", "curve.json", "json");

            if (file != null && file.Length > 1)
            {
                var fileURI = new System.Uri(file);
                var refURI = new Uri(Application.streamingAssetsPath);
                var relative = refURI.MakeRelativeUri(fileURI).ToString();

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
