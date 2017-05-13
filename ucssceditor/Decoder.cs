﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ucssceditor
{
    class Decoder
    {
        public ushort m_vExportCount;
        private List<ScObject> m_vTextures;
        private List<ScObject> m_vShapes;
        private List<ScObject> m_vExports;
        private List<ScObject> m_vMovieClips;
        private List<ScObject> m_vPendingChanges;
        private string m_vFileName;
        private long m_vEofOffset;
        private long m_vStartExportsOffset;

        public Decoder(string fileName)
        {
            m_vTextures = new List<ScObject>();
            m_vShapes = new List<ScObject>();
            m_vExports = new List<ScObject>();
            m_vMovieClips = new List<ScObject>();
            m_vPendingChanges = new List<ScObject>();
            m_vFileName = fileName;
        }

        public void AddChange(ScObject sco)
        {
            if(m_vPendingChanges.IndexOf(sco) == -1)
                m_vPendingChanges.Add(sco);
        }

        public void AddExport(Export e)
        {
            m_vExports.Add(e);
        }

        public void AddShape(Shape s)
        {
            m_vShapes.Add(s);
        }

        public void AddTexture(Texture t)
        {
            m_vTextures.Add(t);
        }

        public void AddMovieClip(MovieClip mc)
        {
            m_vMovieClips.Add(mc);
        }

        public void Decode()
        {
            using (var br2 = new BinaryReader(File.Open(m_vFileName + "_tex", FileMode.Open)))
            {
                try
                {
                    while (true)
                    {
                        byte fileType = br2.ReadByte();
                        uint fileSize = br2.ReadUInt32();
                        var texture = new Texture(this);
                        texture.SetOffset(0);
                        texture.ParseData2(br2, fileType);
                        m_vTextures.Add(texture);
                    }
                }
                catch(EndOfStreamException e)
                {

                }
            }
            using (var br = new BinaryReader(File.Open(m_vFileName, FileMode.Open)))
            {
                ushort shapeCount = br.ReadUInt16();//a1 + 8
                ushort movieClipCount = br.ReadUInt16();//a1 + 12
                ushort textureCount = br.ReadUInt16();//a1 + 16
                ushort textFieldCount = br.ReadUInt16();//a1 + 24
                ushort matrix2x3Count = br.ReadUInt16();//a1 + 28
                ushort colorTransformCount = br.ReadUInt16();//a1 + 32

                Log("ShapeCount: " + shapeCount);
                Log("MovieClipCount: " + movieClipCount);
                Log("TextureCount: " + textureCount + " -- " + m_vTextures.Count);
                Log("TextFieldCount: " + textFieldCount);
                Log("Matrix2x3Count: " + matrix2x3Count);
                Log("ColorTransformCount: " + colorTransformCount);

                //5 useless bytes, not even used by Supercell
                br.ReadByte();//1 octet
                br.ReadUInt16();//2 octets
                br.ReadUInt16();//2 octets

                m_vStartExportsOffset = br.BaseStream.Position;
                m_vExportCount = br.ReadUInt16();//a1 + 20
                Log("ExportCount: " + m_vExportCount);

                for (int i = 0; i < m_vExportCount; i++)
                {
                    var sce = new Export(this);
                    sce.SetId(br.ReadInt16());
                    m_vExports.Add(sce);
                }

                //m_vExportNames = new string[m_vExportCount];
                for (int i = 0; i < m_vExportCount; i++)
                {
                    //((Export)m_vExports[i]).SetOffset(br.BaseStream.Position);
                    byte nameLength = br.ReadByte();
                    ((Export)m_vExports[i]).SetExportName(Encoding.UTF8.GetString(br.ReadBytes(nameLength)));
                }
                while(0x1A != br.ReadByte())
                {
                    br.ReadInt32();
                }
                br.ReadInt32();
                do
                {
                    long offset = br.BaseStream.Position;
                    byte dataType = br.ReadByte();
                    int dataLength = br.ReadInt32();
                    switch (dataType)
                    {
                        case 1:
                        case 16:
                        case 19:
                            {
                                var texture = new Texture(this);
                                texture.SetOffset(offset);
                                texture.ParseData(br);
                                //m_vTextures.Add(texture);
                                textureCount--;
                                continue;
                            }
                        case 24:
                            {
                                var texture = new Texture(this);
                                texture.SetOffset(offset);
                                texture.ParseData(br);
                                //m_vTextures.Add(texture);
                                textureCount--;
                                continue;
                            }
                        case 2:
                        case 18:
                            {
                                var shape = new Shape(this);
                                shape.SetOffset(offset);
                                m_vShapes.Add(shape);
                                shape.ParseData(br);
                                shapeCount--;
                                continue;
                            }
                        case 3:
                        case 10:
                        case 12:
                        case 14:
                            {
                                var movieClip = new MovieClip(this, dataType);
                                movieClip.SetOffset(offset);
                                movieClip.ParseData(br);
                                m_vMovieClips.Add(movieClip);
                                movieClipCount--;
                                continue;
                            }
                        case 7:
                        case 15:
                        case 20:
                        case 25:
                            {
                                //textFields
                                textFieldCount--;
                                break;
                            }
                        case 8:
                            {
                                //matrix2x3
                                matrix2x3Count--;
                                break;
                            }
                        case 9:
                            {
                                //colorTransform
                                colorTransformCount--;
                                break;
                            }
                        case 13:
                            {
                                Log("*** data type " + dataType.ToString());
                                break;
                            }
                        default:
                            {
                                Log("Unkown data type " + dataType.ToString());
                                break;
                            }
                        case 0:
                            {
                                m_vEofOffset = offset;
                                for (int i = 0; i < m_vExports.Count; i++)
                                {
                                    int index = m_vMovieClips.FindIndex(movie => movie.GetId() == m_vExports[i].GetId());
                                    if (index != -1)
                                        ((Export)m_vExports[i]).SetDataObject((MovieClip)m_vMovieClips[index]);
                                }

                                Log("-----------------------------------");
                                Log("ShapeCount: " + shapeCount);
                                Log("MovieClipCount: " + movieClipCount);
                                Log("TextureCount: " + textureCount);
                                Log("TextFieldCount: " + textFieldCount);
                                Log("Matrix2x3Count: " + matrix2x3Count);
                                Log("ColorTransformCount: " + colorTransformCount);
                                return;
                            }
                    }
                    //Log("Unhandled data type " + dataType.ToString());
                    if (dataLength >= 1)
                    {
                        br.ReadBytes(dataLength);
                    }
                }
                while (true);

            }
        }

        public long GetEofOffset()
        {
            return m_vEofOffset;
        }

        public List<ScObject> GetExports()
        {
            return m_vExports;
        }

        public string GetFileName()
        {
            return m_vFileName;
        }

        public List<ScObject> GetMovieClips()
        {
            return m_vMovieClips;
        }

        public List<ScObject> GetShapes()
        {
            return m_vShapes;
        }

        public long GetStartExportsOffset()
        {
            return m_vStartExportsOffset;
        }

        public List<ScObject> GetTextures()
        {
            return m_vTextures;
        }

        public void Save(FileStream input)
        {
            List<ScObject> exports = new List<ScObject>();
            foreach(ScObject data in m_vPendingChanges)
            {
                if (data.GetDataType() == 7)
                    exports.Add(data);
                else
                    data.Save(input);
            }
            m_vPendingChanges.Clear();
            if(exports.Count > 0)
            {
                foreach (ScObject data in exports)
                {
                    data.Save(input);
                }
            }
            //saving metadata
            input.Seek(0, SeekOrigin.Begin);
            input.Write(BitConverter.GetBytes((ushort)m_vShapes.Count),0,2);
            input.Write(BitConverter.GetBytes((ushort)m_vMovieClips.Count), 0, 2);
            input.Write(BitConverter.GetBytes((ushort)m_vTextures.Count), 0, 2);
        }

        public void SetEofOffset(long offset)
        {
            m_vEofOffset = offset;
        }

        public void SetStartExportsOffset(long offset)
        {
            m_vStartExportsOffset = offset;
        }

        private void Log(string content)
        {
            Debug.WriteLine(content);
        }
    }
}
