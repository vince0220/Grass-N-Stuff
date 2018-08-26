using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS;

namespace GNS.Core{
	public abstract class GNS_Renderer<T>: GNS_IRenderer where T : GNS_Patch {
		#region Public variables
		// Patches
		public T[] Patches;
		#endregion

		#region Private variables
		// General Data
		public GrassNStuff Manager;
		protected GNS_Data RenderData;

		// Culling
		private BoundingSphere[] _CullingSpheres;
		private CullingGroup _CullingGroup;
		#endregion

		#region Constructor
		public GNS_Renderer(GrassNStuff Manager,GNS_Data RenderData){
			this.Manager = Manager;
			this.RenderData = RenderData;
		}
		#endregion

		#region Public voids
		// Base
		public void Render(){
			UpdateCulling (); // update culling group before rendering
			OnRender (); // Call abstract
		}
		public void Initialize(){
			if(Application.isPlaying){
				OnInitialize (); // Call abstract
				RegeneratePatches (); // generate patches
				InitializeCulling (); // init culling
			}
		}
		public void Destroy(){
			DestroyCulling (); // destroy culling group
			OnDestroy (); // Call abstract
		}
		public void DrawGizmos(){
			if (Manager.Debug) {
				// only render if total patch count is lower then max count
				if (Manager.TotalPatchCount < GrassNStuff.MaxEditorGridCount) {
					Gizmos.color = Manager.GridColor;
					Vector2 Count = Manager.PatchCount;
					Vector2 Size = Manager.PatchRounded; // cache rounded position
					Vector3 Pos = Manager.Terrain.transform.position;
					Vector3 TerrainSize = Manager.Terrain.terrainData.size;

					// X
					for (int i = 1; i < Count.x; i++) {
						Gizmos.DrawLine (
							new Vector3(Pos.x + (Size.x * i),Pos.y,Pos.z),
							new Vector3(Pos.x + (Size.x * i),Pos.y,Pos.z + TerrainSize.z)
						);
					}

					// Y
					for (int i = 1; i < Count.y; i++) {
						Gizmos.DrawLine (
							new Vector3(Pos.x,Pos.y,Pos.z + (Size.y * i)),
							new Vector3(Pos.x + TerrainSize.x,Pos.y,Pos.z + (Size.y * i))
						);
					}
				}

				// draw grid boundaries
				Gizmos.color = Color.black;
				Gizmos.DrawWireCube (Manager.GridCenter, new Vector3 (Manager.BoundSize.x, Manager.BoundSize.y, Manager.BoundSize.z));

				OnDrawGizmos ();
			}
		}
		public void UpdatePatchHeight(int Index,float Height){
			Vector3 Pos = _CullingSpheres [Index].position;
			Pos.y = Height;
			_CullingSpheres [Index].position = Pos;
			Patches [Index].UpdatePatchHeight (Height);
		}
		public void UpdateVisibility (Vector2 Point, Vector2 Bounds)
		{
			OnUpdateVisibility (Point,Bounds);
		}
		public void Paint (int Layer,Vector2 Point, Vector2 Bounds, bool Visible, Texture2D BrushTexture = null)
		{
			OnPaint (Layer,Point,Bounds,Visible,BrushTexture);
		}

		// Get voids
		public GNS_Patch GetPatch(int Index){
			return Patches [Index];
		}
		#endregion

		#region Protected abstract voids
		protected abstract void OnRender();
		protected abstract void OnInitialize();
		protected abstract void OnDestroy();
		protected abstract T CreatePatch(Vector3 Position);
		protected abstract void OnPaint (int Layer,Vector2 Point, Vector2 Bounds, bool Visible, Texture2D BrushTexture = null);
		protected abstract void OnUpdateVisibility (Vector2 Point, Vector2 Bounds);
		#endregion

		#region Protected virtual voids
		protected virtual void OnDrawGizmos (){}
		protected virtual void OnPatchCullingChanged(CullingGroupEvent Event){}
		#endregion

		#region Private voids

		#region Patches
		// Generate
		private void RegeneratePatches(){
			// Initial variables
			Patches = new T[Manager.TotalPatchCount];
			int PatchIndex = 0;
			Vector2 Counts = Manager.PatchCount;
			Vector2 PatchSizes = Manager.PatchRounded;

			for (int z = 0; z < Counts.y; z++) {
				for (int x = 0; x < Counts.x; x++) {
					// Calculate patch position
					Vector3 PatchPosition = new Vector3 (
						(x * PatchSizes.x) + (PatchSizes.x * 0.5f),
						0,
						(z * PatchSizes.y) + (PatchSizes.y * 0.5f)
					);

					// set patches
					Patches [PatchIndex] = CreatePatch(PatchPosition);

					PatchIndex++; // up the patch index
				}
			}
		}
		#endregion

