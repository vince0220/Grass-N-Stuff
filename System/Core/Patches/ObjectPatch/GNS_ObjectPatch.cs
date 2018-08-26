using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS;
using GNS.Renderers;
using GNS.Core;

namespace GNS.Renderers.Patches{
	public class GNS_ObjectPatch : GNS_Patch {
		#region Private variables
		private GNS_Object[] LayerPatches;
		private MonoBehaviour Mono;
		//private MaterialPropertyBlock[] PropertyBlocks;
		private IEnumerator[] CrossFaders;
		private bool CrossFade;
		private float CrossFadeDuration;
		private GrassNStuff Manager;
		#endregion

		#region Constructor
		public GNS_ObjectPatch(Vector3 RelativePosition,int LayerCount,MonoBehaviour Mono,bool CrossFade, float CrossFadeDuration):base(RelativePosition,LayerCount){
			this.Mono = Mono;
			this.CrossFade = CrossFade;
			this.CrossFadeDuration = CrossFadeDuration;
		}
		#endregion

		#region Public voids
		public void InitializeObjectPatch(GrassNStuff Manager,Vector3 ParentPosition,Vector2 PatchSize,GNS_BulkMesh[][] Meshes,Material[] LayerMaterials,UnityEngine.Rendering.ShadowCastingMode Shadows){
			this.Manager = Manager;
			LayerPatches = new GNS_Object[Meshes.Length];
			CrossFaders = new IEnumerator[Meshes[0].Length];

			for (int x = 0; x < Meshes.Length; x++) {
				LayerPatches [x] = new GNS_Object (RelativePosition,ParentPosition,PatchSize,Meshes[x],LayerMaterials[x],Shadows,(int)Manager.PaintResolution);
			}
		}
		public void UpdatePatchState(CullingGroupEvent Event){
			for (int y = 0; y < LayerPatches [0].LODRenderers.Length; y++) {
				if ((y != LODIndex && LayerPatches [0].LODRenderers [y].enabled) || (y == LODIndex) || !Event.isVisible) {
					SetLODState (y, (ShouldRender [0] && y == LODIndex && Event.isVisible),(Event.hasBecomeVisible || Event.hasBecomeInvisible));
				}
			}
		}
		public void SetLODState(int LOD,bool Enabled,bool force = false){
			if (CrossFade && !force) {
				StopCrossFade (LOD); // stop previous fade
				CrossFaders [LOD] = CrossFadeLerp (LOD, Enabled, CrossFadeDuration);
				Mono.StartCoroutine (CrossFaders [LOD]); // fade
			} else {
				for (int x = 0; x < LayerPatches.Length; x++) {
					MaterialPropertyBlock Block = LayerPatches[x].Blocks[LOD];
					Block.SetFloat ("_CrossFade",1f);

					if (ShouldRender [x]) {
						LayerPatches [x].LODRenderers [LOD].SetPropertyBlock (Block);
					}

					LayerPatches [x].LODRenderers [LOD].enabled = Enabled;
				}
			}
		}
		public void DrawGizmosPatch(){
			
		}

		// Edits
		public void Paint(int Layer,Vector2 point, Vector2 bound, bool visible, Texture2D BrushTexture = null){
			LayerPatches [Layer].PaintLayer (Manager,Manager.Layers[Layer],point,bound,visible,BrushTexture);
			LayerPatches [Layer].UpdateVisibility (Manager);
		}
		#endregion

		#region Virtuals
		protected override void OnUpdatePatchHeight (float Height)
		{
			for (int i = 0; i < LayerPatches.Length; i++) {
				Vector3 Pos = LayerPatches [i].CachePosition;
				Pos.y = Height;

				for (int x = 0; x < LayerPatches [i].LODCacheTransform.Length; x++) {
					LayerPatches [i].LODCacheTransform [x].position = Pos;
				}
			}
		}
		#endregion

		#region IEnumerators
		private void StopCrossFade(int index){
			if (CrossFaders [index] != null) {
				Mono.StopCoroutine (CrossFaders [index]);
				CrossFaders [index] = null;
			}
		}
		private IEnumerator CrossFadeLerp(int index,bool Enabled,float FadeTime){
			for (int x = 0; x < LayerPatches.Length; x++) {
				float LerpTime = FadeTime;
				MaterialPropertyBlock Block = LayerPatches [x].Blocks[index];

				float From = Block.GetFloat("_CrossFade");
				float To = (Enabled) ? 1f : 0.401f;

				if (ShouldRender [x]) {
					LayerPatches [x].LODRenderers [index].enabled = true;
				}

				while (LerpTime > 0f) {
					LerpTime -= 1f * Time.fixedDeltaTime;

					float Opacity = FastLerp (From,To,1f - (LerpTime / FadeTime));

					// Set property block
					Block.SetFloat ("_CrossFade",Opacity);

					// Update renderers
					if (ShouldRender [x]) {
						LayerPatches [x].LODRenderers [index].SetPropertyBlock (Block);
					}

					yield return null;
				}
			}

			// Done lerping
			for (int x = 0; x < LayerPatches.Length; x++) {
				if (ShouldRender [x]) {
					LayerPatches [x].LODRenderers [index].enabled = Enabled;
				}
			}

			StopCrossFade(index); // stop previous fade
		}
		#endregion

		#region Private voids
		private float FastLerp(float From, float To, float T){
			return From + (To - From) * T;
		}
		#endregion
	}
}
