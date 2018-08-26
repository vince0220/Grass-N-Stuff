using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS;
using GNS.Core;
using GNS.Renderers.Patches;

namespace GNS.Renderers{
	public class GNS_DrawRenderer : GNS_Renderer<GNS_DrawPatch> {
		#region Constructor
		public GNS_DrawRenderer(GrassNStuff Manager,GNS_Data RenderData):base(Manager,RenderData){

		}
		#endregion

		#region Abstract members of GNS_Renderer
		protected override void OnRender ()
		{
			RenderAll ();
		}
		protected override void OnInitialize ()
		{
			InitializeMatrices ();
		}
		protected override void OnDestroy ()
		{
			
		}
		protected override GNS_DrawPatch CreatePatch (Vector3 Position)
		{
			return new GNS_DrawPatch (Position,Manager.Layers.Count);
		}
		protected override void OnPaint (int Layer, Vector2 Point, Vector2 Bounds, bool Visible, Texture2D BrushTexture = null)
		{
			throw new System.NotImplementedException ();
		}
		protected override void OnUpdateVisibility (Vector2 Point, Vector2 Bounds)
		{
			throw new System.NotImplementedException ();
		}
		#endregion

		#region Private voids

		// Rendering voids
		private void RenderAll(){
			// Gather patch matrices
			int[][] LODMatrices = CurrentLODMatrices(); // get current LOD matrices

			for (int i = 0; i < Manager.Layers.Count; i++) { // render layers
				RenderLayer (i,LODMatrices);
			}
		}
		private void RenderLayer(int LayerIndex,int[][] LODPatches){
			GNS_BulkMesh[] BulkMeshes = RenderData.LODLayerBulkMeshes[LayerIndex]; // Bulk meshes

			for (int i = 0; i < BulkMeshes.Length; i++) {
				RenderBulkMesh (BulkMeshes[i],LODPatches[i],RenderData.PropertyBlocks[LayerIndex],RenderData.LayerMaterials[LayerIndex]);
			}
		}
		private void RenderBulkMesh(GNS_BulkMesh Mesh, int[] PatchIndexes,MaterialPropertyBlock Block,Material Material){
			int[][] SplittedArray = GNS_Extensions.SplitArray<int> (ref PatchIndexes,GrassNStuff.MaxPatchDrawCount);
			for (int i = 0; i < SplittedArray.Length; i++) {
				Matrix4x4[] Matrices = new Matrix4x4[SplittedArray[i].Length]; // build matrices
				for (int x = 0; x < Matrices.Length; x++) {
					Matrices [x] = Patches[SplittedArray [i] [x]].TransformMatrix;
				}
				Graphics.DrawMeshInstanced(Mesh.BulkMesh, 0, Material, Matrices, SplittedArray[i].Length, Block, Manager.Shadows);
			}
		}

		// Init voids
		private void InitializeMatrices(){
			for (int i = 0; i < Patches.Length; i++) {
				Patches [i].InitializeMatrix (Manager.transform.position); // initialize Matrices
			}
		}
		#endregion
	}
}
