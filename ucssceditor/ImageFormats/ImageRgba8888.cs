﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ucssceditor
{
    class ImageRgba8888 : ScImage
    {
        public ImageRgba8888()
        {
        }

        public override string GetImageTypeName()
        {
            return "RGB8888";
        }

        public override void ParseImage(BinaryReader br, byte fileType)
        {
            base.ParseImage(br, fileType);
            m_vBitmap = new Bitmap(m_vWidth, m_vHeight, PixelFormat.Format32bppArgb);

            List<Color> pixels = new List<Color>(m_vHeight * m_vWidth);
            for (int column = 0; column < m_vHeight; column++)
            {
                for (int row = 0; row < m_vWidth; row++)
                {
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();
                    byte a = br.ReadByte();

                    pixels.Add(Color.FromArgb((int)((a << 24) | (r << 16) | (g << 8) | b)));
                }
            }
            FillImage(pixels, fileType);
        }

        public override void Print()
        {
            base.Print();
        }

        public override void WriteImage(FileStream input)
        {
            base.WriteImage(input);
            for (int column = 0; column < m_vBitmap.Height; column++)
            {
                for (int row = 0; row < m_vBitmap.Width; row++)
                {
                    input.WriteByte(m_vBitmap.GetPixel(row, column).R);
                    input.WriteByte(m_vBitmap.GetPixel(row, column).G);
                    input.WriteByte(m_vBitmap.GetPixel(row, column).B);
                    input.WriteByte(m_vBitmap.GetPixel(row, column).A);
                }
            }
        }
    }
}
