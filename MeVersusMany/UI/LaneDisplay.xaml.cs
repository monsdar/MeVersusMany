using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MeVersusMany.UI
{
    /// <summary>
    /// Interaktionslogik für LaneDisplay.xaml
    /// </summary>
    public partial class LaneDisplay : UserControl
    {
        Polygon boat = new Polygon();

        public LaneDisplay()
        {
            InitializeComponent();

            var fill = BoatColor.Clone();
            fill.Opacity = 0.5;
            boat.Stroke = Brushes.Black;
            boat.Fill = fill;
            canvas.Children.Add(boat);
            canvas.Background = Brushes.AliceBlue;

            canvas.Loaded += Canvas_Loaded;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            //draw the boat after the canvas is loaded completely
            //NOTE: This is kind of a dirty hack... If we wouldn't recreate the whole Listbox-Items each frame we wouldn't need to redraw everything every frame... This is very performance heavy.
            HandleDistanceChanged();
        }

        public double CurrentDistance
        {
            get { return (double)GetValue(CurrentDistanceProperty); }
            set { SetValue(CurrentDistanceProperty, value); }
        }
        public static readonly DependencyProperty CurrentDistanceProperty =
            DependencyProperty.Register("CurrentDistance", typeof(double), typeof(LaneDisplay), new PropertyMetadata(0.0));

        public Brush WaterColor
        {
            get { return (Brush)GetValue(WaterColorProperty); }
            set { SetValue(WaterColorProperty, value); }
        }
        public static readonly DependencyProperty WaterColorProperty =
            DependencyProperty.Register("WaterColor", typeof(Brush), typeof(LaneDisplay), new PropertyMetadata(Brushes.Blue));
        public Brush BoatColor
        {
            get { return (Brush)GetValue(BoatColorProperty); }
            set { SetValue(BoatColorProperty, value); }
        }
        public static readonly DependencyProperty BoatColorProperty =
            DependencyProperty.Register("BoatColor", typeof(Brush), typeof(LaneDisplay), new PropertyMetadata(Brushes.Red));
        public double MaxDistance
        {
            get { return (double)GetValue(MaxDistanceProperty); }
            set { SetValue(MaxDistanceProperty, value); }
        }
        public static readonly DependencyProperty MaxDistanceProperty =
            DependencyProperty.Register("MaxDistance", typeof(double), typeof(LaneDisplay), new PropertyMetadata(10.0));
        public double BaseDistance
        {
            get { return (double)GetValue(BaseDistanceProperty); }
            set { SetValue(BaseDistanceProperty, value); }
        }
        public static readonly DependencyProperty BaseDistanceProperty =
            DependencyProperty.Register("BaseDistance", typeof(double), typeof(LaneDisplay), new PropertyMetadata(0.0));

        private IEnumerable<Line> GetLines(int lineDistance, double baseDistance, double maxDistance, double canvasWidth, double lineWidth)
        {
            var lines = new List<Line>();

            var minVisibleDistance = baseDistance - maxDistance;
            var maxVisibleDistance = baseDistance + maxDistance;
            var firstLine = (int)(minVisibleDistance / lineDistance) * lineDistance; //round to the nearest value dividable by lineDistance
            for (double currentDist = firstLine; currentDist <= maxVisibleDistance; currentDist += lineDistance)
            {
                var factor = (currentDist - minVisibleDistance) / (maxVisibleDistance - minVisibleDistance);
                var posX = factor * canvasWidth;
                var newLine = GetVerticalLine(posX, lineWidth);
                lines.Add(newLine);
                
            }
            return lines;
        }
        private void HandleDistanceChanged()
        {

            var canvasWidth = canvas.ActualWidth;
            var canvasHeight = canvas.ActualHeight;

            var smallLines = GetLines(10, BaseDistance, MaxDistance, canvasWidth, 1.0);
            foreach (var line in smallLines)
            {
                canvas.Children.Add(line);
            }
            var bigLines = GetLines(100, BaseDistance, MaxDistance, canvasWidth, 3.0);
            foreach (var line in bigLines)
            {
                canvas.Children.Add(line);
            }

            var lineDistance = 10;
            var minVisibleDistance = BaseDistance - MaxDistance;
            var maxVisibleDistance = BaseDistance + MaxDistance;
            var firstLine = (int)(minVisibleDistance / lineDistance) * lineDistance; //round to the nearest value dividable by lineDistance
            for (double currentDist = firstLine; currentDist <= maxVisibleDistance; currentDist+=lineDistance)
            {
                var factor = (currentDist-minVisibleDistance) / (maxVisibleDistance-minVisibleDistance);
                var posX = factor * canvasWidth;
                var newLine = GetVerticalLine(posX, 1);
                canvas.Children.Add(newLine);
            }

            //do not paint anything that is outside of our range to display
            if (Math.Abs(CurrentDistance) > MaxDistance)
            {
                return;
            }

            var currentPosPoint = (canvasWidth / 2.0) + ((CurrentDistance / MaxDistance) * (canvasWidth / 2.0));
            var actualBoatSize = 5.0;
            var canvasBoatSize = (actualBoatSize / (MaxDistance * 2.0)) * canvasWidth;
            var upperBounds = canvasHeight / 4.0;
            var lowerBounds = canvasHeight - (canvasHeight / 4.0);

            var points = new PointCollection();
            points.Add(new Point(currentPosPoint, canvasHeight / 2.0));
            points.Add(new Point(currentPosPoint - canvasBoatSize / 2.0, upperBounds));
            points.Add(new Point(currentPosPoint - canvasBoatSize, upperBounds));
            points.Add(new Point(currentPosPoint - canvasBoatSize, lowerBounds));
            points.Add(new Point(currentPosPoint - canvasBoatSize / 2.0, lowerBounds));
            boat.Points = points;

            canvas.InvalidateVisual();
        }

        private Line GetVerticalLine(double x, double width)
        {
            var line = new Line();
            line.X1 = x;
            line.Y1 = 0.0;
            line.X2 = x;
            line.Y2 = canvas.ActualHeight;

            line.StrokeThickness = width;
            line.Stroke = Brushes.Black;
            line.StrokeDashArray = new DoubleCollection(new List<double>(){ 1.0, 2.0 });

            // https://stackoverflow.com/questions/2879033/how-do-you-draw-a-line-on-a-canvas-in-wpf-that-is-1-pixel-thick
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            return line;
        }
    }
}
