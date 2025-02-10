
static const float3 lightpos = float3(7.2, 4.2, 3);
static const float3 lightnorm = normalize(float3(0.707, 0.707, 0.3));

static const float3 lightpos1 = float3(0, 5.4, 3);
static const float3 lightnorm1 = normalize(float3(0, 1, 0.3));


struct VS_IN
{
    float2 pos : POSITION;
    float2 tex : TEXCOORD;
    float id : ID;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float3 wpos : WORLDPOS;
    float3 normal : NORMAl;
    float4 col : COLOR;
    float3 tex : TEXCOORD;
};


float4x4 worldViewProj;

struct Card
{
    float4x4 Position;
    float4 Color;
    float4 ID;
};

cbuffer CardsInfo
{
    Card Cards[60];
};

Texture2DArray cardTexture : register(t0);
Texture2D tableTexture : register(t1);

PS_IN VS(VS_IN input)
{
    int id = abs(input.id) - 1;
	
    PS_IN output = (PS_IN) 0;
	
    Card inf = Cards[id];
	
    float4 pos = mul(inf.Position, float4(input.pos, input.id < 0 ? -0.01f : 0.01f, 1));
    output.wpos = pos.xyz;
    output.pos = mul(worldViewProj, pos);
	
    output.normal = mul(inf.Position, float4(0, 0, 1, 0)).xyz;
	
    float TexId = inf.ID.x;
	
    output.tex = float3(input.tex, inf.ID.x);
    if (input.id < 0)
    {
        output.normal = -output.normal;
        output.tex.z = inf.ID.y;
    }

    output.col = inf.Color;
    return output;
}

PS_IN VSShadow(VS_IN input)
{
    int id = abs(input.id) - 1;
	
    PS_IN output = (PS_IN) 0;
	
    Card inf = Cards[id];
	
    float4 pos = mul(inf.Position, float4(input.pos, 0, 1));
    output.wpos = pos.xyz;
    output.pos = mul(worldViewProj, pos);
	
    return output;
}
float4 PSShadow(PS_IN input) : SV_Target
{
    float Val = input.pos.z / input.pos.w;
    return float4(Val.rrr, 1);
}

PS_IN VSTable(VS_IN input)
{
    PS_IN output = (PS_IN) 0;
	
    output.pos = mul(worldViewProj, float4(input.pos, -0.1f, 1));
    output.wpos = float3(input.pos, -0.1f);
    output.normal = float3(0, 0, 1);
    output.tex = input.tex.rgg;
	
    return output;
}

SamplerState samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

float CalcLightIntensity(float3 WPos, float3 Normal, float3 lightPos, float3 lightNorm)
{
    float3 LightDist = lightPos - WPos;
    float DistLight = 1 / length(LightDist);
    float3 LightDirection = LightDist * DistLight;
    
    float Intensity = max(dot(LightDirection, Normal), 0.0) * max((DistLight - 0.09f), 0);
    float Cos = dot(LightDirection, lightNorm);
    float max = 1;
    float min = 0.72;
    
    Intensity *= 15 * clamp((Cos - min) / (max - min), 0, 1);
    return Intensity;
}

float CalcLightIntensityQuat(float3 WPos, float3 Normal, float3 scalar)
{
    return CalcLightIntensity(WPos, Normal, scalar * lightpos, scalar * lightnorm);
}
float TotalIntensity(float3 wpos, float3 normal)
{
    return CalcLightIntensityQuat(wpos, normal, float3(1, 1, 1))
                    + CalcLightIntensityQuat(wpos, normal, float3(-1, 1, 1))
                    + CalcLightIntensityQuat(wpos, normal, float3(1, -1, 1))
                    + CalcLightIntensityQuat(wpos, normal, float3(-1, -1, 1))
                    + CalcLightIntensity(wpos, normal, lightpos1,lightnorm1)
                    + CalcLightIntensity(wpos, normal, lightpos1 * float3(1, -1, 1), lightnorm1 * float3(1, -1, 1));
}

float4 PS(PS_IN input) : SV_Target
{
    if (!any(input.tex.rg))
        return float4(0, 0, 0, 1);
    
    float Intensity = min(1, TotalIntensity(input.wpos, input.normal));
    return cardTexture.Sample(samLinear, input.tex) * input.col * float4(Intensity.rrr, 1);
}

float4 PSTable(PS_IN input) : SV_Target
{
    float Intensity = TotalIntensity(input.wpos, input.normal);
    return tableTexture.Sample(samLinear, input.tex.rg) * float4(Intensity.rrr, 1);
}