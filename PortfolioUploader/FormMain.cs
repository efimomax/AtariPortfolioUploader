using Pofoduino_V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortfolioUploader
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }



        private void FormMain_Load(object sender, EventArgs e)
        {
            Init();

            listBoxDrives.SelectedIndex = 0;
        }

        private void Init()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            listBoxDrives.Items.AddRange(drives.Select(z=> new MyDriveInfo() { Name = z.Name, Type = z.DriveType }).ToArray());

            //serial
            FillSerialPorts();


            //drive select
            listBoxDrives.SelectedIndexChanged += (sender, e) => {
                if ((sender as ListBox).SelectedItem is MyDriveInfo driveInfo) OpenFolder(driveInfo.Name);
            };

            //dbl_click on folder or parent
            listViewFiles.MouseDoubleClick += (sender, e) => {
                var lv = (sender as ListView);
                if (lv is null) return;

                ListViewItem si = lv.SelectedItems[0];
                if (!si.IsFile())
                {
                    try
                    {
                        OpenFolder(si.Tag);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            };

            listViewFiles.ColumnClick += (sender, e) => {
                Process.Start($@"{columnHeader1.Text}");
            };

            listViewFiles.ItemSelectionChanged += (sender, e) =>
            {
                var lv = (sender as ListView);
                if (lv is null) return;

                var files = new List<ListViewItem>();
                foreach (ListViewItem item in lv.SelectedItems) 
                    if (item.IsFile()) 
                        files.Add(item);
                

                var sizeAll = files.Sum(z => new FileInfo(z.Tag.ToString()).Length);
                var sizeText = Helper.SizeSuffix(sizeAll);

                string msg = null;
                if (files.Count == 1)
                {
                    msg = $"{sizeText} in 1 selected file";
                }
                else if (files.Count > 1)
                {
                    msg = $"{sizeText} in {files.Count} selected files";
                }

                Label_Status.Text = $"{msg}";

                buttonSend.Enabled = files.Count > 0;
            };
        }

        private bool FillSerialPorts()
        {
            var names = SerialPort.GetPortNames();

            if (names.Length > 0)
            {
                comboBoxSerial.Items.Clear();

                foreach (var name in names)
                {
                    comboBoxSerial.Items.Add(name);
                }
                comboBoxSerial.SelectedItem = names[0];

                return true;
            }

            return false;
        }




        private void OpenFolder(object folderName)
        {
            if (folderName is null) return;
            var fullFolderName = folderName.ToString();

            var di = new System.IO.DirectoryInfo(fullFolderName);

            columnHeader1.Text = di.FullName;
            
            var parentFolder = System.IO.Directory.GetParent(fullFolderName);
            var folders = di.GetDirectories();
            var files = di.GetFiles();

            
            var parentFolderForList = new ListViewItem("...", "Tree.png") { Tag = parentFolder };
            var foldersForList = folders.Select(z => new ListViewItem(z.Name, "Folder.png") { Tag = z.FullName }).ToArray();
            var filesForList = files.Select(z => new ListViewItem(new[] { z.Name, Helper.SizeSuffix(new FileInfo(z.FullName).Length) }, "File.png") { Tag = z.FullName}).ToArray();

            listViewFiles.Items.Clear();
            if (parentFolder != null)
                listViewFiles.Items.Add(parentFolderForList);
            listViewFiles.Items.AddRange(foldersForList);
            listViewFiles.Items.AddRange(filesForList);

            
        }

        public class MyDriveInfo
        {
            public string Name { get; set; }
            public DriveType Type { get; set; }

            public override string ToString()
            {
                return $@"{Name} ({Type})";
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            var n = listViewFiles.SelectedItems.Count;
            var c = 0;
            ProgressBar_upload.Visible = true;
            Label_Percent.Visible = true;
            foreach (ListViewItem item in listViewFiles.SelectedItems)
            {
                c++;
                if(item != null && item.IsFile())
                {
                    Pofoduino MyPofoDuino = new Pofoduino();

                    if (MyPofoDuino.OpenPofoduino(comboBoxSerial.Text, Label_Status))
                    {

                        Label_Status.Text = $"Uploading file {c} from {n}";
                        MyPofoDuino.SendFile(item.Tag.ToString(), this.ProgressBar_upload, this.Label_Percent);


                        MyPofoDuino.ClosePofoduino();
                        Label_Status.Text = "Port Com closed";

                       

                    }
                    else
                    {
                        MessageBox.Show(" Please select a Port Com ");
                    }
                }
            }

            MessageBox.Show("File(s) uploaded");
            ProgressBar_upload.Value= 0;
            Label_Percent.Text = "";
            ProgressBar_upload.Visible = false;
            Label_Percent.Visible = false;
        }
    }
    
}
