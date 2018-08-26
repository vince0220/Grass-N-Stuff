using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS.Core;
using System.Runtime.InteropServices;

namespace GNS{
	public class GrassNStuff : MonoBehaviour {
		#region Inspector variables
		[Header("Global Settings")]
		[SerializeField]private Terrain _Terrain;
		[SerializeField]private Camera _camera;
			
		[Header("Layer Settings")]
		[SerializeField]private List<GNS_Layer> _Layers = new List<GNS_Layer>();

		[Header("Performance Settings")]
		[SerializeField]private TextureResolution _TransformResolution = TextureResolution.Normal;
		[SerializeField]private PatchResolution _PaintResolution = PatchResolution.Normal;
		[SerializeField]private UnityEngine.Rendering.ShadowCastingMode _Shadows;
		[SerializeField][Range(0f,50f)]private float _PatchSize = 10f;
		[SerializeField]private List<Vector2> _LODDistances = new List<Vector2> ();
		[SerializeField]private float _LODFadeDistance = 50;
		[SerializeField]private RenderPath _RenderPath = RenderPath.ObjectRendering;
		[SerializeField]private bool _CrossFade = true;
		[SerializeField]private float _CrossFadeDuration = 0.5f;

		[Header("Wind Settings")]
		[SerializeField]private TextureResolution _WindQuality = TextureResolution.Normal;
		[SerializeField]private float _WindSpeed = 5f;
		[SerializeField]private float _WindStrength = 1f;
		[SerializeField]private float _WindNoiseAmplitude = 0.5f;
		[SerializeField]private float _WindNoiseFrequency = 0.5f;

		[Header("Editor Settings")]
		[SerializeField]private Color _GridColor = new Color32(93,208,238,76);
		[SerializeField]private bool _Debug = true;
		#endregion

		#region Constant variables
		public const int MaxEditorGridCount = 100000;
		public const int MaxMeshVertexCount = 65534;
		public const int MaxPatchDrawCount = 1023;
		public const string KernelShader = "Hidden/GrassNStuff/GNS_Kernel";
		public const string GrassShader = "GrassNStuff/GNS_Shader";
		#endregion

		#region Private variables
		// Cache variables
		private GNS_BulkMesh[][] _LODLayerBulkMeshes;
		private Material _GNSKernel;
		private Material[] _LayerMaterials;
		private MaterialPropertyBlock[] _PropertyBlocks;
		private float _WindTime = 0f;
		private GNS_IRenderer _Renderer;
		private Color[] _CacheHeightMap;

		// Buffers
		private RenderTexture[] _PositionBuffers;
		private RenderTexture[] _RotationBuffers;
		private RenderTexture[] _ScaleBuffers;
		private RenderTexture[] _NoiseBuffers;
		private RenderTexture _HeightBuffer;
		private RenderTexture _WindBuffer;
		private Texture2D _TerrainHeightBuffer;
		#endregion

		#region Base voids
		private void Awake(){
			if (_camera == null) {AutoFillCamera ();} // auto fill cam if needed
			ResortLOD (); // resort lod distances
			RegenerateLODMeshes (); // generate layer meshes
			RegenerateTransformTextures (); // generate transform textures
            UpdateHeightMap(); // update height map data
			UpdateWindValues(); // update wind

			UpdatePatchCullingGroups ();
			InitializeRenderer ();
        }
		private void Update(){
			Renderer.Render ();
		}
		private void OnDestroy(){
			Renderer.Destroy ();
		}
		#endregion

		#region Editor voids
		// Public
		public void NotifyPatchesChanged(){
			Renderer.Initialize ();
		}
		public void ResortLOD(){
			_LODDistances.Sort ((p1,p2)=>p1.x.CompareTo(p2.x));
		}

		// private voids
		private void OnDrawGizmos(){
			Renderer.DrawGizmos ();
		}
		#endregion

