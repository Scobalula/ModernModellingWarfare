using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    public enum TokenDataType
    {
        Comment          = 00,
        Section          = 01,
        Short            = 02,
        UShort           = 03,
        UInt             = 04,
        Int              = 05,
        Vector48Bit      = 06,
        Vector316Bit     = 07,
        Float            = 08,
        Vector2          = 09,
        Vector3          = 10,
        Vector4          = 11,
        BoneWeight       = 12,
        UVSet            = 13,
        UShortString     = 14,
        UShortStringX3   = 15,
        Unk4             = 16,
        BoneInfo         = 17,
        Tri             = 18,
        Tri16             = 19,
        Unk9             = 20,
    }
}
