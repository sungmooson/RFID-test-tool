using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;

namespace RFID_Read
{
    public partial class Form1 : Form
    {

        SerialPort serialPort = new SerialPort();

        private List<byte> recvBuffer = new List<byte>();

        byte[] data = new byte[] { 0x06, 0x00, 0x01, 0x04, 0xFF, 0xD4, 0x39 };

        byte[] addr = new byte[] { 0xaf, 0x75, 0x31, 0xE0 };

        double totalCount = 0;
        double sentCount = 0;

        double successCount = 0;

        int send_flag = 0;

        int sec_interval = 0;
        double sec_sendCount = 0;
        double sec_successCount = 0;

        int sec_successCount_flag = 0;

        double sec_suc = 0;

        string selectedFolderPath;

        int tag_ID = 0;

        int sec_sum_count = 0;
        int sec_success_sum_count = 0;

        int scan_flag = 0;

        int set_tag_flag = 0;

        uint combo_tag_ID = 0;

        public Form1()
        {
            InitializeComponent();    
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames(); 
            comboBox1.Items.AddRange(ports);

            if(comboBox1.Items.Count > 0 )
                comboBox1.SelectedIndex = 0;

            timer2.Interval = 1000;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("COM 포트를 선택하세요.");
                return;
            }

            string portName = comboBox1.SelectedItem.ToString(); // ComboBox에서 선택한 포트명

            try
            {
                serialPort.PortName = portName;
                serialPort.BaudRate = 57600;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Handshake = Handshake.None;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

                serialPort.Open();
                
                label2.Text = "연결됨";
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 열기 실패: " + ex.Message);
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesCount = serialPort.BytesToRead;
            byte[] buffer = new byte[bytesCount];

            

            serialPort.Read(buffer, 0, bytesCount);

            recvBuffer.AddRange(buffer);

            //Debug.WriteLine("수신된 패킷2: " + BitConverter.ToString(recvBuffer.ToArray()).Replace("-", " "));


            while (recvBuffer.Count >= 1)
            {
                int packetLength = recvBuffer[0] + 1;

                

                Debug.WriteLine("패킷 길이: " + packetLength);
                if (recvBuffer.Count < packetLength)
                {
                    break;
                }

                if (recvBuffer.Count >= packetLength)
                {
                    byte[] packet = recvBuffer.GetRange(0, packetLength).ToArray();
                    recvBuffer.RemoveRange(0, packetLength);

                    

                    if (packetLength >= 22)
                    {

                        uint value = (uint)BitConverter.ToInt32(packet, 15);

                        if(scan_flag == 1)
                        {
                            this.Invoke(new Action(() =>
                            {
                                if (!comboBox2.Items.Contains(value))
                                {
                                    comboBox2.Items.Add(value);
                                }
                            }));

                            scan_flag = 0;
                        }
                        
                        if(combo_tag_ID != 0)
                        {
                            if (value == combo_tag_ID)
                            {
                                if (send_flag == 1)
                                {
                                    sec_successCount++;
                                    successCount++;
                                    Console.WriteLine("송신횟수: " + sentCount + ",성공횟수: " + successCount);

                                    send_flag = 0;
                                }


                                string hex = BitConverter.ToString(packet).Replace("-", " ");

                                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

                                Debug.WriteLine("수신된 패킷3: " + hex);

                                double success_p = (successCount / sentCount) * 100;


                                this.Invoke(new Action(() =>
                                {
                                    if (sentCount != 0)
                                    {
                                        textBox1.Text = success_p.ToString("F2") + "% (" + sentCount + "회 중 " + successCount + "회 성공)";
                                    }

                                    richTextBox1.AppendText($"[{timestamp}] 수신: " + hex + Environment.NewLine);
                                }));
                            }
                        }

                       
                        
                    }
                    /*
                    string hex = BitConverter.ToString(packet).Replace("-", " ");

                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

                    Debug.WriteLine("수신된 패킷3: " + hex);

                    this.Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText($"[{timestamp}] 수신: " + hex + Environment.NewLine);
                    }));
                    */





                    }
                else
                {
                    break;
                }
            }

        }


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            serialPort.Write(data, 0, data.Length);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort.IsOpen && sentCount < totalCount)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

