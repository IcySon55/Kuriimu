using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.IO;

namespace image_tim2
{
    public sealed class TIM2
    {
        public Header header;
        public PicHeader picHeader;

        public List<Bitmap> bmpList;
        public Bitmap bmp;
        public ImageSettings settings;

        public TIM2(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();
                br.BaseStream.Position += 8;

                //Picture Header
                picHeader = br.ReadStruct<PicHeader>();

                /*if (picHeader.mipMapTextureCount>0)
                {
                    int i;
                    PSM psm = (PSM)(picHeader.imageType - 1);
                    //u_long128* pImage;
                    int w=picHeader.width, h=picHeader.height;
                    int tbw;

                    var sceGsTex0 = new GsTex();
                    sceGsTex0.psm = psm;

                    // 奶丟奈斥犯奈正及珂螹失玉伊旦毛�呾
                    pImage = (u_long128*)((char*)ph + ph->HeaderSize);
                    if (tbp == -1)
                    {
                        // tbp及硌隅互-1及午五﹜疋弁民乓目永母卞硌隅今木凶GsTex0井日tbp,tbw毛腕月
                        tbp = ((sceGsTex0*)&ph->GsTex0)->TBP0;
                        tbw = ((sceGsTex0*)&ph->GsTex0)->TBW;

                        Tim2LoadTexture(psm, tbp, tbw, w, h, pImage); // 氾弁旦民乓犯奈正毛GS卞�冞
                    }
                    else
                    {
                        // tbp及硌隅互硌隅今木凶午五﹜疋弁民乓目永母及GsTex0丟件田及tbp,tbw毛左奈田奈仿奶玉
                        tbw = Tim2CalcBufWidth(psm, w);
                        // GS及TEX0伊斥旦正卞偞隅允月�毛載陔
                        ((sceGsTex0*)&ph->GsTex0)->TBP0 = tbp;
                        ((sceGsTex0*)&ph->GsTex0)->TBW = tbw;
                        Tim2LoadTexture(psm, tbp, tbw, w, h, pImage); // 氾弁旦民乓犯奈正毛GS卞�冞
                        tbp += Tim2CalcBufSize(psm, w, h);            // tbp及�毛載陔
                    }

                    if (ph->MipMapTextures > 1)
                    {
                        // MIPMAP氾弁旦民乓互丐月磁
                        TIM2_MIPMAPHEADER* pm;

                        pm = (TIM2_MIPMAPHEADER*)(ph + 1); // 疋弁民乓目永母及眻摽卞MIPMAP目永母

                        // LV0及氾弁旦民乓扔奶朮毛腕月
                        // tbp毛竘杅匹硌隅今木凶午五﹜疋弁民乓目永母卞丐月miptbp毛�仄化赻�呾
                        if (tbp != -1)
                        {
                            pm->GsMiptbp1 = 0;
                            pm->GsMiptbp2 = 0;
                        }

                        pImage = (u_long128*)((char*)ph + ph->HeaderSize);
                        // 跪MIPMAP伊矛伙及奶丟奈斥毛�冞
                        for (i = 1; i < ph->MipMapTextures; i++)
                        {
                            // MIPMAP伊矛伙互丐互月午﹜氾弁旦民乓扔奶朮反1/2卞卅月
                            w = w / 2;
                            h = h / 2;

                            pImage = (u_long128*)((char*)pImage + pm->MMImageSize[i - 1]);
                            if (tbp == -1)
                            {
                                // 氾弁旦民乓矢奈斥及硌隅互-1及午五﹜MIPMAP目永母卞硌隅今木凶tbp,tbw毛妏蚚允月
                                int miptbp;
                                if (i < 4)
                                {
                                    // MIPMAP伊矛伙1,2,3及午五
                                    miptbp = (pm->GsMiptbp1 >> ((i - 1) * 20)) & 0x3FFF;
                                    tbw = (pm->GsMiptbp1 >> ((i - 1) * 20 + 14)) & 0x3F;
                                }
                                else
                                {
                                    // MIPMAP伊矛伙4,5,6及午五
                                    miptbp = (pm->GsMiptbp2 >> ((i - 4) * 20)) & 0x3FFF;
                                    tbw = (pm->GsMiptbp2 >> ((i - 4) * 20 + 14)) & 0x3F;
                                }
                                Tim2LoadTexture(psm, miptbp, tbw, w, h, pImage);
                            }
                            else
                            {
                                // 氾弁旦民乓矢奈斥互硌隅今木化中月午五﹜MIPMAP目永母毛婬偞隅
                                tbw = Tim2CalcBufWidth(psm, w);    // 氾弁旦民乓盟毛�呾
                                if (i < 4)
                                {
                                    // MIPMAP伊矛伙1,2,3及午五
                                    pm->GsMiptbp1 |= ((u_long)tbp) << ((i - 1) * 20);
                                    pm->GsMiptbp1 |= ((u_long)tbw) << ((i - 1) * 20 + 14);
                                }
                                else
                                {
                                    // MIPMAP伊矛伙4,5,6及午五
                                    pm->GsMiptbp2 |= ((u_long)tbp) << ((i - 4) * 20);
                                    pm->GsMiptbp2 |= ((u_long)tbw) << ((i - 4) * 20 + 14);
                                }
                                Tim2LoadTexture(psm, tbp, tbw, w, h, pImage);
                                tbp += Tim2CalcBufSize(psm, w, h); // tbp及�毛載陔
                            }
                        }
                    }
                }

                bmp = bmpList[0];*/
            }
        }

        //Save will be added after Kukkii multi image support
        public void Save(Stream input)
        {

        }
    }
}
