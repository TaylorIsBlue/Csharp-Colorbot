using IniParser;
using IniParser.Model;
using System.Runtime.Serialization;
namespace Csharp_ColorBot.Classes
{
    class Config
    {
        public string AimKey { get; set; }
        public string SwitchModeKey { get; set; }
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

        // just public variables to clean up code, this is not included in the INI file
        public bool Toggled { get; set; } = false;
        public int SwitchMode { get; set; } = 1;
        public int Clicks { get; set; } = 0;
        public bool Shooting { get; set; } = false;

        public static void LoadConfig(Config cfg)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("config.ini"); //read it duh lol

            cfg = new Config
            {
                AimKey = data["Settings"]["AimKey"],
                SwitchModeKey = data["Settings"]["SwitchModeKey"],
                FovKeyUp = data["Settings"]["FovKeyUp"],
                FovKeyDown = data["Settings"]["FovKeyDown"],
                CamFov = int.Parse(data["Settings"]["CamFov"]),
                AimOffsetY = int.Parse(data["Settings"]["AimOffsetY"]),
                AimOffsetX = int.Parse(data["Settings"]["AimOffsetX"]),
                AimSpeedX = float.Parse(data["Settings"]["AimSpeedX"]),
                AimSpeedY = float.Parse(data["Settings"]["AimSpeedY"]),
                AimFov = int.Parse(data["Settings"]["AimFov"]),
                Triggerbot = bool.Parse(data["Settings"]["Triggerbot"]),
                TriggerbotDelay = int.Parse(data["Settings"]["TriggerbotDelay"]),
                TriggerbotDistance = int.Parse(data["Settings"]["TriggerbotDistance"]),
                Smoothening = bool.Parse(data["Settings"]["Smoothening"]),
                SmootheningFactor = float.Parse(data["Settings"]["SmootheningFactor"]),
            };
        }
    }   
}
