namespace FormTest
{
    partial class FormDataCollection
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
            this.lab_Code = new System.Windows.Forms.Label();
            this.tbox_Code = new System.Windows.Forms.TextBox();
            this.lab_BeginTime = new System.Windows.Forms.Label();
            this.dateTimePicker_Begin = new System.Windows.Forms.DateTimePicker();
            this.lab_EndTime = new System.Windows.Forms.Label();
            this.dateTimePicker_End = new System.Windows.Forms.DateTimePicker();
            this.button_Select = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lab_Code
            // 
            this.lab_Code.Location = new System.Drawing.Point(72, 43);
            this.lab_Code.Name = "lab_Code";
            this.lab_Code.Size = new System.Drawing.Size(57, 33);
            this.lab_Code.TabIndex = 0;
            this.lab_Code.Text = "条码";
            this.lab_Code.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbox_Code
            // 
            this.tbox_Code.Location = new System.Drawing.Point(135, 45);
            this.tbox_Code.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbox_Code.Name = "tbox_Code";
            this.tbox_Code.Size = new System.Drawing.Size(265, 25);
            this.tbox_Code.TabIndex = 1;
            // 
            // lab_BeginTime
            // 
            this.lab_BeginTime.AutoSize = true;
            this.lab_BeginTime.Location = new System.Drawing.Point(472, 48);
            this.lab_BeginTime.Name = "lab_BeginTime";
            this.lab_BeginTime.Size = new System.Drawing.Size(67, 15);
            this.lab_BeginTime.TabIndex = 2;
            this.lab_BeginTime.Text = "起始时间";
            // 
            // dateTimePicker_Begin
            // 
            this.dateTimePicker_Begin.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_Begin.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_Begin.Location = new System.Drawing.Point(548, 45);
            this.dateTimePicker_Begin.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dateTimePicker_Begin.Name = "dateTimePicker_Begin";
            this.dateTimePicker_Begin.Size = new System.Drawing.Size(202, 25);
            this.dateTimePicker_Begin.TabIndex = 3;
            // 
            // lab_EndTime
            // 
            this.lab_EndTime.AutoSize = true;
            this.lab_EndTime.Location = new System.Drawing.Point(772, 48);
            this.lab_EndTime.Name = "lab_EndTime";
            this.lab_EndTime.Size = new System.Drawing.Size(67, 15);
            this.lab_EndTime.TabIndex = 4;
            this.lab_EndTime.Text = "终止时间";
            // 
            // dateTimePicker_End
            // 
            this.dateTimePicker_End.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_End.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_End.Location = new System.Drawing.Point(849, 45);
            this.dateTimePicker_End.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dateTimePicker_End.Name = "dateTimePicker_End";
            this.dateTimePicker_End.Size = new System.Drawing.Size(209, 25);
            this.dateTimePicker_End.TabIndex = 5;
            // 
            // button_Select
            // 
            this.button_Select.BackColor = System.Drawing.SystemColors.Control;
            this.button_Select.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_Select.Location = new System.Drawing.Point(1098, 39);
            this.button_Select.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Select.Name = "button_Select";
            this.button_Select.Size = new System.Drawing.Size(89, 33);
            this.button_Select.TabIndex = 6;
            this.button_Select.Text = "查询";
            this.button_Select.UseVisualStyleBackColor = false;
            this.button_Select.Click += new System.EventHandler(this.button_Select_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 113);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 30;
            this.dataGridView1.Size = new System.Drawing.Size(1338, 505);
            this.dataGridView1.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lab_EndTime);
            this.panel1.Controls.Add(this.lab_Code);
            this.panel1.Controls.Add(this.button_Select);
            this.panel1.Controls.Add(this.tbox_Code);
            this.panel1.Controls.Add(this.dateTimePicker_End);
            this.panel1.Controls.Add(this.lab_BeginTime);
            this.panel1.Controls.Add(this.dateTimePicker_Begin);
            this.panel1.Location = new System.Drawing.Point(12, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1338, 109);
            this.panel1.TabIndex = 8;
            // 
            // FormDataCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1362, 625);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dataGridView1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormDataCollection";
            this.Text = "数据采集";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lab_Code;
        private System.Windows.Forms.TextBox tbox_Code;
        private System.Windows.Forms.Label lab_BeginTime;
        private System.Windows.Forms.DateTimePicker dateTimePicker_Begin;
        private System.Windows.Forms.Label lab_EndTime;
        private System.Windows.Forms.DateTimePicker dateTimePicker_End;
        private System.Windows.Forms.Button button_Select;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel1;
    }
}