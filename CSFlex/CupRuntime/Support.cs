/*
 * C# CUP runtime compatibility source recovered from runtime.dll shipped by
 * CSFlex 1.4 and modernized in July 2026. Copyright 1996-1999 Scott Hudson,
 * Frank Flannery, and C. Scott Ananian. See LICENSE.
 */

using System;

namespace java_cup;

public class Support
{
    private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

    public static long currentTimeMillis()
    {
        return (long)(DateTime.Now - epoch).TotalMilliseconds;
    }
}
