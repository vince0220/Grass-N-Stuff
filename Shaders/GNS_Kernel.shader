Shader "Hidden/GrassNStuff/GNS_Kernel"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    float _PaintResolution;
    fixed2 _PaintPatchSize;
    float4 _CurrentPatchMin;
    sampler2D _PaintTexture;
    sampler2D _PaintBrush;
    fixed2 _PaintMin;
    fixed2 _PaintArea;
    float _PaintTarget;
    float4 _TerrainSize;

    float2 _Scroll;

    float _RandomPitch;
    float3 _RotationNoise;  // freq, amp, time
    float3 _RotationAxis;

    float3 _BaseScale;
    float2 _RandomScale;    // min, max
    float2 _ScaleNoise;     // freq, amp
	
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, 0);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    float4 qmul(float4 q1, float4 q2)
    {
        return float4(
            q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
            q1.w * q2.w - dot(q1.xyz, q2.xyz)
        );
    }

    float2 get_point(float2 uv)
    {
        float2 p = float2(nrand(uv, 0), nrand(uv, 1));
        return (p - 0.5);
    }

    float2 get_point_offs(float2 uv, float2 offs)
    {
        float2 p = float2(nrand(uv, 0), nrand(uv, 1));
        return (frac(p + offs) - 0.5);
    }

    float4 random_yaw(float2 uv)
    {
        float a = (nrand(uv, 2) - 0.5) * UNITY_PI * 2;
        float sn, cs;
        sincos(a * 0.5, sn, cs);
        return float4(0, sn, 0, cs);
    }

    float4 random_pitch(float2 uv)
    {
        float a1 = (nrand(uv, 3) - 0.5) * UNITY_PI * 2;
        float a2 = (nrand(uv, 4) - 0.5) * _RandomPitch * 2;
        float sn1, cs1, sn2, cs2;
        sincos(a1 * 0.5, sn1, cs1);
        sincos(a2 * 0.5, sn2, cs2);
        return float4(float3(cs1, 0, sn1) * sn2, cs2);
    }

    float4 frag_position(v2f_img i) : SV_Target
    {
        float2 p = get_point_offs(i.uv, _Scroll);
        return float4(p.x, p.y, 0, 0);
    }

    float4 frag_rotation(v2f_img i) : SV_Target
    {
        float4 r1 = random_yaw(i.uv);
        float4 r2 = random_pitch(i.uv);

        float2 np = i.uv * _RotationNoise.x;
        float3 nc = float3(np.xy, _RotationNoise.z);
        float na = cnoise(nc) * _RotationNoise.y;

        float sn, cs;
        sincos(na * 0.5, sn, cs);
        float4 r3 = float4(_RotationAxis * sn, cs);

        return qmul(r3, qmul(r2, r1));
    }

    float4 frag_scale(v2f_img i) : SV_Target
    {
        float s1 = lerp(_RandomScale.x, _RandomScale.y, i.uv);
        return float4(_BaseScale * (s1), 0);
    }
    float4 frag_noice(v2f_img i) : SV_Target
    {
        float2 np = i.uv * _ScaleNoise.x;
        float3 nc = float3(np.xy, 0.92);
        float s2 = cnoise(nc) * _ScaleNoise.y;

        return float4(_BaseScale + s2,0);
    }


    float4 visibility_update(v2f_img i) : SV_Target{
    	return tex2D(_PaintTexture,i.uv).r;
    }

    float4 paint(v2f_img i):SV_Target{
    	fixed4 col = tex2D(_PaintTexture,i.uv);
    	fixed2 RelativeUV = fixed2(i.uv.x * _PaintPatchSize.x+_CurrentPatchMin.x,i.uv.y * _PaintPatchSize.y+_CurrentPatchMin.z);
    		
    	RelativeUV -= _PaintMin;
    	RelativeUV.x = RelativeUV.x / _PaintArea.x;
    	RelativeUV.y = RelativeUV.y / _PaintArea.y;


    	if(RelativeUV.x >= 0 && RelativeUV.x <= 1 &&
    	   RelativeUV.y >= 0 && RelativeUV.y <= 1
    	){
    		return lerp(col,fixed4(_PaintTarget,0,0,0),tex2D(_PaintBrush,RelativeUV).r);
    	}

    	return col;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_position
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_rotation
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_scale
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_noice
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment visibility_update
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment paint
            ENDCG
        }
    }
}
