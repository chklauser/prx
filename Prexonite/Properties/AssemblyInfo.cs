// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Prexonite")]
[assembly: AssemblyDescription("Prexonite Scripting Engine")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Christian Klauser")]
[assembly: AssemblyProduct("Prexonite")]
[assembly: AssemblyCopyright("Copyright ©  2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]
// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("08e2ed9a-d0e0-471d-8a8b-d87cd9422bfa")]
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("1.2.9.0")]
[assembly: AssemblyFileVersion("1.2.9.0")]
[assembly: CLSCompliant(true)]

// Makes internal members of Prexonite visible to the unit testing project 
//  PrexoniteTests.
// The public key can be obtained by running
//  $> sn -p Prexonite.snk -t Prexonite.pub
//  $> sn -tp Prexonite.pub
// (Both Prexonite and PrexoniteTests are signed with the same key)
[assembly: InternalsVisibleTo("PrexoniteTests, PublicKey=002400000480000094000000" 
    + "06020000002400005253413100040000010001005def07f2a41140759af9fb2bbc95134590"
    + "655b13d80802066631489fe40f030ef270d151f62ff968e715f08e3df0e22f8f8f587b3e90" 
    + "28903c2ca2bd2b7b779ed0de24679aa3463cde1f484464f0af527a7443941f83ef4272e468" 
    + "a3e8ae7f05ff7fef7b3d0f99f4f6d42a3811d0d02350d074209283f95dccd26bbb5f7d2ebc")]