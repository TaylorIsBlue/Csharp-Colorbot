using Csharp_ColorBot.Classes;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;


namespace Colorbot
{
    class Main
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        static Config config; // Dynamic configuration

        static void Main(string[] args)
        {
            LoadConfig(); // Load configuration from INI file

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
                            if (!AIMtoggled)
                            {
                                AIMtoggled = true;
                                while (AIMtoggled)
                                {
                                    Process();
                                    if (GetAsyncKeyState(Convert.ToInt32(config.AimKey)) >= 0)
                                    {
                                        AIMtoggled = false;
                                    }
                                }
                            }
                        }
                    }
                    else if (config.SwitchMode == 1)
                    {
                        AIMtoggled = !AIMtoggled;
                        Thread.Sleep(100);
                    }
                }

                Thread.Sleep(100);
            }
        }

        static void Process()
        {
            int[] upper = [255, 255, 255];
            int[] lower = [0, 0, 0];

            Bitmap img = new Bitmap(CAM_FOV, CAM_FOV);
            Graphics g = Graphics.FromImage(img);
            g.CopyFromScreen(new Point(Cursor.Position.X - CAM_FOV / 2, Cursor.Position.Y - CAM_FOV / 2), Point.Empty, img.Size);

            Bitmap hsv = new Bitmap(img.Width, img.Height);
            Bitmap mask = new Bitmap(hsv.Width, hsv.Height);
            Bitmap dilated = new Bitmap(mask.Width, mask.Height);
            Bitmap thresh = new Bitmap(dilated.Width, dilated.Height);
            Bitmap contour = new Bitmap(thresh.Width, thresh.Height);

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    int r = pixel.R;
                    int gVal = pixel.G;
                    int b = pixel.B;

                    int max = Math.Max(r, Math.Max(gVal, b));
                    int min = Math.Min(r, Math.Min(gVal, b));

                    int h = 0;
                    if (max == r && gVal >= b)
                    {
                        h = (int)(60 * (gVal - b) / (max - min));
                    }
                    else if (max == r && gVal < b)
                    {
                        h = (int)(60 * (gVal - b) / (max - min) + 360);
                    }
                    else if (max == gVal)
                    {
                        h = (int)(60 * (b - r) / (max - min) + 120);
                    }
                    else if (max == b)
                    {
                        h = (int)(60 * (r - gVal) / (max - min) + 240);
                    }

                    int s = max == 0 ? 0 : (max - min) / max * 255;
                    int v = max;

                    hsv.SetPixel(i, j, Color.FromArgb(h, s, v));
                }
            }

            for (int i = 0; i < hsv.Width; i++)
            {
                for (int j = 0; j < hsv.Height; j++)
                {
                    Color pixel = hsv.GetPixel(i, j);
                    int h = pixel.R;
                    int s = pixel.G;
                    int v = pixel.B;

                    if (h >= lower[0] && h <= upper[0] && s >= lower[1] && s <= upper[1] && v >= lower[2] && v <= upper[2])
                    {
                        mask.SetPixel(i, j, Color.White);
                    }
                    else
                    {
                        mask.SetPixel(i, j, Color.Black);
                    }
                }
            }

            for (int i = 0; i < mask.Width; i++)
            {
                for (int j = 0; j < mask.Height; j++)
                {
                    if (mask.GetPixel(i, j).R == 255)
                    {
                        dilated.SetPixel(i, j, Color.White);
                    }
                    else
                    {
                        dilated.SetPixel(i, j, Color.Black);
                    }
                }
            }

            for (int i = 0; i < dilated.Width; i++)
            {
                for (int j = 0; j < dilated.Height; j++)
                {
                    if (dilated.GetPixel(i, j).R >= 60)
                    {
                        thresh.SetPixel(i, j, Color.White);
                    }
                    else
                    {
                        thresh.SetPixel(i, j, Color.Black);
                    }
                }
            }

            int maxArea = 0;
            int maxIndex = -1;

            for (int i = 0; i < contour.Width; i++)
            {
                for (int j = 0; j < contour.Height; j++)
                {
                    if (thresh.GetPixel(i, j).R == 255)
                    {
                        int area = FloodFill(contour, i, j);
                        if (area > maxArea)
                        {
                            maxArea = area;
                            maxIndex = i;
                        }
                    }
                }
            }

            if (maxIndex != -1)
            {
                int x = maxIndex - CAM_FOV / 2 + AIM_OFFSET_X;
                int y = maxArea - CAM_FOV / 2 + AIM_OFFSET_Y;
                double distance = Math.Sqrt(x * x + y * y);
                if (distance <= AIM_FOV)
                {
                    int x2 = (int)(x * AIM_SPEED_X);
                    int y2 = (int)(y * AIM_SPEED_Y);
                    mouse_event(0x0001, x2, y2, 0, 0);
                    if (TRIGGERBOT != "disabled" && distance <= TRIGGERBOT_DISTANCE)
                    {
                        if (TRIGGERBOT_DELAY != 0)
                        {
                            if (!shooting)
                            {
                                shooting = true;
                                Thread.Sleep(TRIGGERBOT_DELAY);
                                mouse_event(0x0002, x2, y2, 0, 0);
                                clicks++;
                                shooting = false;
                            }
                        }
                        else
                        {
                            mouse_event(0x0002, x2, y2, 0, 0);
                            clicks++;
                        }
                    }
                }
            }
        }

        static int FloodFill(Bitmap image, int x, int y)
        {
            int area = 0;
            if (x >= 0 && x < image.Width && y >= 0 && y < image.Height && image.GetPixel(x, y).R == 255)
            {
                image.SetPixel(x, y, Color.Black);
                area++;
                area += FloodFill(image, x - 1, y);
                area += FloodFill(image, x + 1, y);
                area += FloodFill(image, x, y - 1);
                area += FloodFill(image, x, y + 1);
            }
            return area;
        }
    }

}
