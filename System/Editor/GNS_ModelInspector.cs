using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GNS;
using GNS.Core;
using UnityEditorInternal;

namespace GNS.Editors{
	[CustomEditor(typeof(GNS_Model))]
	public class GNS_ModelInspector : EditorBaseUtility {

		#region Base voids
		private void OnEnable(){
			GNS_Model Model = (GNS_Model)target;

			ReorderableList MeshList = AddReorderableList<Mesh>("Meshes", Model.LODMeshes, (Rect rect, int index, bool isActive, bool isFocused) => {
				float HalfWidth = rect.width * 0.5f;

				string Name = (Model.LODMeshes[index] != null)?Model.LODMeshes[index].name + " ("+Model.LODMeshes[index].vertexCount+" verts)":"Empty Mesh";

				Model.LODMeshes[index] = (Mesh)EditorGUI.ObjectField(new Rect(
					rect.position.x,
					rect.position.y,
					HalfWidth,
					rect.height
				),Model.LODMeshes[index],typeof(Mesh));

				Model.LODIndexes[index] = (int)EditorGUI.IntField(new Rect(
					rect.position.x + (HalfWidth + 16f),
					rect.position.y,
					(HalfWidth) - 16f,
					rect.height
				),"LOD Index:",Model.LODIndexes[index]);
			}, false);

			MeshList.onAddCallback = (ReorderableList List) => {
				Model.LODMeshes.Add(null);
				Model.LODIndexes.Add(0);
			};

			MeshList.onRemoveCallback = (ReorderableList list) => {
				Model.LODMeshes.RemoveAt(list.index);
				Model.LODIndexes.RemoveAt(list.index);
			};
		}

		public override void OnInspectorGUI ()
		{
			GNS_Model Model = (GNS_Model)target;

			EditorGUILayout.LabelField ("Model Settings",EditorStyles.boldLabel);
			RenderListAt (0);
			Model.WindInfluence = EditorGUILayout.Slider (new GUIContent("Wind Influence","How much should this model be influenced by wind?"),Model.WindInfluence,0f,1f);
			Model.CustomNormals = EditorGUILayout.Toggle (new GUIContent("Custom Normals","Does this model have its own custom normals or do you want this model to have automated normals"),Model.CustomNormals);

			base.OnInspectorGUI ();
		}
		#endregion
	}
}
