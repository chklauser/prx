// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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
#line 28

#region Shared Source License

// The above license text has been added by an automated tool. 
//  However, for this particular file a different license is in effect:

/* **********************************************************************************
*
* Copyright (c) Microsoft Corporation. All rights reserved.
*
* This source code is subject to terms and conditions of the Shared Source License
* for IronPython. A copy of the license can be found in the License.html file
* at the root of this distribution. If you can not locate the Shared Source License
* for IronPython, please send an email to ironpy@microsoft.com.
* By using this source code in any fashion, you are agreeing to be bound by
* the terms of the Shared Source License for IronPython.
*
* You must not remove this notice, or any other, from this software.
*
* **********************************************************************************/

#endregion

/*
 * Adaption for use as a general purpose console wrapper.
 * Copyright of changes Christian Klauser
 * Changes are marked with (-P)
 */

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Prx.Win32
{
    public static class User32
    {
        private const uint
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_ALT = 0x12,
            //a.k.a. "VK_MENU"
            VK_CAPITAL = 0x14; //CAPS LOCK

        public static string ToUnicode(ConsoleKeyInfo key, bool relaxed)
        {
            // Intercept Packet keystrokes (unicode characters sent to the console by other applications) (-P) 
            if (key.Key == ConsoleKey.Packet)
            {
                return key.KeyChar.ToString();
            }
            else
            {
                const int bufferLength = 16;
                var outputBuilder = new StringBuilder(bufferLength);
                var modifiers = GetKeyState(key.Modifiers);
                var result = ToUnicode((uint) key.Key, 0, modifiers, outputBuilder, bufferLength, 0);
                if (result > 0 && result <= outputBuilder.Length)
                    return outputBuilder.ToString(0, result);
                else if (relaxed)
                    return string.Empty;
                else //Fail early (-P)
                    throw new Exception("Invalid key (" + key.KeyChar + "/" + result + "/" +
                        outputBuilder + ")");
            }
        }

        public static string ToAscii(ConsoleKeyInfo key)
        {
            var outputBuilder = new StringBuilder(2);
            var modifiers = GetKeyState(key.Modifiers);
            var result = ToAscii((uint) key.Key, 0, modifiers,
                outputBuilder, 0);
            if (result > 0)
                return outputBuilder.ToString(0, result);
            else
                throw new Exception("Invalid key (" + key + ")");
        }

        private const byte HighBit = 0x80;

        private static byte[] GetKeyState(ConsoleModifiers modifiers)
        {
            var keyState = new byte[256];

            if ((modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
                keyState[VK_SHIFT] = HighBit;
            if ((modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                keyState[VK_CONTROL] = HighBit;
            if ((modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
                keyState[VK_ALT] = HighBit;

            keyState[VK_CAPITAL] = (byte) (_queryKeyState(VK_CAPITAL) & 0xFF);

            return keyState;
        }

        [DllImport("user32.dll", EntryPoint = "GetKeyState")]
        private static extern short _queryKeyState(uint nVirtKey);

        [DllImport("user32.dll")]
        private static extern int ToAscii(uint uVirtKey, uint uScanCode,
            byte[] lpKeyState,
            [Out] StringBuilder lpChar,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);
    }
}