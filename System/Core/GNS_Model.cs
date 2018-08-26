using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS.Utils;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GNS.Core{
	public class GNS_Model : AScriptableBase {
		#if UNITY_EDITOR
		#region CreateFunctions
		[MenuItem("Assets/Create/GrassNStuff/Model",false,111)]
		public static void CreateAsset(){
			I.CreateDataAsset<GNS_Model> ("Model");
		}
		#endregion
		#endif

		#region Private variables
		[SerializeField]private List<Mesh> _LODMeshes = new List<Mesh>();
		[SerializeField]private List<int> _LODIndexes = new List<int>();
		[SerializeField]private float _WindInfluence = 1f;
		[SerializeField]private bool _CustomNormals = false;
		#endregion

		#region public voids
		public Mesh GetLODMesh(int LODIndex){
			int Index = 0;
			for (int i = 0; i < _LODIndexes.Count; i++) {
				if (i == LODIndex) {
					Index = i;
					break;
				}

				if (i > Index) {
					Index = i;
				}
			}

			return _LODMeshes [Index];
		}
		#endregion

		#region Get / Set
		public List<Mesh> LODMeshes{
			get{
				return _LODMeshes;
			}
		}
		public List<int> LODIndexes{
			get{
				return _LODIndexes;
			}
		}
		public float WindInfluence{
			get{
				return _WindInfluence;
			}
			set{
				_WindInfluence = value;
			}
		}
		public bool CustomNormals{
			get{
				return _CustomNormals;
			}
			set{
				_CustomNormals = value;
			}
		}
		#endregion
	}
}
