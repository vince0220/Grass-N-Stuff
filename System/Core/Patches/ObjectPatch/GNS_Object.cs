using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS;
using GNS.Core;
using GNS.Renderers.Patches;

namespace GNS.Renderers{
	public class GNS_Object{
		#region public variables
		public MeshRenderer[] LODRenderers;
		public Transform[] LODCacheTransform;
		public Vector3 CachePosition;
		public float BaseHeight;
		public MaterialPropertyBlock[] Blocks;
		#endregion

		#region Private variables
		private RenderTexture PaintBuffer;
		private int PaintResolution;
		private Vector3 MinOffset;
		#endregion

		#region Constructor
		public GNS_Object(Vector3 RelativePosition,Vector3 ParentPosition,Vector2 PatchSize,GNS_BulkMesh[] LODS, Material Material,UnityEngine.Rendering.ShadowCastingMode Shadows, int PaintResolution){
			this.PaintResolution = PaintResolution;
			this.Blocks = new MaterialPropertyBlock[LODS.Length];
			for (int i = 0; i < Blocks.Length; i++) {
				Blocks [i] = new MaterialPropertyBlock ();
			}
			InitializeLODS (RelativePosition,ParentPosition,PatchSize,LODS,Material,Shadows);
		}
		#endregion

		#region Public voids
		public void PaintLayer(GrassNStuff Manager,GNS_Layer Layer,Vector2 Point, Vector2 Bounds,bool Visible, Texture2D BrushTexture = null){
			if (BrushTexture == null) { // if null
				BrushTexture = new Texture2D (1, 1);
				BrushTexture.SetPixel (0,0,Color.white);
				BrushTexture.Apply ();
			}

			int TargetStrength = (Visible) ? 1 : 0; // get target strength
			if(PaintBuffer == null){PaintBuffer = CreateTempBuffer(PaintResolution);}
			RenderTexture TempBuffer = CreateTempBuffer (PaintResolution);// check if layer is null

			// Set kernel settings
			Manager.SetKernelSettings(Layer);

			Material Kernel = Manager.GNSKernel;
			Kernel.SetTexture("_PaintTexture",PaintBuffer); // set texture paint buffer
			Kernel.SetVector("_PaintMin",new Vector4(Point.x,Point.y,0,0)); // set min
			Kernel.SetVector("_PaintArea",new Vector4(Bounds.x,Bounds.y,0,0)); // set max
			Kernel.SetTexture ("_PaintBrush",BrushTexture);
			Kernel.SetFloat ("_PaintTarget",TargetStrength);
			Kernel.SetVector ("_CurrentPatchMin",(CachePosition - Manager.Terrain.transform.position) - MinOffset);

			Graphics.Blit (null, TempBuffer, Manager.GNSKernel, 5); // Update paint buffer

			if(PaintBuffer != null){PaintBuffer.Release ();}
			PaintBuffer = TempBuffer;
			UpdateBlocks (); // update materials
		}
		public void UpdateVisibility(GrassNStuff Manager){
			
		}
		#endregion

		#region Private voids
		private void UpdateBlocks(){
			for (int i = 0; i < LODRenderers.Length; i++) {
				Blocks[i].SetTexture("_PaintBuffer",PaintBuffer);
				LODRenderers [i].SetPropertyBlock (Blocks[i]);
			}
		}
		private void InitializeLODS(Vector3 RelativePosition,Vector3 ParentPosition,Vector2 PatchSize,GNS_BulkMesh[] LODS, Material Material,UnityEngine.Rendering.ShadowCastingMode Shadows){
			LODRenderers = new MeshRenderer[LODS.Length];
			LODCacheTransform = new Transform[LODS.Length];
			for (int i = 0; i < LODS.Length; i++) {
				LODRenderers [i] = InitializeMeshRenderer (RelativePosition,ParentPosition,PatchSize,LODS[i],Material,Shadows);
				LODCacheTransform [i] = LODRenderers [i].transform;
				LODRenderers [i].enabled = false;
			}

			MinOffset = new Vector3(PatchSize.x,0,PatchSize.y);
			CachePosition = LODCacheTransform[0].position; // cache position
			BaseHeight = LODRenderers[0].GetComponent<MeshFilter> ().sharedMesh.bounds.center.y; // get base height
		}
		private MeshRenderer InitializeMeshRenderer(Vector3 RelativePosition,Vector3 ParentPosition,Vector2 PatchSize,GNS_BulkMesh Mesh, Material Material,UnityEngine.Rendering.ShadowCastingMode Shadows){
			GameObject Obj = new GameObject ("Patch",typeof(MeshRenderer),typeof(MeshFilter));
			MeshRenderer Renderer = Obj.GetComponent<MeshRenderer> ();
			MeshFilter Filter = Obj.GetComponent<MeshFilter> ();
			Vector3 MeshSize = Mesh.BulkMesh.bounds.size;

			// Set settings
			Renderer.shadowCastingMode = Shadows;
			Obj.transform.position = ParentPosition + RelativePosition;
			Filter.sharedMesh = Mesh.BulkMesh;
			Renderer.sharedMaterial = Material;
			Obj.hideFlags = HideFlags.HideAndDontSave;

			// Set base property block
			MaterialPropertyBlock Block = new MaterialPropertyBlock();
			Block.SetFloat ("_CrossFade", 1f);
			Renderer.SetPropertyBlock (Block);

			return Renderer;
		}

		// Creation
		private RenderTexture CreateBuffer(int resolution)
		{
			return CreateBuffer (resolution,resolution);
		}
		private RenderTexture CreateBuffer(int width,int height)
		{
			var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.R8,RenderTextureReadWrite.Linear);
			buffer.filterMode = FilterMode.Point;
			buffer.wrapMode = TextureWrapMode.Repeat;
			return buffer;
		}
		private RenderTexture CreateTempBuffer(int resolution){
			return CreateTempBuffer (resolution,resolution);
		}
		private RenderTexture CreateTempBuffer(int width, int height){
			var buffer = RenderTexture.GetTemporary (width,height,0,RenderTextureFormat.R8,RenderTextureReadWrite.Linear);
			buffer.filterMode = FilterMode.Point;
			buffer.wrapMode = TextureWrapMode.Repeat;
			return buffer;
		}
		#endregion
	}
}