                if (sec_sendCount == sec_interval)
                {
                    sec_suc = ((double)sec_successCount / (double)sec_sendCount) * 100;

                    sec_sum_count++;

                    if(sec_suc > 0)
                    {
                        sec_success_sum_count++;

                        this.Invoke(new Action(() =>
                        {
                            richTextBox2.AppendText($"[{timestamp}] 수신: {"성공"}{Environment.NewLine}");
                        }));
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            richTextBox2.AppendText($"[{timestamp}] 수신: {"실패"}{Environment.NewLine}");
                        }));
                    }

                    //this.Invoke(new Action(() =>
                    //{
                    //    richTextBox2.AppendText($"[{timestamp}] 수신: {sec_suc:F2}%{Environment.NewLine}");
                    //}));

                    Console.WriteLine("초당 성공률:" + sec_suc);
                    sec_sendCount = 0;
                    sec_successCount = 0;
                }

                serialPort.Write(data, 0, data.Length);

                send_flag = 1;

                sec_sendCount++;

                string hex = BitConverter.ToString(data).Replace("-", " ");

                sentCount++;

                double remaining = totalCount - sentCount;

                this.Invoke(new Action(() =>
                {
                    richTextBox1.AppendText($"[{timestamp}] 송신: {hex}{Environment.NewLine}");

                    textBox3.Text = remaining.ToString();
                }));

                
            }

            if(sentCount >= totalCount)
            {
                timer1.Stop();
               
                timer2.Start();
                
            }
        }

       

        private void button4_Click(object sender, EventArgs e)
        {

            try
            {
                
                textBox5.Clear();
                textBox6.Clear();
                int interval = Convert.ToInt32(textBox2.Text.Trim());

                timer2.Interval = interval;

                sec_interval = 1000 / interval;

                totalCount = Convert.ToInt32(textBox3.Text.Trim());
                successCount = 0;
                sentCount = 0;
                sec_sum_count = 0;
                sec_success_sum_count = 0;

                if(combo_tag_ID == 0)
                {
                    MessageBox.Show("Tag 주소를 선택해주세요.");
                    return;
                }


                if(interval > 1 && totalCount >= 1)
                {
                    textBox2.Enabled = false;
                    textBox3.Enabled = false;


                    timer1.Interval = interval;
                    timer1.Start();
                }
                else
                {
                    MessageBox.Show("Count란에 1 이상의 값을 입력해주세요.");
                }
            }
            catch
            {
                MessageBox.Show("interval란에 올바른 숫자를 입력해주세요.");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer1.Stop();

            double success_p = (successCount / sentCount) * 100;

            double success_sec = (sec_success_sum_count / sec_sum_count) * 100;

            textBox6.Text =  success_p.ToString("F2") + "% (" + sentCount + "회 중 " + successCount + "회 성공)";

            textBox5.Text =  success_sec.ToString("F2") + "% (" + sec_sum_count + "회 중 " + sec_success_sum_count + "회 성공)";

            textBox2.Enabled = true;
            textBox3.Enabled = true;

            textBox3.Text = totalCount.ToString();

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            richTextBox1.AppendText($"[{timestamp}] 송신 중지됨 (남은 횟수: {totalCount - sentCount}){Environment.NewLine}");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(serialPort.IsOpen)
            {
                serialPort.Close();
                label2.Text = "연결끊김";
            }
            else
            {
                MessageBox.Show("이미 끊겨있습니다.");
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox1.ResetText();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            if (sec_sendCount == sec_interval)
            {
                sec_sum_count++;

                sec_suc = ((double)sec_successCount / (double)sec_sendCount) * 100;
                if(sec_suc > 0)
                {
                    sec_success_sum_count++;
                    this.Invoke(new Action(() =>
                    {
                        richTextBox2.AppendText($"[{timestamp}] 마지막 수신: {"성공"}{Environment.NewLine}");
                    }));
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        richTextBox2.AppendText($"[{timestamp}] 마지막 수신: {"실패"}{Environment.NewLine}");
                    }));
                }
               
                Console.WriteLine("초당 성공률:" + sec_suc);
                sec_sendCount = 0;
                sec_successCount = 0;
            }

            textBox2.Enabled = true;
            textBox3.Enabled = true;

            //MessageBox.Show("송신 완료");

            double success_p = (successCount /  sentCount ) * 100;
            double success_sec = (sec_success_sum_count / sec_sum_count) * 100;

            textBox6.Text = success_p.ToString("F2") + "% (" +  sentCount + "회 중 " + successCount + "회 성공)";
            textBox5.Text =  success_sec.ToString("F2") + "% (" + sec_sum_count + "회 중 " + sec_success_sum_count + "회 성공)";

            successCount = 0;
            sentCount = 0;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            richTextBox1.ResetText();
            richTextBox2.ResetText();
            textBox1.Clear();
            textBox5.Clear();
            textBox6.Clear();
        }

       

        private void button9_Click(object sender, EventArgs e)
        {
            SelectFolderAndDisplay();
        }

        private void button10_Click(object sender, EventArgs e)
        {

            string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");

            string filepath = Path.Combine(selectedFolderPath, timestamp + ".txt");

            string allText = "";

            allText += richTextBox1.Text + Environment.NewLine;
            allText += "실시간누적성공률: " + textBox1.Text + Environment.NewLine;
            allText += "전체누적성공률: " + textBox6.Text + Environment.NewLine;
            allText += "초당누적성공률: " + textBox5.Text + Environment.NewLine;
            try
            {


                File.WriteAllText(filepath, allText);
                textBox1.Clear();
                textBox5.Clear();
                textBox6.Clear();
                richTextBox1.Clear();
                richTextBox2.Clear();
                MessageBox.Show("파일이 성공적으로 저장되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 중 오류 발생: " + ex.Message);
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void SelectFolderAndDisplay()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true; // 폴더 선택 모드로 설정

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedFolderPath = dialog.FileName;
                textBox4.Text = selectedFolderPath; // 선택한 폴더 경로를 텍스트박스에 표시
            }
        }

        private void label4_Click_1(object sender, EventArgs e)
        {

        }

        private void label7_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {


        }
        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            scan_flag = 1;
            comboBox2.Items.Clear();
            serialPort.Write(data, 0, data.Length);
            combo_tag_ID = 0;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            combo_tag_ID = (uint)comboBox2.SelectedItem;

            label11.Text = $"Tag 주소 저장됨- ID: {combo_tag_ID}";

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }
    }
}