		#region Public voids
		// Generation
		public void RegenerateTransformTextures(){
			if (Application.isPlaying) { // only generate if application is playing
				RegeneratePositionTextures(false);
				RegenerateRotationTextures (false);
				RegenerateScaleTextures (false);
				RegenerateWindTexture (false);

				// Apply buffers to materials
				UpdateMaterialBuffers ();
			}
		}
		public void RegenerateWindTexture(bool UpdateMaterial = true){
			if (Application.isPlaying) { // only generate if application is playing
				if (_WindBuffer == null) {_WindBuffer = CreateBuffer ((int)WindQuality);}

				SetKernelWindSettings ();
				Graphics.Blit (null, _WindBuffer, GNSKernel, 3); // calculate position buffer

				if (UpdateMaterial) {
					// Apply buffers to materials
					UpdateMaterialBuffers ();
				}
			}
		}
		public void RegeneratePositionTextures(bool UpdateMaterial = true){
			if (Application.isPlaying) { // only generate if application is playing
				if (_PositionBuffers == null) {_PositionBuffers = new RenderTexture[Layers.Count];}

				for (int i = 0; i < Layers.Count; i++) {
					SetKernelSettings (Layers [i]); // set Kernel settings

					if (_PositionBuffers [i] == null) {_PositionBuffers [i] = CreateBuffer ((int)TransformResolution);} // create position buffer
					Graphics.Blit (null, _PositionBuffers [i], GNSKernel, 0); // calculate position buffer
				}

				if (UpdateMaterial) {
					// Apply buffers to materials
					UpdateMaterialBuffers ();
				}
			}
		}
		public void RegenerateRotationTextures(bool UpdateMaterial = true){
			if (Application.isPlaying) { // only generate if application is playing
				if (_RotationBuffers == null) {
					
					_RotationBuffers = new RenderTexture[Layers.Count];}

				for (int i = 0; i < Layers.Count; i++) {
					SetKernelSettings (Layers [i]); // set Kernel settings

					if (_RotationBuffers [i] == null) {_RotationBuffers [i] = CreateBuffer ((int)TransformResolution);} // create rotation buffer

					Graphics.Blit (null, _RotationBuffers [i], GNSKernel, 1); // calculate rotation buffer
				}

				if (UpdateMaterial) {
					// Apply buffers to materials
					UpdateMaterialBuffers ();
				}
			}
		}
		public void RegenerateScaleTextures(bool UpdateMaterial = true){
			if (Application.isPlaying) { // only generate if application is playing
				if (_ScaleBuffers == null) {_ScaleBuffers = new RenderTexture[Layers.Count];}
				if (_NoiseBuffers == null) {_NoiseBuffers = new RenderTexture[Layers.Count];}

				for (int i = 0; i < Layers.Count; i++) {
					SetKernelSettings (Layers [i]); // set Kernel settings

					if (_ScaleBuffers [i] == null) {_ScaleBuffers [i] = CreateBuffer ((int)TransformResolution);} // create scale buffer
					if (_NoiseBuffers [i] == null) {_NoiseBuffers [i] = CreateBuffer ((int)TransformResolution);} // create noise buffer

					Graphics.Blit (null, _ScaleBuffers [i], GNSKernel, 2); // calculate scale buffer
					Graphics.Blit (null, _NoiseBuffers [i], GNSKernel, 3); // calculate noise buffer
				}

				if (UpdateMaterial) {
					// Apply buffers to materials
					UpdateMaterialBuffers ();
				}
			}
		}

