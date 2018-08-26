using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNS.Core{
	public interface GNS_IRenderer {
		// Base voids
		void Render ();
		void Initialize ();
		void Destroy ();
		void DrawGizmos ();

		// Edits
		void UpdatePatchHeight(int Index,float Height);
		void UpdateVisibility(Vector2 Point, Vector2 Bounds);
		void Paint(int Layer,Vector2 Point, Vector2 Bounds,bool Visible,Texture2D BrushTexture = null);

		// Get Voids
		GNS_Patch GetPatch(int Index);
	}
}
