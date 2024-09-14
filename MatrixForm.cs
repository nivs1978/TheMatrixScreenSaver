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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TheMatrix
{
    public partial class MatrixForm : Form
    {
        // From observing the original movie, this logic has been applied (not all attempts of matrix screen savers found online, follow the original in the movie)
        // - It does not look like a rain drop can dop in an already active rain drop
        // - All rain drops have the same speed

        readonly Random r = new();
        int fontSize = 24;
        Font f;
        Graphics g;
        // This forum answer was used to get the characters for the screen saver: https://scifi.stackexchange.com/questions/137575/is-there-a-list-of-the-symbols-shown-in-the-matrixthe-symbols-rain-how-many/182823#182823
        const string arabicNumerals = "٠١٢٣٤٥٦٧٨٩١٠";
        const string punctuations = ":.\"=*+-¦|_╌";
        const string latinNumerals = "012345789";
        const string latinLetters = "Z";
        const string katakanaCharacters = "ｦｱｳｴｵｶｷｹｺｻｼｽｾｿﾀﾂﾃﾅﾆﾇﾈﾊﾋﾎﾏﾐﾑﾒﾓﾔﾕﾗﾘﾜ";
        const string kanjiCharacters = "日";
        const string allCharacters = $"{arabicNumerals}{punctuations}{latinNumerals}{latinLetters}{katakanaCharacters}{kanjiCharacters}";
        const int newRandomCharacterChance = 20; // 1 in x characters on screen will be replaced by a new
        const int fadeCharacterChance = 50; // 1 in x characters will fade to dark green
        const int newRainDropChange = 2; // 1 in x change of making an available column into a new raindrop
        const int ColumnCount = 60; // Looks like 60 columns when I look at the original movie
        const int columnHeightMultiplier = 2; // a rain drop can only survive maximum of the same time it took to fall the entire screen length when compared to how it behaves in the movie 
        const float fontSqueezing = 0.8f; // The unicode font has too much "air" on the sides, so we squeeze them a bit together to makre it look more true to the original
        const int rainDropSpeedMs = 3000; // From the movie it looks like each rain drop takes approximatly 3 seconds from top to bottom of screen
        int ColumnHeight; // Calculated during initialization based on screen size and font size
        TheMatrix theMatrix; // Holds whats goin on in the matrix
        Bitmap[] whiteChars; // Predrawn characters as bitmaps
        Bitmap[] greenChars;
        Bitmap[] fadedChars;
        List<int> activeColumns = new(); // Active columns of drops of rain
        List<int> availableColumns = new(); // Columns that curretly has no rain drops
        Brush blackBrush = new SolidBrush(Color.Black);
        private Point lastMousePosition;

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        private class Column
        {
            public int Timeout; // How many frames should the columns characters be displayed before bein removed (length of rain drop)
            public List<Cell> Cells = new(); // All the rows on the screen for this rain drop
        }

        // Represents a cell in the matrix (character on screen)
        private class Cell
        {
            public int CharNo; // Index in the bitmap array
            public bool Faded; // if character is 50% faded away (dark green)
            public int Timer; // How many frames the character has been displayed on screen, when reaching 0 it will not be displayed anymore
        }

        private class TheMatrix
        {
            public List<Column> Columns = new();
        }

        private void Initialize()
        {
            this.MouseWheel += new MouseEventHandler(MatrixForm_MouseClick);
            // Go full screen
            lastMousePosition = Cursor.Position;
            Cursor.Hide();
            // Create GDI+ drawing canvas from form
            g = this.CreateGraphics();
            g.Clear(Color.Black);
            // Setup font and constants based on monitor resolution

            fontSize = (int)((this.Width * 1.2) / (ColumnCount * fontSqueezing)); // We squeez the characters a bit together, this looks more true to the original matrix code in the movie
            f = new Font("System", fontSize * 1.2f, FontStyle.Regular, GraphicsUnit.Pixel);

            var size = g.MeasureString("日", f);

            ColumnHeight = this.Height / fontSize;
            //////////TimerDraw.Interval = rainDropSpeedMs / ColumnHeight; // Calculate character drop delay based on desired speed and screen height
            // Start with "empty" matrix, character -1 = blank character
            theMatrix = new TheMatrix();
            for (int x = 0; x < ColumnCount; x++)
            {
                availableColumns.Add(x);
                var column = new Column() { Timeout = r.Next(ColumnHeight, ColumnHeight * columnHeightMultiplier) };
                for (var y = 0; y < ColumnHeight; y++)
                {
                    column.Cells.Add(new Cell { Timer = -1, Faded = false, CharNo = -1 });
                }
                theMatrix.Columns.Add(column);
            }
            // Pregenerate all characters as bitmaps. Drawing these will be faster than erasing a character and drawing it again
            whiteChars = new Bitmap[allCharacters.Length];
            greenChars = new Bitmap[allCharacters.Length];
            fadedChars = new Bitmap[allCharacters.Length];
            for (int i = 0; i < allCharacters.Length; i++)
            {
                whiteChars[i] = CharacterToBitmap(i, Color.White); // Character at the front of the falling rain
                greenChars[i] = CharacterToBitmap(i, Color.Lime); // Color of the majority of characters
                fadedChars[i] = CharacterToBitmap(i, Color.DarkGreen); // Some characters are randomly faded 50% or dark green
            }
        }

        public MatrixForm(int screenIndex)
        {
            InitializeComponent();
            this.Bounds = Screen.AllScreens[screenIndex].Bounds;
            Initialize();
        }

        public MatrixForm(IntPtr PreviewWndHandle)
        {
            InitializeComponent();

            SetParent(this.Handle, PreviewWndHandle);
            SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));
            Rectangle ParentRect;
            GetClientRect(PreviewWndHandle, out ParentRect);
            Size = ParentRect.Size;
            Location = new Point(0, 0);

            Initialize();
        }

        // Generate a bitmap from a character and color. All characters in the matrix screensaver are mirrored, so we also make sure to draw mirrored
        private Bitmap CharacterToBitmap(int charNo, Color c)
        {
            var bmp = new Bitmap((int)(fontSize * fontSqueezing), fontSize, PixelFormat.Format32bppArgb);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.TextRenderingHint = TextRenderingHint.AntiAlias;
                gr.InterpolationMode = InterpolationMode.HighQualityBilinear;
                gr.Clear(Color.Black);
                gr.ScaleTransform(-1f, 1f);
                using var b = new SolidBrush(c);
                gr.DrawString(allCharacters[charNo].ToString(), f, b, (-fontSize) + (fontSize / 8), -(fontSize / 5));
            }
            return bmp;
        }

        // Generate a random character, including blank
        private Cell NewCell()
        {
            int charNo = r.Next(0, allCharacters.Length) - 1;
            return new Cell { CharNo = charNo, Faded = false, Timer = 0 };
        }

        // Print character on screen based on cell information
        private void PrintCell(int col, int row, Cell c)
        {
            int x = (int)(col * fontSize * fontSqueezing);
            int y = row * fontSize;
            if (c.Timer == -1 || c.CharNo == -1) // If character is blank or timer is up (end of rain drop), clear the square on screen
            {
                g.FillRectangle(blackBrush, x, y, fontSize * fontSqueezing, fontSize);
            }
            else if (c.Timer == 0) // First time the character is displayed (leading rain drop)
            {
                g.DrawImageUnscaled(whiteChars[c.CharNo], x, y);
            }
            else if (c.Faded) // Character is faded
            {
                g.DrawImageUnscaled(fadedChars[c.CharNo], x, y);
            }
            else // Normal character (the majority is this color)
            {
                g.DrawImageUnscaled(greenChars[c.CharNo], x, y);
            }
        }

        private void UpdateTheMatrix()
        {
            // Main loop that updates the matrix and draw changes to the screen
            // Instead of redrawing the whole screen, it's faster to just draw or erase characters
            var finished = new List<int>();
            foreach (var col in activeColumns)
            {
                var column = theMatrix.Columns[col];
                for (int row = ColumnHeight - 1; row >= 0; row--) // Go trough the column from bottom to top
                {
                    var cell = column.Cells[row];
                    if (cell.Timer >= 0) // Only look at cells that contain a character
                    {
                        if (cell.Timer == 0) // 0 means that we are at the bottom of the rain drop
                        {
                            if (row < ColumnHeight - 1) // If we are not at the bottom of the screen, draw a new character below the current
                            {
                                var cNew = NewCell();
                                column.Cells[row + 1] = cNew;
                                PrintCell(col, row + 1, cNew);
                            }
                            var cCurrent = column.Cells[row]; // Increase timer of the current character
                            cCurrent.Timer++;
                            PrintCell(col, row, cCurrent);
                        }
                        else if (cell.Timer > column.Timeout) // Character has been too long on the screen, remove it
                        {
                            var c = new Cell { Timer = -1, Faded = false, CharNo = -1 };
                            column.Cells[row] = c;
                            PrintCell(col, row, c);
                            if (row == ColumnHeight - 1)
                            {
                                finished.Add(col);
                                continue;
                            }
                        }
                        else
                        { // 
                            bool changed = false;
                            if (r.Next(0, newRandomCharacterChance) == 0) // caracter will be set to a new character
                            {
                                cell.CharNo = r.Next(0, allCharacters.Length - 1);
                                cell.Faded = false;
                                changed = true;
                            }
                            else
                            if (r.Next(0, fadeCharacterChance) == 0) // character will be faded to dark green 
                            {
                                cell.Faded = true;
                                changed = true;
                            }
                            cell.Timer++;
                            if (changed) // only redraw if caracter is changed
                            {
                                PrintCell(col, row, cell);
                            }
                        }
                    }
                }
            }

            // Check if column (rain drop) is finished (disapeared from screen) and remove it and make it available for a new rain drop
            foreach (int col in finished)
            {
                activeColumns.Remove(col);
                availableColumns.Add(col);
            }
            finished.Clear();

            // If rain drop columns available check if we want to start a new rain drop
            if (availableColumns.Count > 0 && r.Next(0, newRainDropChange) == 0)
            {
                int idx = r.Next(0, availableColumns.Count - 1);
                int nextColumn = availableColumns[idx];
                availableColumns.RemoveAt(idx);
                var c = NewCell();
                theMatrix.Columns[nextColumn].Cells[0] = c;
                PrintCell(nextColumn, 0, c);
                activeColumns.Add(nextColumn);
            }
        }

        private void TimerDraw_Tick(object sender, EventArgs e)
        {
            UpdateTheMatrix();
        }

        private void MatrixForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            this.Close();
        }

        private void MatrixForm_MouseMove(object sender, MouseEventArgs e)
        {
            var movement = Math.Max(Math.Abs(e.X - lastMousePosition.X), Math.Abs(e.Y - lastMousePosition.Y));
            lastMousePosition = e.Location;
            if (movement > 20)
            {
                this.Close();
            }
        }

        private void MatrixForm_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }

        private void MatrixForm_Scroll(object sender, ScrollEventArgs e)
        {
            this.Close();
        }
    }
}
