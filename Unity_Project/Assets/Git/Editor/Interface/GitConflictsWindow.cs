using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GitConflictsWindow : EditorWindow {
	public static GitConflictsWindow Instance { get; private set; }

	class ConflictData {
		public string index = "";
		public string fileName = "";
		public List<string> hashCodes = new List<string>();
		public List<string> locationCodes = new List<string>();
		public bool useMine = true;
	}
	Dictionary<string, ConflictData> conflicts = new Dictionary<string, ConflictData>();


	[MenuItem ("Git/Resolve Conflicts (Unmerged)")]
	static void Init () {
		Init(null);
	}


	public static void Init (string[] conflictedFiles) {
		// Get existing open window or if none, make a new one:
		Instance = EditorWindow.GetWindow<GitConflictsWindow>(true, "Git Push");

		if ( conflictedFiles == null )
			Instance.ParseData(GitSystem.GetUnmergedFilesList());
		else
			Instance.ParseData(conflictedFiles);
	}


	void ReInit() {
		ParseData(GitSystem.GetUnmergedFilesList());
	}


	void ParseData(string[] conflictedFiles) {
		for ( int i = 0; i < conflictedFiles.Length; i++ ) {
			string[] dataSplit = conflictedFiles[i].Split('\t');
			string[] secondSplit = dataSplit[0].Split(' ');
			string fileName = dataSplit[1];
			string index = secondSplit[0];
			string hashCode = secondSplit[1];
			string locationCode = secondSplit[2];

			if ( !conflicts.ContainsKey(index) )
				conflicts.Add(index, new ConflictData());

			conflicts[index].fileName = fileName;
			conflicts[index].hashCodes.Add(hashCode);
			conflicts[index].locationCodes.Add(locationCode);
		}
	}


	string[] resolveUsing = { "Mine", "Theirs" };

	void OnGUI() {
		foreach ( string key in conflicts.Keys ) {
			ConflictData conflict = conflicts[key];

			GUILayout.BeginHorizontal();
			GUILayout.Label(conflict.fileName);
			conflict.useMine = EditorGUILayout.Popup(conflict.useMine ? 0 : 1, resolveUsing) == 0;
			if ( GUILayout.Button("Resolve") ) {
				conflicts.Remove(key);
				return;
			}

			GUILayout.EndHorizontal();

			foreach ( string code in conflict.locationCodes ) {
				// Is it ours?
				if ( code == "2" ) {
					GUILayout.Label(GitSystem.RunGitCmd("diff -" + code + " " + conflict.fileName));
					GUILayout.Label(GitSystem.RunGitCmd("diff -ours " + conflict.fileName));
				}

				// Is it theirs?
				if ( code == "3" ) {
//					GUILayout.Label(GitSystem.RunGitCmd("diff -theirs " + conflict.fileName));
				}
			}
		}
	}
}