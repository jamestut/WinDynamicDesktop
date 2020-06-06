# WinDynamicDesktop
Port of macOS Mojave Dynamic Desktop feature to Windows 10. Available on GitHub and the Microsoft Store.

[![GitHub releases](https://img.shields.io/github/downloads/t1m0thyj/WinDynamicDesktop/total)](https://github.com/t1m0thyj/WinDynamicDesktop/releases)
[![Chocolatey package](https://img.shields.io/chocolatey/v/windynamicdesktop?color=brightgreen)](https://chocolatey.org/packages/windynamicdesktop)
[![Build status](https://img.shields.io/appveyor/build/t1m0thyj/WinDynamicDesktop)](https://ci.appveyor.com/project/t1m0thyj/WinDynamicDesktop)

[![Gitter chat](https://img.shields.io/gitter/room/t1m0thyj/WinDynamicDesktop)](https://gitter.im/t1m0thyj/WinDynamicDesktop)
[![Donate](https://img.shields.io/badge/donate-paypal-brightgreen.svg)](https://paypal.me/t1m0thyj)
[![Translate](https://img.shields.io/badge/translate-poeditor-brightgreen.svg)](https://poeditor.com/join/project/DEgfVpyuiK)

<a href='//www.microsoft.com/store/apps/9NM8N7DQ3Z5F?ocid=badge'><img src='https://assets.windowsphone.com/85864462-9c82-451e-9355-a3d5f874397a/English_get-it-from-MS_InvariantCulture_Default.png' alt='Microsoft Store' width='160'/></a>

## Themes

Pick from the 3 themes bundled with macOS, or many more themes available for download [here](https://windd.info/themes/)

![Screenshot of Select Theme window](imgs/select_theme.png)

## Timing

The timing is modified from the original repository. Originally one day is dividied into 4 segments: sunrise, noon, sunset, and night. However, this version divides a day into the following 14 segments, ordered by time, starting from midnight:

- Nadir
- NightEnd
- NauticalDawn
- Dawn
- Sunrise
- SunriseEnd
- GoldenHourEnd
- SolarNoon
- GoldenHour
- SunsetStart
- Sunset
- Dusk
- NauticalDusk
- Night

Refer to [here](https://rdrr.io/cran/suncalc/man/getSunlightTimes.html) for explanation of each day phase.

### Theme JSON File

With the new timing method came a new theme JSON format. In particular, all `*ImageList` in the original configuration file should removed and replaced with the new `imageList` key, which contains a dictionary with keys of the above day phase, and value of integer image list as previous. Here is an example of a full theme configuration file:

```
{
  "imageFilename": "mojave_*.jpg",
  "imageCredits": "Apple",
  "imageList": {
    "Nadir": [16],
    "NightEnd": [15],
    "NauticalDawn":[1],
    "Dawn":[2],
    "Sunrise":[3],
    "SunriseEnd":[4],
    "GoldenHourEnd":[5,6],
    "SolarNoon":[7,8,9],
    "GoldenHour":[10],
    "SunsetStart":[11],
    "Sunset":[12],
    "Dusk":[13],
    "Night":[14]
  }
}
```

Any part of the day can be skipped, in which case the last image from the previous phase will be used. For example, if current phase is `NightEnd` but the definition only contains up to `Night`, then the last image from `Night` will be used.

### Known Issues

- App will crash if location is different from current time zone.


## Supported Platforms

WinDynamicDesktop is developed primarily for Windows 10, but should run on any version of Windows with .NET Framework 4.5 or newer installed. If your version of .NET Framework is too old, you can install a newer one from [here](https://www.microsoft.com/net/download).

## Resources

* [Documentation](https://github.com/t1m0thyj/WinDynamicDesktop/wiki)
* [Themes](https://windd.info/themes/)
* [Scripts](https://windd.info/scripts/)

## Known Issues

* [Wallpaper fit not remembered in Microsoft Store app](https://github.com/t1m0thyj/WinDynamicDesktop/wiki/Known-issues#wallpaper-fit-not-saved-with-multiple-monitors)
* [Wallpaper gets stuck and won't update](https://github.com/t1m0thyj/WinDynamicDesktop/wiki/Known-issues#wallpaper-gets-stuck-and-wont-update)

## Disclaimers

* Wallpaper images are not owned by me, they belong to Apple
* [LocationIQ API](https://locationiq.org/) is used when your enter your location, to convert it to latitude and longitude
* Microsoft Store app uses the Windows location API if permission is granted
* App icon made by [Roundicons](https://www.flaticon.com/authors/roundicons) from [flaticon.com](https://www.flaticon.com/) and is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/)
