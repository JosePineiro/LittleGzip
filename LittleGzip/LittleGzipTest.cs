using ClsParallel;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace TestLittleGzip
{
    public partial class LittleGzipTest : Form
    {
        public LittleGzipTest()
        {
            InitializeComponent();
            if (IntPtr.Size == 8)
                this.Text = Application.ProductName + " v" + Application.ProductVersion + " [x64]";
            else
                this.Text = Application.ProductName + " v" + Application.ProductVersion + " [x86]";

            CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>Select the source directory</summary>
        private void buttonSource_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    if (Directory.Exists(this.textBoxSource.Text))
                        folderBrowserDialog.SelectedPath = this.textBoxSource.Text;
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        this.textBoxSource.Text = folderBrowserDialog.SelectedPath;
                        this.textBoxGzipFile.Text = folderBrowserDialog.SelectedPath + ".gz";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\nEn frmMain.buttonInPath_Click\r\nCapture esta pantalla y reporte en el foro de soporte.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Select the destination ZIP file</summary>
        private void buttonGzipFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.RestoreDirectory = true;
            if (Directory.Exists(Path.GetDirectoryName(this.textBoxGzipFile.Text)))
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(this.textBoxGzipFile.Text);
            saveFileDialog.Filter = "GZ (*.GZip)|*.GZ";
            saveFileDialog.DefaultExt = ".gz";
            saveFileDialog.OverwritePrompt = false;
            if (DialogResult.OK == saveFileDialog.ShowDialog())
            {
                this.textBoxGzipFile.Text = saveFileDialog.FileName;
            }
        }

        private void buttonGZIP_Click(object sender, EventArgs e)
        {
            //Get the files in dir
            string[] files = Directory.GetFiles(this.textBoxSource.Text, "*.*", SearchOption.AllDirectories);
            this.progressBar.Maximum = files.Length;

            //ZIP file exist append the files
            using (LittleGzip gzip = new LittleGzip(this.textBoxGzipFile.Text))
            {
                clsParallel.For(0, files.Length, delegate(int f)
                //for (int f = 0; f < files.Length; f++)
                {
                    gzip.AddFile(files[f], files[f].Substring(this.textBoxSource.Text.Length), 13, "");
                    this.progressBar.Value++;
                    Application.DoEvents();
                } );
            }
            this.progressBar.Value = 0;
        }
    }
}
