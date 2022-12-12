using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pofoduino_V2
{
    public class Pofoduino
    {
        private SerialPort Pofoduino_PortCom;
        public Boolean ConnectedToPofoduino;


        public Boolean OpenPofoduino(String Port_Com, ToolStripLabel Label_Status)
        {
            try
            {
                Label_Status.Text = "Try to open Port Com";

                Pofoduino_PortCom = new SerialPort(Port_Com, 115200);
                Pofoduino_PortCom.DtrEnable = true;
                Pofoduino_PortCom.RtsEnable = true;
                Pofoduino_PortCom.Open();

                ConnectedToPofoduino = true;

                return true;
            }
            catch (Exception e)
            {
                Label_Status.Text = "Error port COM";
                return false;
            }
        }

        public Boolean SendFile(string PathAndNameFile,ToolStripProgressBar My_ProgressBar, ToolStripLabel My_Percent)
        {

            Pofoduino_PortCom.DiscardInBuffer();

            if (ConnectedToPofoduino)
            {
                byte[] USB_Buffer = new byte[10];


                byte[] NameinArrayOfByte = Encoding.ASCII.GetBytes(Path.GetFileName(PathAndNameFile));

                //send lengh of the name
                USB_Buffer[0] = (byte)NameinArrayOfByte.Length;
                Pofoduino_PortCom.Write(USB_Buffer, 0, 1);
                Pofoduino_PortCom.ReadByte();

                //send the name
                for (int i = 0; i < NameinArrayOfByte.Length; i++)
                {
                    USB_Buffer[0] = NameinArrayOfByte[i];
                    Pofoduino_PortCom.Write(USB_Buffer, 0, 1);
                    Pofoduino_PortCom.ReadByte();
                }


                //send size of File
                ulong SizeOfFile = Convert.ToUInt64(new System.IO.FileInfo(PathAndNameFile).Length);

                USB_Buffer[0] = (byte)((int)(SizeOfFile & 0xFF000000) >> 24);
                USB_Buffer[1] = (byte)((int)(SizeOfFile & 0x00FF0000) >> 16);
                USB_Buffer[2] = (byte)((int)(SizeOfFile & 0x0000FF00) >> 8);
                USB_Buffer[3] = (byte)((int)(SizeOfFile & 0x000000FF));
                Pofoduino_PortCom.Write(USB_Buffer, 0, 4);

                My_ProgressBar.Minimum = 0;
                My_ProgressBar.Maximum = (int)SizeOfFile - 1;



                //send the data
                byte[] FileBuffer = System.IO.File.ReadAllBytes(PathAndNameFile);

                for (ulong i = 0; i < SizeOfFile; i++)
                {
                    USB_Buffer[0] = FileBuffer[i];
                    Pofoduino_PortCom.Write(USB_Buffer, 0, 1);
                    Pofoduino_PortCom.ReadByte();
                    My_ProgressBar.Value = (int)i;


                    int Value = (My_ProgressBar.Value * 100) / My_ProgressBar.Maximum;

                    My_Percent.Text = Value.ToString() + "%";
                    //My_Percent.Refresh();
                }

                My_Percent.Text = "100%";
                //My_Percent.Refresh();
            }
            return false;
        }

        public Boolean ClosePofoduino()
        {
            if (ConnectedToPofoduino)
            {
                try
                {
                    Pofoduino_PortCom.Close();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return false;
        }




    }
}
