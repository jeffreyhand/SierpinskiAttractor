//  Jeff Hand
//  MainWindow.xaml.cs


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Attractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        const int maxControlPoints = 10;

        private string selectedShape;   // shape selected on the form
        private int selectedSize;   // size selected on the form
        private Brush selectedColor;   // color selected on form
        private int maxShapes;   // number of total shapes selected on the form
        private int controlsElem;   // the currently selected control point within the controls List
        private Shape relocateControlPoint;   // a control point selected to be relocated
        private List<SAPoint> controls = new List<SAPoint>();
        private bool _isCanvasDrawn;   // user has submitted the form to draw the attractor
        private bool _isRelocatingControl;   // track while dragging a control point to new location
        private bool _isUserSpecifiedTotal;   // user has specified total shapes to draw, do not automatically increase scale with every control point 
        private Random random;


        public MainWindow()
        {
            InitializeComponent();

            random = new Random();

            selectedShape = "ellipse";
            selectedSize = 2;
            selectedColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            maxShapes = 2500;
            _isCanvasDrawn = false;
            _isRelocatingControl = false;
            _isUserSpecifiedTotal = false;
            controlsElem = 0;
        }




        /// <summary>
        /// Recursive method called by the canvasRedraw() helper method
        /// cp: the index for the control point from the list
        /// x: the constantly recalculated midpoint left coordinate
        /// y: the constantly recalculated midpoint top coordinate
        /// n: a decremented counter for the number of interations
        /// </summary>
        void updateShapes(int cp, int x, int y, int n)
        {
            // base case: draw shape based on last iteration's color and size of starting control point
            if (n == 0)
            {
                if (controls[cp].shape.Equals("rectangle"))
                {
                    Shape s = new Rectangle
                    {
                        Stroke = controls[cp].color,
                        Fill = controls[cp].color,
                        StrokeThickness = controls[cp].size
                    };
                    Canvas.SetLeft(s, x + 5);   // offset by 5px to match 5px offset (midpoint) of all control points
                    Canvas.SetTop(s, y + 5);   // offset by 5px to match 5px offset (midpoint) of all control points
                    canvas.Children.Add(s);
                }
                if (controls[cp].shape.Equals("ellipse"))
                {
                    Shape s = new Ellipse
                    {
                        Stroke = controls[cp].color,
                        Fill = controls[cp].color,
                        StrokeThickness = controls[cp].size
                    };
                    Canvas.SetLeft(s, x + 5);   // offset by 5px to match 5px offset (midpoint) of all control points
                    Canvas.SetTop(s, y + 5);   // offset by 5px to match 5px offset (midpoint) of all control points
                    canvas.Children.Add(s);
                }

                return;
            }
            else   // recursion continues to create midpoints and decrements number of iterations (n)
            {
                int randNum = random.Next(0, controls.Count);

                if (n == 1)
                {
                    cp = randNum;
                }

                int midPointX = (controls[randNum].X + x) / 2;
                int midPointY = (controls[randNum].Y + y) / 2;

                updateShapes(cp, midPointX, midPointY, n - 1);

            }

        }


        /// <summary>
        /// Helper method called by Event Listeners that places shapes between control points
        /// a random number of iterations chosen between 1 and 8 and passed into the updateShapes() method
        /// </summary>
        private void canvasRedraw()
        {
            int count = controls.Count;
            int m = 0;
            bool shapesMax = false;
            do
            {
                for (int i = 0; i < count; i++)   // traverse through each control point in the list one at a time
                {
                    if (m < maxShapes)   // ensure that it does not iterate beyond the indicated number of shapes to draw
                    {
                        int currX = controls[i].X;
                        int currY = controls[i].Y;

                        int randControl = random.Next(0, controls.Count);

                        int randIterations = random.Next(1, 8);  // # of iterations determines the shape's location between control points

                        int midPointX = (controls[randControl].X + currX) / 2;
                        int midPointY = (controls[randControl].Y + currY) / 2;

                        updateShapes(-1, midPointX, midPointY, randIterations);   // initially set control point to -1, it is determined randomly in updateShapes() method
                        m++;
                    }
                    else
                    {
                        shapesMax = true;
                    }
                }

            } while (!shapesMax);   // continue until the toal number of shapes to draw has been reached

        }



        /// <summary>
        /// Event Listener for canvas MouseLeftButtonUp event 
        /// Raised when user trying to add a new control point on the canvas
        /// </summary>
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if ((controls.Count == maxControlPoints))
            {
                MessageBox.Show("You can only add up to 10 control points");   // alert user that maximum number of control points were created
            }

            // less than 10 control points and the user isn't instead relocating an existing control point
            if ((controls.Count < maxControlPoints) && (_isRelocatingControl == false))
            {
                Point startPoint;
                Shape controlPoint;

                startPoint = e.GetPosition(canvas);   // get location to place control point on canvas

                int left = (int)startPoint.X;
                int top = (int)startPoint.Y;

                // create new control point based on form's values
                SAPoint cp = new SAPoint(selectedSize, selectedShape, selectedColor, left, top);

                controlPoint = cp.getPoint;

                // attach listener to control point
                controlPoint.MouseLeftButtonDown += point_MouseLeftButtonDown;

                Canvas.SetLeft(controlPoint, left);
                Canvas.SetTop(controlPoint, top);

                canvas.Children.Add(controlPoint);

                Canvas.SetZIndex(controlPoint, 500);   // set z value so control point is on top of shapes drawn under it

                // add point to the list of controls points
                controls.Add(cp);

            }


            // if minimum of control points (3) are placed 
            if ((controls.Count > 2))
            {
                // user has not specified that they want a custom value, so adjust it for ever control point
                if (_isUserSpecifiedTotal == false)
                {
                    int newTotal = 1250 + (controls.Count * 250);
                    totalShapes.Text = newTotal.ToString();
                }

                // enable run and clear buttons and adjust 
                resetButton.IsEnabled = true;
                submitButton.IsEnabled = true;
            }

            _isRelocatingControl = false;
        }




        /// <summary>
        /// Event Listener for left mouse click down on control point to move it
        /// </summary>
        private void point_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var mousePos = e.GetPosition(canvas);

            relocateControlPoint = sender as Shape;

            double x = Canvas.GetLeft(relocateControlPoint);
            double y = Canvas.GetTop(relocateControlPoint);

            // check list of controls backwards (in case a control point was on top of another one in that order)
            for (int i = controls.Count - 1; i >= 0; i--)
            {
                if ((x.Equals(controls[i].X)) && (y.Equals(controls[i].Y)))
                {
                    controlsElem = i;
                }
            }

            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseLeave += canvas_MouseLeave;
            canvas.MouseLeftButtonUp += canvas_MouseLeftButtonUp;

            _isRelocatingControl = true;

        }


        /// <summary>
        /// Event Listener for dragging control point
        /// </summary>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {

            this.Cursor = Cursors.Arrow;

            var mousePos = e.GetPosition(canvas);

            double left = mousePos.X;
            double top = mousePos.Y;

            Canvas.SetLeft(relocateControlPoint, left - 5.0);   // ?? -5.0
            Canvas.SetTop(relocateControlPoint, top - 5.0); // ?? -5.0

        }


        /// <summary>
        /// Event Listener for left mouse click up on control point to reposition it
        /// </summary>
        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            // get the position of the mouse within the canvas
            var mousePos = e.GetPosition(canvas);
            double left = mousePos.X - 5.0;     // - 5 ????
            double top = mousePos.Y - 5.0;     // - 5 ????

            controls[controlsElem].X = (int)left;
            controls[controlsElem].Y = (int)top;

            Canvas.SetLeft(relocateControlPoint, left);
            Canvas.SetTop(relocateControlPoint, top);   // - 5 ????

            if (_isCanvasDrawn)
            {
                Mouse.OverrideCursor = Cursors.Wait;   // cursor becomes "wait" so user knows it is still processing

                // there must be at least three control points to draw shapes
                if (controls.Count >= 3)
                {
                    var cShapes = canvas.Children.OfType<Shape>().ToList();
                    foreach (var s in cShapes)
                    {
                        if (s.StrokeThickness < 10)   // faster to remove all shapes (ignoring the few 10-point control points)
                        {
                            canvas.Children.Remove(s);
                        }
                    }

                    canvasRedraw();

                }

                Mouse.OverrideCursor = Cursors.Arrow;   // return cursor to normal
            }

            canvas.MouseLeave -= canvas_MouseLeave;
            canvas.MouseMove -= canvas_MouseMove;
            canvas.MouseLeftButtonUp -= canvas_MouseLeftButtonUp;

            e.Handled = true;  // prevent bubble up to canvas left mosue up
        }


        /// <summary>
        /// Event Listener to release all listeners when mouse leaves canvas area
        /// </summary>
        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {

            // get the position of the mouse within the canvas
            var mousePos = e.GetPosition(canvas);
            double left = mousePos.X;
            double top = mousePos.Y;

            Canvas.SetLeft(controls[controlsElem].getPoint, controls[controlsElem].X);
            Canvas.SetTop(controls[controlsElem].getPoint, controls[controlsElem].Y);

            canvas.MouseLeave -= canvas_MouseLeave;
            canvas.MouseMove -= canvas_MouseMove;
            canvas.MouseLeftButtonUp -= canvas_MouseLeftButtonUp;

        }



        /// <summary>
        /// Event Listener for menu item clicks
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            MenuItem item = (MenuItem)sender;

            this.Title = "Info: " + item.Header;

            if (item.Header.Equals("Instructions"))
            {
                MessageBox.Show("Select a Shape, Size, Color, and Quantity for control points\n\n");
            }

            if (item.Header.Equals("About"))
            {
                MessageBox.Show("Application:  Attractor\n\nAuthor:  Jeff Hand\n\n");
            }

            if (item.Header.Equals("Exit"))
            {
                Application.Current.Shutdown();
            }

        }



        /// <summary>
        /// Event Listener for shape option mouse rollover enter
        /// </summary>
        private void ShapeSelector_MouseEnter(object sender, RoutedEventArgs e)
        {
            // if mouse over Ellipse option and rectangle is currently selected
            if ((((Canvas)sender).Name.Equals("ShapeEllipse")) && !(selectedShape.Equals("ellipse")))
            {
                ShapeEllipse.Background = new SolidColorBrush(Colors.LightBlue);
            }
            // if mouse over Rectangle option and ellipse is currently selected
            if ((((Canvas)sender).Name.Equals("ShapeRectangle")) && !(selectedShape.Equals("rectangle")))
            {
                ShapeRectangle.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }



        /// <summary>
        /// Event Listener for shape option mouse rollover leave
        /// </summary>
        private void ShapeSelector_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (selectedShape.Equals("ellipse"))
            {
                ShapeEllipse.Background = new SolidColorBrush(Colors.Yellow);
                ShapeRectangle.Background = new SolidColorBrush(Colors.White);
            }
            if (selectedShape.Equals("rectangle"))
            {
                ShapeRectangle.Background = new SolidColorBrush(Colors.Yellow);
                ShapeEllipse.Background = new SolidColorBrush(Colors.White);
            }
        }



        /// <summary>
        /// Event Listener for Updating total number of shapes to be drawn slider and value
        /// </summary>
        private void TotalShapes_ValueChanged(object sender, RoutedEventArgs e)
        {
            maxShapes = Int32.Parse(totalShapes.Text);
        }



        /// <summary>
        /// Event Listener for shape option selected by mouse click which also changes display icons of point size
        /// </summary>
        private void ShapeSelector_MouseDown(object sender, RoutedEventArgs e)
        {
            Brush ptColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));

            if (((Canvas)sender).Name.Equals("ShapeEllipse"))  // if ellipse checked, turn preview shape sizes into ellipses
            {
                ShapeEllipse.Background = new SolidColorBrush(Colors.Yellow);
                ShapeRectangle.Background = new SolidColorBrush(Colors.White);
                selectedShape = "ellipse";

                // hide rectangle icons and show ellipse icons in size section
                ellipse2.Visibility = Visibility.Visible;
                ellipse4.Visibility = Visibility.Visible;
                ellipse6.Visibility = Visibility.Visible;
                rectangle2.Visibility = Visibility.Hidden;
                rectangle4.Visibility = Visibility.Hidden;
                rectangle6.Visibility = Visibility.Hidden;
            }

            if (((Canvas)sender).Name.Equals("ShapeRectangle"))  // if rectangle checked, turn preview shape sizes into rectangles
            {
                ShapeRectangle.Background = new SolidColorBrush(Colors.Yellow);
                ShapeEllipse.Background = new SolidColorBrush(Colors.White);
                selectedShape = "rectangle";

                // hide ellipse icons and show rectangle icons in size section
                rectangle2.Visibility = Visibility.Visible;
                rectangle4.Visibility = Visibility.Visible;
                rectangle6.Visibility = Visibility.Visible;
                ellipse2.Visibility = Visibility.Hidden;
                ellipse4.Visibility = Visibility.Hidden;
                ellipse6.Visibility = Visibility.Hidden;
            }

        }



        /// <summary>
        /// Event Listener for size option mouse rollover
        /// </summary>
        private void SizeSelector_MouseEnter(object sender, RoutedEventArgs e)
        {
            // if mouse over Size2 option and size2 is currently selected
            if ((((Canvas)sender).Name.Equals("Size2")) && !(selectedSize == 2))
            {
                Size2.Background = new SolidColorBrush(Colors.LightBlue);
            }
            // if mouse over Size4 option and size4 is currently selected
            if ((((Canvas)sender).Name.Equals("Size4")) && !(selectedSize == 4))
            {
                Size4.Background = new SolidColorBrush(Colors.LightBlue);
            }
            // if mouse over Size6 option and size6 is currently selected
            if ((((Canvas)sender).Name.Equals("Size6")) && !(selectedSize == 6))
            {
                Size6.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }



        /// <summary>
        /// Event Listener for shape option mouse rollover leave
        /// </summary>
        private void SizeSelector_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (selectedSize == 2)
            {
                Size2.Background = new SolidColorBrush(Colors.Yellow);
                Size4.Background = new SolidColorBrush(Colors.White);
                Size6.Background = new SolidColorBrush(Colors.White);
            }
            if (selectedSize == 4)
            {
                Size2.Background = new SolidColorBrush(Colors.White);
                Size4.Background = new SolidColorBrush(Colors.Yellow);
                Size6.Background = new SolidColorBrush(Colors.White);
            }
            if (selectedSize == 6)
            {
                Size2.Background = new SolidColorBrush(Colors.White);
                Size4.Background = new SolidColorBrush(Colors.White);
                Size6.Background = new SolidColorBrush(Colors.Yellow);
            }
        }



        /// <summary>
        /// Event Listener for size option selected by mouse click
        /// </summary>
        private void SizeSelector_MouseDown(object sender, RoutedEventArgs e)
        {
            if (((Canvas)sender).Name.Equals("Size2"))
            {
                Size2.Background = new SolidColorBrush(Colors.Yellow);
                Size4.Background = new SolidColorBrush(Colors.White);
                Size6.Background = new SolidColorBrush(Colors.White);
                selectedSize = 2;
            }
            if (((Canvas)sender).Name.Equals("Size4"))
            {
                Size2.Background = new SolidColorBrush(Colors.White);
                Size4.Background = new SolidColorBrush(Colors.Yellow);
                Size6.Background = new SolidColorBrush(Colors.White);
                selectedSize = 4;
            }
            if (((Canvas)sender).Name.Equals("Size6"))
            {
                Size2.Background = new SolidColorBrush(Colors.White);
                Size4.Background = new SolidColorBrush(Colors.White);
                Size6.Background = new SolidColorBrush(Colors.Yellow);
                selectedSize = 6;
            }
        }




        /// <summary>
        /// Event Listener for color options changed on form
        /// </summary>
        private void ColorSelection_Changed(object sender, EventArgs e)
        {
            // Get RGB sizes from form's color combo boxes
            try
            {
                byte red = Convert.ToByte(colorRed.Text);
                byte green = Convert.ToByte(colorGreen.Text);
                byte blue = Convert.ToByte(colorBlue.Text);

                Brush ptColor = new SolidColorBrush(Color.FromRgb(red, green, blue));

                selectedColor = ptColor;
                colorSample.Fill = ptColor;
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex); }
        }




        /// <summary>
        /// Event Listener for "Run" Button (draws attractor shapes)
        /// </summary>
        private void Button_drawAttractorPoints(object sender, RoutedEventArgs e)
        {
            // there must be at least three control points to draw shapes
            if (controls.Count >= 3)
            {

                Mouse.OverrideCursor = Cursors.Wait;   // cursor becomes "wait" so user knows it is still processing

                var cShapes = canvas.Children.OfType<Shape>().ToList();
                foreach (var s in cShapes)
                {
                    if (s.StrokeThickness < 10)   // faster to remove all shapes (ignoring the few 10-point control points)
                    {
                        canvas.Children.Remove(s);
                    }
                }

                int tempCount = Int32.Parse(totalShapes.Text);
                if (tempCount < 2500) { tempCount = 2500; }
                if (tempCount > 7000) { tempCount = 7000; }
                maxShapes = tempCount;

                canvasRedraw();

                Mouse.OverrideCursor = Cursors.Arrow;   // return cursor to normal

                _isCanvasDrawn = true;
            }

        }




        /// <summary>
        /// Event Listener to reset canvas
        /// </summary>
        private void Button_resetAttractorPoints(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            controls.Clear();    // reset list of control points
            resetButton.IsEnabled = false;
            submitButton.IsEnabled = false;
            _isCanvasDrawn = false;
        }



        private void totalShapesSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isUserSpecifiedTotal = true;
        }



        private void totalShapesSlider_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            _isUserSpecifiedTotal = true;
        }



    }



}
