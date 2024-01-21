using IniParser;
using IniParser.Model;



namespace Csharp_ColorBot.Classes
{
    class Config
    {
        public string AimKey { get; set; }
        public string BindMode { get; set; }
        public string SwitchModeKey { get; set; }
        public string UpdateKey { get; set; }
        public string FovKeyUp { get; set; }
        public string FovKeyDown { get; set; }
        public int CamFov { get; set; }
        public int AimOffsetY { get; set; }
        public int AimOffsetX { get; set; }
        public float AimSpeedX { get; set; }
        public float AimSpeedY { get; set; }
        public int AimFov { get; set; }
        public bool Triggerbot { get; set; }
        public int TriggerbotDelay { get; set; }
        public int TriggerbotDistance { get; set; }
        public bool Smoothening { get; set; }
        public float SmootheningFactor { get; set; }
        public HsvColor upperColor { get; set; } // this is complicated, HSVColor is a class in the other file - it's a color format. I'm very sure there's a better way to do this.
        public HsvColor lowerColor { get; set; }
        public string Color { get; set; }

        // just public variables to clean up code, this is not included in the INI file
        public bool Toggled { get; set; } = false;
        public int SwitchMode { get; set; } = 1;
        public int Clicks { get; set; } = 0;
        public bool Shooting { get; set; } = false;

        public void LoadConfig(Config cfg)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("config.ini"); //read it duh lol

            cfg = new Config
            {
                AimKey = data["Config"]["AIM_KEY"],
                BindMode = data["Config"]["BINDMODE"],
                SwitchModeKey = data["Config"]["SWITCH_MODE_KEY"],
                UpdateKey = data["Config"]["UPDATE_KEY"],
                FovKeyUp = data["Config"]["FOV_KEY_UP"],
                FovKeyDown = data["Config"]["FOV_KEY_DOWN"],
                CamFov = int.Parse(data["Config"]["CAM_FOV"]),
                AimFov = int.Parse(data["Config"]["AIM_FOV"]),
                AimOffsetY = int.Parse(data["Config"]["AIM_OFFSET_Y"]),
                AimOffsetX = int.Parse(data["Config"]["AIM_OFFSET_X"]),
                AimSpeedX = float.Parse(data["Config"]["AIM_SPEED_X"]),
                AimSpeedY = float.Parse(data["Config"]["AIM_SPEED_Y"]),
                Triggerbot = bool.Parse(data["Config"]["TRIGGERBOT"]),
                TriggerbotDelay = int.Parse(data["Config"]["TRIGGERBOT_DELAY"]),
                TriggerbotDistance = int.Parse(data["Config"]["TRIGGERBOT_DISTANCE"]),
                Smoothening = bool.Parse(data["Config"]["SMOOTHENING"]),
                SmootheningFactor = float.Parse(data["Config"]["SMOOTH_FACTOR"]),
                upperColor = ParseHsvColor(data["Config"]["UPPER_COLOR"]),
                lowerColor = ParseHsvColor(data["Config"]["LOWER_COLOR"]),
                Color = data["Config"]["COLOR"]
            };
        }
        // There is DEFINITElY A BETTER WAY TO DO THIS?!
        public void SetColorConfig(string color)
        {
            color = color.ToLower();

            if (color == "yellow")
            {
                upperColor = ParseHsvColor("38,255,203");
                lowerColor = ParseHsvColor("30,255,201");
            }
            else if (color == "blue")
            {
                upperColor = ParseHsvColor("123,255,217");
                lowerColor = ParseHsvColor("113,206,189");
            }
            else if (color == "pink" || color == "magenta" || color == "purple")
            {
                upperColor = ParseHsvColor("150,255,201");
                lowerColor = ParseHsvColor("150,255,200");
            }
            else if (color == "green")
            {
                upperColor = ParseHsvColor("60,255,201");
                lowerColor = ParseHsvColor("60,255,201");
            }
            else if (color == "cyan")
            {
                upperColor = ParseHsvColor("90,255,201");
                lowerColor = ParseHsvColor("90,255,201");
            }
            else if (color == "red")
            {
                upperColor = ParseHsvColor("0,255,201");
                lowerColor = ParseHsvColor("0,255,201");
            }
            else if (color == "custom")
            {
                upperColor = ParseHsvColor("255,255,255"); // Update with your custom values
                lowerColor = ParseHsvColor("0,0,0");       // Update with your custom values
            }
            else if (color == "0000ff")
            {
                upperColor = ParseHsvColor("123,255,255");
                lowerColor = ParseHsvColor("120,147,69");
            }
            else if (color == "aimblox")
            {
                upperColor = ParseHsvColor("4,225,206");
                lowerColor = ParseHsvColor("0,175,119");
            }
            else if (color == "black")
            {
                upperColor = ParseHsvColor("0,0,0");
                lowerColor = ParseHsvColor("0,0,0");
            }
            else
            {
                // Default to red if no color is found
                upperColor = ParseHsvColor("0,255,201");
                lowerColor = ParseHsvColor("0,255,201");
            }
        }
        public HsvColor ParseHsvColor(string values)
        {
            if (string.IsNullOrWhiteSpace(values))
            {
                throw new ArgumentException($"'{nameof(values)}' cannot be null or whitespace.", nameof(values));
            }

            string[] parts = values.Split(',');

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int hue) &&
                int.TryParse(parts[1], out int saturation) &&
                int.TryParse(parts[2], out int value))
            {
                return new HsvColor { Hue = hue, Saturation = saturation, Value = value };
            }
            else
            {
                throw new ArgumentException("Invalid HSV color format in config.");
            }
        }
    }   
}
