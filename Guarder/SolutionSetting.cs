using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml;
using System.Threading;
using System.Diagnostics;

namespace Biobanking
{
    public enum PipettingApproach
    {
        Z_Tight = 0,
        Z_Rotate90_Tight,
        Z_UseNewRow,
        Z_Rotate_UseNewColumn
    }


    public class DetectedHeight
    {
        public double Z1; //1/10 mm
        public double Z2;

    }

    [Serializable]
    public class PipettingSettings
    {
        public int buffyAspirateLayers;
        public double r_mm;
        public int dstPlasmaSlice;
        public int dstbuffySlice;
        public int deltaXYForMSD;
        public int buffyVolume;
        //public int mixTimes;
        public int safeDelta;
        public double buffySpeedFactor;
        public double plasmaGreedyVolume;
        public int dstRedCellSlice;
        public double redCellGreedyVolume;
        public double redCellBottomHeight;
        public bool giveUpNotEnough;

        public PipettingSettings()
        {
            buffyAspirateLayers = 6;
            dstPlasmaSlice = 5;
            dstbuffySlice = 1;
            deltaXYForMSD = 13;
            //mixTimes = 2;
            safeDelta = 2;
            r_mm = 5.5;
            buffySpeedFactor = 2.5;
            buffyVolume = 300;
            plasmaGreedyVolume = 0;
            dstRedCellSlice = 0;
            redCellGreedyVolume = 300;
            redCellBottomHeight = 80; //8mm
            giveUpNotEnough = false;
        }
    }

    public class DestRack
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public PipettingApproach Alignment { get; set; }
    }

    

    [Serializable]
    public class LabwareSettings
    {
        public int tipCount;
        public int dstLabwareRows;
        public int dstLabwareStartGrid;
        //public int dstBuffyStartGrid;
        //public int dstRedCellStartGrid;
        public int dstLabwareColumns;
        public int sourceWells;
        public int sourceLabwareStartGrid;
        public int sourceLabwareGrids;
        public int wasteGrid;
        public int regions;
        public int gridsPerRegion;
        public int sitesPerRegion;

        public LabwareSettings()
        {
            sourceLabwareStartGrid = 1;
            dstLabwareStartGrid = 3;
            sourceLabwareGrids = 2;
            tipCount = 2;
            sourceWells = 10;
            wasteGrid = 21;
            regions = 1;
            dstLabwareRows = 8;
            dstLabwareColumns = 6;
            gridsPerRegion = 1;
            sitesPerRegion = 1;
        }
    }


    class Helper
    {
        public static void CloseWaiter(string windowTitle)
        {
            Thread.Sleep(1000);
            windowTitle = windowTitle.ToLower();
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (process.MainWindowTitle.ToLower().Contains(windowTitle))
                    {
                        process.CloseMainWindow();
                    }
                }
            }
        }

        public static void WriteRetryOrIgnore(bool bRetry)
        {
            string retryOrIgnore = bRetry ? "Retry" : "Ignore";
            string sFile = GetExeFolder() + "retryOrIgnore.txt";
            File.WriteAllText(sFile, retryOrIgnore);
        }

        public static void WriteResult(bool bok)
        {
            string sFile = GetExeFolder() + "result.txt";
            File.WriteAllText(sFile, bok.ToString());
        }

        public static string GetExeFolder()
        {
            string s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s+"\\";
        }

        public static void LoadSettings(string sFolder, ref PipettingSettings pipettingSettings, ref LabwareSettings labwareSettings)
        {
            XmlSerializer xs = new XmlSerializer(typeof(PipettingSettings));
            string sFile = sFolder + "\\pipettingSettings.XML";
            if (!File.Exists(sFile))
            {
                throw new Exception("Cannot find pipettingSettings.xml");
            }
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            pipettingSettings = xs.Deserialize(stream) as PipettingSettings;
            stream.Close();

            xs = new XmlSerializer(typeof(LabwareSettings));
            sFile = sFolder + "\\labwareSettings.XML";
            if (!File.Exists(sFile))
            {
                throw new Exception("Cannot find labwareSettings.xml");
            }
            stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            labwareSettings = xs.Deserialize(stream) as LabwareSettings;
            stream.Close();
        }


    }
    
}
