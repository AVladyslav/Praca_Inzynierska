using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Helper
{
    public static Texture2D RotatePreviewTexture(Texture2D textureIn, bool clockwise)
    {
        //  Preview Texture is 128x128 px
        int pixelsCount = 128;
        Texture2D textureOut = new Texture2D(pixelsCount, pixelsCount, textureIn.format, true);

        if (clockwise)
        {
            for (int i = 0; i < pixelsCount; i++)
            {
                for (int j = 0; j < pixelsCount; j++)
                {
                    textureOut.SetPixel(j, i, textureIn.GetPixel(pixelsCount - i, j));
                }
            }
        }
        else
        {
            for (int i = 0; i < pixelsCount; i++)
            {
                for (int j = 0; j < pixelsCount; j++)
                {
                    textureOut.SetPixel(j, i, textureIn.GetPixel(i, pixelsCount - j));
                }
            }
        }

        return textureOut;
    }
}
