using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;

namespace ucssceditor
{
    public struct PixelPoint
    {
        public int x;
        public int y;
    }
    public struct VetexPair
    {
        public int indexA;
        public int indexB;
    }

    public class Polygon
    {
        public Point[] points;
        public VetexPair[] pairs;
        public Polygon(Point[] points, VetexPair[] pairs)
        {
            this.pairs = pairs;
            this.points = points;
        }

        public int GetLineCount()
        {
            return pairs.Length;
        }

        public bool GetLine(int index, Point[] ends)
        {
            if(index < 0 || index >= pairs.Length)return false;
            ends[0] = this.points[this.pairs[index].indexA];
            ends[1] = this.points[this.pairs[index].indexB];
            return true;
        }
    }

    public class Region
    {
        public Rectangle bound;
        public List<int>[] ends;

        public Region(Rectangle bound, Polygon polygon)
        {
            this.bound = bound;
            this.ends = new List<int>[bound.Height];
            InitializeByPolygon(polygon);
        }

        private void InitializeByPolygon(Polygon polygon)
        {
            Point[] line = new Point[2];
            int lineCount = polygon.GetLineCount();
            for (int i = 0; i < lineCount; i++)
            {
                polygon.GetLine(i, line);
                AddLine(line);
            }
            SortEnds();
        }

        private void AddLine(Point[] ends)
        {

        }
        public void SortEnds()
        {

        }
    }

    class FillPolygon
    {
        const int IMAGE_TOP = 0;
        const int IMAGE_BOT = 0;
        const int MAX_POLY_CORNERS = 100;
        const int IMAGE_RIGHT = 0;
        const int IMAGE_LEFT = 100;

        public void Calculate(Bitmap image, System.Drawing.Region region)
        {

        }

        //  public-domain code by Darel Rex Finley, 2007

        public void Calculate(float[] polyX, float[] polyY, int polyCorners)
        {
            int nodes, pixelX, pixelY, i, j, swap;
            int[] nodeX = new int[MAX_POLY_CORNERS];

            //  Loop through the rows of the image.
            for (pixelY = IMAGE_TOP; pixelY < IMAGE_BOT; pixelY++)
            {

                //  Build a list of nodes.
                nodes = 0; j = polyCorners - 1;
                for (i = 0; i < polyCorners; i++)
                {
                    if (polyY[i] < (double)pixelY && polyY[j] >= (double)pixelY
                    || polyY[j] < (double)pixelY && polyY[i] >= (double)pixelY)
                    {
                        nodeX[nodes++] = (int)(polyX[i] + (pixelY - polyY[i]) / (polyY[j] - polyY[i])
                        * (polyX[j] - polyX[i]));
                    }
                    j = i;
                }

                //  Sort the nodes, via a simple “Bubble” sort.
                i = 0;
                while (i < nodes - 1)
                {
                    if (nodeX[i] > nodeX[i + 1])
                    {
                        swap = nodeX[i]; nodeX[i] = nodeX[i + 1]; nodeX[i + 1] = swap; if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }

                //  Fill the pixels between node pairs.
                for (i = 0; i < nodes; i += 2)
                {
                    if (nodeX[i] >= IMAGE_RIGHT) break;
                    if (nodeX[i + 1] > IMAGE_LEFT)
                    {
                        if (nodeX[i] < IMAGE_LEFT) nodeX[i] = IMAGE_LEFT;
                        if (nodeX[i + 1] > IMAGE_RIGHT) nodeX[i + 1] = IMAGE_RIGHT;
                        for (pixelX = nodeX[i]; pixelX < nodeX[i + 1]; pixelX++) fillPixel(pixelX, pixelY);
                    }
                }
            }

        }

        private void fillPixel(int pixelX, int pixelY)
        {

        }


    }

}
