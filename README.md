# da6
6502 disassembler targeting the NES platform

This is a port of disasm6 created by Frantik and posted to the nesdev boards.
https://forums.nesdev.org/viewtopic.php?t=7466

The original was written in php. This project is written in C#. The first iteration is more or less a direct port of the php, with the goal of producing matching output. Many of the php function names have been retained and implemented (at least partially) so the baseine C# code closely follow the php script structure. There are a few minor fixes included but generally the asm produced should match disasm6's output.

Future plans include:
a revised version of the C# code with methods and patterns more natural to the language
mapper support
general fixes and improvements
