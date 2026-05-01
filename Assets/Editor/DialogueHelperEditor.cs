using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueHelper))]
public class DialogueHelperEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		DialogueHelper helper = (DialogueHelper)target;
		helper.SubDirectory = helper.SubDirectory.Trim();
		helper.FileName = helper.FileName.Trim();
		if (GUILayout.Button("Clear Inputted Data")) helper.ClearValues();
		if (helper.FileName.Length > 0) if (GUILayout.Button($"Load \"{helper.FileAndSub}.json\"")) helper.LoadFile();
		if (!string.IsNullOrWhiteSpace(helper.FileNameToSaveOrDelete))
		{
			if (GUILayout.Button($"Create/Modify \"{helper.FileToSaveOrDeleteAndSub}.json\"")) helper.SaveFile();
			if (GUILayout.Button($"Delete \"{helper.FileToSaveOrDeleteAndSub}.json\"")) helper.DeleteFile();
		}
		EditorUtility.SetDirty(target);
	}
}
