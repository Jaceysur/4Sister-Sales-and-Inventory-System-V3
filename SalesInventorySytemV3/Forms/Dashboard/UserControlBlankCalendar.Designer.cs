namespace SalesInventorySytemV3.Forms.Main
{
    partial class UserControlBlankCalendar
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbDays1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbDays1
            // 
            this.lbDays1.AutoSize = true;
            this.lbDays1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDays1.ForeColor = System.Drawing.Color.DarkGray;
            this.lbDays1.Location = new System.Drawing.Point(39, 0);
            this.lbDays1.Name = "lbDays1";
            this.lbDays1.Size = new System.Drawing.Size(23, 15);
            this.lbDays1.TabIndex = 0;
            this.lbDays1.Text = "00";
            // 
            // UserControlBlankCalendar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lbDays1);
            this.Name = "UserControlBlankCalendar";
            this.Size = new System.Drawing.Size(62, 38);
            this.Load += new System.EventHandler(this.UserControlBlankCalendar_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbDays1;
    }
}
