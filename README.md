# Auto Torrent Inspection [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) [![Build status](https://ci.appveyor.com/api/projects/status/pq6a7ju9q9hue3np?svg=true&passingText=%E7%BC%96%E8%AF%91%20-%20%E7%A8%B3%20&pendingText=%E5%B0%8F%E5%9C%9F%E8%B1%86%E7%82%B8%E4%BA%86%20&failingText=%E6%88%91%E6%84%9F%E8%A7%89%E5%8D%9C%E8%A1%8C%20)](https://ci.appveyor.com/project/tautcony/auto-torrent-inspection)

- A naïve tool to check naming issues and tracker list for torrent file before publish it, you can get naming rules from [here](https://github.com/vcb-s/VCB-S_Collation).

## Directions

- You must have .NET 6.0 available from [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

- Although this software is named as `Auto Torrent Inspection`, if you drop a folder, it additionally includes following features:
    - [x] `.flac`: compress rate calculation and meta data extract.
    - [x] `.cue`: encoding detect & fix, file matching check.
    - [x] `.log`: log checksum integrity validate.
    - [x] `.ass`: font, unused style, mod tag detect.
    - [x] file signature and file extension matching check.
    - [x] duplicate files detect.

## Thanks to

- [BencodeNET](https://github.com/Krusen/BencodeNET)
- [Crc32C](https://github.com/robertvazan/crc32c.net)
- [UDE](https://github.com/errepi/ude)
- [EAC Log Signer](https://github.com/puddly/eac_logsigner)
- [pprp](https://github.com/dsoprea/RijndaelPbkdf)

## Source Code

- https://github.org/vcb-s/auto-torrent-inspection
