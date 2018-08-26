using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNS{
	public static class GNS_Extensions {
		public static Texture2D ToTexture2D(this RenderTexture rTex)
		{
			return ToTexture2D (rTex,new Vector2(0,0), new Vector2(rTex.width,rTex.height));
		}
		public static Texture2D ToTexture2D(this RenderTexture rTex,Vector2 samplePos, Vector2 sampleSize){
			Texture2D tex = new Texture2D((int)sampleSize.x, (int)sampleSize.y,TextureFormat.RGBAFloat, false,true);
			RenderTexture.active = rTex;
			tex.ReadPixels(new Rect(samplePos.x, samplePos.y, sampleSize.x, sampleSize.y), 0, 0,false);
			tex.Apply();
			return tex;
		}
		public static T[][] SplitArray<T>(ref T[] array,uint split){
			uint Remainder = (uint)(array.Length % split);
			uint RowCount = (uint)((float)(array.Length - Remainder) / (float)split);

			T[][] collection = new T[RowCount+1][];
			for(uint i = 0; i < collection.Length - 1; i++)collection[i] = new T[split];
			collection[collection.Length -1] = new T[Remainder];

			int currentArray = 0,index = 0;
			for(uint i = 0; i< array.Length; i++, index++){
				collection[currentArray][index] = array[i];
				if(index-(split-1)==0){currentArray++; index = -1;}
			}
			return collection;
		}
	}
}
