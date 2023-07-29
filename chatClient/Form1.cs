using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private TcpClient client;
        bool connected = false;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                client.Connect(DecodeInvite(IP_TextBox.Text), 8000);
                Disconnect_Button.Visible = true;
                SendTestMsg_Button.Visible = true;
                button1.Enabled = false;
                IP_TextBox.Enabled = false;
                connected = true;
                getMsgs.Start();
                chatTextBox.Text = string.Empty;
                this.Invoke(new Action(() =>
                {
                    string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] You connected to {IP_TextBox.Text}!";
                    this.chatTextBox.SelectionStart = this.chatTextBox.TextLength;
                    this.chatTextBox.SelectionLength = 0;
                    this.chatTextBox.SelectionColor = Color.Green;
                    this.chatTextBox.AppendText(formattedMessage + Environment.NewLine);
                    this.chatTextBox.SelectionColor = this.chatTextBox.ForeColor;
                }));
            }
            catch
            {
                this.Invoke(new Action(() =>
                {
                    string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] Can't connect to {IP_TextBox.Text} :c. Make sure the code is correct!";
                    this.chatTextBox.SelectionStart = this.chatTextBox.TextLength;
                    this.chatTextBox.SelectionLength = 0;
                    this.chatTextBox.SelectionColor = Color.Red;
                    this.chatTextBox.AppendText(formattedMessage + Environment.NewLine);
                    this.chatTextBox.SelectionColor = this.chatTextBox.ForeColor;
                }));
            }
        }

        private void SendTestMsg_Button_Click(object sender, EventArgs e)
        {
            SendData(textBox1.Text);
            textBox1.Clear(); // Clear the input box after sending
        }

        private void Disconnect_Button_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] You disconnected from the chat!";
                this.chatTextBox.SelectionStart = this.chatTextBox.TextLength;
                this.chatTextBox.SelectionLength = 0;
                this.chatTextBox.SelectionColor = Color.Red;
                this.chatTextBox.AppendText(formattedMessage + Environment.NewLine);
                this.chatTextBox.SelectionColor = this.chatTextBox.ForeColor;
            }));
            client.Close();
            Disconnect_Button.Visible = false;
            SendTestMsg_Button.Visible = false;
            getMsgs.Stop();
            connected = false;
            button1.Enabled = true;
            IP_TextBox.Enabled = true;
        }

        private string EncodeIP(string ip)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ip));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 10);  // Take the first 10 characters
            }
        }

        private string DecodeInvite(string code)
        {
            byte[] decodedBytes = Convert.FromBase64String(code);
            return Encoding.UTF8.GetString(decodedBytes);
        }

        private Color DetermineColor(string encodedIP)
        {
            string rHex = encodedIP.Substring(0, 2);
            string gHex = encodedIP.Substring(2, 2);
            string bHex = encodedIP.Substring(4, 2);

            int r = int.Parse(rHex,System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(gHex,System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(bHex,System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(r, g, b);
        }

        private async Task HandleServerMessages(NetworkStream clientStream)
        {
            byte[] message = new byte[4096];
            try
            {
                int bytesRead = 0;
                if (connected)
                {
                    bytesRead = await clientStream.ReadAsync(message, 0, 4096);
                }
                if (bytesRead == 0)
                {
                    this.chatTextBox.AppendText("Server disconnected" + Environment.NewLine);
                    Disconnect_Button.PerformClick();
                }

                string data = Encoding.ASCII.GetString(message, 0, bytesRead);
                if (data == "heartbeat")
                {
                    SendData("HEARTBEAT-REPLY");
                }
                else
                {
                    Match match = Regex.Match(data, @"\[ MESSAGE \| (?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}) \]");
                    if (match.Success)
                    {
                        string ip = match.Groups["ip"].Value;
                        string encodedIP = EncodeIP(ip);
                        Color userColor = DetermineColor(encodedIP);

                        string userMessage = Regex.Replace(data, @"\[ MESSAGE \| \d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3} \] Message from client: ", "");

                        this.Invoke(new Action(() =>
                        {
                            string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {encodedIP}: {userMessage}";
                            this.chatTextBox.SelectionStart = this.chatTextBox.TextLength;
                            this.chatTextBox.SelectionLength = 0;
                            this.chatTextBox.SelectionColor = userColor;
                            this.chatTextBox.AppendText(formattedMessage + Environment.NewLine);
                            this.chatTextBox.SelectionColor = this.chatTextBox.ForeColor;
                        }));
                    }
                }
            }
            catch (IOException)
            {
                // Handle IOException
            }
            catch (Exception ex)
            {
                this.chatTextBox.AppendText("ERROR: " + ex + Environment.NewLine);
            }
        }

        private async void getMsgs_Tick(object sender, EventArgs e)
        {
            try
            {
                NetworkStream clientStream = client.GetStream();
                await Task.Run(() => HandleServerMessages(clientStream));
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        private async void SendData(string data)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] message = Encoding.ASCII.GetBytes(data);
                await stream.WriteAsync(message, 0, message.Length);
                if (data == "HEARTBEAT-REPLY")
                {
                    Console.WriteLine($"[ HEARTBEAT REPLY SENT | Server ]");
                }
                else
                {
                    this.chatTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] You: {data}" + Environment.NewLine);
                }
                stream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private void chatTextBox_TextChanged(object sender, EventArgs e)
        {
            // This method can be removed if not used
        }
    }
}
