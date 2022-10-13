#ifndef LIBII_LIGHT_2D_CORE
#define LIBII_LIGHT_2D_CORE

half _LightFlat;
half _LightDensity;


struct fout
{
    half4 color:SV_Target;
    half4 light:SV_Target1;
    half4 normal:SV_Target2;
};

fout CreateSimpleFromColor(half4 color)
{
    fout fo;
    fo.color = color;
    fo.light = half4(color.rgb * _LightDensity * _LightFlat, color.a);
    return fo;
}


#endif
