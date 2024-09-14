
namespace TheMatrix
{
    partial class MatrixForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MatrixForm));
            TimerDraw = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // TimerDraw
            // 
            TimerDraw.Enabled = true;
            TimerDraw.Interval = 120;
            TimerDraw.Tick += TimerDraw_Tick;
            // 
            // MatrixForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(800, 450);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "MatrixForm";
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "The Matrix";
            Scroll += MatrixForm_Scroll;
            KeyPress += MatrixForm_KeyPress;
            MouseClick += MatrixForm_MouseClick;
            MouseMove += MatrixForm_MouseMove;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer TimerDraw;
    }
}

