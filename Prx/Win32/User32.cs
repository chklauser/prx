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

namespace Prx.Win32;

public static class User32
{
    const uint VK_SHIFT = 0x10,
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
            var result = ToUnicode((uint)key.Key, 0, modifiers, outputBuilder, bufferLength, 0);
            if (result > 0 && result <= outputBuilder.Length)
                return outputBuilder.ToString(0, result);
            else if (relaxed)
                return string.Empty;
            else //Fail early (-P)
                throw new("Invalid key (" + key.KeyChar + "/" + result + "/" + outputBuilder + ")");
        }
    }

    public static string ToAscii(ConsoleKeyInfo key)
    {
        var outputBuilder = new StringBuilder(2);
        var modifiers = GetKeyState(key.Modifiers);
        var result = ToAscii((uint)key.Key, 0, modifiers, outputBuilder, 0);
        if (result > 0)
            return outputBuilder.ToString(0, result);
        else
            throw new("Invalid key (" + key + ")");
    }

    const byte HighBit = 0x80;

    static byte[] GetKeyState(ConsoleModifiers modifiers)
    {
        var keyState = new byte[256];

        if ((modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
            keyState[VK_SHIFT] = HighBit;
        if ((modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            keyState[VK_CONTROL] = HighBit;
        if ((modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
            keyState[VK_ALT] = HighBit;

        keyState[VK_CAPITAL] = (byte)(_queryKeyState(VK_CAPITAL) & 0xFF);

        return keyState;
    }

    [DllImport("user32.dll", EntryPoint = nameof(GetKeyState))]
    static extern short _queryKeyState(uint nVirtKey);

    [DllImport("user32.dll")]
    static extern int ToAscii(
        uint uVirtKey,
        uint uScanCode,
        byte[] lpKeyState,
        [Out] StringBuilder lpChar,
        uint uFlags
    );

    [DllImport("user32.dll")]
    static extern int ToUnicode(
        uint wVirtKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff,
        int cchBuff,
        uint wFlags
    );
}
