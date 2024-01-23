using Csharp_ColorBot.Classes;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge;
using System.Threading;

namespace Colorbot
{
    class Main
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        static Config config; // Dynamic configuration
        

        static void main(string[] args)
        {
            if(config == null) throw new ArgumentException("Config is null");
            config.LoadConfig(); // Load configuration from INI file

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                if (GetAsyncKeyState(Convert.ToInt32(config.SwitchModeKey)) < 0)
                {
                    config.SwitchMode = (config.SwitchMode == 0) ? 1 : 0;
                    Thread.Sleep(100);
                }

                if (GetAsyncKeyState(Convert.ToInt32(config.FovKeyUp)) < 0)
                {
                    config.AimFov += 5;
                    Thread.Sleep(100);
                }

                if (GetAsyncKeyState(Convert.ToInt32(config.FovKeyDown)) < 0)
                {
                    config.AimFov -= 5;
                    Thread.Sleep(100);
                }

                if (GetAsyncKeyState(Convert.ToInt32(config.AimKey)) < 0)
                {
                    if (config.SwitchMode == 0)
                    {
                        while (GetAsyncKeyState(Convert.ToInt32(config.AimKey)) < 0)
                        {
                            if (!config.Toggled)
                            {
                                config.Toggled = true;
                                while (config.Toggled)
                                {
                                    Process();
                                    if (GetAsyncKeyState(Convert.ToInt32(config.AimKey)) >= 0)
                                    {
                                        config.Toggled = false;
                                    }
                                }
                            }
                        }
                    }
                    else if (config.SwitchMode == 1)
                    {
                        config.Toggled = !config.Toggled;
                        Thread.Sleep(100);
                    }
                }

                Thread.Sleep(100);
            }
        }
        /* Do i understand ANY of this AI code that GPT made.. no. so we are not going to use it! instead, i'm gonna try to make the colorbot from scratch. 😭
        static void Process()
        {
            try
            {
                // Capture screenshot
                Bitmap img = new Bitmap(screenshot.Width, screenshot.Height);
                Graphics g = Graphics.FromImage(img);
                g.CopyFromScreen(new Point(Cursor.Position.X - screenshot.Width / 2, Cursor.Position.Y - screenshot.Height / 2), Point.Empty, img.Size);

                // Convert image to HSV format
                Bitmap hsv = ConvertToHSV(img);

                // Apply mask based on color ranges
                Bitmap mask = ApplyColorMask(hsv, lowerColor, upperColor);

                // Dilate the mask
                Bitmap dilated = Dilate(mask);

                // Thresholding
                Bitmap thresh = Threshold(dilated, 60);

                // Find contours
                List<Point> contours = FindContours(thresh);

                if (contours.Count != 0)
                {
                    // Find the contour with the maximum area
                    List<Point> maxContour = contours.OrderByDescending(p => p.Y).ToList();

                    // Find the topmost point in the max contour
                    Point topmost = maxContour.OrderBy(p => p.Y).First();

                    // Calculate the aiming position
                    int x = topmost.X - center + AIM_OFFSET_X;
                    int y = topmost.Y - center + AIM_OFFSET_Y;

                    double distance = Math.Sqrt(x * x + y * y);

                    if (distance <= AIM_FOV)
                    {
                        int x2 = (int)(x * AIM_SPEED_X);
                        int y2 = (int)(y * AIM_SPEED_Y);

                        // Move the mouse towards the position
                        mouse_event(0x0001, x2, y2, 0, 0);

                        // Triggerbot functionality
                        if (TRIGGERBOT != "disabled" && distance <= TRIGGERBOT_DISTANCE)
                        {
                            if (TRIGGERBOT_DELAY != 0 && !shooting)
                            {
                                shooting = true;
                                Thread delayThread = new Thread(() => DelayedAim());
                                delayThread.Start();
                            }
                            else
                            {
                                mouse_event(0x0002, x2, y2, 0, 0);
                                clicks++;

                                // Start a thread to stop shooting after a short delay
                                Thread stopThread = new Thread(() => StopShooting());
                                stopThread.Start();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in processing: " + e.Message);
            }
        }


        static List<System.Drawing.Point> FindContours(Bitmap image)
        {
            // Convert the image to grayscale
            Bitmap grayImage = Grayscale.CommonAlgorithms.BT709.Apply(image);

            // Use the Canny edge detector for edge detection
            CannyEdgeDetector edgeDetector = new CannyEdgeDetector();
            Bitmap edges = edgeDetector.Apply(grayImage);

            // Use the BlobCounter to find blobs (contours) in the image
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(edges);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            // Extract points from the blobs
            List<System.Drawing.Point> contours = new List<System.Drawing.Point>();

            foreach (Blob blob in blobs)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blob);
                foreach (IntPoint point in edgePoints)
                {
                    contours.Add(new System.Drawing.Point(point.X, point.Y));
                }
            }

            return contours;
        }
        static Bitmap ConvertToHSV(Bitmap image)
        {
            // Convert the image to grayscale
            Bitmap grayImage = Grayscale.CommonAlgorithms.BT709.Apply(image);

            // Create an instance of the ColorFiltering class
            ColorFiltering colorFilter = new ColorFiltering();

            // Set the filter's color
            colorFilter.Red = new IntRange(0, 255);
            colorFilter.Green = new IntRange(0, 255);
            colorFilter.Blue = new IntRange(0, 255);

            // Apply the color filter
            Bitmap filteredImage = colorFilter.Apply(grayImage);

            // Create an instance of the HSLFiltering class
            HSLFiltering hslFilter = new HSLFiltering();

            // Set the filter's color space
            hslFilter.Hue = new IntRange(0, 360);
            hslFilter.Saturation = new AForge.Range(0, 1);
            hslFilter.Luminance = new AForge.Range(0, 1);

            // Apply the HSL filter
            Bitmap hsvImage = hslFilter.Apply(filteredImage);

            return hsvImage;
        }

        static Bitmap ApplyColorMask(Bitmap hsvImage)
        {
            // Create an instance of the HueModifier class
            HueModifier hueModifier = new HueModifier();

            // Set the hue range to keep
            hueModifier.Hue = new;

            // Apply the hue modification
            Bitmap hueModifiedImage = hueModifier.Apply(hsvImage);

            return hueModifiedImage;
        }

        static Bitmap Dilate(Bitmap image)
        {
            // Create an instance of the Dilatation filter
            Dilatation dilatationFilter = new Dilatation();

            // Apply the dilatation filter
            Bitmap dilatedImage = dilatationFilter.Apply(image);

            return dilatedImage;
        }
        */
    }

}
