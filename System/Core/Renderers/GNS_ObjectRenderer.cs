using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS;
using GNS.Core;
using GNS.Renderers.Patches;

namespace GNS.Renderers{
	public class GNS_ObjectRenderer : GNS_Renderer<GNS_ObjectPatch> {
		#region Private variables
		private Vector2 _CachePatchRounded;
		private Vector3 _CacheTerrainPosition;
		#endregion

		#region implemented abstract members of GNS_Renderer
		protected override void OnRender ()
		{
			
		}
		protected override void OnInitialize ()
		{	
			_CachePatchRounded = Manager.PatchRounded * 0.5f;
			_CacheTerrainPosition = Manager.Terrain.transform.position;
		}
		protected override void OnDestroy ()
		{
			
		}
		protected override GNS_ObjectPatch CreatePatch (Vector3 Position)
		{
			GNS_ObjectPatch Patch = new GNS_ObjectPatch (Position,Manager.Layers.Count,Manager,Manager.CrossFade,Manager.CrossFadeDuration);
			Patch.InitializeObjectPatch (Manager,_CacheTerrainPosition,_CachePatchRounded,RenderData.LODLayerBulkMeshes,RenderData.LayerMaterials,Manager.Shadows);
			return Patch;
		}
		protected override void OnPaint (int Layer,Vector2 Point, Vector2 Bounds, bool Visible, Texture2D BrushTexture = null)
		{
			ForEachIn (Point,Bounds,(GNS_ObjectPatch obj) => { // for each patch in range
				obj.Paint(Layer,Point,Bounds,Visible,BrushTexture);
			});
		}

		protected override void OnUpdateVisibility (Vector2 Point, Vector2 Bounds)
		{
			ForEachIn (Point,Bounds,(GNS_ObjectPatch obj) => { // for each patch in range

			});
		}
		#endregion

		#region Constructor
		public GNS_ObjectRenderer(GrassNStuff Manager, GNS_Data RenderData):base(Manager,RenderData){

		}
		#endregion

		#region Virtuals
		protected override void OnDrawGizmos ()
		{
			
		}
		protected override void OnPatchCullingChanged (CullingGroupEvent Event)
		{
			Patches [Event.index].UpdatePatchState (Event);
		}
		#endregion

		#region Private voids
		private void ForEachIn(Vector2 Point, Vector2 Bounds,System.Action<GNS_ObjectPatch> ForPatch){
			for (int i = 0; i < Patches.Length; i++) {
				Vector3 Relative = Patches [i].RelativePosition;

				if ( // intersection test
					Relative.x + _CachePatchRounded.x >= Point.x &&
					Relative.x - _CachePatchRounded.x <= Point.x + Bounds.x &&
					Relative.z + _CachePatchRounded.y >= Point.y &&
					Relative.z - _CachePatchRounded.y <= Point.y + Bounds.y
				) {
					ForPatch (Patches[i]);
				}
			}
		}
		#endregion
	}
}