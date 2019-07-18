@echo off

set packageName=ChannelAdam.TestFramework.BizTalk

set /p version="What is the version you want to push?"

..\tools\nuget\nuget.exe push "%packageName%.%version%.nupkg" -Source https://www.nuget.org

..\tools\nuget\nuget.exe push "%packageName%.%version%.Symbols.nupkg" -Source https://nuget.smbsrc.net/

pause
