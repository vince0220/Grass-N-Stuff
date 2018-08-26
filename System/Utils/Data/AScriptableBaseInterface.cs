#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GNS.Utils{
	[CustomEditor(typeof(AScriptableBase),true)]
	public class AScriptableBaseInterface : Editor {
		public override void OnInspectorGUI ()
		{
			AScriptableBase Data = (AScriptableBase)target; // get target

			EditorGUILayout.Space ();
			EditorGUILayout.LabelField ("Other Values",EditorStyles.boldLabel);
			Data.Name = EditorGUILayout.TextField ("Name",Data.Name);
			EditorGUILayout.LabelField ("Key: "+Data.Key,EditorStyles.wordWrappedMiniLabel);

			string Path = AssetDatabase.GetAssetPath (Data);
			string[] SplitSlash = Path.Split ('/','.');
			string Name = SplitSlash [SplitSlash.Length - 2];

			if (Data.Name != Name && Data.Name != "" && GUILayout.Button("Rename Asset")) {
				AssetDatabase.RenameAsset(Path,Data.Name);
				AssetDatabase.SaveAssets ();
				Data.RegenerateKey ();
			}

			EditorUtility.SetDirty (Data); // set data dirty
		}
	}
}
#endif