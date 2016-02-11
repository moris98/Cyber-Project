using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrList
{
    public partial class Form1 : Form
    {

        public delegate void dele(string[] data,int n);
        public delegate void dele2(int num);
        public Thread engine;
        public NamedPipeServerStream server;
        public static int x = 13, y = 13, w = 100, l = 25;
        public List<Label[]> clients = new List<Label[]>();
        private int index = 0;
        public string length;
        public Form1()
        {
            InitializeComponent();
            
            //Process pythonProcess = new Process();
            //pythonProcess.StartInfo.FileName = @"C:\Python27\python.exe";
            //pythonProcess.StartInfo.Arguments = @"Python\ServerEngine.py";
            //pythonProcess.StartInfo.WorkingDirectory = Application.StartupPath;
            ////pythonProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //pythonProcess.Start();

            engine = new Thread(new ThreadStart(PipeReader));
            engine.Start();
            
            server = new NamedPipeServerStream("Orders");
            server.WaitForConnection();

           
        }
        public string Getting_Info(Semaphore semaphoreObject,BinaryReader br,BinaryWriter bw)
        {
            string msg = "", str = "";

            semaphoreObject.WaitOne();
                    
            while (msg != "#DONE#")
            {
                var len = (int)br.ReadUInt32();            // Read string length
                if (len == 0)
                    continue;
                msg = new string(br.ReadChars(len));    // Read string

                if (msg != "#DONE#")
                {
                    str += msg;
                    var buf = Encoding.ASCII.GetBytes("ok");

                    bw.Write((uint)buf.Length);                // Write string length
                    bw.Write(buf);

                }

            }

            semaphoreObject.Release();
            return str;
        }

        public byte[] Getting_Image(Semaphore semaphoreObject, BinaryReader br, BinaryWriter bw, int Imglen)
        {
            byte[] msg = new byte[1024];
            byte[] str = new byte [0];

            semaphoreObject.WaitOne();

            while (Encoding.ASCII.GetString(msg).Equals("#DONE#") == false)
            {
                var len = (int)br.ReadUInt32();            // Read string length
                if (len == 0)
                    continue;
                msg = br.ReadBytes(len);               //new string(br.ReadChars(len));    // Read string

                if (Encoding.ASCII.GetString(msg).Equals("#DONE#") == false)
                {
                    str = str.Concat(msg).ToArray();
                    
                    var buf = Encoding.ASCII.GetBytes("ok");
                    bw.Write((uint)buf.Length);                // Write string length
                    bw.Write(buf);

                }
            }
            semaphoreObject.Release();
            return str;
        }

        //public void new_form_image(MemoryStream ms)
        //{
        //    Form form2 = new Form();
        //    //this.SuspendLayout();
        //    // 
        //    // Form2
        //    // 
        //    form2.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        //    form2.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        //    form2.AutoScroll = true;
        //    form2.ClientSize = new System.Drawing.Size(866, 461);
        //    form2.Name = "form2";
        //    form2.Text = "form2";
        //    form2.ResumeLayout(false);

        //    PictureBox pictureBox1 = new PictureBox();
        //    Image x = Image.FromStream(ms);

        //    form2.BackgroundImage = x;
        //    form2.Show();
        //}

        public void PipeReader()
        {
            // Open the named pipe.
            var server = new NamedPipeServerStream("Data");
            var Respond = new NamedPipeServerStream("Respond");
            //Console.WriteLine("Waiting for connection...");
            server.WaitForConnection();
            Respond.WaitForConnection();
            
            //Console.WriteLine("Connected.");
            var br = new BinaryReader(server);
            var bw = new BinaryWriter(Respond);
            //var bw = new BinaryWriter(server);
            
            Semaphore semaphoreObject = new Semaphore(3, 4);
            

            while (true)
            {
                try
                {
                    string str=Getting_Info(semaphoreObject, br, bw);
                    
                    if (str.StartsWith("IMAGE"))
                    {
                        string tempSTR = (string)(str.Substring(6));
                        byte[] byteArrayIn = new byte[Int32.Parse(str.Substring(6))];
                        byteArrayIn = Getting_Image(semaphoreObject, br, bw, Int32.Parse(str.Substring(6)));
                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                length = byteArrayIn.Length.ToString();
                            });
                         
                        }
                        MemoryStream ms = new MemoryStream(byteArrayIn);
                        Image.FromStream(ms).Save("C:\\Users\\User\\Desktop\\IMAGE_IN_GUI.jpg");
                        
                    }
                    else
                    {
                        string[] ClientAndPr = str.Split(',');

                        int n = index;
                        bool exist = false;
                        for (int i = 0; i < clients.Count; i++)
                            if (clients[i][0].Text == ClientAndPr[0])
                            {
                                dele2 invokeDELE2 = new dele2(this.removeC);
                                this.Invoke(invokeDELE2, i);
                                n = i;
                                index -= 1;
                                exist = true;
                                break;
                            }
                        index++;




                        if (!exist)
                            clients.Add(new Label[ClientAndPr.Length]);
                        else
                            clients[n] = new Label[ClientAndPr.Length];

                        dele invokeDELE = new dele(this.addC);
                        this.Invoke(invokeDELE, ClientAndPr, n);





                    }


                    //Console.WriteLine("Read: " + str);
                }
                
                catch(Exception e)
                {

                    MessageBox.Show(e.ToString());
                }
            }

            MessageBox.Show("Engine has disconnected");
            server.Close();
            server.Dispose();

        }

        public void PipeWriter(object sender, EventArgs e)
        {



            Semaphore semaphoreObject = new Semaphore(3, 4);
            var bw = new BinaryWriter(server);
            string msg = "";
           
                try
                {
                    if (sender.GetType() == typeof(Label))
                    {
                        index = -1;
                        for (int i = 0; i < clients.Count; i++)
                        {
                            if (clients[i][0].Text == ((Label)sender).Text)
                                index = i;
                        }

                        string str = Microsoft.VisualBasic.Interaction.InputBox("Which procces you want to end? If the procces does not exist it will start it", "Popup", "Pr name");
                        if (str == "")
                            return;
                        bool exist = false;
                        for (int i = 0; i < clients[index].Length; i++)
                        {
                            if (clients[index][i].Text == str)
                                exist = true;
                        }
                        
                        if (exist)
                            msg = ((Label)sender).Text + ",DEL" + " " + str;
                        else
                            msg = ((Label)sender).Text + ",START" + " " + str;
                    }
                    if (sender.GetType() == typeof(Button))
                    {

                        string Computer_Name = ((Button)sender).Text.Split(new string[] { "- " }, StringSplitOptions.None)[1];
                        
                        msg = Computer_Name + ", Screen_Capture";
                    }
                    var buf = Encoding.ASCII.GetBytes(msg);     // Get ASCII byte array    
                    semaphoreObject.WaitOne(); 
                    bw.Write((uint)buf.Length);                // Write string length
                    bw.Write(buf);
                    semaphoreObject.Release();
                    
                    return;
                }
                catch (EndOfStreamException)
                {
                    
                    server.Close();
                    server.Dispose();
                                        
                }


        }




        public void removeC(int num)
        {
            for (int i = 0; i < clients[num].Length; i++)
                this.Controls.Remove(clients[num][i]);

        }

        public void addC(string[] ClientAndPr, int n)
        {
            Button B = new Button();
            this.Controls.Add(B);

            B.Text = "Tap to Get Print Screen - " + ClientAndPr[0];
            B.Click += new System.EventHandler(this.PipeWriter);


            clients[n][0] = new Label();
            clients[n][0].Text = ClientAndPr[0];
            clients[n][0].Font = new Font(clients[n][0].Font.FontFamily, 10, FontStyle.Bold);
            clients[n][0].Size = new Size(w, l);
            clients[n][0].Location = new Point(x + n * w, y);
            clients[n][0].Parent = this;
            clients[n][0].Click += new System.EventHandler(this.PipeWriter);
            B.Location=new Point(x + n * w, y+20);
            for (int j = 1; j < ClientAndPr.Length; j++)
            {
                clients[n][j] = new Label();
                clients[n][j].Text = ClientAndPr[j];
                clients[n][j].Size = new Size(w, l);
                clients[n][j].Location = new Point(x + n * w, y + l * j+20);
                clients[n][j].Parent = this;
                //clients[n][j + 1] = new Label();
                //clients[n][j + 1].Text = ClientAndPr[j];
                //clients[n][j + 1].Size = new Size(w, l);
                //clients[n][j + 1].Location = new Point(x + n * w + 20, y + l * j + 20);
                //clients[n][j + 1].Parent = this;
                //clients[n][j + 2] = new Label();
                //clients[n][j + 2].Text = ClientAndPr[j];
                //clients[n][j + 2].Size = new Size(w, l);
                //clients[n][j + 2].Location = new Point(x + n * w + 40, y + l * j + 20);
                //clients[n][j + 2].Parent = this;



            }
            

        }

       
        

       















    }
}