		// Update voids
		public void UpdateHeightMap(bool MapOnly = false)
        {
            int TextureWidth = Terrain.terrainData.heightmapWidth;
            int TextureHeight = Terrain.terrainData.heightmapHeight;
			float[,] pixels = Terrain.terrainData.GetHeights(0, 0, TextureWidth, TextureHeight);

            // convert to color
			if (_CacheHeightMap == null) { // init cache
				_CacheHeightMap = new Color[TextureWidth * TextureHeight];
			}

            int Index = 0;
            for(int x = 0; x < TextureWidth; x++)
            {
                for(int y = 0; y < TextureHeight; y++)
                {
					_CacheHeightMap[Index].r = pixels[x,y];
                    Index++;
                }
            }

            // generate texture
			if (_TerrainHeightBuffer == null) {
				_TerrainHeightBuffer = new Texture2D (TextureWidth, TextureHeight, TextureFormat.RGBAFloat, false, true);
				_TerrainHeightBuffer.wrapMode = TextureWrapMode.Clamp;
				_TerrainHeightBuffer.filterMode = FilterMode.Point;
			}
			_TerrainHeightBuffer.SetPixels(_CacheHeightMap);
            _TerrainHeightBuffer.Apply();

			UpdateMaterialBuffers ();
			if (!MapOnly) {UpdatePatchCullingGroups ();}
        }
		public void UpdateWindValues(){
			for (int i = 0; i < LayerMaterials.Length; i++) {
				LayerMaterials [i].SetFloat ("_WindSpeed",_WindSpeed); // set wind speed
				LayerMaterials [i].SetFloat ("_WindStrength",_WindStrength); // set wind strength
			}
		}
		public void UpdateMaterialBuffers(){
			for (int i = 0; i < LayerMaterials.Length; i++) {
				// Set material settings
				LayerMaterials [i].SetColor("_MainColor",Layers[i].Color); // set color
				LayerMaterials [i].SetTexture ("_MainTex",Layers[i].Albedo); // set albedo
				LayerMaterials [i].SetTexture ("_Normal",Layers[i].Normal); // set albedo
				LayerMaterials [i].SetTexture ("_SpecGloss",Layers[i].SpecGloss); // set SpecGloss
				LayerMaterials [i].SetTexture ("_WindBuffer",_WindBuffer); // set wind buffer
				LayerMaterials [i].SetFloat ("_GlossStrength",Layers[i].GlossStrength); // set gloss strength
				LayerMaterials [i].SetFloat ("_SpecularStrength",Layers[i].SpecularStrength); // set specular strength
				LayerMaterials [i].SetFloat ("_Cutoff",Layers[i].CutOff); // set specular strength
				LayerMaterials [i].SetFloat ("_OcclusionStrength",Layers[i].OcclusionStrength); // set occlusion strength
				LayerMaterials [i].SetFloat ("_OcclusionHeight",Layers[i].OcclusionHeight); // set occlusion height
				LayerMaterials [i].SetFloat ("_OcclusionToColor",Layers[i].OcclusionToColor); // set occlusion height
                LayerMaterials [i].SetTexture ("_HeightBuffer",_TerrainHeightBuffer); // set terrain height
				LayerMaterials [i].SetInt("_HeightMapResolution",_Terrain.terrainData.heightmapResolution - 1); // set heightmap resolution

				// Set property settings
				LayerMaterials [i].SetTexture ("_PositionBuffer",_PositionBuffers[i]); // set position buffer
				LayerMaterials [i].SetTexture ("_RotationBuffer",_RotationBuffers[i]); // set rotation buffer
				LayerMaterials [i].SetTexture ("_ScaleBuffer",_ScaleBuffers[i]); // set scale buffer
				LayerMaterials [i].SetTexture ("_NoiseBuffer",_NoiseBuffers[i]); // set noice buffer

				LayerMaterials [i].SetVector ("_BoundSize",BoundSize); // set bound size
				LayerMaterials [i].SetVector ("_BoundPosition",Terrain.transform.position); // set bound position
				LayerMaterials [i].SetVector ("_PatchSize",PatchRounded); // set patch size

				LayerMaterials [i].SetFloat ("_FadeDistance",LODFadeDistance);
				LayerMaterials [i].SetFloat ("_FadeLength",Mathf.Max(MaxLODDistance - LODFadeDistance,0f));
			}
		}
		
		// Paint voids
		public void PaintLayer(int Layer,Vector2 Point, Vector2 Bounds,bool Visible,Texture2D BrushTexture = null,bool UpdateVisibility = true){
			if (Layer < Layers.Count) { // check if layer is in layer array
				Vector2 RelativePos = Point - new Vector2 (Terrain.transform.position.x, Terrain.transform.position.z);
				Vector2 Min = RelativePos - (Bounds * 0.5f);

				Renderer.Paint (Layer, Min, Bounds, Visible, BrushTexture);
			}
		}
		public void PaintLayer(int Layer, bool Visible, Texture2D BrushTexture = null,bool UpdateVisibility = true){
			Vector3 Pos = this.Terrain.transform.position;
			Vector3 Size = this.Terrain.terrainData.size;
			Vector3 Center = Pos + (Size * 0.5f);

			PaintLayer (Layer,new Vector2(Center.x,Center.z),new Vector2(Size.x,Size.z),Visible,BrushTexture,UpdateVisibility);
		}
		#endregion

