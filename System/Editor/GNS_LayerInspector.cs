using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GNS;
using GNS.Core;
using UnityEditorInternal;

namespace GNS.Editors{
	[CustomEditor(typeof(GNS_Layer))]
	public class GNS_LayerInspector : EditorBaseUtility {

		#region Base voids
		private void OnEnable(){
			ListLabelWidth = 70;
			// Data
			GNS_Layer Layer = (GNS_Layer)target;

			ReorderableList MeshList = AddReorderableList<GNS_Model>("Meshes", Layer.LODModels, (Rect rect, int index, bool isActive, bool isFocused) => {
				float HalfWidth = rect.width * 0.5f;

				string Name = (Layer.LODModels[index] != null)?Layer.LODModels[index].name:"Empty Mesh";

				Layer.LODModels[index] = (GNS_Model)EditorGUI.ObjectField(new Rect(
					rect.position.x,
					rect.position.y,
					HalfWidth,
					rect.height
				),Layer.LODModels[index],typeof(GNS_Model));
					
				Layer.LODMeshesPercentiles[index] = EditorGUI.Slider(new Rect(
					rect.position.x + (HalfWidth + 16f),
					rect.position.y,
					(HalfWidth) - 16f,
					rect.height
				),"Percentile:",Layer.LODMeshesPercentiles[index],0f,100f);
			}, false);

			MeshList.onAddCallback = (ReorderableList List) => {
				Layer.LODModels.Add(null);
				Layer.LODMeshesPercentiles.Add(100f);
			};

			MeshList.onRemoveCallback = (ReorderableList list) => {
				Layer.LODModels.RemoveAt(list.index);
				Layer.LODMeshesPercentiles.RemoveAt(list.index);
			};
		}
		public override void OnInspectorGUI ()
		{
			// Data
			GNS_Layer Layer = (GNS_Layer)target;

			// Mesh settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Meshes Settings", EditorStyles.boldLabel);
			RenderListAt (0); // render mesh list

			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Render Settings", EditorStyles.boldLabel);
			Layer.Density = EditorGUILayout.Slider (new GUIContent("Layer Density"),Layer.Density,0f,1f);

			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Rotation Settings", EditorStyles.boldLabel);
			Layer.RandomPitchAngle = EditorGUILayout.Slider (new GUIContent("Random Pitch Angle"),Layer.RandomPitchAngle,0f,90f);
			Layer.NoisePitchAngle = EditorGUILayout.Slider (new GUIContent("Noise Pitch Angle"),Layer.NoisePitchAngle,0f,90f);
			Layer.RotationNoiseFrequency = EditorGUILayout.Slider (new GUIContent("Rotation Noise Frequency"),Layer.RotationNoiseFrequency,0f,10f);
			Layer.RotationNoiseAxis = EditorGUILayout.Vector3Field (new GUIContent("Rotation Noise Axis"),Layer.RotationNoiseAxis);

			// check if value has changed Rotation
			if (EditorGUI.EndChangeCheck ()) { 
				ForEachManager ((GrassNStuff Manager)=>{
					Manager.RegenerateRotationTextures (); // update managers
				});
			}

			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Scale Settings", EditorStyles.boldLabel);
			Layer.BaseScale = EditorGUILayout.Vector3Field (new GUIContent("Base Scale"),Layer.BaseScale);
			Layer.MinRandomScale = EditorGUILayout.Slider (new GUIContent("Min Random Scale"),Layer.MinRandomScale,0f,5f);
			Layer.MaxRandomScale = EditorGUILayout.Slider (new GUIContent("Max Random Scale"),Layer.MaxRandomScale,0f,5f);
			Layer.ScaleNoiseAmplitude = EditorGUILayout.Slider (new GUIContent("Scale Noise Amplitude"),Layer.ScaleNoiseAmplitude,0f,5f);
			Layer.ScaleNoiseFrequency = EditorGUILayout.Slider (new GUIContent("Scale Noise Frequency"),Layer.ScaleNoiseFrequency,0f,2000f);

			// check if value has changed Scale
			if (EditorGUI.EndChangeCheck ()) { 
				ForEachManager ((GrassNStuff Manager)=>{
					Manager.RegenerateScaleTextures (); // update managers
				});
			}

			// material settings
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Material Settings", EditorStyles.boldLabel);
			Layer.Color = EditorGUILayout.ColorField (new GUIContent("Main Color"),Layer.Color);
			Layer.Albedo = (Texture2D)EditorGUILayout.ObjectField (new GUIContent("Albedo"),Layer.Albedo,typeof(Texture2D),false);
			Layer.Normal = (Texture2D)EditorGUILayout.ObjectField (new GUIContent("Normal"),Layer.Normal,typeof(Texture2D),false);
			Layer.SpecGloss = (Texture2D)EditorGUILayout.ObjectField (new GUIContent("Specular(R) + Gloss(G)"),Layer.SpecGloss,typeof(Texture2D),false);
			Layer.SpecularStrength = EditorGUILayout.Slider (new GUIContent("Specular Strength"),Layer.SpecularStrength,0f,1f);
			Layer.GlossStrength = EditorGUILayout.Slider (new GUIContent("Gloss Strength"),Layer.GlossStrength,0f,1f);
			Layer.CutOff = EditorGUILayout.Slider (new GUIContent("Cut Off"),Layer.CutOff,0f,1f);
			Layer.OcclusionHeight = EditorGUILayout.Slider (new GUIContent("Occlusion Height"),Layer.OcclusionHeight,0f,10f);
			Layer.OcclusionStrength = EditorGUILayout.Slider (new GUIContent("Occlusion Strength"),Layer.OcclusionStrength,0f,10f);
			Layer.OcclusionToColor = EditorGUILayout.Slider (new GUIContent("Occlusion To Color"),Layer.OcclusionToColor,0f,1f);
			if (EditorGUI.EndChangeCheck ()) { 
				ForEachManager ((GrassNStuff Manager)=>{
					Manager.UpdateMaterialBuffers (); // update managers
				});
			}

			base.OnInspectorGUI (); // draw base

			EditorUtility.SetDirty (Layer); // set dirty
		}
		#endregion
	
		#region Private voids
		private void ForEachManager(System.Action<GrassNStuff> Callback){
			GrassNStuff[] Managers = GameObject.FindObjectsOfType<GrassNStuff>(); // find all managers
			for (int i = 0; i < Managers.Length; i++) {
				Callback.Invoke (Managers[i]);
			}
		}
		#endregion
	}
}
