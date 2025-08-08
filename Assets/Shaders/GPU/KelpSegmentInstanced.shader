Shader "Custom/KelpSegmentInstanced"
{
    Properties
    {
        // Exposed color property (not used directly here — node.color is used instead)
        _Color ("Color", Color) = (0,1,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            // === Shader Entry Points ===
            #pragma vertex vert               // Vertex shader function
            #pragma fragment frag             // Fragment shader function
            #pragma target 4.5                // Shader model 4.5 (required for compute buffer access)
            #pragma multi_compile_instancing // Enable GPU instancing support 

            #include "UnityCG.cginc"         // Include Unity helper functions (like UnityObjectToClipPos)

            // === GPU-Side Struct for Stalk Node ===
            struct StalkNode
            {
                float3 currentPos;   // Current world-space position of the node
                float padding0;

                float3 previousPos;  // Previous position (for Verlet integration, unused in vertex shader)
                float padding1;

                float3 direction;    // Direction vector (used to rotate mesh segment)
                float padding2;

                float4 color;        // Color for this node (used in vertex color)
                float bendAmount;    // Optional bend data (not yet used)
                float3 padding3;

				int isTip;
				float3 padding4; 
            };

            // === Structured Buffer Holding All Stalk Nodes ===
            StructuredBuffer<StalkNode> _StalkNodesBuffer;

			float3 _WorldOffset; 

            // A fallback color (not used currently)
            float4 _Color;

            // === Appdata: Per-vertex data + instance ID ===
            struct appdata
            {
                float3 vertex : POSITION;      // Vertex position in local space
                uint instanceID : SV_InstanceID; // ID of the mesh instance (used to index into buffer)
            };

            // === V2F: Data passed to fragment shader ===
            struct v2f
            {
                float4 pos : SV_POSITION;      // Final screen-space position
                float4 color : COLOR0;         // Per-vertex color (from StalkNode)
            };

            // === Vertex Shader ===
            v2f vert(appdata v)
            {
                v2f o;

                // Get the StalkNode for this instance
                StalkNode node = _StalkNodesBuffer[v.instanceID];

                float3 pos = node.currentPos;               // World position of the node
                float3 dir = normalize(node.direction);     // Normalize direction vector 
                dir = (all(abs(dir) < 0.001)) ? float3(0,1,0) : dir; // Fallback to up if zero

                // === Build a Rotation Matrix to Rotate From UP to dir ===

                float3 up = float3(0, 1, 0);         // Local Y-up direction 
                float3 axis = cross(up, dir);       // Rotation axis
                float angle = acos(saturate(dot(up, dir))); // Angle between up and dir

                // Compute components for rotation matrix
                float s = sin(angle);
                float c = cos(angle);
                float t = 1.0 - c;

                float3x3 rotationMatrix;

                if (length(axis) < 0.001)
                {
                    // No rotation needed — use identity
                    rotationMatrix = float3x3(1, 0, 0,
                                             0, 1, 0,
                                             0, 0, 1);
                }
                else
                {
                    // Normalized axis for rotation
                    axis = normalize(axis);

                    // Rodriguez rotation formula to build 3x3 matrix
                    rotationMatrix = float3x3(
                        t * axis.x * axis.x + c,         t * axis.x * axis.y - s * axis.z,  t * axis.x * axis.z + s * axis.y,
                        t * axis.x * axis.y + s * axis.z, t * axis.y * axis.y + c,           t * axis.y * axis.z - s * axis.x,
                        t * axis.x * axis.z - s * axis.y, t * axis.y * axis.z + s * axis.x,  t * axis.z * axis.z + c
                    );
                }

                // === Deform the vertex to create a "pinched" tip ===
                float3 vertex = v.vertex;

				// Only pinch if it's the last node
				if (node.isTip == 1)
				{
					float pinchFactor = saturate(1.0 - vertex.y); 
					vertex.xz *= pinchFactor; 
				}

                // Apply rotation to the local-space vertex
                float3 rotated = mul(rotationMatrix, vertex);

                // Final world position = node position + rotated offset
                float3 worldPos = _WorldOffset + node.currentPos + rotated;
				o.pos = UnityWorldToClipPos(float4(worldPos, 1.0)); 

                // Pass color to fragment shader
                o.color = node.color;

                return o;
            }

            // === Fragment Shader ===
            fixed4 frag(v2f i) : SV_Target
            {
                // Simply output the interpolated color
                return i.color;
            }

            ENDHLSL
        }
    }
} 