		#region Private voids
		// Generation voids
		private void RegenerateLODMeshes(){
			_LODLayerBulkMeshes = new GNS_BulkMesh[_Layers.Count][];
			for (int i = 0; i < _Layers.Count; i++) {
				_LODLayerBulkMeshes [i] = _Layers [i].Meshes (this); // generate bulk meshes
			}
		}

		// Update voids
		private void UpdatePatchCullingGroups(){
			if (Application.isPlaying) { // only generate if application is playing
				InitializeRenderer();

				// Cache values
				Vector3 _CacheBoundSize = BoundSize;
				Vector3 _TerrainPosition = _Terrain.transform.position;

				for (int i = 0; i < Layers.Count; i++) {
					SetKernelSettings(Layers[i]); // update kernel settings
					if (_HeightBuffer == null) {_HeightBuffer = CreateBuffer ((int)PatchCount.x,(int)PatchCount.y);}
					GNSKernel.SetTexture("_PaintTexture",_TerrainHeightBuffer); // set texture paint buffer

					Graphics.Blit (null, _HeightBuffer, GNSKernel, 4); // Update paint buffer

					Texture2D Tex = _HeightBuffer.ToTexture2D (); // Convert Buffer to texture
					Color[] Colors = Tex.GetPixels ();

					for(int x = 0; x < Colors.Length; x++){
						Renderer.UpdatePatchHeight (x,(_CacheBoundSize.y * Colors [x].r)+_TerrainPosition.y);
					}
				}
			}
		}

		// Initialization / destroy voids
		private void AutoFillCamera(){
			_camera = Camera.main;
		}
		private void InitializeRenderer(){
			if (_Renderer == null) {
				_Renderer = new GNS.Renderers.GNS_ObjectRenderer (this,RenderData);
				_Renderer.Initialize ();
			}
		}

		// Set voids
		public void SetKernelSettings(GNS_Layer Layer){
			// Rotation
			GNSKernel.SetFloat("_RandomPitch", Layer.RandomPitchAngle * Mathf.Deg2Rad); // set rotation pitch
			GNSKernel.SetVector("_RotationNoise", new Vector3(Layer.RotationNoiseFrequency, Layer.NoisePitchAngle * Mathf.Deg2Rad,0f)); // set rotation noice
			GNSKernel.SetVector("_RotationAxis", Layer.RotationNoiseAxis.normalized); // set rotation axis

			// Scale
			GNSKernel.SetVector("_BaseScale", Layer.BaseScale);
			GNSKernel.SetVector("_RandomScale", new Vector2(Layer.MinRandomScale, Layer.MaxRandomScale));
			GNSKernel.SetVector("_ScaleNoise", new Vector2(Layer.ScaleNoiseFrequency, Layer.ScaleNoiseAmplitude));

			// Paint
			GNSKernel.SetFloat("_PaintResolution",(float)(int)PaintResolution);
			GNSKernel.SetVector("_PaintPatchSize",new Vector4(PatchRounded.x,PatchRounded.y));
			GNSKernel.SetVector ("_TerrainSize",new Vector4(_Terrain.terrainData.size.x,_Terrain.terrainData.size.y,_Terrain.terrainData.size.z,0));
		}
		private void SetKernelWindSettings(){
			GNSKernel.SetVector("_BaseScale", new Vector3(1,1,1));
			GNSKernel.SetVector("_ScaleNoise", new Vector2(_WindNoiseFrequency, _WindNoiseAmplitude));
		}

		// calulate
		private int CalculateMaxDrawCalls(float Distance){
			Distance = Distance * 2f;
			float Fit = Distance / Mathf.Max (PatchRounded.x,PatchRounded.y);
			Fit = Fit * Fit;
			return Mathf.Clamp((int)Fit,0,TotalPatchCount);
		}

