using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vector.App.Models;

namespace Vector.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private enum ShapeType
        {
            None, Line, Rectangle, Triangle, Ellipse
        }

        private ColorsList colors;

        private ColorInfo defaultStrokeColor;
        private ColorInfo defaultFillColor;

        private ShapeType selectedShapeType = ShapeType.None;

        private Shape selectedShape;

        private int selectedShapeZindex = 0;
        public int SelectedShapeZindex
        {
            get
            {
                return this.selectedShapeZindex;
            }
            set
            {
                this.selectedShapeZindex = value;
                if (this.selectedShape != null)
                    Panel.SetZIndex(this.selectedShape, value);

                OnPropertyChanged(nameof(SelectedShapeZindex));
            }
        }

        private ColorInfo selectedFillColor;

        private ColorInfo selectedStrokeColor;

        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();

            this.colors = ColorsList.Instance;
            this.defaultStrokeColor = colors[Colors.Black];
            this.defaultFillColor = colors[Colors.Transparent];

            this.FillColor.ItemsSource = colors.List;
            this.StrokeColor.ItemsSource = colors.List;

            this.StrokeColor.SelectedItem = defaultStrokeColor;
            this.FillColor.SelectedItem = defaultFillColor;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e) => selectedShapeType = ShapeType.None;

        private void LineButton_Click(object sender, RoutedEventArgs e) => selectedShapeType = ShapeType.Line;

        private void RectangleButton_Click(object sender, RoutedEventArgs e) => selectedShapeType = ShapeType.Rectangle;

        private void TriangleButton_Click(object sender, RoutedEventArgs e) => selectedShapeType = ShapeType.Triangle;

        private void EllipseButton_Click(object sender, RoutedEventArgs e) => selectedShapeType = ShapeType.Ellipse;

        private Point start;
        private Point? end;

        private void VectorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.start = e.GetPosition(this.VectorCanvas);
        }

        private void VectorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.end == null)
                return;

            switch (selectedShapeType)
            {
                case ShapeType.Line:
                    this.DrawLine();
                    break;
                case ShapeType.Rectangle:
                    this.DrawRectangle();
                    break;
                case ShapeType.Triangle:
                    this.DrawTriangle();
                    break;
                case ShapeType.Ellipse:
                    this.DrawEllipse();
                    break;
                default:
                    break;
            }

            this.end = null;
        }

        private void VectorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.end = e.GetPosition(this.VectorCanvas);
            }
        }

        private void DrawLine()
        {
            Line line = new Line
            {
                Stroke = new SolidColorBrush(this.selectedStrokeColor.Color),
                StrokeThickness = 3,
                Fill = new SolidColorBrush(this.selectedFillColor.Color),
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.Value.X,
                Y2 = end.Value.Y,
            };

            this.selectedShape = line;

            line.MouseLeftButtonUp += Shape_Selected;

            line.MouseMove += Shape_MouseMove;

            this.VectorCanvas.Children.Add(line);
        }

        private void DrawEllipse()
        {
            Ellipse ellipse = new Ellipse()
            {
                Stroke = new SolidColorBrush(this.selectedStrokeColor.Color),
                StrokeThickness = 3,
                Fill = new SolidColorBrush(this.selectedFillColor.Color)
            };

            // If the user the user tries to draw from
            // any direction then down and to the right
            // you'll get an error unless you use if
            // to change Left & TopProperty and Height
            // and Width accordingly

            if (this.end.Value.X >= this.start.X)
            {
                // Defines the left part of the ellipse
                ellipse.SetValue(Canvas.LeftProperty, this.start.X);
                ellipse.Width = this.end.Value.X - this.start.X;
            }
            else
            {
                ellipse.SetValue(Canvas.LeftProperty, this.end.Value.X);
                ellipse.Width = start.X - this.end.Value.X;
            }

            if (end.Value.Y >= start.Y)
            {
                ellipse.SetValue(Canvas.TopProperty, this.start.Y);
                ellipse.Height = this.end.Value.Y - start.Y;
            }
            else
            {
                ellipse.SetValue(Canvas.TopProperty, this.end.Value.Y);
                ellipse.Height = this.start.Y - this.end.Value.Y;
            }

            this.selectedShape = ellipse;

            ellipse.MouseLeftButtonUp += Shape_Selected;

            ellipse.MouseMove += Shape_MouseMove;

            this.VectorCanvas.Children.Add(ellipse);
        }

        private void DrawTriangle()
        {
            Polygon polygon = new Polygon
            {
                Stroke = new SolidColorBrush(this.selectedStrokeColor.Color),
                StrokeThickness = 3,
                Fill = new SolidColorBrush(this.selectedFillColor.Color)
            };

            bool drawFromLeftToRight = this.start.X <= this.end.Value.X;

            int repositionIndex = -1;

            if (drawFromLeftToRight)
                repositionIndex = 1;

            Point a = new Point
            {
                X = this.start.X + repositionIndex * (Math.Abs(this.end.Value.X - this.start.X) / 2),
                Y = this.start.Y,
            };

            Point b = new Point
            {
                X = this.end.Value.X,
                Y = this.end.Value.Y
            };

            Point c = new Point
            {
                X = this.end.Value.X - repositionIndex * (Math.Abs(this.end.Value.X - a.X) * 2),
                Y = this.end.Value.Y
            };

            PointCollection points = new PointCollection();
            points.Add(a);
            points.Add(b);
            points.Add(c);

            polygon.Points = points;

            this.selectedShape = polygon;

            polygon.MouseLeftButtonUp += Shape_Selected;

            polygon.MouseMove += Shape_MouseMove;

            this.VectorCanvas.Children.Add(polygon);
        }

        private void DrawRectangle()
        {
            Rectangle rectangle = new Rectangle
            {
                Stroke = new SolidColorBrush(this.selectedStrokeColor.Color),
                StrokeThickness = 3,
                Fill = new SolidColorBrush(this.selectedFillColor.Color)
            };

            if (this.end.Value.X >= this.start.X)
            {
                // Defines the left part of the rectangle
                rectangle.SetValue(Canvas.LeftProperty, this.start.X);
                rectangle.Width = this.end.Value.X - this.start.X;
            }
            else
            {
                rectangle.SetValue(Canvas.LeftProperty, this.end.Value.X);
                rectangle.Width = this.start.X - this.end.Value.X;
            }

            if (this.end.Value.Y >= this.start.Y)
            {
                // Defines the top part of the rectangle
                rectangle.SetValue(Canvas.TopProperty, this.start.Y);
                rectangle.Height = this.end.Value.Y - this.start.Y;
            }
            else
            {
                rectangle.SetValue(Canvas.TopProperty, this.end.Value.Y);
                rectangle.Height = this.start.Y - this.end.Value.Y;
            }

            this.selectedShape = rectangle;

            rectangle.MouseLeftButtonUp += Shape_Selected;

            rectangle.MouseMove += Shape_MouseMove;

            this.VectorCanvas.Children.Add(rectangle);
        }

        private void Shape_Selected(object sender, MouseButtonEventArgs e)
        {
            if (this.selectedShapeType != ShapeType.None)
                return;

            this.selectedShape = (Shape)sender;

            this.FillColor.SelectedValue = this.colors[(this.selectedShape.Fill as SolidColorBrush).Color];
            this.StrokeColor.SelectedValue = this.colors[(this.selectedShape.Stroke as SolidColorBrush).Color];

            this.SelectedShapeZindex = Panel.GetZIndex(this.selectedShape);
        }

        private void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Source is Shape shape && this.selectedShapeType == ShapeType.None)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point p = e.GetPosition(this.VectorCanvas);
                    Canvas.SetLeft(shape, p.X - shape.ActualWidth);
                    Canvas.SetTop(shape, p.Y - shape.ActualHeight);
                    shape.CaptureMouse();
                }
                else
                {
                    shape.ReleaseMouseCapture();
                }
            }
        }

        private void FillColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedFillColor = (ColorInfo)this.FillColor.SelectedItem;

            if (selectedShape != null)
                this.selectedShape.Fill = new SolidColorBrush(this.selectedFillColor.Color);
        }

        private void StrokeColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedStrokeColor = (ColorInfo)this.StrokeColor.SelectedItem;

            if (selectedShape != null)
                this.selectedShape.Stroke = new SolidColorBrush(this.selectedStrokeColor.Color);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedShape != null)
            {
                this.VectorCanvas.Children.Remove(this.selectedShape);
                this.selectedShape = null;
            }
        }
    }
}
