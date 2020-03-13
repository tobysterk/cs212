using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FernNamespace {

    class Fern {
        private static int NUM_BASE_STALKS = 2; // number of offshoots at base of plant
        private static int NUM_OFFSHOOT_TENDRILS = 2; // number of offshoots at each offshoot site
        private static int TENDRILMIN = 20;
        private static int NUM_OFFSHOOTS = 7; // number of offshoot sites along tendril
        private static double DELTATHETA = 0.1; // backup change in angle
        private static double SEGLENGTH = 3.0; // length of a short line segment
        private static double REDUX = 3.0; // reduction in size for each offshoot

        /* 
         * Fern constructor erases screen and then draws a fern onto the canvas
         * 
         * plantSize: number of 3-pixel segments of tendrils
         * sunSize: how much smaller children clusters are compared to parents
         * turnbias: how likely to turn right vs. left (0=always left, 0.5 = 50/50, 1.0 = always right)
         * canvas: the canvas that the fern will be drawn on
         */
        public Fern(double plantSize, double sunSize, double turnbias, Canvas canvas) {
            canvas.Children.Clear(); // delete old canvas contents
            DrawSun(sunSize, canvas);
            DrawPot(canvas);
            // draw a new fern at the top of the pot of the given size and turnbias 
            DrawCluster((int)(canvas.Width / 2), (int)(canvas.Height - 40), plantSize, NUM_BASE_STALKS, Math.PI / 2, turnbias, canvas);
            
        }

        /* DrawCluster calculates the angle for the tendrils and then draws them onto the canvas
         * 
         * x: x-coordinate of cluster base
         * y: y-coordinate of cluster base
         * size: size of tendril
         * angle: angle of most recent tendril segment, or starting angle if called by Fern()
         * turnbias: bias of tendrils to turn towards the right or left
         * canvas: the canvas on which to draw the cluster
         */
        private void DrawCluster(int x, int y, double size, int numTendrils, double angle, double turnbias, Canvas canvas) {
            for (int i = 1; i <= numTendrils; i++) {
                double theta;
                if ((numTendrils % 2) == 0) { // base stalks
                    if ((i % 2) == 0)
                        theta = angle + ((i - 1) * Math.PI / (numTendrils + 2));
                    else
                        theta = angle - (i * Math.PI / (numTendrils + 2));
                }
                else {
                    if ((i % 2) == 0)
                        theta = angle + (i / 2 * Math.PI / (numTendrils + 1));
                    else
                        theta = angle - ((i - 1) / 2 * Math.PI / (numTendrils + 1));
                }
                DrawTendril(x, y, size, turnbias, theta, canvas);
            }
        }

        /* DrawTendril draws a wavy, randomly-greenish-colored tendril onto the canvas and draws clusters along it if the line is big enough
         * 
         * x1: x-coordinate of tendril base
         * y1: y-coordinate of tendril base
         * size: size of tendril
         * turnbias: the bias of the tendril to turn towards the left or right
         * direction: relative starting direction to travel
         * canvas: the canvas on which to draw the tendril
         */
        private void DrawTendril(int x1, int y1, double size, double turnbias, double direction, Canvas canvas) {
            int x2 = x1, y2 = y1;
            
            // determine the color of the tendril, where the blue value will be 0
            Random random = new Random();
            byte red = (byte) random.Next(0, 128);
            byte green = (byte) random.Next(102, 255);

            // create wavy line from segments
            for (int i = 0; i < size / 2; i++) {

                // determine endpoint from starting point and random angle
                direction -= (random.NextDouble() > turnbias) ? -1 * DELTATHETA : DELTATHETA;
                x1 = x2; y1 = y2;
                x2 = x1 + (int)(SEGLENGTH * Math.Cos(direction));
                y2 = y1 - (int)(SEGLENGTH * Math.Sin(direction));

                // draw the small line segment
                drawLine(x1, y1, x2, y2, red, green, 0, 1 + size / 80, canvas);

                // draw evenly-spaced clusters along tendril if it's large enough
                if ((size > TENDRILMIN) && (i % (int) (size/2 / (NUM_OFFSHOOTS + 1)) == 0) && (i != 0)) {
                    DrawCluster(x2, y2, size / REDUX, NUM_OFFSHOOT_TENDRILS, direction, turnbias, canvas);
                }
            }
            // draw a cluster at the end if it's large enough
            if (size > TENDRILMIN)
                DrawCluster(x2, y2, size / REDUX, NUM_OFFSHOOT_TENDRILS, direction, turnbias, canvas);
        }

        /* DrawSun draws the yellow sun with an orange border centered at (0,0) onto the canvas
         * 
         * size: The diameter of the sun
         * canvas: The canvas on which to draw the sun
         */
        private void DrawSun(double size, Canvas canvas) {
            Ellipse myEllipse = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // determine color
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 153);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 1;
            myEllipse.Stroke = Brushes.Orange;

            // determine alignment, size, and center point
            myEllipse.HorizontalAlignment = HorizontalAlignment.Center;
            myEllipse.VerticalAlignment = VerticalAlignment.Center;
            myEllipse.Width = size;
            myEllipse.Height = size;
            myEllipse.SetCenter(0, 0);

            // add the sun to the canvas
            canvas.Children.Add(myEllipse);
        }
        /* DrawPot draws the randomly-colored flower pot at the bottom center of the canvas
         * 
         * canvas: The canvas on which to draw the flower pot
         * Uses the Polygon class
         */
        private void DrawPot(Canvas canvas) {
            Polygon pot = new Polygon();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            
            // determine color
            Random random = new Random();
            byte red = (byte) random.Next(0, 255);
            byte green = (byte)random.Next(0, 255);
            byte blue = (byte)random.Next(0, 255);
            mySolidColorBrush.Color = Color.FromArgb(255, red, green, blue);
            pot.Fill = mySolidColorBrush;

            // determine algnment and shape
            pot.HorizontalAlignment = HorizontalAlignment.Center;
            pot.VerticalAlignment = VerticalAlignment.Center;
            Point p1 = new Point(canvas.Width / 2 - 20, canvas.Height);
            Point p2 = new Point(canvas.Width / 2 + 20, canvas.Height);
            Point p3 = new Point(canvas.Width / 2 + 30, canvas.Height - 30);
            Point p4 = new Point(canvas.Width / 2 + 40, canvas.Height - 30);
            Point p5 = new Point(canvas.Width / 2 + 40, canvas.Height - 40);
            Point p6 = new Point(canvas.Width / 2 - 40, canvas.Height - 40);
            Point p7 = new Point(canvas.Width / 2 - 40, canvas.Height - 30);
            Point p8 = new Point(canvas.Width / 2 - 30, canvas.Height - 30);
            PointCollection myPtColl = new PointCollection(8);
            myPtColl.Add(p1);
            myPtColl.Add(p2);
            myPtColl.Add(p3);
            myPtColl.Add(p4);
            myPtColl.Add(p5);
            myPtColl.Add(p6);
            myPtColl.Add(p7);
            myPtColl.Add(p8);
            pot.Points = myPtColl;

            // add the flower pot to the canvas
            canvas.Children.Add(pot);
            // add the across the bottom of the rim to the canvas
            drawLine((int)p3.X, (int)p3.Y, (int)p8.X, (int)p8.Y, 255, 255, 255, 1, canvas);
        }

        /*
         * draw a line segment (x1,y1) to (x2,y2) with given color, thickness on canvas
         */
        private void drawLine(int x1, int y1, int x2, int y2, byte r, byte g, byte b, double thickness, Canvas canvas) {
            Line myLine = new Line();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(255, r, g, b);
            myLine.X1 = x1;
            myLine.Y1 = y1;
            myLine.X2 = x2;
            myLine.Y2 = y2;
            myLine.Stroke = mySolidColorBrush;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.StrokeThickness = thickness;
            canvas.Children.Add(myLine);
        }
    }
}

/*
 * this class is needed to enable us to set the center for an ellipse (not built in?!)
 */
public static class EllipseX {
    public static void SetCenter(this Ellipse ellipse, double X, double Y) {
        Canvas.SetTop(ellipse, Y - ellipse.Height / 2);
        Canvas.SetLeft(ellipse, X - ellipse.Width / 2);
    }
}

