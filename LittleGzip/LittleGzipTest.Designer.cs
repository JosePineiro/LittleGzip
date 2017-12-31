namespace TestLittleGzip
{
    partial class LittleGzipTest
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.buttonGZIP = new System.Windows.Forms.Button();
            this.textBoxGzipFile = new System.Windows.Forms.TextBox();
            this.buttonGzipFile = new System.Windows.Forms.Button();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.buttonSource = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 217);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(940, 35);
            this.progressBar.TabIndex = 18;
            // 
            // buttonGZIP
            // 
            this.buttonGZIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGZIP.Location = new System.Drawing.Point(398, 158);
            this.buttonGZIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonGZIP.Name = "buttonGZIP";
            this.buttonGZIP.Size = new System.Drawing.Size(112, 46);
            this.buttonGZIP.TabIndex = 17;
            this.buttonGZIP.Text = "GZIP";
            this.buttonGZIP.UseVisualStyleBackColor = true;
            this.buttonGZIP.Click += new System.EventHandler(this.buttonGZIP_Click);
            // 
            // textBoxGzipFile
            // 
            this.textBoxGzipFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGzipFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGzipFile.Location = new System.Drawing.Point(18, 62);
            this.textBoxGzipFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxGzipFile.Name = "textBoxGzipFile";
            this.textBoxGzipFile.Size = new System.Drawing.Size(832, 30);
            this.textBoxGzipFile.TabIndex = 16;
            this.textBoxGzipFile.Text = "GZipFile";
            // 
            // buttonGzipFile
            // 
            this.buttonGzipFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGzipFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGzipFile.Location = new System.Drawing.Point(858, 56);
            this.buttonGzipFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonGzipFile.Name = "buttonGzipFile";
            this.buttonGzipFile.Size = new System.Drawing.Size(69, 42);
            this.buttonGzipFile.TabIndex = 15;
            this.buttonGzipFile.Text = ". . .";
            this.buttonGzipFile.UseVisualStyleBackColor = true;
            this.buttonGzipFile.Click += new System.EventHandler(this.buttonGzipFile_Click);
            // 
            // textBoxSource
            // 
            this.textBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSource.Location = new System.Drawing.Point(18, 11);
            this.textBoxSource.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(832, 30);
            this.textBoxSource.TabIndex = 14;
            this.textBoxSource.Text = "Souce";
            // 
            // buttonSource
            // 
            this.buttonSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSource.Location = new System.Drawing.Point(858, 5);
            this.buttonSource.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonSource.Name = "buttonSource";
            this.buttonSource.Size = new System.Drawing.Size(69, 42);
            this.buttonSource.TabIndex = 13;
            this.buttonSource.Text = ". . .";
            this.buttonSource.UseVisualStyleBackColor = true;
            this.buttonSource.Click += new System.EventHandler(this.buttonSource_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 252);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonGZIP);
            this.Controls.Add(this.textBoxGzipFile);
            this.Controls.Add(this.buttonGzipFile);
            this.Controls.Add(this.textBoxSource);
            this.Controls.Add(this.buttonSource);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonGZIP;
        private System.Windows.Forms.TextBox textBoxGzipFile;
        private System.Windows.Forms.Button buttonGzipFile;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Button buttonSource;
    }
}

