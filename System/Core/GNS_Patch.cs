using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNS.Core{
	public class GNS_Patch  {
		#region Public variables
		public Vector3 RelativePosition;
		public int LODIndex = 0;
		public int PreviousLODIndex;
		public bool[] ShouldRender;
		#endregion

		#region Constructor
		public GNS_Patch(Vector3 RelativePosition,int LayerCount){
			this.RelativePosition = RelativePosition;
			this.ShouldRender = new bool[LayerCount];

			for (int i = 0; i < this.ShouldRender.Length; i++) {
				ShouldRender [i] = true;
			}
		}
		#endregion

		#region Public voids
		public bool ShouldRenderPatch(){
			for (int i = 0; i < ShouldRender.Length; i++) {
				if (ShouldRender [i]) {
					return true;
				}
			}
			return false;	
		}
		public void UpdatePatchHeight(float Height){
			OnUpdatePatchHeight (Height);
		}
		#endregion

		#region Virtual voids
		protected virtual void OnUpdatePatchHeight(float Height){}
		#endregion
	}
}
