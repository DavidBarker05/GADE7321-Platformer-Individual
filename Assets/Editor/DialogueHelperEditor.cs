using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DialogueEditor : EditorWindow
{
	static readonly string s_ResourcesFolder = $"{Application.dataPath.TrimEnd('/')}/Resources";

	bool m_bClearOnDelete = true;

	string m_DirectoryName = "";
	string FullDirectoryPath => $"{Application.dataPath.TrimEnd('/')}/{m_DirectoryName}";
	readonly string[] m_DirectoryOptions = new string[2]
	{
		"Use Existing Subdirectory",
		"Create New Subdirectory"
	};
	int m_SelectedDirectoryOptionIndex = 0;
	int m_LastSelectedDirectoryOptionIndex = 0;
	string[] m_SubDirectories;
	int m_SelectedDirectoryIndex = 0;
	int m_LastSelectedDirectoryIndex = 0;

	string m_FileName = "";
	string FilePath => $"{m_DirectoryName.TrimEnd('/')}/{m_FileName}";
	string FullFilePath => $"{FullDirectoryPath.TrimEnd('/')}/{m_FileName}";
	readonly string[] m_FileOptions = new string[2]
	{
		"Use Existing JSON",
		"Create New JSON"
	};
	int m_SelectedFileOptionIndex = 0;
	int m_LastSelectedFileOptionIndex = 0;
	string[] m_Files;
	int m_SelectedFileIndex = 0;
	int m_LastSelectedFileIndex = 0;

	public Dialogue Dialogue;
	SerializedObject m_SerializedObject;
	SerializedProperty m_SerializedProperty;

	[MenuItem("Window/Edit Dialogue")]
	public static void ShowWindow() => GetWindow<DialogueEditor>("Dialogue Editor");

	void OnEnable()
	{
		m_SerializedObject = new SerializedObject(this);
		m_SerializedProperty = m_SerializedObject.FindProperty("Dialogue");
	}

	void OnGUI()
	{
		m_bClearOnDelete = EditorGUILayout.Toggle("Clear on Delete", m_bClearOnDelete);

		m_SelectedDirectoryOptionIndex = EditorGUILayout.Popup("Choose Directory to Use", m_SelectedDirectoryOptionIndex, m_DirectoryOptions);
		if (m_LastSelectedDirectoryOptionIndex != m_SelectedDirectoryOptionIndex)
		{
			m_DirectoryName = "";
			m_LastSelectedDirectoryOptionIndex = m_SelectedDirectoryOptionIndex;
			m_SelectedDirectoryIndex = 0;
			m_LastSelectedDirectoryIndex = 0;
			m_SelectedFileIndex = 0;
			m_LastSelectedFileIndex = 0;
			m_SubDirectories = null;
			m_Files = null;
			Dialogue = null;
		}
		if (m_SubDirectories == null && m_SelectedDirectoryOptionIndex == 0) GetDirectories();
		if (m_SelectedDirectoryOptionIndex == 0)
		{
			m_SelectedDirectoryIndex = EditorGUILayout.Popup("Select an Existing Directory", m_SelectedDirectoryIndex, m_SubDirectories);
			m_DirectoryName = m_SubDirectories.Length > 0 ? m_SubDirectories[m_SelectedDirectoryIndex] : "";
			if (m_LastSelectedDirectoryIndex != m_SelectedDirectoryIndex)
			{
				m_LastSelectedDirectoryIndex = m_SelectedDirectoryIndex;
				m_SelectedFileIndex = 0;
				m_LastSelectedFileIndex = 0;
				GetDirectories();
				if (m_SelectedFileOptionIndex == 0) GetFiles();
				Dialogue = null;
			}
			m_SelectedFileOptionIndex = EditorGUILayout.Popup("Choose File to Use", m_SelectedFileOptionIndex, m_FileOptions);
			if (m_LastSelectedFileOptionIndex != m_SelectedFileOptionIndex)
			{
				m_FileName = "";
				m_LastSelectedFileOptionIndex = m_SelectedFileOptionIndex;
				m_SelectedFileIndex = 0;
				m_Files = null;
				Dialogue = null;
			}
			if (m_Files == null && m_SelectedFileOptionIndex == 0) GetFiles();
			if (m_SelectedFileOptionIndex == 0)
			{
				m_SelectedFileIndex = EditorGUILayout.Popup("Select an Existing File", m_SelectedFileIndex, m_Files);
				m_FileName = m_Files.Length > 0 ? m_Files[m_SelectedFileIndex] : "";
				if (m_LastSelectedFileIndex != m_SelectedFileIndex)
				{
					GetFiles();
					m_LastSelectedFileIndex = m_SelectedFileIndex;
					if (!string.IsNullOrWhiteSpace(m_FileName)) LoadFile();
				}
				if (Dialogue == null && !string.IsNullOrWhiteSpace(m_FileName)) LoadFile();
			}
			else
			{
				m_FileName = EditorGUILayout.TextField("New File Name", m_FileName);
				m_SelectedFileIndex = 0;
				m_LastSelectedFileIndex = 0;
			}
		}
		else
		{
			m_DirectoryName = EditorGUILayout.TextField("New Directory Name", m_DirectoryName);
			m_FileName = EditorGUILayout.TextField("New File Name", m_FileName);
			m_SelectedDirectoryIndex = 0;
			m_LastSelectedDirectoryIndex = 0;
			m_SelectedFileOptionIndex = 1;
			m_LastSelectedFileOptionIndex = 1;
			m_SelectedFileIndex = 0;
			m_LastSelectedFileIndex = 0;
		}
		m_DirectoryName.Trim();
		m_FileName.Trim();

		Dialogue ??= new Dialogue();

		m_SerializedObject.Update();
		EditorGUILayout.PropertyField(m_SerializedProperty, true);
		m_SerializedObject.ApplyModifiedProperties();

		if (GUILayout.Button("Clear Input")) Clear();

		if (string.IsNullOrEmpty(m_FileName)) return;
		if (m_SelectedFileOptionIndex == 0)
		{
			if (GUILayout.Button($"Modify \"{FilePath}{(m_FileName.EndsWith(".json") ? "" : ".json")}\""))
			{
				if (!m_FileName.EndsWith(".json")) m_FileName += ".json";
				SaveFile();
			}
			if (GUILayout.Button($"Delete \"{FilePath}{(m_FileName.EndsWith(".json") ? "" : ".json")}\""))
			{
				if (!m_FileName.EndsWith(".json")) m_FileName += ".json";
				DeleteFile();
			}
		}
		else if (!string.IsNullOrWhiteSpace(m_DirectoryName))
		{
			if (GUILayout.Button($"Create \"{FilePath}{(m_FileName.EndsWith(".json") ? "" : ".json")}\""))
			{
				if (!m_FileName.EndsWith(".json")) m_FileName += ".json";
				SaveFile();
			}
		}
	}

	void GetDirectories()
	{
		string[] _subDirectories = Directory.GetDirectories(s_ResourcesFolder, "*", SearchOption.AllDirectories);
		for (int i = 0; i < _subDirectories.Length; ++i)
		{
			_subDirectories[i] = _subDirectories[i].Replace('\\', '/');
			_subDirectories[i] = _subDirectories[i].Substring(Application.dataPath.Length).TrimStart('/');
		}
		m_SubDirectories = new string[_subDirectories.Length + 1];
		m_SubDirectories[0] = "Resources";
		_subDirectories.CopyTo(m_SubDirectories, 1);
	}

	void GetFiles()
	{
		m_Files = Directory.GetFiles(FullDirectoryPath, "*.json");
		for (int i = 0; i < m_Files.Length; ++i)
		{
			m_Files[i] = m_Files[i].Replace('\\', '/');
			int slashIndex = m_Files[i].LastIndexOf('/');
			m_Files[i] = m_Files[i].Substring(slashIndex, m_Files[i].Length - slashIndex).Trim('/');
		}
	}

	void LoadFile()
	{
		string jsonPath = FilePath;
		if (jsonPath.StartsWith("Resources/")) jsonPath = jsonPath.Substring("Resources/".Length).TrimStart('/');
		if (jsonPath.EndsWith(".json")) jsonPath = jsonPath.Substring(0, jsonPath.Length - ".json".Length);
		TextAsset json = Resources.Load<TextAsset>(jsonPath);
		if (!json)
		{
			Debug.LogError($"\"{jsonPath}.json\" does not exist!");
			return;
		}
		try
		{
			Dialogue = JsonUtility.FromJson<SerialisedDialogue>(json.text).Deserialised;
			Debug.Log($"Successfully loaded \"Assets/Resources/{jsonPath}.json\"");
		}
		catch (ArgumentException)
		{
			Debug.LogError($"Invalid data in \"Assets/Resources/{jsonPath}.json\"");
		}
	}

	void SaveFile()
	{
		if (!Directory.Exists(FullDirectoryPath)) Directory.CreateDirectory(FullDirectoryPath);
		string json = JsonUtility.ToJson(Dialogue.Serialised, prettyPrint: true);
		string message = $"Successfully {(File.Exists(FullFilePath) ? "modified" : "created")} \"{FilePath}\"";
		File.WriteAllText(FullFilePath, json);
		Debug.Log(message);
	}

	void DeleteFile()
	{
		string message = $"Successfully deleted \"{FilePath}\"";
		File.Delete(FullFilePath);
		if (File.Exists($"{FullFilePath}.meta"))
		{
			File.Delete($"{FullFilePath}.meta");
			message += $" and \"{FilePath}.meta\"";
		}
		if (m_bClearOnDelete) Clear();
		Debug.Log(message);
	}

	void Clear()
	{
		m_LastSelectedDirectoryOptionIndex = 0;
		m_LastSelectedDirectoryOptionIndex = 0;
		m_SelectedDirectoryIndex = 0;
		m_LastSelectedDirectoryIndex = 0;
		m_LastSelectedFileOptionIndex = 0;
		m_LastSelectedFileOptionIndex = 0;
		m_SelectedFileIndex = 0;
		m_LastSelectedFileIndex = 0;
		GetDirectories();
		m_DirectoryName = m_SubDirectories[0];
		GetFiles();
		m_FileName = m_Files.Length > 0 ? m_Files[0] : "";
		if (!string.IsNullOrWhiteSpace(m_FileName)) LoadFile();
		else Dialogue = new Dialogue();
		m_SerializedObject.Update();
		m_SerializedObject.ApplyModifiedProperties();
	}
}
