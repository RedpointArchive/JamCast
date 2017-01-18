namespace JamCast.Windows
{
    partial class Projector
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._displayPanel = new System.Windows.Forms.Panel();
            this._ffmpegStatusLabel = new System.Windows.Forms.Label();
            this._timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // _displayPanel
            // 
            this._displayPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._displayPanel.BackColor = System.Drawing.Color.Black;
            this._displayPanel.Location = new System.Drawing.Point(0, 60);
            this._displayPanel.Margin = new System.Windows.Forms.Padding(0);
            this._displayPanel.Name = "_displayPanel";
            this._displayPanel.Size = new System.Drawing.Size(400, 280);
            this._displayPanel.TabIndex = 1;
            // 
            // _ffmpegStatusLabel
            // 
            this._ffmpegStatusLabel.BackColor = System.Drawing.Color.Black;
            this._ffmpegStatusLabel.Font = new System.Drawing.Font("Segoe UI", 20F);
            this._ffmpegStatusLabel.ForeColor = System.Drawing.Color.White;
            this._ffmpegStatusLabel.Location = new System.Drawing.Point(10, 10);
            this._ffmpegStatusLabel.Margin = new System.Windows.Forms.Padding(0);
            this._ffmpegStatusLabel.Name = "_ffmpegStatusLabel";
            this._ffmpegStatusLabel.Size = new System.Drawing.Size(180, 40);
            this._ffmpegStatusLabel.TabIndex = 0;
            this._ffmpegStatusLabel.Text = "Starting";
            this._ffmpegStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _timer
            // 
            this._timer.Enabled = true;
            this._timer.Interval = 16;
            this._timer.Tick += new System.EventHandler(this._timer_Tick);
            // 
            // Projector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(400, 400);
            this.Controls.Add(this._ffmpegStatusLabel);
            this.Controls.Add(this._displayPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Projector";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JamCast Projector";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Projector_FormClosed);
            this.Load += new System.EventHandler(this.Projector_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Projector_Paint);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel _displayPanel;
        private System.Windows.Forms.Timer _timer;
        private System.Windows.Forms.Label _ffmpegStatusLabel;
    }
}

