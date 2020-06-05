// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using SunCalcNet.Model;

namespace WinDynamicDesktop
{
    public enum PolarPeriod { None, PolarDay, PolarNight };

    public enum DaySegment
    {
        Nadir, NightEnd, NauticalDawn, Dawn, Sunrise, SunriseEnd,
        GoldenHourEnd, SolarNoon, GoldenHour, SunsetStart, Sunset, 
        Dusk, NauticalDusk, Night
    };

    public class SolarData
    {
        public PolarPeriod polarPeriod = PolarPeriod.None;
        public DateTime sunriseTime { get; set; }
        public DateTime sunsetTime { get; set; }
        public DateTime[] solarTimes { get; set; }
    }

    public static class DaySegmentCompute
    {
        public static int NumPhases { get; private set; }

        private static Dictionary<string, DaySegment> phaseStrMap;
        private static Dictionary<DaySegment, int> phaseIndexMap;
        private static DaySegment[] phaseOrder;
        static DaySegmentCompute()
        {
            phaseStrMap = new Dictionary<string, DaySegment>();
            phaseIndexMap = new Dictionary<DaySegment, int>();

            // order starts from midnight
            phaseOrder = new DaySegment[] {
                DaySegment.Nadir, DaySegment.NightEnd, DaySegment.NauticalDawn, DaySegment.Dawn,
                DaySegment.Sunrise, DaySegment.SunriseEnd, DaySegment.GoldenHourEnd, DaySegment.SolarNoon,
                DaySegment.GoldenHour, DaySegment.SunsetStart, DaySegment.Sunset, DaySegment.Dusk,
                DaySegment.NauticalDusk, DaySegment.Night };
            NumPhases = phaseOrder.Length;
            for (int i = 0; i < NumPhases; ++i)
                phaseIndexMap.Add(phaseOrder[i], i);

            // names (these names doesn't have spaces, that's why we're typing them all over again here)
            phaseStrMap.Add("Nadir", DaySegment.Nadir);
            phaseStrMap.Add("NightEnd", DaySegment.NightEnd);
            phaseStrMap.Add("NauticalDawn", DaySegment.NauticalDawn);
            phaseStrMap.Add("Dawn", DaySegment.Dawn);
            phaseStrMap.Add("Sunrise", DaySegment.Sunrise);
            phaseStrMap.Add("SunriseEnd", DaySegment.SunriseEnd);
            phaseStrMap.Add("GoldenHourEnd", DaySegment.GoldenHourEnd);
            phaseStrMap.Add("SolarNoon", DaySegment.SolarNoon);
            phaseStrMap.Add("GoldenHour", DaySegment.GoldenHour);
            phaseStrMap.Add("SunsetStart", DaySegment.SunsetStart);
            phaseStrMap.Add("Sunset", DaySegment.Sunset);
            phaseStrMap.Add("Dusk", DaySegment.Dusk);
            phaseStrMap.Add("NauticalDusk", DaySegment.NauticalDusk);
            phaseStrMap.Add("Night", DaySegment.Night);
        }

        public static DaySegment? GetPhaseObject(string name)
        {
            if(name != null)
            {
                if (phaseStrMap.ContainsKey(name))
                    return phaseStrMap[name];
            }
            return null;
        }

        public static DaySegment? GetPhaseObject(int index)
        {
            if (index < 0 || index >= phaseOrder.Length)
                return null;
            return phaseOrder[index];
        }

        public static int GetPhaseIndex(DaySegment obj)
        {
            return phaseIndexMap[obj];
        }

        public static int[] GetThemeImageList(ThemeConfig theme, DaySegment phase)
        {
            var phaseIndex = GetPhaseIndex(phase);
            if ((theme.imageListSorted[phaseIndex]?.Length ?? 0) > 0)
                return theme.imageListSorted[phaseIndex];

            // find the last image in the phase preceding this immediately
            for (int i = NumPhases + phaseIndex; i >= 0; --i)
            {
                var imgArr = theme.imageListSorted[i % NumPhases];
                if ((imgArr?.Length ?? 0) > 0)
                {
                    return new int[] { imgArr[imgArr.Length - 1] };
                }
            }

            // theme validator should catch this up before we ever reach this point
            return null;
        }
    }

