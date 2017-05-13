using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace ucssceditor
{
    class ScImage
    {
        protected ushort m_vWidth;
        protected ushort m_vHeight;
        protected Bitmap m_vBitmap; 

        public ScImage()
        {
        }

        public ScImage(ScImage im)
        {
            m_vWidth = im.GetWidth();
            m_vHeight = im.GetHeight();
            m_vBitmap = new Bitmap(im.GetBitmap());
        }

        public Bitmap GetBitmap()
        {
            return m_vBitmap;
        }

        public virtual string GetImageTypeName()
        {
            return "unknown";
        }

        public ushort GetHeight()
        {
            return m_vHeight;
        }


        public ushort GetWidth()
        {
            return m_vWidth;
        }

        public virtual void ParseImage(BinaryReader br, byte fileType)
        {
            m_vWidth = br.ReadUInt16();
            m_vHeight = br.ReadUInt16();
        }

        public virtual void Print()
        {
            Log("Width: " + m_vWidth.ToString());
            Log("Height: " + m_vHeight.ToString());
        }

        public void SetBitmap(Bitmap b)
        {
            m_vBitmap = b;
            m_vWidth = (ushort)b.Width;
            m_vHeight = (ushort)b.Height;
        }

        public virtual void WriteImage(FileStream input)
        {
            input.Write(BitConverter.GetBytes(m_vWidth), 0, 2);
            input.Write(BitConverter.GetBytes(m_vHeight), 0, 2);
        }

        public void FillImage(List<Color> pixels, byte fileType)
        {
            // from this part all of codes just converted to csharp with comments                    
            if (fileType != 27 && fileType != 28)
            {
                int iSrcPix = 0;
                for (int column = 0; column < m_vWidth; column++)
                {
                    for (int row = 0; row < m_vHeight; row++)
                    {
                        m_vBitmap.SetPixel(column, row, pixels[iSrcPix]);
                        iSrcPix++;
                    }
                }
            }
            else
            {
                int iSrcPix = 0;

                for (int l = 0; l < Math.Floor((decimal)m_vHeight / 32); l++) //block of 32 lines
                {
                    // normal 32-pixels blocks
                    for (int k = 0; k < Math.Floor((decimal)m_vWidth / 32); k++) // 32-pixels blocks in a line
                    {
                        for (int j = 0; j < 32; j++) // line in a multi line block
                        {
                            for (int h = 0; h < 32; h++) // pixels in a block
                            {
                                this.m_vBitmap.SetPixel((h + (k * 32)), (j + (l * 32)), pixels[iSrcPix]);

                                iSrcPix++;
                            }
                        }
                    }

                    // line end blocks
                    for (int j = 0; j < 32; j++)
                    {
                        for (int h = 0; h < GetWidth() % 32; h++)
                        {
                            this.m_vBitmap.SetPixel((h + (m_vWidth - (m_vWidth % 32))), (j + (l * 32)), pixels[iSrcPix]);
                            iSrcPix++;
                        }
                    }
                }

                // final lines
                for (int k = 0; k < Math.Floor((decimal)m_vWidth / 32); k++) // 32-pixels blocks in a line
                {
                    for (int j = 0; j < (m_vHeight % 32); j++) // line in a multi line block
                    {
                        for (int h = 0; h < 32; h++) // pixels in a 32-pixels-block
                        {
                            this.m_vBitmap.SetPixel((h + (k * 32)), (j + (m_vHeight - (m_vHeight % 32))), pixels[iSrcPix]);
                            iSrcPix++;
                        }
                    }
                }
            }
        }

        private void Log(string content)
        {
            //Debug.WriteLine(content);
        }

    }
}
