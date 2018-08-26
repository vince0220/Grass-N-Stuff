using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNS.Core{
	public class GNS_BulkMesh {
		#region Private variables
		private Mesh _BulkMesh;
		private int _TotalMeshCount = 0;
		#endregion

		#region Public voids
		public void InitializeBulkMesh(List<GNS_Model> Meshes, List<float> Percentiles, int MaxVertCount,int LODIndex,Vector3 BoundSize){
			_BulkMesh = new Mesh ();
			
			// Calculate Mesh Indexes
			int BulkMeshVertCount = 0;
			List<int> MeshIndexes = new List<int> ();
			while (BulkMeshVertCount < MaxVertCount) { // loop until mesh is full
				int RandomIndex = WeightedRandom(Percentiles); // get random mesh index
				Mesh RandomMesh = Meshes [RandomIndex].LODMeshes[0]; // get random mesh
				if (BulkMeshVertCount + RandomMesh.vertexCount > MaxVertCount) {break;} // cant add cause it exceeds limit
				BulkMeshVertCount += RandomMesh.vertexCount;
				MeshIndexes.Add(RandomIndex); // add index to index list
			}

			// Generate combined mesh
			int BulkMeshTextureSize = Mathf.CeilToInt(Mathf.Sqrt (MeshIndexes.Count));
			CombineInstance[] CombinedMeshes = new CombineInstance[MeshIndexes.Count];
			for (int i = 0; i < MeshIndexes.Count; i++) {
				GNS_Model RandomModel = Meshes [MeshIndexes [i]]; // get random mesh
				Mesh RandomMesh = RandomModel.GetLODMesh(LODIndex); // get mesh
				Mesh TempMesh = new Mesh();

				// Copy data into temp mesh
				TempMesh.vertices = RandomMesh.vertices; // copy verts
				TempMesh.triangles = RandomMesh.triangles; // copy triangles
				TempMesh.normals = RandomMesh.normals; // copy normals
				TempMesh.tangents = RandomMesh.tangents; // copy tangents
				TempMesh.uv = RandomMesh.uv; // copy uvs

				// Generate uv2 coords
				Vector2[] uv2 = new Vector2[TempMesh.vertexCount];
				Vector2[] uv3 = new Vector2[TempMesh.vertexCount];

				int Row = (int)(i / BulkMeshTextureSize); // calculate texture row
				int Colum = i - (Row * BulkMeshTextureSize); // calulate texture colum
				Vector2 InstanceUV2 = new Vector2( // generate uv2
					((float)Colum) / (float)(BulkMeshTextureSize), // calculate relative uv2 coord
					((float)Row) / (float)(BulkMeshTextureSize) // calculate relative uv2 coord
				);
				Vector2 WindInfluence = new Vector2 (RandomModel.WindInfluence,(RandomModel.CustomNormals)?0f:1f);

				for (int x = 0; x < uv2.Length; x++) {
					uv2 [x] = InstanceUV2;
					uv3 [x] = WindInfluence;
				} // set all uv2 coords

				// Set
				TempMesh.uv2 = uv2; // set uv2 coords of temp mesh
				TempMesh.uv3 = uv3;
				CombinedMeshes [i].mesh = TempMesh; // set combine instance mesh
				CombinedMeshes [i].transform = Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one); // set transform of combine instance
			}

			// Final combination of meshes
			_BulkMesh.CombineMeshes (CombinedMeshes); // combine meshes into bulk mesh
			_BulkMesh.bounds = new Bounds(_BulkMesh.bounds.center,BoundSize); // set bound size
			_TotalMeshCount = MeshIndexes.Count; //  set total mesh count
		}
		#endregion

		#region Private voids
		private int WeightedRandom(List<float> Weights){
			float Summation = 0f;
			for (int i = 0; i < Weights.Count; i++) {
				Summation += Weights [i];
			}

			float Random = UnityEngine.Random.Range (0,Summation);
			for (int i = 0; i < Weights.Count; i++) {
				Random -= Weights [i];
				if (Random <= 0) {
					return i;
				}
			}
			return Weights.Count - 1;
		}
		#endregion

		#region Get / Set
		public Mesh BulkMesh{
			get{
				return this._BulkMesh;
			}
		}
		public int TotalMeshCount{
			get{
				return _TotalMeshCount;
			}
		}
		public int TextureSize{
			get{
				return Mathf.CeilToInt (Mathf.Sqrt (_TotalMeshCount));
			}
		}
		#endregion
	}
}