    class SunriseSunsetService
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        private static SolarData GetUserProvidedSolarData()
        {
            SolarData data = new SolarData();
            data.sunriseTime = UpdateHandler.SafeParse(JsonConfig.settings.sunriseTime);
            data.sunsetTime = UpdateHandler.SafeParse(JsonConfig.settings.sunsetTime);

            int halfSunriseSunsetDuration = JsonConfig.settings.sunriseSunsetDuration * 30;
            data.solarTimes = new DateTime[4]
            {
                data.sunriseTime.AddSeconds(-halfSunriseSunsetDuration),
                data.sunriseTime.AddSeconds(halfSunriseSunsetDuration),
                data.sunsetTime.AddSeconds(-halfSunriseSunsetDuration),
                data.sunsetTime.AddSeconds(halfSunriseSunsetDuration)
            };

            return data;
        }

        private static List<SunPhase> GetSunPhases(DateTime date, double latitude, double longitude)
        {
            return SunCalcNet.SunCalc.GetSunPhases(date.AddHours(12).ToUniversalTime(), latitude, longitude).ToList();
        }

        private static DateTime GetSolarTime(List<SunPhase> sunPhases, SunPhaseName desiredPhase)
        {
            return sunPhases.Single(sunPhase => sunPhase.Name.Value == desiredPhase.Value).PhaseTime.ToLocalTime();
        }

        public static SolarData GetSolarData(DateTime date)
        {
            if (JsonConfig.settings.dontUseLocation)
            {
                return GetUserProvidedSolarData();
            }

            double latitude = double.Parse(JsonConfig.settings.latitude, CultureInfo.InvariantCulture);
            double longitude = double.Parse(JsonConfig.settings.longitude, CultureInfo.InvariantCulture);
            var sunPhases = GetSunPhases(date, latitude, longitude);

            // debug
            System.Diagnostics.Debug.WriteLine("Solar data (in local time):");
            foreach (var sunPhase in sunPhases)
            {
                System.Diagnostics.Debug.WriteLine(" - {0} : {1}", sunPhase.Name, sunPhase.PhaseTime.ToLocalTime());
            }

            SolarData data = new SolarData();

            try
            {
                data.sunriseTime = GetSolarTime(sunPhases, SunPhaseName.Sunrise);
                data.sunsetTime = GetSolarTime(sunPhases, SunPhaseName.Sunset);
                data.solarTimes = new DateTime[14]
                {
                    GetSolarTime(sunPhases, SunPhaseName.Nadir),
                    GetSolarTime(sunPhases, SunPhaseName.NightEnd),
                    GetSolarTime(sunPhases, SunPhaseName.NauticalDawn),
                    GetSolarTime(sunPhases, SunPhaseName.Dawn),
                    GetSolarTime(sunPhases, SunPhaseName.Sunrise),
                    GetSolarTime(sunPhases, SunPhaseName.SunriseEnd),
                    GetSolarTime(sunPhases, SunPhaseName.GoldenHourEnd),
                    GetSolarTime(sunPhases, SunPhaseName.SolarNoon),
                    GetSolarTime(sunPhases, SunPhaseName.GoldenHour),
                    GetSolarTime(sunPhases, SunPhaseName.SunsetStart),
                    GetSolarTime(sunPhases, SunPhaseName.Sunset),
                    GetSolarTime(sunPhases, SunPhaseName.Dusk),
                    GetSolarTime(sunPhases, SunPhaseName.NauticalDusk),
                    GetSolarTime(sunPhases, SunPhaseName.Night)
                };
            }
            catch (InvalidOperationException)  // Handle polar day/night
            {
                DateTime solarNoon = GetSolarTime(sunPhases, SunPhaseName.SolarNoon);
                double sunAltitude = SunCalcNet.SunCalc.GetSunPosition(solarNoon.ToUniversalTime(), latitude,
                    longitude).Altitude;

                if (sunAltitude > 0)
                {
                    data.polarPeriod = PolarPeriod.PolarDay;
                }
                else
                {
                    data.polarPeriod = PolarPeriod.PolarNight;
                }
            }

            return data;
        }

        public static string GetSunriseSunsetString(SolarData solarData)
        {
            switch (solarData.polarPeriod)
            {
                case PolarPeriod.PolarDay:
                    return _("Sunrise/Sunset: Up all day");
                case PolarPeriod.PolarNight:
                    return _("Sunrise/Sunset: Down all day");
                default:
                    return string.Format(_("Sunrise: {0}, Sunset: {1}"), solarData.sunriseTime.ToShortTimeString(),
                        solarData.sunsetTime.ToShortTimeString());
            }
        }
    }
}
