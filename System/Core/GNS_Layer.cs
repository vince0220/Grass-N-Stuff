using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS.Utils;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GNS.Core{
	public class GNS_Layer : AScriptableBase {
		#if UNITY_EDITOR
		#region CreateFunctions
		[MenuItem("Assets/Create/GrassNStuff/Layer",false,111)]
		public static void CreateAsset(){
			I.CreateDataAsset<GNS_Layer> ("Layer");
		}
		#endregion
		#endif

		#region Private variable
		// Material variables
		[SerializeField]private Color _Color = Color.white;
		[SerializeField]private Texture2D _Albedo;
		[SerializeField]private Texture2D _Normal;
		[SerializeField]private Texture2D _SpecGloss;
		[SerializeField]private float _GlossStrength;
		[SerializeField]private float _SpecularStrength;
		[SerializeField]private float _CutOff;
		[SerializeField]private float _OcclusionHeight = 0.3f;
		[SerializeField]private float _OcclusionStrength = 1f;
		[SerializeField]private float _OcclusionToColor = 0.5f;

		// Global variables
		[SerializeField]private float _Density = 1f;
		[SerializeField]private List<GNS_Model> _LODModels = new List<GNS_Model>();
		[SerializeField]private List<float> _LODMeshesPercentiles = new List<float>();

		// Rotation variables
		[SerializeField, Range(0, 90)]float _RandomPitchAngle = 45;
		[SerializeField, Range(0, 90)]float _NoisePitchAngle = 30.0f;
		[SerializeField]float _RotationNoiseFrequency = 1.0f;
		[SerializeField]Vector3 _RotationNoiseAxis = Vector3.right;

		// Scale variables
		[SerializeField] Vector3 _BaseScale = Vector3.one;
		[SerializeField] float _MinRandomScale = 0.8f;
		[SerializeField] float _ScaleNoiseAmplitude = 0.5f;
		[SerializeField]float _ScaleNoiseFrequency = 0.5f;
		[SerializeField]float _MaxRandomScale = 1.0f;
		#endregion

		#region Public voids
		public GNS_BulkMesh[] Meshes(GrassNStuff Manager){
			if (Manager.LODDistances.Count > 0) {
				GNS_BulkMesh[] Meshes = new GNS_BulkMesh[Manager.LODDistances.Count];
				for (int i = 0; i < Manager.LODDistances.Count; i++) {
					Meshes [i] = new GNS_BulkMesh ();
					Meshes [i].InitializeBulkMesh (_LODModels,_LODMeshesPercentiles,
						(int)(GNS.GrassNStuff.MaxMeshVertexCount * (Manager.LODDistances[i].y / 100f) * _Density) // calculate max vertex count (Max vertex * LOD percentile * Density)
						,i,new Vector3(Manager.PatchRounded.x,Mathf.Max(Manager.PatchRounded.x,Manager.PatchRounded.y),Manager.PatchRounded.y));
				}
				return Meshes;
			}
			return new GNS_BulkMesh[0];
		}
		#endregion

		#region Get / Set
		public float Density{
			get{
				return _Density;
			}
			set{
				_Density = value;
			}
		}
		public List<GNS_Model> LODModels{
			get{
				return _LODModels;
			}
		}
		public List<float> LODMeshesPercentiles{
			get{
				return _LODMeshesPercentiles;
			}
		}

		// Material Get / Set
		public Color Color{
			get{
				return _Color;
			}
			set{
				_Color = value;
			}
		}
		public Texture2D Albedo{
			get{
				return _Albedo;
			}
			set{
				_Albedo = value;
			}
		}
		public Texture2D Normal{
			get{
				return _Normal;
			}
			set{
				_Normal = value;
			}
		}
		public Texture2D SpecGloss{
			get{
				return _SpecGloss;
			}
			set{
				_SpecGloss = value;
			}
		}
		public float GlossStrength{
			get{
				return _GlossStrength;
			}
			set{
				_GlossStrength = value;
			}
		}
		public float SpecularStrength{
			get{
				return _SpecularStrength;
			}
			set{
				_SpecularStrength = value;
			}
		}
		public float CutOff{
			get{
				return _CutOff;
			}
			set{
				_CutOff = value;
			}
		}
		public float OcclusionHeight{
			get{
				return _OcclusionHeight;
			}
			set{
				_OcclusionHeight = value;
			}
		}
		public float OcclusionStrength{
			get{
				return _OcclusionStrength;
			}
			set{
				_OcclusionStrength = value;
			}
		}
		public float OcclusionToColor{
			get{
				return _OcclusionToColor;
			}
			set{
				_OcclusionToColor = value;
			}
		}

		// Rotation Get / Set
		public float RandomPitchAngle {
			get { return _RandomPitchAngle; }
			set { _RandomPitchAngle = value; }
		}
		public float NoisePitchAngle {
			get { return _NoisePitchAngle; }
			set { _NoisePitchAngle = value; }
		}
		public float RotationNoiseFrequency {
			get { return _RotationNoiseFrequency; }
			set { _RotationNoiseFrequency = value; }
		}
		public Vector3 RotationNoiseAxis {
			get { return _RotationNoiseAxis; }
			set { _RotationNoiseAxis = value; }
		}

		// Scale Get / Set
		public Vector3 BaseScale {
			get { return _BaseScale; }
			set { _BaseScale = value;}
		}
		public float MinRandomScale {
			get { return _MinRandomScale; }
			set { _MinRandomScale = value;}
		}
		public float MaxRandomScale {
			get { return _MaxRandomScale; }
			set { _MaxRandomScale = value;}
		}
		public float ScaleNoiseAmplitude {
			get { return _ScaleNoiseAmplitude; }
			set { _ScaleNoiseAmplitude = value;}
		}
		public float ScaleNoiseFrequency {
			get { return _ScaleNoiseFrequency; }
			set { _ScaleNoiseFrequency = value;}
		}
		#endregion
	}
}
