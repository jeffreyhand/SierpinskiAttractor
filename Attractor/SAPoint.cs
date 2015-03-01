//  Jeff Hand
//  SAPoint.cs


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Attractor
{

    class SAPoint
    {


        /// <summary>
        /// Data Fields
        /// </summary>
        private int pointSize;
        private string pointShape;
        private Brush pointColor;
        private int coordinateX;
        private int coordinateY;

        private Ellipse ellipse;
        private Rectangle rectangle;

        /// <summary>
        /// Constructors
        /// </summary>
        public SAPoint() { }
        public SAPoint(int size, string shape, Brush color, int xPos, int yPos)
        {

            pointSize = size;
            pointShape = shape;
            pointColor = color;
            coordinateX = xPos;
            coordinateY = yPos;


            rectangle = new Rectangle
            {
                Stroke = pointColor,
                Fill = pointColor,
                StrokeThickness = size
            };


            ellipse = new Ellipse
            {
                Stroke = pointColor,
                Fill = pointColor,
                StrokeThickness = size
            };
        }

        /// <summary>
        /// Properties
        /// </summary>
        public int X
        {
            get { return coordinateX; }
            set { coordinateX = value; }
        }
        public int Y
        {
            get { return coordinateY; }
            set { coordinateY = value; }
        }
        public Brush color
        {
            get { return pointColor; }
            set { pointColor = value; }
        }
        public int size
        {
            get { return pointSize; }
            set { pointSize = value; }
        }
        public string shape
        {
            get { return pointShape; }
            set { pointShape = value; }
        }

        public Shape getPoint
        {
            get
            {
                if (shape.Equals("rectangle"))
                {
                    rectangle.Stroke = pointColor;
                    rectangle.Fill = pointColor;
                    rectangle.StrokeThickness = size + 8;  //  add 8px to designated shape size to make control point stand out
                    return rectangle;
                }
                else
                {
                    ellipse.Stroke = pointColor;
                    ellipse.Fill = pointColor;
                    ellipse.StrokeThickness = size + 8;  //  add 8px to designated shape size to make control point stand out
                    return ellipse;
                }

            }

        }

    }

}