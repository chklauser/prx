using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Prx.Win32
{
    public static class User32
    {

        private const uint
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_ALT = 0x12, //a.k.a. "VK_MENU"
            VK_CAPITAL = 0x14; //CAPS LOCK

        public static string ToUnicode(ConsoleKeyInfo key, bool relaxed)
        {
            const int bufferLength = 16;
            var outputBuilder = new StringBuilder(bufferLength);
            var modifiers = GetKeyState(key.Modifiers);
            var result = ToUnicode((uint) key.Key, 0, modifiers, outputBuilder, bufferLength, 0);
            if (result > 0)
                return outputBuilder.ToString(0, result);
            else if (relaxed)
                return String.Empty;
            else
                throw new Exception("Invalid key (" + key.KeyChar + "/" + result + "/" + outputBuilder +")");
        }

        public static string ToAscii(ConsoleKeyInfo key)
        {
            var outputBuilder = new StringBuilder(2);
            var modifiers = GetKeyState(key.Modifiers);
            var result = ToAscii((uint)key.Key, 0, modifiers,
                                     outputBuilder, 0);
            if (result > 0)
                return outputBuilder.ToString(0,result);
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

            //foreach (ConsoleModifiers key in Enum.GetValues(typeof(ConsoleModifiers)))
            //{
            //    if ((modifiers & key) == key)
            //    {
            //        keyState[(int)key] = HighBit;
            //    }
            //}
            return keyState;
        }

        [DllImport("user32.dll",EntryPoint = "GetKeyState")]
        private static extern short _queryKeyState(uint nVirtKey);

        [DllImport("user32.dll")]
        private static extern int ToAscii(uint uVirtKey, uint uScanCode,
                                          byte[] lpKeyState,
                                          [Out] StringBuilder lpChar,
                                          uint uFlags);

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
           [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff, int cchBuff,
           uint wFlags);

    }
}