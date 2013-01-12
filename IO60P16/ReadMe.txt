This is a Microsoft .NET Gadgeteer module project created using the Module Template (2011-08-17) included in .NET Gadgeteer Core.

This template provides a simple way to build software for a custom Microsoft .NET Gadgeteer hardware module 
that is compliant with the Microsoft .NET Gadgeteer Module Builder's Guide specifications found at http://gadgeteer.codeplex.com/ 
Using this template auto-generates an installer (MSI) that can easily be distributed to end users, and an 
installation merge module (MSM) that can be used to make kit installers including many modules/mainboards/templates/etc.

Some of the functionality referred to in this template is targeted toward Visual Studio Add-ins that are not yet available,
but, following this guide will make your module forward-compatible with forthcoming add-ins.

We recommend that you read the Module Builder's Guide at http://gadgeteer.codeplex.com/ for full details.

==============================================

SYSTEM REQUIREMENT

To build the installer MSI automatically, this template requires WiX 3.5 to be installed:
http://wix.codeplex.com/releases/view/60102 (Wix35.msi)

==============================================

BUILD NOTES

Building with the Release configuration generates an MSI installer, which includes your code, in the output directory 
of the project (bin/Release/Installer).  This takes a little time, and Visual Studio/C# Express may be unresponsive during the build.  
To avoid this delay, build with the Debug configuration. 

Visual C# Express always builds in Release configuration. In order to turn off the installer build to speed up the build process,
you can go to Menu->Project->[ModuleName] Properties->Build tab and tick the "Define DEBUG constant" box.

If you see the error "The system cannot find the file..." try "Rebuild" rather than "Build"

==============================================

MODULE TEMPLATE USE INSTRUCTIONS 

1) In Menu->Project->Properties, change "ManufacturerName" to the short name of your institution with no spaces or punctuation,
   in the two places it appears.  This should match what you print on the module PCB.

2) Do a Quick Replace (Ctrl-H) on the Current Project, searching for "ManufacturerName" and replacing with the short name of your 
   institution with no spaces or punctuation.  You may want to skip the matches in this Readme file.  This will replace matches in:
   [ModuleName].cs, common.wxi, GadgeteerHardware.xml, and AssemblyInfo.cs.  (NB this does not remove the need for step 1 above.)

3) Edit the [ModuleName].cs file to implement software for your module.
   There are comments and examples in this file to assist you with this process.

4) Test your module. Modules cannot be run directly, since they are class libraries (dll) not executables (exe).   
   Testing is most easily accomplished by adding a new Gadgeteer project to the same Visual Studio/Visual C# Express solution. 
   With the new Program.cs file open, use the menu item Project->Add Reference, and, in the Projects tab, choose your module. 
   Then you should be able to instantiate the module using GTM.[Manufacturer Name].[Module Name] as usual.

5) Edit the GadgeteerHardware.xml file to specify information about your module, as described in that file.

6) Optionally, change Resources\Image.jpg to a good quality top-down image of the module with the socket side facing up,
   cropped tight (no margin), in the same orientation as the width and height specified in GadgeteerHardware.xml (not rotated).   

7) Edit Setup\common.wxi to specify parameters for the installer, as described in that file.  
   You don't need to edit any other file in the Setup directory.

8) Build in Release configuration to build the module installer!

==============================================

RELEASING THE MODULE SOFTWARE, INDIVIDUALLY OR IN A KIT

The MSI installer generated in the bin\Release\Installer directory can be distributed to end users.
The MSM merge module in the bin\Release\Installer directory can be used to build other installers such as "kit" installers that incorporate multiple
module(s)/mainboard(s).  This will install/remove correctly - e.g. if two kits including a Foo Module are both installed, there will be one copy
of the Foo module (the most up-to-date version) and if either kit is removed, the Foo module will remain installed, because the other kit requires it.

==============================================

MAKING CHANGES

If you make want to release a new version of your module, make sure to change the version number in Setup\common.wxi. 
Otherwise, the auto-generated installer will not be able to upgrade the older version correctly (an error message will result).
It is also necessary to change the versions in Properties\AssemblyInfo.cs and often a good idea to keep these synchronized with your Setup\common.wxi version.  

If you want to change the name of your module, be sure to search all the files for instances of the name.
As per the Module Builder's Guide, the software module name should match the name printed on the module itself.
The ManufacturerName should match the manufacturer name printed on the module (remove any spaces/punctuation).

==============================================

MODULE TEMPLATE FILE DETAILS

1) [ModuleName].cs - software implementation of the module's "device driver".
2) GadgeteerHardware.xml - defines some parameters about your module.
3) Resources\Image.jpg - placeholder for an image representing the module.
4) Setup\common.wxi - WiX (installer) configuration file that specifies parameters for the installer, including the version number. 
5) Setup\en-us.wxl - WiX (installer) localization file that specifies text strings that are displayed during installation.
6) Setup\msm.wxs - WiX (installer) script that generates an installation "merge module".
7) Setup\msi.wxs - WiX (installer) script that generates an installer (msi file) using the merge module.
8) Setup\G.ico - G graphic used by the installer.

==============================================
