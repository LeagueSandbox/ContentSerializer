[![Build status](https://ci.appveyor.com/api/projects/status/9h6vd7lgbvflk8o9/branch/master?svg=true)](https://ci.appveyor.com/project/MythicManiac/contentserializer/branch/master)
[![codecov.io](https://codecov.io/github/LeagueSandbox/ContentSerializer/coverage.svg?branch=master)](https://codecov.io/github/LeagueSandbox/ContentSerializer?branch=master)
# The League Sandbox project's content serializer
Project website along with more specifications can be fround from: https://leaguesandbox.github.io/  
Project chat on Discord: https://discord.gg/0vmmZ6VAwXB05gB6

# Contributing
We're looking for people interested in contributing to the project.  
Currently the technologies we use include:
* C#
* Lua
* Electron
* Node.js
* Angular
* Socket.io

For more detailed project specifications head over to https://leaguesandbox.github.io/  
If you're interested in contributing, come find us from [Discord](https://discord.gg/0vmmZ6VAwXB05gB6) and let us know

# Setup guide
* Install Microsoft Visual Studio 2015 (Community Edition is fine)
* Install DotNet 4.6.1 Framework
* Install NuGet package installer (https://visualstudiogallery.msdn.microsoft.com/5d345edc-2e2d-4a9c-b73b-d53956dc458d)
* Install StyleCop (https://stylecop.codeplex.com/)
* Install Editor Guidelines (https://visualstudiogallery.msdn.microsoft.com/da227a0b-0e31-4a11-8f6b-3a149cf2e459)
* Download the 4.20 version of League client (https://mega.nz/#!pFRVxBJQ!AMbsJnS9kqhvQ-tfP8QxoBikbrjlGQ4MdzNYGo0fIKM)
* Clone the git repository
* Add a `ContentSerializer/bin/Debug/rads-path.txt` with the path to your League 4.20 folder
    * `S:/LeagueSandbox/League-of-Legends-4-20/RADS/projects/lol_game_client`
* Build and run

# Project policies
* Line length should be 120 characters maximum whenever possible (use Editor Guidelines plugin for a ruler)
* No StyleCop warnings should be present in new code
* No pushing/committing to master—all changes must go through pull requests
* Don't merge your own pull requests—get someone else to review/merge it
* Pull requests should not be merged before the build has passed
    * If the build fails, ping the pull request creator and tell him to fix it
* Files and folders in `PascalCase`
* JSON dictionary keys in `PascalCase`

# C# guidelines
* Function names in `PascalCase`
* Constants in `ALL_CAPS`
* Private variables in `_camelCaseWithUnderscore`
* Public properties as getters / setters in `PascalCase`
* All public variable access should happen through getters / setters
