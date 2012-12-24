*******************************************************************************
KParser - Combat parser for Final Fantasy XI
Copyright (C) 2007-2009 by David Smith

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The GPL text should also be available at http://www.gnu.org/licenses/gpl-2.0.html

This source code for this software is available at:
http://code.google.com/p/kparser/

*******************************************************************************


System Requirements:

OS:
Microsoft Windows XP, Vista, or 7; 32-bit or 64-bit.
Program may run on Windows 2000, but is not directly supported.


Support Services:
Microsoft .NET Framework 3.5 SP1
This may be installed via Windows Update or downloaded directly
from Microsoft's website at:
http://www.microsoft.com/downloads/details.aspx?familyid=ab99342f-5d1a-413d-8319-81da479ab0d7&displaylang=en

KParser's installer should try to automatically install the
.NET Framework if it is not already present on your computer.


Microsoft SQL Server Compact Edition 3.5 SP1 (SQLCE 3.5sp1)
This may be downloaded directly from Microsoft's website at:
http://www.microsoft.com/downloads/details.aspx?FamilyId=DC614AEE-7E1C-4881-9C32-3A6CE53384D9&displaylang=en

If you are running a 32 bit OS (eg: Windows XP, Windows 7 32-bit, etc)
then get the x86 install (SSCERuntime-ENU-x86.msi).

If you are running a 64 bit OS (eg: Windows Vista x64, Windows 7 64-bit, etc),
then you need to install both the x86 portion (SSCERuntime-ENU-x86.msi) and
the x64 portion (SSCERuntime-ENU-x64.msi).

KParser's installer should try to automatically install
SQLCE if it is not already present on your computer.



Troubleshooting:

KParser requires administrative privileges to run.  If you are running Windows Vista or Windows 7,
you will need to authorize this via the UAC prompt.  You do not need to adjust the properties
tab to run the program as administrator since that is automatically requested in the program
manifest.

If you are running 64-bit Windows Vista you may need to install the patch
from this page in order to fix some problems with .NET 3.5:

http://www.microsoft.com/downloads/details.aspx?FamilyID=98E83614-C30A-4B75-9E05-0A9C3FBDD20D&amp;displaylang=en&displaylang=en

It is uncertain if this patch is already included in 3.5 SP1.

