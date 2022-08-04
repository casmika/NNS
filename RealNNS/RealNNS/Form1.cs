using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace RealNNS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            toolStripButton2.PerformClick();
            if (toolStripComboBox1.Items.Count > 0) toolStripComboBox1.SelectedIndex = 0;
            textBox16.Text = "A\tB\tC\tD\tE\tF\tG\tH\tI\tJ\tK\tL\r\n";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) button1.Text = "Save";
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            toolStripComboBox1.Items.Clear();
            String[] ports = SerialPort.GetPortNames();
            toolStripComboBox1.Items.AddRange(ports);
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.Text == "")
            {
                MessageBox.Show("Select Com Port First");
                return;
            }
            if (toolStripButton1.Text == "Open")
            {
                serialPort1.PortName = toolStripComboBox1.Text;
                serialPort1.BaudRate = 9600;
                serialPort1.ReadTimeout = 1000;
                serialPort1.WriteTimeout = 1000;
                serialPort1.DataReceived += SerialPort1_DataReceived;
                serialPort1.Open();
                toolStripButton1.Text = "Close";
            }
            else
            {
                serialPort1.Close();
                toolStripButton1.Text = "Open";
            }

        }

        string data = "";
        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                data = sp.ReadLine();
                this.Invoke(new EventHandler(deCode));
            }
            catch (TimeoutException)
            {
                serialPort1.DiscardInBuffer();
            }
        }

        double time = -1;
        DateTime curTime;

        private void deCode(object sender, EventArgs e)
        {
            try
            {
                double dataIn;
                if(double.TryParse(data, out dataIn))
                {
                    dataIn = double.Parse(data);
                    if(domainUpDown2.Text == "kPa")
                    {
                        dataIn = dataIn / 1000;
                        textBox2.Text = dataIn.ToString("F3");
                    } else if (domainUpDown2.Text == "cm H2O")
                    {
                        dataIn = dataIn / 98.0665;
                        textBox2.Text = dataIn.ToString("F2");
                    } else if(domainUpDown2.Text == "mm Hg")
                    {
                        dataIn = dataIn / 133.322;
                        textBox2.Text = dataIn.ToString("F2");
                    } else if(domainUpDown2.Text == "mBar")
                    {
                        dataIn = dataIn / 100;
                        textBox2.Text = dataIn.ToString("F2");
                    } else textBox2.Text = dataIn.ToString("F0");

                    if (checkBox1.Checked)
                    {
                        if (time < 0) curTime = DateTime.Now;
                        time = (double)(DateTime.Now.ToBinary() - curTime.ToBinary()) / 10000000.0;
                        chart1.Series["Series1"].Points.AddXY(time, dataIn);
                        double sps = (double)chart1.Series["Series1"].Points.Count / time;
                        checkBox1.Text = "Record (" + chart1.Series["Series1"].Points.Count + "). Avg. " + sps.ToString("F1") + " SPS";
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.WriteLine("T");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (button3.Text == "Start") {
                    serialPort1.WriteLine("S");
                    button3.Text = "Stop";
                    toolStripButton1.Enabled = false;
                }
                else
                {
                    serialPort1.WriteLine("M");
                    button3.Text = "Start";
                    toolStripButton1.Enabled = true;
                }                
            }                
        }
        string bufferTextOut = "";
        private void Button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "")
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Text File .txt|*.txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = sfd.FileName;
                }
                else return;
            }

            checkBox1.Checked = false;

            bufferTextOut = "t (s)\tP (" + domainUpDown2.Text + ")\r\n";
            for (int i = 0; i < chart1.Series["Series1"].Points.Count; i++)
            {
                bufferTextOut += chart1.Series["Series1"].Points[i].XValue.ToString() + "\t" + chart1.Series["Series1"].Points[i].YValues[0].ToString() + "\r\n";
            }
            StreamWriter sw = new StreamWriter(textBox1.Text);
            sw.Write(bufferTextOut);
            sw.Close();
            button1.Text = "Saved";
        }

        private void TextBox1_DoubleClick(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File .txt|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = sfd.FileName;
            }
        }

        private void DomainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                int St = (int)(400.0 / (double)numericUpDown1.Value);
                string setSampling = "A" + St.ToString();
                serialPort1.WriteLine(setSampling);
            }
        }

        private void DomainUpDown2_SelectedItemChanged(object sender, EventArgs e)
        {
            label3.Text = domainUpDown2.Text;
            label6.Text = "(B) NNS amplitude (" + domainUpDown2.Text + ")*";
            label11.Text = "(D) NNS amplitude (" + domainUpDown2.Text + ")";
            label14.Text = "(E) Max. Amplitude (" + domainUpDown2.Text + ")*";
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series2"].Points.Clear();
            chart1.Series["Series3"].Points.Clear();

            checkBox1.Text = "Record (" + chart1.Series["Series1"].Points.Count + ")";
            time = -1;

            textBox4.Text = "";
            textBox5.Text = "";
            textBox11.Text = "";
            textBox10.Text = "";

            textBox13.Text = "";
            textBox12.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";

            textBox9.Text = "";
            textBox8.Text = "";
            textBox15.Text = "";
            textBox14.Text = "";
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File .txt|*.txt";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(ofd.FileName);
                string loadDatas = sr.ReadToEnd();
                string[] dataLines = loadDatas.Split(new char[] { '\n' });
                double cekDouble;
                chart1.Series["Series1"].Points.Clear();
                for (int i=0; i<dataLines.Length; i++)
                {
                    string[] eachLine = dataLines[i].Split(new char[] { '\t' });
                    if(eachLine.Length == 2)
                    {
                        if(double.TryParse(eachLine[0], out cekDouble) && double.TryParse(eachLine[1], out cekDouble))
                        {
                            chart1.Series["Series1"].Points.AddXY(double.Parse(eachLine[0]), double.Parse(eachLine[1]));
                        }
                    }
                }
            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked) checkBox1.Checked = false;
            if (chart1.Series["Series1"].Points.Count < 200)
            {
                MessageBox.Show("Not enough data!");
                return;
            }
            // Save to temporary variable
            // And looking for threshold
            int ns = chart1.Series["Series1"].Points.Count;
            double[] xi = new double[ns];
            double[] yi = new double[ns];
            double max = 0;
            double min = 0;
            for (int i = 0; i < ns; i++)
            {
                xi[i] = chart1.Series["Series1"].Points[i].XValue;
                yi[i] = chart1.Series["Series1"].Points[i].YValues[0];
                if (yi[i] > max) max = yi[i];
                if (yi[i] < min) min = yi[i];
            }

            double movAvg = 0;
            List<double> ypeaks = new List<double>();
            List<double> xpeaks = new List<double>();

            double th = 0.05 * (max - min);

            double localMax = 0;
            double xMax = 0;
            for (int i = 4; i < ns; i++)
            {
                movAvg = (yi[i - 4] + yi[i - 3] + yi[i - 2] + yi[i - 1] + yi[i])/5;
                if (yi[i] > th + movAvg)
                {
                    if (yi[i] > localMax)
                    {
                        localMax = yi[i];
                        xMax = xi[i];
                    }
                }
                else
                {
                    if(localMax != 0)
                    {
                        ypeaks.Add(localMax);
                        xpeaks.Add(xMax);
                    }
                    localMax = 0;
                    xMax = 0;
                }
            }

            // Identify the bursts
            List<List<double[]>> bursts = new List<List<double[]>>();
            
            List<double[]> sucks = new List<double[]>();
            double[] suck;
            
            for (int i = 1; i < xpeaks.Count; i++)
            {
                suck = new double[2];
                suck[0] = xpeaks[i - 1];
                suck[1] = ypeaks[i - 1];

                sucks.Add(suck);

                if (xpeaks[i] - xpeaks[i-1] > 1)
                {
                    if (sucks.Count > 3) bursts.Add(sucks);                    
                    sucks = new List<double[]>();
                }
            }

            suck = new double[2];
            suck[0] = xpeaks[xpeaks.Count - 1];
            suck[1] = ypeaks[xpeaks.Count - 1];
            sucks.Add(suck);

            if (sucks.Count > 3) bursts.Add(sucks);


            // Plot Bursts and Sucks Peak
            // Calculates Parameters
            
            chart1.Series["Series2"].Points.Clear();
            chart1.Series["Series3"].Points.Clear();

            //(A) NNS frequency (Hz)*
            double intraBurstTime1 = 0;
            double countIntraBurstTime1 = 0;
            double stdIntraBurstTime1 = 0;
            double seIntraBurstTime1 = 0;

            double intraBurstTime = 0;
            double countIntraBurstTime = 0;
            double stdIntraBurstTime = 0;
            double seIntraBurstTime = 0;


            double maxAmplitudeSuck = 0;

            double amplitudeSuck1 = 0;
            double countAmplitudeSuck1 = 0;
            double stdAmplitudeSuck1 = 0;
            double seAmplitudeSuck1 = 0;

            double amplitudeSuck = 0;
            double countAmplitudeSuck = 0;
            double stdAmplitudeSuck = 0;
            double seAmplitudeSuck = 0;

            double burstLength1 = 0;
            double burstLength = 0;
            double stdBurstLength = 0;
            double seBurstLength = 0;

            double pauseLength1 = 0;
            double pauseLength = 0;
            double stdPauseLength = 0;
            double sePauseLength = 0;

            double suckPerBurst1 = 0;
            double suckPerBurst = 0;
            double stdSuckPerBurst = 0;
            double seSuckPerBurst = 0;

            double prevX = 0;
            for (int i = 0; i < bursts.Count; i++)
            {
                List<double[]> sucksi = bursts[i];
                double maxInBurst = 0;

                burstLength += sucksi[sucksi.Count - 1][0] - sucksi[0][0];

                if (i == 1)
                {
                    pauseLength1 = sucksi[0][0] - prevX;
                }

                if (i > 0)
                {
                    pauseLength += sucksi[0][0] - prevX;
                }

                prevX = sucksi[sucksi.Count - 1][0];

                for (int j = 0; j < sucksi.Count; j++)
                {
                    chart1.Series["Series3"].Points.AddXY(sucksi[j][0], sucksi[j][1]);
                    if (sucksi[j][1] > maxInBurst) maxInBurst = sucksi[j][1];
                    if(j < sucksi.Count-1)
                    {
                        intraBurstTime += sucksi[j+1][0] - sucksi[j][0];
                        countIntraBurstTime++;
                        if (i == 0)
                        {
                            intraBurstTime1 += sucksi[j + 1][0] - sucksi[j][0];
                            countIntraBurstTime1++;
                        }
                    }

                    if (i == 0)
                    {
                        amplitudeSuck1 += sucksi[j][1];
                        countAmplitudeSuck1++;
                    }
                    amplitudeSuck += sucksi[j][1];
                    countAmplitudeSuck++;
                }

                if (i == 0)
                {
                    burstLength1 = sucksi[sucksi.Count - 1][0] - sucksi[0][0];
                    maxAmplitudeSuck = maxInBurst;
                }
                chart1.Series["Series2"].Points.AddXY(sucksi[0][0], 0);
                chart1.Series["Series2"].Points.AddXY(sucksi[0][0], maxInBurst);
                chart1.Series["Series2"].Points.AddXY(sucksi[sucksi.Count - 1][0], maxInBurst);
                chart1.Series["Series2"].Points.AddXY(sucksi[sucksi.Count - 1][0], 0);
            }

            suckPerBurst1 = countAmplitudeSuck1;
            suckPerBurst = countAmplitudeSuck / (double)bursts.Count;

            amplitudeSuck1 = amplitudeSuck1 / countAmplitudeSuck1;
            amplitudeSuck = amplitudeSuck / countAmplitudeSuck;
            intraBurstTime1 = intraBurstTime1 / countIntraBurstTime1;
            intraBurstTime = intraBurstTime / countIntraBurstTime;

            burstLength = burstLength / (double)bursts.Count + intraBurstTime;
            pauseLength = pauseLength / ((double)bursts.Count - 1) - intraBurstTime;

            double burstPerMinute = 60.0 / (burstLength + pauseLength);


            double intraFreqBurst1 = 1 / intraBurstTime1;
            double stdIntraFreqBurst1 = 0;
            double seIntraFreqBurst1 = 0;

            double intraFreqBurst = 1 / intraBurstTime;
            double stdIntraFreqBurst = 0;
            double seIntraFreqBurst = 0;


            for (int i = 0; i < bursts.Count; i++)
            {
                List<double[]> sucksi = bursts[i];

                stdBurstLength += Math.Pow(sucksi[sucksi.Count - 1][0] - sucksi[0][0] - burstLength, 2);

                if (i > 0)
                {
                    stdPauseLength += Math.Pow(sucksi[0][0] - prevX - pauseLength, 2);
                }

                stdSuckPerBurst += Math.Pow(sucksi.Count - suckPerBurst, 2);

                prevX = sucksi[sucksi.Count - 1][0];

                for (int j = 0; j < sucksi.Count; j++)
                {
                    if (j < sucksi.Count - 1)
                    {
                        stdIntraBurstTime += Math.Pow(sucksi[j + 1][0] - sucksi[j][0] - intraBurstTime, 2);
                        stdIntraFreqBurst += Math.Pow(1 / (sucksi[j + 1][0] - sucksi[j][0]) - intraFreqBurst, 2);

                        if (i == 0)
                        {
                            stdIntraBurstTime1 += Math.Pow(sucksi[j + 1][0] - sucksi[j][0] - intraBurstTime1, 2);
                            stdIntraFreqBurst1 += Math.Pow(1 / (sucksi[j + 1][0] - sucksi[j][0]) - intraFreqBurst1, 2);
                        }
                    }

                    if (i == 0)
                    {
                        stdAmplitudeSuck1 += Math.Pow(sucksi[j][1] - amplitudeSuck1, 2);
                    }
                    stdAmplitudeSuck += Math.Pow(sucksi[j][1] - amplitudeSuck, 2);
                }
            }

            stdIntraFreqBurst1 = Math.Sqrt(stdIntraFreqBurst1 / countIntraBurstTime1);
            seIntraFreqBurst1 = stdIntraFreqBurst1 / Math.Sqrt(countIntraBurstTime1);

            stdAmplitudeSuck1 = Math.Sqrt(stdAmplitudeSuck1 / countAmplitudeSuck1);
            seAmplitudeSuck1 = stdAmplitudeSuck1 / Math.Sqrt(countAmplitudeSuck1);


            stdIntraFreqBurst = Math.Sqrt(stdIntraFreqBurst / countIntraBurstTime);
            seIntraFreqBurst = stdIntraFreqBurst / Math.Sqrt(countIntraBurstTime);

            stdAmplitudeSuck = Math.Sqrt(stdAmplitudeSuck / countAmplitudeSuck);
            seAmplitudeSuck = stdAmplitudeSuck / Math.Sqrt(countAmplitudeSuck);

            stdBurstLength = Math.Sqrt(stdBurstLength / bursts.Count);
            seBurstLength = stdBurstLength / Math.Sqrt(bursts.Count);

            stdPauseLength = Math.Sqrt(stdPauseLength / ((double)bursts.Count - 1));
            sePauseLength = stdPauseLength / Math.Sqrt((double)bursts.Count - 1);

            stdSuckPerBurst = Math.Sqrt(stdSuckPerBurst / bursts.Count);
            seSuckPerBurst = stdSuckPerBurst / Math.Sqrt(bursts.Count);

            textBox4.Text = intraFreqBurst1.ToString("F3") + " (SD: " + stdIntraFreqBurst1.ToString("F3") + "; SE: " + seIntraFreqBurst1.ToString("F3") + ")";
            textBox5.Text = amplitudeSuck1.ToString("F3") + " (SD: " + stdAmplitudeSuck1.ToString("F3") + "; SE: " + seAmplitudeSuck1.ToString("F3") + ")";
            textBox11.Text = intraFreqBurst.ToString("F3") + " (SD: " + stdIntraFreqBurst.ToString("F3") + "; SE: " + seIntraFreqBurst.ToString("F3") + ")";
            textBox10.Text = amplitudeSuck.ToString("F3") + " (SD: " + stdAmplitudeSuck.ToString("F3") + "; SE: " + seAmplitudeSuck.ToString("F3") + ")";

            textBox13.Text = maxAmplitudeSuck.ToString("F3");
            textBox12.Text = suckPerBurst1.ToString("F3");
            textBox6.Text = burstPerMinute.ToString("F3");
            textBox7.Text = suckPerBurst.ToString("F3") + " (SD: " + stdSuckPerBurst.ToString("F3") + "; SE: " + seSuckPerBurst.ToString("F3") + ")";

            textBox9.Text = burstLength1.ToString("F3");
            textBox8.Text = pauseLength1.ToString("F3");
            textBox15.Text = burstLength.ToString("F3") + " (SD: " + stdBurstLength.ToString("F3") + "; SE: " + seBurstLength.ToString("F3") + ")";
            textBox14.Text = pauseLength.ToString("F3") + " (SD: " + stdPauseLength.ToString("F3") + "; SE: " + sePauseLength.ToString("F3") + ")";
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            textBox16.Text = "A\tB\tC\tD\tE\tF\tG\tH\tI\tJ\tK\tL\r\n";
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            textBox16.Text += textBox4.Text + "\t" + textBox5.Text + "\t" + textBox11.Text + "\t" + textBox10.Text + "\t" + textBox13.Text + "\t" + textBox12.Text + "\t" + textBox6.Text + "\t" + textBox7.Text + "\t" + textBox9.Text + "\t" + textBox8.Text + "\t" + textBox15.Text + "\t" + textBox14.Text + "\r\n";
        }
    }
}
