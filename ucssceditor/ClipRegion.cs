using System;
using System.Collections.Generic;
using System.Drawing;

namespace ucssceditor
{
    public struct VetexPair
    {
        public int indexA;
        public int indexB;
        public VetexPair(int a, int b)
        {
            this.indexA = a;
            this.indexB = b;
        }
    }

    public class Polygon
    {
        public PointF[] points;
        public VetexPair[] lines;
        public Polygon(PointF[] points, VetexPair[] lines)
        {
            this.lines = lines;
            this.points = points;
        }

        public int GetLineCount()
        {
            return lines.Length;
        }

        public bool GetLine(int index, PointF[] lineEnds)
        {
            if (index < 0 || index >= lines.Length) return false;
            lineEnds[0] = this.points[this.lines[index].indexA];
            lineEnds[1] = this.points[this.lines[index].indexB];
            return true;
        }
    }

    public class ClipRegion
    {
        public Rectangle bound;
        public List<int>[] ends;

        public ClipRegion(Rectangle bound, Polygon polygon)
        {
            this.bound = bound;
            this.ends = new List<int>[bound.Height];
            for(int i = 0; i < bound.Height; i++)
            {
                this.ends[i] = new List<int>();
            }
            InitializeByPolygon(polygon);
        }

        public Bitmap ClipBitmap(Bitmap source)
        {
            Bitmap bmp = new Bitmap(this.bound.Width, this.bound.Height);
            for (int y = 0; y < this.bound.Height; y++)
            {
                for (int i = 0; i < this.ends[y].Count - 1; i += 2)
                {
                    int x1 = this.ends[y][i];
                    int x2 = this.ends[y][i + 1];
                    for (int x = x1; x < x2; x++)
                    {
                        Color color = source.GetPixel(x, this.bound.Top + y);
                        bmp.SetPixel(x - this.bound.X, y, color);
                    }
                    bmp.SetPixel(x1 - this.bound.X, y, Color.Red);
                    bmp.SetPixel(x2 - this.bound.X, y, Color.Red);
                }
            }
            return bmp;
        }

        private void InitializeByPolygon(Polygon polygon)
        {
            PointF[] line = new PointF[2];
            int lineCount = polygon.GetLineCount();
            for (int i = 0; i < lineCount; i++)
            {
                polygon.GetLine(i, line);
                AddLine(line);
            }
            SortEnds();
            FitBound();
        }

        private void AddLine(PointF[] vertexs)
        {
            int minIndex, maxIndex;
            if (vertexs[0].Y > vertexs[1].Y)
            {
                minIndex = 1;
                maxIndex = 0;
            }
            else
            {
                minIndex = 0;
                maxIndex = 1;
            }
            int minY = (int)Math.Floor((double)vertexs[minIndex].Y);
            int maxY = (int)Math.Floor((double)vertexs[maxIndex].Y);
            minY = Math.Max(bound.Top, minY);
            maxY = Math.Min(bound.Bottom - 1, maxY);
            if (minY > maxY) return;
            if (minY == maxY)
            {
                this.ends[minY - bound.Top].Add((int)Math.Floor(vertexs[minIndex].X));
                //this.ends[minY - bound.Top].Add((int)Math.Floor(vertexs[maxIndex].X));
                return;
            }
            for (int y = minY; y < maxY; y++)
            {
                float fX = vertexs[minIndex].X + (y - vertexs[minIndex].Y) * (vertexs[maxIndex].X - vertexs[minIndex].X) / (vertexs[maxIndex].Y - vertexs[minIndex].Y);
                this.ends[y - bound.Top].Add((int)Math.Floor(fX));
            }
        }
        private void SortEnds()
        {
            for (int i = 0; i < this.ends.Length; i++)
            {
                this.ends[i].Sort(comparer);
            }
        }
        private void FitBound()
        {
            for (int i = 0; i < this.ends.Length; i++)
            {
                LineFitBount(this.ends[i]);
            }
        }

        private void LineFitBount(List<int> row)
        {
            bool flag = false;
            while (row.Count > 0)
            {
                if (row[0] < this.bound.Left)
                {
                    flag = !flag;
                    row.RemoveAt(0);
                    continue;
                }
                int i = 0;
                if (flag)
                {
                    row.Insert(0, this.bound.Left);
                    i++;
                }
                for (; i < row.Count; i++)
                {
                    if (row[i] >= this.bound.Right-1)
                    {
                        row.RemoveRange(i, row.Count - i);
                        if (flag)
                        {
                            row.Add(this.bound.Right-1);
                        }
                        break;
                    }
                    else
                    {
                        flag = !flag;
                    }
                }
                break;
            }
        }

        private int comparer(int a, int b)
        {
            return a - b;
        }
    }

}