		// Creation voids
		private Material CreateMaterial(Shader shader)
		{
			var material = new Material(shader);
			material.hideFlags = HideFlags.DontSave;
			return material;
		}
		private Texture2D CreateTextureBuffer(int resolution){
			var buffer = new Texture2D(resolution,resolution,TextureFormat.ARGB32,false,true);
			buffer.hideFlags = HideFlags.DontSave;
			buffer.filterMode = FilterMode.Point;
			buffer.wrapMode = TextureWrapMode.Repeat;
			return buffer;
		}
		private RenderTexture CreateBuffer(int resolution)
		{
			return CreateBuffer (resolution,resolution);
		}
		private RenderTexture CreateBuffer(int width,int height)
		{
			var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
			buffer.filterMode = FilterMode.Point;
			buffer.wrapMode = TextureWrapMode.Repeat;
			return buffer;
		}
		private RenderTexture CreateTempBuffer(int width, int height){
			var buffer = RenderTexture.GetTemporary (width,height,0,RenderTextureFormat.ARGBFloat);
			buffer.filterMode = FilterMode.Point;
			buffer.wrapMode = TextureWrapMode.Repeat;
			return buffer;
		}
		#endregion

		#region Get / Set
		// Public get / set
		public List<GNS_Layer> Layers{
			get{
				return _Layers;
			}
		}
		public List<Vector2> LODDistances{
			get{
				return _LODDistances;
			}
		}
		public Terrain Terrain{
			get{
				if (_Terrain == null) {_Terrain = this.GetComponent<Terrain> ();} // first terrain check
				if (_Terrain == null) {_Terrain = GameObject.FindObjectOfType<Terrain>();} // second terrain check
				return _Terrain;
			}
			set{
				_Terrain = value;
			}
		}
		public Camera Camera{
			get{
				return _camera;
			}
			set{
				_camera = value;
			}
		}
		public float[] LODFloatDistances{
			get{
				float[] Distances = new float[LODDistances.Count];

				for (int i = 0; i < Distances.Length; i++) {
					Distances [i] = LODDistances [i].x;
				}
				return Distances;
			}
		}
		public float PatchSize{
			get{
				return _PatchSize;
			}
			set{
				_PatchSize = value;
			}
		}
		public Color GridColor{
			get{
				return _GridColor;
			}
			set{
				_GridColor = value;
			}
		}
		public int MaxPatchRenderCount{
			get{
				if (LODDistances != null && LODDistances.Count > 0) {
					List<Vector2> TempVecs = new List<Vector2> (LODDistances);
					TempVecs.Sort ((p1, p2) => p1.x.CompareTo (p2.x));

					float Distance = TempVecs [TempVecs.Count - 1].x;
					return CalculateMaxDrawCalls (Distance);
				}
				return 0;
			}
		}
		public int EstimatedDrawCalls{
			get{
				if (LODDistances != null && LODDistances.Count > 0 && Layers != null && Layers.Count > 0) {
					int LODLevels = LODDistances.Count;

					for (int i = 0; i < LODDistances.Count; i++) {
						int Fit = (int)(CalculateMaxDrawCalls (LODDistances[i].x) / 1023f);
						if (Fit > 0) {
							LODLevels += Fit;
						}
					}
					return LODLevels * Layers.Count;
				}
				return 0;
			}
		}
		public UnityEngine.Rendering.ShadowCastingMode Shadows{
			get{
				return _Shadows;
			}
			set{
				_Shadows = value;
			}
		}
		public TextureResolution TransformResolution{
			get{
				return _TransformResolution;
			}
			set{
				_TransformResolution = value;
			}
		}
		public PatchResolution PaintResolution{
			get{
				return _PaintResolution;
			}
			set{
				_PaintResolution = value;
			}
		}
		public Material GNSKernel{
			get{
				if (_GNSKernel == null) {
					_GNSKernel = CreateMaterial(Shader.Find(KernelShader));
				}
				return _GNSKernel;
			}
		}
		public Material[] LayerMaterials{
			get{
				if (_LayerMaterials == null) {
					_LayerMaterials = new Material[Layers.Count];
					for (int i = 0; i < _LayerMaterials.Length; i++) {
						_LayerMaterials [i] = CreateMaterial (Shader.Find(GrassShader));
					}
				}
				return _LayerMaterials;
			}
		}
		public MaterialPropertyBlock[] PropertyBlocks{
			get{
				if (_PropertyBlocks == null) {
					_PropertyBlocks = new MaterialPropertyBlock[Layers.Count];
					for (int i = 0; i < _PropertyBlocks.Length; i++) {
						_PropertyBlocks [i] = new MaterialPropertyBlock();
					}
				}
				return _PropertyBlocks;
			}
		}
		public Vector3 GridCenter{
			get{
				return transform.position + (BoundSize * 0.5f);
			}
		}
		public int TotalPatchCount{
			get{
				return (int)(PatchCount.x * PatchCount.y);
			}
		}
		public Vector2 PatchCount{
			get{
				return new Vector2 (
					(int)(BoundSize.x / _PatchSize),
					(int)(BoundSize.z / _PatchSize)
				);
			}
		}
		public Vector3 BoundSize{
			get{
				Terrain CurrentTerrain = Terrain;
				if (CurrentTerrain != null) {
					return CurrentTerrain.terrainData.size;
				}
				return Vector3.zero;
			}
		}
		public Vector2 PatchRounded{
			get{
				Vector2 Count = PatchCount;
				return new Vector2 (
					BoundSize.x / Count.x,
					BoundSize.z / Count.y
				);
			}
		}
		public bool Debug{
			get{
				return _Debug;
			}
			set{
				_Debug = value;
			}
		}
		public float WindSpeed{
			get{
				return _WindSpeed;
			}
			set{
				this._WindSpeed = value;
			}
		}
		public float WindStrength{
			get{
				return _WindStrength;
			}
			set{
				this._WindStrength = value;
			}
		}
		public float WindNoiseFrequency{
			get{
				return _WindNoiseFrequency;
			}
			set{
				_WindNoiseFrequency = value;
			}
		}
		public float WindNoiseAmplitude{
			get{
				return _WindNoiseAmplitude;
			}
			set{
				_WindNoiseAmplitude = value;
			}
		}
		public TextureResolution WindQuality{
			get{
				return _WindQuality;
			}
			set{
				_WindQuality = value;
			}
		}
		public float LODFadeDistance{
			get{
				return _LODFadeDistance;
			}
			set{
				_LODFadeDistance = value;
			}
		}
		public float MaxLODDistance{
			get{
				if (LODDistances != null && LODDistances.Count > 0) {
					float Distances = LODDistances [0].x;

					for (int i = 1; i < LODDistances.Count; i++) {
						if (Distances < LODDistances [i].x) {
							Distances = LODDistances [i].x;
						}
					}

					return Distances;
				}
				return 0f;
			}
		}
		public bool CrossFade{
			get{
				return _CrossFade;
			}
			set{
				_CrossFade = value;
			}
		}
		public float CrossFadeDuration{
			get{
				return _CrossFadeDuration;
			}
			set{
				_CrossFadeDuration = value;
			}
		}