		#region Culling
		// Base voids
		private void InitializeCulling(){
			DestroyCulling (); // destroy old group
			_CullingGroup = new CullingGroup (); // initialize new culling group

			// Get Cache values
			float MaxPatchSize = Mathf.Max(Manager.PatchRounded.x,Manager.PatchRounded.y);
			Vector3 Position = Manager.Terrain.transform.position;

			// Set Settings
			_CullingGroup.targetCamera = Camera.main;

			// set bounding spheres
			_CullingSpheres = new BoundingSphere[Patches.Length]; // initialize bounding spheres
			for (int i = 0; i < _CullingSpheres.Length; i++) {
				_CullingSpheres [i] = new BoundingSphere (Position + Patches[i].RelativePosition,MaxPatchSize);
			}

			_CullingGroup.onStateChanged = OnCullingChanged;

			_CullingGroup.SetBoundingDistances(Manager.LODFloatDistances);
			_CullingGroup.SetBoundingSpheres (_CullingSpheres);
			_CullingGroup.SetBoundingSphereCount (_CullingSpheres.Length);
		}
		private void UpdateCulling(){
			_CullingGroup.SetDistanceReferencePoint (Manager.Camera.transform.position); // update culling group position
		}
		private void OnCullingChanged(CullingGroupEvent Event){
			if (Patches != null) {// update LOD
				GNS_Patch Patch = Patches[Event.index];
				if (Event.currentDistance < Manager.LODDistances.Count) {
					Patch.LODIndex = Event.currentDistance;
				}

				OnPatchCullingChanged (Event);

				Patch.PreviousLODIndex = Patch.LODIndex;
			}
		}
		private void DestroyCulling(){
			if (_CullingGroup != null) {
				_CullingGroup.Dispose ();
				_CullingGroup = null;
			}
		}

		// Patch voids
		private int[] VisiblePatches(out int ResultCount){
			int[] ResultIndices = new int[Patches.Length];
			ResultCount = _CullingGroup.QueryIndices(true,ResultIndices,0);

			// remove patches that should not be renderd
			return RemoveShouldNotRender(ResultIndices);
		}
		private int[] VisiblePatches(out int ResultCount,int LODIndex){
			int[] ResultIndices = new int[Patches.Length];
			ResultCount = _CullingGroup.QueryIndices(true,LODIndex,ResultIndices,0);

			// remove patches that should not be renderd
			return RemoveShouldNotRender(ResultIndices);
		}
		private int[] RemoveShouldNotRender(int[] InputArray){
			GNS_Patch[] PatchesCache = Patches;
			int Count = 0;
			for (int i = 0; i < InputArray.Length; i++) {
				if (PatchesCache [InputArray[i]].ShouldRenderPatch()) {
					Count++;
				}
			}

			int[] UpdatedList = new int[Count];
			int Index = 0;
			for (int i = 0; i < InputArray.Length; i++) {
				if (PatchesCache [InputArray[i]].ShouldRenderPatch()) {
					UpdatedList [Index] = InputArray[i];
					Index++;
				}
			}

			return UpdatedList;
		}
		private void ForEachVisiblePatch(System.Action<GNS_Patch> OnPatch){
			int NumResults = 0;
			int[] ResultIndices = VisiblePatches (out NumResults);
			GNS_Patch[] PatchesCache = Patches;

			// Find all visible spheres
			NumResults = _CullingGroup.QueryIndices(true,ResultIndices,0);

			// Loop
			for (int i = 0; i < NumResults; i++) {
				OnPatch.Invoke (PatchesCache[ResultIndices[i]]); // invoke patch
			}
		}
		#endregion

		#endregion

		#region Protected voids
		// Culling
		protected int[][] CurrentLODMatrices(){
			int[][] LODMatrices = new int[Manager.LODDistances.Count][];

			for (int i = 0; i < Manager.LODDistances.Count; i++) {
				int PatchCount = 0;
				int[] PatchIndexes = VisiblePatches (out PatchCount,i);
				int[] LODMatrix = new int[PatchIndexes.Length];

				for (int x = 0; x < PatchIndexes.Length; x++) {
					LODMatrix [x] = PatchIndexes [x];
				}

				LODMatrices[i] = LODMatrix; // set matrix
			}

			return LODMatrices;
		}
		#endregion
	}
}
