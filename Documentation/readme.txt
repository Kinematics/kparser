System requirements:

.NET 3.5 (SP1)

Available at:

http://www.microsoft.com/downloads/details.aspx?familyid=ab99342f-5d1a-413d-8319-81da479ab0d7&displaylang=en

Note that this is not required for Windows 7, since it is already part of the default operating system install.


SQLCE 3.5 SP1

Available at:

http://www.microsoft.com/downloads/details.aspx?FamilyId=DC614AEE-7E1C-4881-9C32-3A6CE53384D9&displaylang=en#filelist

If you are running a 32 bit OS (eg: Windows XP, Windows 7 32-bit, etc) then get the x86 install (SSCERuntime-ENU-x86.msi).

If you are running a 64 bit OS (eg: Windows Vista x64, Windows 7 64-bit, etc), then you need to install both the x86 portion (SSCERuntime-ENU-x86.msi) and the x64 portion (SSCERuntime-ENU-x64.msi).


SQLCE 3.5

It may still be necessary to install the pre-SP1 version of SQLCE.  Do so only if the program does not work with the SP1 version above.  If so, you can download it here:

http://www.microsoft.com/downloads/details.aspx?FamilyId=7849B34F-67AB-481F-A5A5-4990597B0297&displaylang=en


Installation:

Extract "KParser 1.x.x.zip" file into its own directory.



If you are running Windows Vista, you may need to adjust the runtime permisssions for the program:

Right click on the .exe file and select Properties. Go to the Compatibility tab and check the check box "Run this program as an Administrator".

If you are running 64-bit Windows Vista you may need to install the patch from this page:

http://www.microsoft.com/downloads/details.aspx?FamilyID=98E83614-C30A-4B75-9E05-0A9C3FBDD20D&amp;displaylang=en&displaylang=en

In order to fix some problems with .NET 3.5.

