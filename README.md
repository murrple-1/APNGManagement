APNGManagement
==============
Created by Murray Christopherson  
Updated February 2014

About the Program
-----------------
This program was written because there was little support for the APNG file format,
and my company was experimenting with using it in our game engine. Included in this
project are an APNG data model, APNG builder and 2 APNG viewers (which use 2 rendering
engines, for testing).  
It is recommended you read the source code to see exactly what the code is
doing.  
Please visit my portfolio at http://murraychristopherson.com

If you discover any bugs or unclear/inconsistent documentation, please contact me
at murraychristopherson@gmail.com with a description of what happened, and I
will try and have a fixed version up shortly.

Installation
------------
APNGLib.sln must be opened in either Visual Studio (2010 and up) or MonoDevelop.  
From there, you can compile and run the various programs.  
The viewers and builder will only work on Microsoft Windows, since they all use the
Winforms library.

Running the Programs
--------------------
### APNGBuilder
This program takes a root directory and traverses each subdirectory. For each subdirectory,
it takes all PNG files inside and assembles them into an APNG, followed by compression with
the 7Z compression algorithm. The PNGs are added as frames in alphabetical order.

### APNGViewer
Select a directory, and all PNG/APNG files are rendered. On one side, the standard
(non-animated) PNG is shown, rendered by the WinForms library. On the other are the
animated PNGs. This is to demonstrate that APNGs have a fallback if animation is not
enabled.

### APNGViewer_OpenGL
Works very similar to APNGViewer, but only displays one PNG/APNG at a time, and uses OpenGL
as its rendering engine (hopefully to speed the rendering process).
