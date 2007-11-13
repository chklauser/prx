/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.IO;
using System.Text;

namespace Prexonite.Helper
{
    /// <summary>
    /// Carries the line caugth by the <see cref="LineCatcher"/>.
    /// </summary>
    public class LineCaughtEventArgs : EventArgs
    {
        /// <summary>
        /// The line caught by the <see cref="LineCatcher"/>.
        /// </summary>
        public string Line
        {
            get { return _line; }
            set { _line = value; }
        }

        private string _line;

        /// <summary>
        /// Creates a new instance of the LineCaughtEventArgs.
        /// </summary>
        /// <param name="line">The line caught by the <see cref="LineCatcher"/>.</param>
        public LineCaughtEventArgs(string line)
        {
            _line = line;
        }
    }

    /// <summary>
    /// The delegate used by the <see cref="LineCatcher"/> to report lines.
    /// </summary>
    /// <param name="sender">The instance of <see cref="LineCatcher"/> that fired the event.</param>
    /// <param name="o">An event argument that contains the line caught.</param>
    public delegate void LineCaughtEventHandler(object sender, LineCaughtEventArgs o);

    /// <summary>
    /// Plugs into Parser classes generated by PxCoco/R.
    /// </summary>
    public class LineCatcher : TextWriter
    {
        public event LineCaughtEventHandler LineCaught;

        ///<summary>
        ///When overridden in a derived class, returns the <see cref="T:System.Text.Encoding"></see> in which the output is written.
        ///</summary>
        ///<returns>
        ///The Encoding in which the output is written.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        private StringBuilder buffer = new StringBuilder();

        /// <summary>
        /// Write text to the buffer. As soon as a newline character is encountered, the <see cref="LineCaught"/> event is raised.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override void Write(char value)
        {
            if (value == '\n')
            {
                LineCaught(this, new LineCaughtEventArgs(buffer.ToString().Trim()));
                buffer.Length = 0;
            }
            else
            {
                buffer.Append(value);
            }
        }
    }
}