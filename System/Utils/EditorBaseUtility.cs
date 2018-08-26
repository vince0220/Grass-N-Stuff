#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using GNS.Utils;

namespace GNS.Editors{
	public class EditorBaseUtility : AScriptableBaseInterface {
		#region Private variables
		private List<ReorderableList> Lists = new List<ReorderableList>();
		#endregion

		#region Protected variables
		protected float ListLabelWidth = 80f;
		#endregion

		#region Protected voids
		protected ReorderableList AddReorderableList<T>(string Header,List<T> List,ReorderableList.ElementCallbackDelegate ElementCallback,bool Draggable = true){
			ReorderableList ReorderList = new ReorderableList(List,typeof(T),Draggable,true,true,true);

			ReorderList.drawHeaderCallback = (Rect rect) => {
				EditorGUI.LabelField(rect,Header);	
			};
				
			ReorderList.drawElementCallback = ElementCallback;
			Lists.Add (ReorderList);
			return ReorderList;
		}
		protected void RenderListAt(int Index){
			if (Lists.Count > Index) {
				float Before = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = ListLabelWidth;
				serializedObject.Update ();
				Lists [Index].DoLayoutList ();
				serializedObject.ApplyModifiedProperties ();
				EditorGUIUtility.labelWidth = Before;
			}
		}
		#endregion
	}
}
#endif
