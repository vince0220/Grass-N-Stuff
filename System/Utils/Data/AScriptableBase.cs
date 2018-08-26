using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GNS.Utils{
	public class AScriptableBase : ScriptableObject {
		#region Singleton
		protected static AScriptableBase _Instance;
		protected static AScriptableBase I{
			get{
				if (_Instance == null) {
					_Instance = new AScriptableBase ();
				}
				return _Instance;
			}
		}
		#endregion

		#region Public inspector variables
		[Header("Scriptable Settings")]
		[SerializeField]private string _Name;
		[SerializeField][HideInInspector]private string _Key;
		#endregion

		#region Public voids
		public void RegenerateKey(){
			#if UNITY_EDITOR
			string ID = AssetDatabase.AssetPathToGUID (AssetDatabase.GetAssetPath(this));
			this._Key = ID;
			#endif
		}
		#if UNITY_EDITOR
		public void CreateDataAsset<T>(string DefaultAssetName) where T : AScriptableBase{
			T Data = ScriptableObject.CreateInstance<T> ();
			string Path = AssetDatabase.GenerateUniqueAssetPath(GetSelectedPathOrFallback()+"/"+DefaultAssetName+".asset");
			AssetDatabase.CreateAsset (Data,Path);
			AssetDatabase.SaveAssets ();

			string[] SplitSlash = Path.Split ('/','.');
			string Name = SplitSlash [SplitSlash.Length - 2];
			Data._Name = Name;

			// assing unique id
			Data.RegenerateKey();
		}
		#endif
		#endregion

		#region Private voids
		private string GetSelectedPathOrFallback()
		{
			string path = "Assets";
			#if UNITY_EDITOR
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
			{
				path = AssetDatabase.GetAssetPath(obj);
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
				{
					path = Path.GetDirectoryName(path);
					break;
				}
			}
			#endif
			return path;
		}
		#endregion

		#region Get / Set
		public string Key{
			get{
				return _Key;
			}
		}
		public string Name{
			get{
				return _Name;
			}
			set{
				_Name = value;
			}
		}
		#endregion
	}
}