		// Private
		private GNS_IRenderer Renderer{
			get{
				if (_Renderer == null) {
					InitializeRenderer ();
				}
				return _Renderer;
			}
		}
		private GNS_Data RenderData{
			get{
				GNS_Data Data = new GNS_Data ();
				Data.LayerMaterials = LayerMaterials;
				Data.LODLayerBulkMeshes = _LODLayerBulkMeshes;
				Data.PropertyBlocks = PropertyBlocks;

				return Data;
			}
		}
		#endregion

		#region Enums
		public enum TextureResolution{
			VeryLow = 256,
			Low = 512,
			Normal = 1024,
			High = 2048,
			VeryHigh = 4096,
			Ultra = 8192
		}
		public enum PatchResolution{
			VeryLow = 16,
			Low = 32,
			Normal = 64,
			High = 128,
			VeryHigh = 256,
			Ultra = 512
		}
		public enum RenderPath{
			ObjectRendering,
			GraphicsDraw
		}
		#endregion
	}
}

/* REQUIREMENTS
 * - LOD
 * - CULLING
 * - LAYERS
 * 		- A SINGLE LAYER CAN HAVE MULTIPLE MESHES
 * 		- EVERY LAYER HAS ITS OWN VISIBILITY TEXTURE
 * 		- EVERY MESH IN A LAYER SHOULD HAVE ITS OWN DENSITY SETTINGS
 * - PREVENTION REPEATED PATTERNS
 * 		- RANDOM ROTATION FOR EVERY PATCH (IN STEPS OF 90)
 * 		- PERLIN NOICE OVER WHOLE TERRAIN TO OFFSET SCALE AND ROTATION / WIND
 * - WIND
 * - INSTANCING
 * - HEIGHT MAP ADDAPTION
 * - RUN TIME EDITABLE
 * - EASY TO USE AND SETUP
 */