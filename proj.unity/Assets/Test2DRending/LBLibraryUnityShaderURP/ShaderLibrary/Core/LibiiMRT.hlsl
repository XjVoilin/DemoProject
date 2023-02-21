#ifndef UNIVERSAL_PIPELINE_LIBII_MRT_INCLUDED
#define UNIVERSAL_PIPELINE_LIBII_MRT_INCLUDED

struct fout
{
    half4 color : SV_Target;
    half4 depth : SV_Target1;
};


fout ComputeLightingMRT(half4 color, half density, half flag, half enableWrite)
{
    fout fo;
    
}


#endif
