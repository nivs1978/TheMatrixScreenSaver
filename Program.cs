/********************************************************************************
 *                                                                              *
 *                           This file is part of                               *
 *                           The Matrix screen saver                            *
 *                                                                              *
 *  Copyright (c) 2024 Hans Milling                                             *
 *                                                                              *
 *  The Matrix screen saver is free software: you can redistribute it and/or    *
 *  modify it under the terms of the GNU General Public License as published by *
 *  the Free Software Foundation, either version 3 of the License, or           *
 *  (at your option) any later version.                                         *
 *                                                                              *
 *  The Matrix screen saver is distributed in the hope that it will be useful,  *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of              *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                *
 *  GNU General Public License for more details. You should have received a     *
 *  copy of the GNU General Public License along with The Matrix screen saver.  *
 *  If not, see http://www.gnu.org/licenses/.                                   *
 *                                                                              *
 ********************************************************************************/


using System;
using System.Linq;
using System.Windows.Forms;

namespace TheMatrix
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            if (args.Length > 0)
            {
                string arg = args[0].ToLower().Trim();
                string handler = null;

                // Handle cases where arguments are separated by colon.
                // Examples: /c:1234567 or /P:1234567
                if (arg.Length > 2)
                {
                    handler = arg.Substring(3).Trim();
                    arg = arg.Substring(0, 2);
                }
                else if (args.Length > 1)
                    handler = args[1];

                if (arg == "/c") // Configuration mode, show configuration form
                {
                    MessageBox.Show("This screen saver has no options that you can set", "The Matrix screen saver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Application.Exit();
                }
                else if (arg == "/p") // Preview mode, handler given to preview window
                {
                    IntPtr previewWndHandle = new IntPtr(long.Parse(handler));
                    Application.Run(new MatrixForm(previewWndHandle));

                }
                else if (arg == "/s") // Run screensaverin full screen mode
                {
                    TheMatrixApplicationContext context = new();
                    Application.Run(context);
                }
            }
            else // If no parameters give, run in configuration mode
            {
                MessageBox.Show("This screen saver has no options that you can set", "The Matrix screen saver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
        }
    }
}
