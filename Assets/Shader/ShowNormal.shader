Shader "Custom/ShowNormal"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"


            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normals : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;

            };


            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = float3(-v.normals.x, -v.normals.z, v.normals.y);
                return o;
            }

            //fixed4 frag(Interpolators i) : SV_Target
            //{
            //    
            //    fixed4 col = fixed4(i.normal, 1);
            //    
            //    if (i.normal.x < 0)
            //        col = fixed4(1, 1, 0, 1);
            //    if (i.normal.y < 0)
            //        col = fixed4(0, 1, 1, 1);
            //    if (i.normal.z < 0)
            //        col = fixed4(1, 0, 1, 1);

            //    return col;
            //}

            float4 frag(Interpolators i) : SV_Target
            {
                
                float4 col = float4(i.normal, 1);
                
                if (i.normal.x < 0)
                    col = float4(1, 1, 0, 1);
                if (i.normal.y < 0)
                    col = float4(0, 1, 1, 1);
                if (i.normal.z < 0)
                    col = float4(1, 0, 1, 1);



                if (i.normal.x == 1)
                    col = float4(1, 0, 0, 1);
                if (i.normal.y == 1)
                    col = float4(0, 1, 0, 1);
                if (i.normal.z == 1)
                    col = float4(0, 0, 1, 1);

                return col;
            }

            //float4 frag(Interpolators i) : SV_Target
            //{

            //    //float4 col = float4(i.normal, 1);
            //    float4 col = 0;

            //    if (i.normal.x < 0)
            //    {
            //        //col = float4(1, 1, 0, 1);
            //        return float4(1, 0, 0, 1);
            //    }
            //    if (i.normal.y < 0)
            //    {
            //        //col = float4(0, 1, 1, 1);
            //        return float4(0, 1, 0, 1);
            //    }
            //    if (i.normal.y > 0)
            //    {
            //        //col = float4(0, 1, 1, 1);
            //        return float4(1, 1, 0, 1);
            //    }
            //    if (i.normal.z < 0)
            //    {
            //        //col = float4(1, 0, 1, 1);
            //        return float4(0, 0, 1, 1);
            //    }

            //    return col;
            //}
            ENDCG
        }
    }
}