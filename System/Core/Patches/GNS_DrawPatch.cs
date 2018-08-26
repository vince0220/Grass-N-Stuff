using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNS;
using GNS.Renderers;
using GNS.Core;

namespace GNS.Renderers.Patches{
	public class GNS_DrawPatch : GNS_Patch {
		#region Variables
		public Matrix4x4 TransformMatrix;
		#endregion

		#region Constructor
		public GNS_DrawPatch(Vector3 RelativePosition,int LayerCount):base(RelativePosition,LayerCount){

		}
		#endregion

		#region Editor
		public void GizmosDrawPatch(Vector3 BoundPosition,Vector2 PatchSize,bool Wire = true){
			if (Wire) {
				Gizmos.DrawWireCube (BoundPosition + RelativePosition, new Vector3 (PatchSize.x, 0f, PatchSize.y));
			} else {
				Gizmos.DrawCube (BoundPosition + RelativePosition, new Vector3 (PatchSize.x, 0f, PatchSize.y));
			}
		}
		#endregion
		
		#region Public voids
		public void InitializeMatrix(Vector3 ParentPosition){
			this.TransformMatrix = Matrix4x4.TRS (
				ParentPosition + RelativePosition, // matrix position
				Quaternion.identity, // matrix rotation
				Vector3.one // matrix scale
			);
		}
		#endregion
	}
}
