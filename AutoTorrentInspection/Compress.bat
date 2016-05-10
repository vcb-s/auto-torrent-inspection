@echo off
rem $(ProjectDir)Compress.bat
if exist .\bin\Release\AutoTorrentInspection.exe (mpress.exe -b -i .\bin\Release\AutoTorrentInspection.exe) else echo FileNotFound