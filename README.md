# ModernModellingWarfare
[![Discord](https://img.shields.io/badge/chat-Discord-blue.svg)](https://discord.gg/RyqyThu)

*Modern Modelling Warfare* is a tool that allows you to export certain content from Modern Warfare/Warzone such as operators and weapons for use in making renders, custom zombies maps, or other stuff. It was made as a small temporary workaround due to the fact that modern Call of Duty games utilize KAC on PC and completely breaks convential ways of reading the games memory. It is made to aid those working on content such as renders and mods made for official Call of Duty Mod Tools, I do not condone or support the use of it for anything else.

Modern Modelling Warfare is based off general knowledge available on how Call of Duty Fast Files work and a ton of debugging of ModernWarfare's fast file loader, I don't care about money, but if you do use it, join my Discord and show our community what you do. ❤️

# Requirements

* [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime)

I get that the tool is indimidating to use, but you must have some understanding of using CLI tools to use it and how to file proper bug reports, you can join our Discord and get help from the community regarding how to use it, once you get the hang of it, it's easy to work with.

In order to access the game files for use in ModernModellingWarfare you'll need to download CASCView (or your CASC tool of choice): http://www.zezula.net/en/casc/main.html and export the files.

Once done, edit the GameDirectory.txt file with the path to where MW is located, an example is provided if you need a hint, this will allow ModernModellingWarfare to stream images.

# Using ModernModellingWarfare

1. Use CASCView (or your CASC tool of choice) to locate streaming fast files, these are small fast files, usually ending in _tr.ff, and have names like
   attachment_vm/wm, body, head, etc. For example: https://i.imgur.com/mAGgc8S.png Make sure to find files with ay in name for weapons, as other files contain
   data such as motion data for Physics, Havok Physics, Blendshapes, etc. For XAnims, you'll need to search for _animpackage.

2. Export these from the CASC and then drag them onto ModernModellingWarfareX.

3. With any luck, you'll have your models!

Exporting HQ Images for Operators/Weapons:

To export HQ Images, you need to enable the streaming option in Modern Warfare's Graphics Menu, then hover over the content you want to rip. You should notice
network activity and the files within xpak_cache growing, you can then close the game. Then export that asset's ff as normal, ModernModellingWarfareX will automatically pull from xpak_cache.
Make sure MW is closed when you attempt to use ModernModellingWarfareX, or else you may get banned by Ricochet.

Some notes:

* ModernModellingWarfareX is purely for weapons/operators, if other files work, it's just luck, but it is not designed for props.
* Some characters share data, so you may need to go deep diving to find what you need. A file for one operator might contain models for another.
* Some files may contain data not supported, it's unlikely this will be fixed, as time is limited and the focus is on higher priority content.

# Understanding Limitations

Call of Duty fast files are both simple and incredibly complex, there is a lot that has be correct for loading them such as alignment and ensuring data is put into the correct memory blocks. What I have done is reverse just the XModel, XModel Mesh, and XAnim loaders in Modern Warfare/Warzone, and even then, just a small subset of them used by operators/weapons. Call of Duty games contain hundreds of structures you must understand to load fast files. A separate tool is being worked on going forward that will allow you to load any file in the game.

