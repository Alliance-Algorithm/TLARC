// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MvGvspPixelType
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>像素格式</summary>
  public enum MvGvspPixelType
  {
    /// <summary>Jpeg</summary>
    PixelType_Gvsp_Jpeg = -2145910783, // 0x80180001
    /// <summary>HB_Mono8</summary>
    PixelType_Gvsp_HB_Mono8 = -2130182143, // 0x81080001
    /// <summary>HB_BayerGR8</summary>
    PixelType_Gvsp_HB_BayerGR8 = -2130182136, // 0x81080008
    /// <summary>HB_BayerRG8</summary>
    PixelType_Gvsp_HB_BayerRG8 = -2130182135, // 0x81080009
    /// <summary>HB_BayerGB8</summary>
    PixelType_Gvsp_HB_BayerGB8 = -2130182134, // 0x8108000A
    /// <summary>HB_BayerBG8</summary>
    PixelType_Gvsp_HB_BayerBG8 = -2130182133, // 0x8108000B
    /// <summary>HB_BayerRBGG8</summary>
    PixelType_Gvsp_HB_BayerRBGG8 = -2130182074, // 0x81080046
    /// <summary>HB_Mono10_Packed</summary>
    PixelType_Gvsp_HB_Mono10_Packed = -2129919996, // 0x810C0004
    /// <summary>HB_Mono12_Packed</summary>
    PixelType_Gvsp_HB_Mono12_Packed = -2129919994, // 0x810C0006
    /// <summary>HB_BayerGR10_Packed</summary>
    PixelType_Gvsp_HB_BayerGR10_Packed = -2129919962, // 0x810C0026
    /// <summary>HB_BayerRG10_Packed</summary>
    PixelType_Gvsp_HB_BayerRG10_Packed = -2129919961, // 0x810C0027
    /// <summary>HB_BayerGB10_Packed</summary>
    PixelType_Gvsp_HB_BayerGB10_Packed = -2129919960, // 0x810C0028
    /// <summary>HB_BayerBG10_Packed</summary>
    PixelType_Gvsp_HB_BayerBG10_Packed = -2129919959, // 0x810C0029
    /// <summary>HB_BayerGR12_Packed</summary>
    PixelType_Gvsp_HB_BayerGR12_Packed = -2129919958, // 0x810C002A
    /// <summary>HB_BayerRG12_Packed</summary>
    PixelType_Gvsp_HB_BayerRG12_Packed = -2129919957, // 0x810C002B
    /// <summary>HB_BayerGB12_Packed</summary>
    PixelType_Gvsp_HB_BayerGB12_Packed = -2129919956, // 0x810C002C
    /// <summary>HB_BayerBG12_Packed</summary>
    PixelType_Gvsp_HB_BayerBG12_Packed = -2129919955, // 0x810C002D
    /// <summary>HB_Mono10</summary>
    PixelType_Gvsp_HB_Mono10 = -2129657853, // 0x81100003
    /// <summary>HB_Mono12</summary>
    PixelType_Gvsp_HB_Mono12 = -2129657851, // 0x81100005
    /// <summary>HB_Mono16</summary>
    PixelType_Gvsp_HB_Mono16 = -2129657849, // 0x81100007
    /// <summary>HB_BayerGR10</summary>
    PixelType_Gvsp_HB_BayerGR10 = -2129657844, // 0x8110000C
    /// <summary>HB_BayerRG10</summary>
    PixelType_Gvsp_HB_BayerRG10 = -2129657843, // 0x8110000D
    /// <summary>HB_BayerGB10</summary>
    PixelType_Gvsp_HB_BayerGB10 = -2129657842, // 0x8110000E
    /// <summary>HB_BayerBG10</summary>
    PixelType_Gvsp_HB_BayerBG10 = -2129657841, // 0x8110000F
    /// <summary>HB_BayerGR12</summary>
    PixelType_Gvsp_HB_BayerGR12 = -2129657840, // 0x81100010
    /// <summary>HB_BayerRG12</summary>
    PixelType_Gvsp_HB_BayerRG12 = -2129657839, // 0x81100011
    /// <summary>HB_BayerGB12</summary>
    PixelType_Gvsp_HB_BayerGB12 = -2129657838, // 0x81100012
    /// <summary>HB_BayerBG12</summary>
    PixelType_Gvsp_HB_BayerBG12 = -2129657837, // 0x81100013
    /// <summary>Float32</summary>
    PixelType_Gvsp_Float32 = -2128609279, // 0x81200001
    /// <summary>Coord3D_A32</summary>
    PixelType_Gvsp_Coord3D_A32 = -2128596987, // 0x81203005
    /// <summary>Coord3D_C32</summary>
    PixelType_Gvsp_Coord3D_C32 = -2128596986, // 0x81203006
    /// <summary>HB_YUV422_Packed</summary>
    PixelType_Gvsp_HB_YUV422_Packed = -2112880609, // 0x8210001F
    /// <summary>HB_YUV422_YUYV_Packed</summary>
    PixelType_Gvsp_HB_YUV422_YUYV_Packed = -2112880590, // 0x82100032
    /// <summary>HB_RGB8_Packed</summary>
    PixelType_Gvsp_HB_RGB8_Packed = -2112356332, // 0x82180014
    /// <summary>HB_BGR8_Packed</summary>
    PixelType_Gvsp_HB_BGR8_Packed = -2112356331, // 0x82180015
    /// <summary>COORD3D_DEPTH_PLUS_MASK</summary>
    PixelType_Gvsp_COORD3D_DEPTH_PLUS_MASK = -2112094207, // 0x821C0001
    /// <summary>HB_RGBA8_Packed</summary>
    PixelType_Gvsp_HB_RGBA8_Packed = -2111832042, // 0x82200016
    /// <summary>HB_BGRA8_Packed</summary>
    PixelType_Gvsp_HB_BGRA8_Packed = -2111832041, // 0x82200017
    /// <summary>HB_RGB16_Packed</summary>
    PixelType_Gvsp_HB_RGB16_Packed = -2110783437, // 0x82300033
    /// <summary>HB_BGR16_Packed</summary>
    PixelType_Gvsp_HB_BGR16_Packed = -2110783413, // 0x8230004B
    /// <summary>HB_BGRA16_Packed</summary>
    PixelType_Gvsp_HB_BGRA16_Packed = -2109734831, // 0x82400051
    /// <summary>HB_RGBA16_Packed</summary>
    PixelType_Gvsp_HB_RGBA16_Packed = -2109734812, // 0x82400064
    /// <summary>Coord3D_AB32f</summary>
    PixelType_Gvsp_Coord3D_AB32f = -2109722622, // 0x82403002
    /// <summary>Coord3D_AB32</summary>
    PixelType_Gvsp_Coord3D_AB32 = -2109722621, // 0x82403003
    /// <summary>Coord3D_AC32</summary>
    PixelType_Gvsp_Coord3D_AC32 = -2109722620, // 0x82403004
    /// <summary>Coord3D_ABC32</summary>
    PixelType_Gvsp_Coord3D_ABC32 = -2107625471, // 0x82603001
    /// <summary>未定义像素格式</summary>
    PixelType_Gvsp_Undefined = -1, // 0xFFFFFFFF
    /// <summary>Mono1p</summary>
    PixelType_Gvsp_Mono1p = 16842807, // 0x01010037
    /// <summary>Mono2p</summary>
    PixelType_Gvsp_Mono2p = 16908344, // 0x01020038
    /// <summary>Mono4p</summary>
    PixelType_Gvsp_Mono4p = 17039417, // 0x01040039
    /// <summary>Mono8</summary>
    PixelType_Gvsp_Mono8 = 17301505, // 0x01080001
    /// <summary>Mono8_Signed</summary>
    PixelType_Gvsp_Mono8_Signed = 17301506, // 0x01080002
    /// <summary>BayerGR8</summary>
    PixelType_Gvsp_BayerGR8 = 17301512, // 0x01080008
    /// <summary>BayerRG8</summary>
    PixelType_Gvsp_BayerRG8 = 17301513, // 0x01080009
    /// <summary>BayerGB8</summary>
    PixelType_Gvsp_BayerGB8 = 17301514, // 0x0108000A
    /// <summary>BayerBG8</summary>
    PixelType_Gvsp_BayerBG8 = 17301515, // 0x0108000B
    /// <summary>BayerRBGG8</summary>
    PixelType_Gvsp_BayerRBGG8 = 17301574, // 0x01080046
    /// <summary>Mono10_Packed</summary>
    PixelType_Gvsp_Mono10_Packed = 17563652, // 0x010C0004
    /// <summary>Mono12_Packed</summary>
    PixelType_Gvsp_Mono12_Packed = 17563654, // 0x010C0006
    /// <summary>BayerGR10_Packed</summary>
    PixelType_Gvsp_BayerGR10_Packed = 17563686, // 0x010C0026
    /// <summary>BayerRG10_Packed</summary>
    PixelType_Gvsp_BayerRG10_Packed = 17563687, // 0x010C0027
    /// <summary>BayerGB10_Packed</summary>
    PixelType_Gvsp_BayerGB10_Packed = 17563688, // 0x010C0028
    /// <summary>BayerBG10_Packed</summary>
    PixelType_Gvsp_BayerBG10_Packed = 17563689, // 0x010C0029
    /// <summary>BayerGR12_Packed</summary>
    PixelType_Gvsp_BayerGR12_Packed = 17563690, // 0x010C002A
    /// <summary>BayerRG12_Packed</summary>
    PixelType_Gvsp_BayerRG12_Packed = 17563691, // 0x010C002B
    /// <summary>BayerGB12_Packed</summary>
    PixelType_Gvsp_BayerGB12_Packed = 17563692, // 0x010C002C
    /// <summary>BayerBG12_Packed</summary>
    PixelType_Gvsp_BayerBG12_Packed = 17563693, // 0x010C002D
    /// <summary>Mono10</summary>
    PixelType_Gvsp_Mono10 = 17825795, // 0x01100003
    /// <summary>Mono12</summary>
    PixelType_Gvsp_Mono12 = 17825797, // 0x01100005
    /// <summary>Mono16</summary>
    PixelType_Gvsp_Mono16 = 17825799, // 0x01100007
    /// <summary>BayerGR10</summary>
    PixelType_Gvsp_BayerGR10 = 17825804, // 0x0110000C
    /// <summary>BayerRG10</summary>
    PixelType_Gvsp_BayerRG10 = 17825805, // 0x0110000D
    /// <summary>BayerGB10</summary>
    PixelType_Gvsp_BayerGB10 = 17825806, // 0x0110000E
    /// <summary>BayerBG10</summary>
    PixelType_Gvsp_BayerBG10 = 17825807, // 0x0110000F
    /// <summary>BayerGR12</summary>
    PixelType_Gvsp_BayerGR12 = 17825808, // 0x01100010
    /// <summary>BayerRG12</summary>
    PixelType_Gvsp_BayerRG12 = 17825809, // 0x01100011
    /// <summary>BayerGB12</summary>
    PixelType_Gvsp_BayerGB12 = 17825810, // 0x01100012
    /// <summary>BayerBG12</summary>
    PixelType_Gvsp_BayerBG12 = 17825811, // 0x01100013
    /// <summary>Mono14</summary>
    PixelType_Gvsp_Mono14 = 17825829, // 0x01100025
    /// <summary>BayerGR16</summary>
    PixelType_Gvsp_BayerGR16 = 17825838, // 0x0110002E
    /// <summary>BayerRG16</summary>
    PixelType_Gvsp_BayerRG16 = 17825839, // 0x0110002F
    /// <summary>BayerGB16</summary>
    PixelType_Gvsp_BayerGB16 = 17825840, // 0x01100030
    /// <summary>BayerBG16</summary>
    PixelType_Gvsp_BayerBG16 = 17825841, // 0x01100031
    /// <summary>Coord3D_C16</summary>
    PixelType_Gvsp_Coord3D_C16 = 17825976, // 0x011000B8
    /// <summary>Coord3D_A32f</summary>
    PixelType_Gvsp_Coord3D_A32f = 18874557, // 0x012000BD
    /// <summary>Coord3D_C32f</summary>
    PixelType_Gvsp_Coord3D_C32f = 18874559, // 0x012000BF
    /// <summary>YUV411_Packed</summary>
    PixelType_Gvsp_YUV411_Packed = 34340894, // 0x020C001E
    /// <summary>YCBCR411_8_CBYYCRYY</summary>
    PixelType_Gvsp_YCBCR411_8_CBYYCRYY = 34340924, // 0x020C003C
    /// <summary>YCBCR601_411_8_CBYYCRYY</summary>
    PixelType_Gvsp_YCBCR601_411_8_CBYYCRYY = 34340927, // 0x020C003F
    /// <summary>YCBCR709_411_8_CBYYCRYY</summary>
    PixelType_Gvsp_YCBCR709_411_8_CBYYCRYY = 34340930, // 0x020C0042
    /// <summary>YUV420SP_NV12</summary>
    PixelType_Gvsp_YUV420SP_NV12 = 34373633, // 0x020C8001
    /// <summary>YUV420SP_NV21</summary>
    PixelType_Gvsp_YUV420SP_NV21 = 34373634, // 0x020C8002
    /// <summary>YUV422_Packed</summary>
    PixelType_Gvsp_YUV422_Packed = 34603039, // 0x0210001F
    /// <summary>YUV422_YUYV_Packed</summary>
    PixelType_Gvsp_YUV422_YUYV_Packed = 34603058, // 0x02100032
    /// <summary>RGB565_Packed</summary>
    PixelType_Gvsp_RGB565_Packed = 34603061, // 0x02100035
    /// <summary>BGR565_Packed</summary>
    PixelType_Gvsp_BGR565_Packed = 34603062, // 0x02100036
    /// <summary>YCBCR422_8</summary>
    PixelType_Gvsp_YCBCR422_8 = 34603067, // 0x0210003B
    /// <summary>YCBCR601_422_8</summary>
    PixelType_Gvsp_YCBCR601_422_8 = 34603070, // 0x0210003E
    /// <summary>YCBCR709_422_8</summary>
    PixelType_Gvsp_YCBCR709_422_8 = 34603073, // 0x02100041
    /// <summary>YCBCR422_8_CBYCRY</summary>
    PixelType_Gvsp_YCBCR422_8_CBYCRY = 34603075, // 0x02100043
    /// <summary>YCBCR601_422_8_CBYCRY</summary>
    PixelType_Gvsp_YCBCR601_422_8_CBYCRY = 34603076, // 0x02100044
    /// <summary>YCBCR709_422_8_CBYCRY</summary>
    PixelType_Gvsp_YCBCR709_422_8_CBYCRY = 34603077, // 0x02100045
    /// <summary>RGB8_Packed</summary>
    PixelType_Gvsp_RGB8_Packed = 35127316, // 0x02180014
    /// <summary>BGR8_Packed</summary>
    PixelType_Gvsp_BGR8_Packed = 35127317, // 0x02180015
    /// <summary>YUV444_Packed</summary>
    PixelType_Gvsp_YUV444_Packed = 35127328, // 0x02180020
    /// <summary>RGB8_Planar</summary>
    PixelType_Gvsp_RGB8_Planar = 35127329, // 0x02180021
    /// <summary>YCBCR8_CBYCR</summary>
    PixelType_Gvsp_YCBCR8_CBYCR = 35127354, // 0x0218003A
    /// <summary>YCBCR601_8_CBYCR</summary>
    PixelType_Gvsp_YCBCR601_8_CBYCR = 35127357, // 0x0218003D
    /// <summary>YCBCR709_8_CBYCR</summary>
    PixelType_Gvsp_YCBCR709_8_CBYCR = 35127360, // 0x02180040
    /// <summary>RGBA8_Packed</summary>
    PixelType_Gvsp_RGBA8_Packed = 35651606, // 0x02200016
    /// <summary>BGRA8_Packed</summary>
    PixelType_Gvsp_BGRA8_Packed = 35651607, // 0x02200017
    /// <summary>RGB10V1_Packe</summary>
    PixelType_Gvsp_RGB10V1_Packed = 35651612, // 0x0220001C
    /// <summary>RGB10V2_Packed</summary>
    PixelType_Gvsp_RGB10V2_Packed = 35651613, // 0x0220001D
    /// <summary>RGB12V1_Packed</summary>
    PixelType_Gvsp_RGB12V1_Packed = 35913780, // 0x02240034
    /// <summary>RGB10_Packed</summary>
    PixelType_Gvsp_RGB10_Packed = 36700184, // 0x02300018
    /// <summary>BGR10_Packed</summary>
    PixelType_Gvsp_BGR10_Packed = 36700185, // 0x02300019
    /// <summary>RGB12_Packed</summary>
    PixelType_Gvsp_RGB12_Packed = 36700186, // 0x0230001A
    /// <summary>BGR12_Packed</summary>
    PixelType_Gvsp_BGR12_Packed = 36700187, // 0x0230001B
    /// <summary>RGB10_Planar</summary>
    PixelType_Gvsp_RGB10_Planar = 36700194, // 0x02300022
    /// <summary>RGB12_Planar</summary>
    PixelType_Gvsp_RGB12_Planar = 36700195, // 0x02300023
    /// <summary>RGB16_Planar</summary>
    PixelType_Gvsp_RGB16_Planar = 36700196, // 0x02300024
    /// <summary>RGB16_Packed</summary>
    PixelType_Gvsp_RGB16_Packed = 36700211, // 0x02300033
    /// <summary>BGR16_Packed/// </summary>
    PixelType_Gvsp_BGR16_Packed = 36700235, // 0x0230004B
    /// <summary>Coord3D_ABC16</summary>
    PixelType_Gvsp_Coord3D_ABC16 = 36700345, // 0x023000B9
    /// <summary>RGBA16_Packed</summary>
    PixelType_Gvsp_RGBA16_Packed = 37748800, // 0x02400040
    /// <summary>BGRA16_Packed</summary>
    PixelType_Gvsp_BGRA16_Packed = 37748817, // 0x02400051
    /// <summary>Coord3D_AC32f</summary>
    PixelType_Gvsp_Coord3D_AC32f = 37748930, // 0x024000C2
    /// <summary>Coord3D_AC32f_64</summary>
    PixelType_Gvsp_Coord3D_AC32f_64 = 37748930, // 0x024000C2
    /// <summary>Coord3D_AC32f_Planar</summary>
    PixelType_Gvsp_Coord3D_AC32f_Planar = 37748931, // 0x024000C3
    /// <summary>Coord3D_ABC32f</summary>
    PixelType_Gvsp_Coord3D_ABC32f = 39846080, // 0x026000C0
    /// <summary>Coord3D_ABC32f_Planar</summary>
    PixelType_Gvsp_Coord3D_ABC32f_Planar = 39846081, // 0x026000C1
  }
}
