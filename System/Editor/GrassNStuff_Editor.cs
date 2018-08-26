/// <summary>
/// Custom editor for GrassNStuff manager
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GNS;
using GNS.Core;

namespace GNS.Editors{
	[CustomEditor(typeof(GrassNStuff))]
	public class GrassNStuff_Editor : EditorBaseUtility {
		#region Base voids
		private void OnEnable(){
			GrassNStuff GNS = (GrassNStuff)target;
			var LayerList = this.AddReorderableList<GNS_Layer> ("Layers", GNS.Layers, (Rect rect, int index, bool isActive, bool isFocused) => {
				GNS.Layers[index] = (GNS_Layer)EditorGUI.ObjectField(rect,(GNS.Layers[index] != null)?GNS.Layers[index].name:"Empty Layer",GNS.Layers[index],typeof(GNS_Layer));
			});
			LayerList.onAddCallback = (UnityEditorInternal.ReorderableList list) => {
				LayerList.list.Add(null);
			};

			this.AddReorderableList<Vector2> ("LOD Levels", GNS.LODDistances, (Rect rect, int index, bool isActive, bool isFocused) => {
				float HalfWidth = rect.width * 0.5f;
				
				float LODDistance = EditorGUI.FloatField(new Rect(
					rect.position.x,
					rect.position.y,
					HalfWidth,
					rect.height
				),(index + 1)+". Distance:",GNS.LODDistances[index].x);

				float LODPercentage = EditorGUI.Slider(new Rect(
					rect.position.x + (HalfWidth + 16f),
					rect.position.y,
					HalfWidth - 16f,
					rect.height
				),"Detail:",GNS.LODDistances[index].y,0f,100f);

				GNS.LODDistances[index] = new Vector2(LODDistance,LODPercentage);
			},false);
		}
		public override void OnInspectorGUI ()
		{
			// Base Data
			GrassNStuff GNS = (GrassNStuff)target;

			// Terrain settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Global Settings", EditorStyles.boldLabel);
			GNS.Terrain = (Terrain)EditorGUILayout.ObjectField (new GUIContent("Terrain"),GNS.Terrain, typeof(Terrain), true);
			GNS.Camera = (Camera)EditorGUILayout.ObjectField (new GUIContent("Camera"),GNS.Camera,typeof(Camera),true);

			// Layer settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Layer Settings", EditorStyles.boldLabel);
			this.RenderListAt (0);

			// Performance Settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Performance Settings", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck ();
			GNS.PatchSize = EditorGUILayout.Slider(new GUIContent("Patch Size"),GNS.PatchSize,1f,200f);
			GNS.TransformResolution = (GrassNStuff.TextureResolution)EditorGUILayout.EnumPopup (new GUIContent("Transform Resolution","The resolution for the textures that store the grass transform data"),GNS.TransformResolution);
			GNS.PaintResolution = (GrassNStuff.PatchResolution)EditorGUILayout.EnumPopup (new GUIContent("Paint Resolution","The resolution for the textures that store the visibility information for the layers"),GNS.PaintResolution);
			GNS.Shadows = (UnityEngine.Rendering.ShadowCastingMode)EditorGUILayout.EnumPopup (new GUIContent("Shadows"),GNS.Shadows);
			if (EditorGUI.EndChangeCheck ()) { // check if Patch values have changed
				GNS.NotifyPatchesChanged ();
			}

			EditorGUI.BeginChangeCheck ();
			GNS.LODFadeDistance = EditorGUILayout.Slider (new GUIContent("LOD Fade Distance"),GNS.LODFadeDistance,0f,200f);
			if (EditorGUI.EndChangeCheck ()) {
				GNS.UpdateMaterialBuffers ();
			}
			GNS.CrossFade = EditorGUILayout.Toggle (new GUIContent("Cross Fade"),GNS.CrossFade);
			GNS.CrossFadeDuration = EditorGUILayout.Slider (new GUIContent("Cross Fade Duration"),GNS.CrossFadeDuration,0f,5f);

			// Render lod list
			EditorGUILayout.Space ();
			this.RenderListAt (1);

			// Editor Settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Wind Settings", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck ();
			GNS.WindQuality = (GrassNStuff.TextureResolution)EditorGUILayout.EnumPopup (new GUIContent("Wind Resolution","The resolution for the textures that store the wind offset data"),GNS.WindQuality);
			GNS.WindSpeed = EditorGUILayout.Slider (new GUIContent("Wind Speed"),GNS.WindSpeed,0f,15f);
			GNS.WindStrength = EditorGUILayout.Slider (new GUIContent("Wind Strength"),GNS.WindStrength,0f,5f);
			if (EditorGUI.EndChangeCheck ()) {
				GNS.UpdateWindValues ();
			}

			EditorGUI.BeginChangeCheck ();
			GNS.WindNoiseFrequency = EditorGUILayout.Slider (new GUIContent("Wind Noise Frequency"),GNS.WindNoiseFrequency,0f,2000f);
			GNS.WindNoiseAmplitude = EditorGUILayout.Slider (new GUIContent("Wind Noise Amplitude"),GNS.WindNoiseAmplitude,0f,10f);
			if (EditorGUI.EndChangeCheck ()) {
				GNS.RegenerateWindTexture();
			}

			// Editor Settings
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Editor Settings", EditorStyles.boldLabel);
			GNS.GridColor = EditorGUILayout.ColorField (new GUIContent("Grid Color"),GNS.GridColor);
			GNS.Debug = EditorGUILayout.Toggle (new GUIContent("Debug"),GNS.Debug);

			// Value display
			EditorGUILayout.Space();
			EditorGUILayout.LabelField ("Details", EditorStyles.boldLabel);

			// Total patch count
			if (GNS.TotalPatchCount > GrassNStuff.MaxEditorGridCount) {
				EditorGUILayout.HelpBox ("There are to much patches to render. Max render patch count: " + GrassNStuff.MaxEditorGridCount,MessageType.Warning);
			}
			EditorGUILayout.LabelField ("Total Patch Count: "+GNS.TotalPatchCount,EditorStyles.helpBox);
			EditorGUILayout.LabelField ("Max Patch Render Count: "+GNS.MaxPatchRenderCount,EditorStyles.helpBox);
			EditorGUILayout.LabelField ("Estimated Drawcalls: "+GNS.EstimatedDrawCalls,EditorStyles.helpBox);

			EditorUtility.SetDirty (GNS); // set dirty
		}
		#endregion
	}
}
