using System.Net.Sockets;
using System.Text;

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
                // Connect to server
                client = new TcpClient();
                client.Connect(IP_TextBox.Text, 8000);

                // Enable Disconnect and SendTestMsg buttons
                Disconnect_Button.Visible = true;
                SendTestMsg_Button.Visible = true;

                // Disable Connect button and IP address text box
                button1.Enabled = false;
                IP_TextBox.Enabled = false;
                connected = true;
                getMsgs.Start();
                chatTextBox.Text = string.Empty;

                // Display message box indicating successful connection
                MessageBox.Show("Connection successful");
            }
            catch
            {
                // Display message box indicating connection failed
                MessageBox.Show("Connection failed");
            }
        }

        private void SendTestMsg_Button_Click(object sender, EventArgs e)
        {
            // Send test message to server
            SendData(textBox1.Text);
        }

        private void Disconnect_Button_Click(object sender, EventArgs e)
        {
            // Disconnect from server
            client.Close();

            // Disable Disconnect and SendTestMsg buttons
            Disconnect_Button.Visible = false;
            SendTestMsg_Button.Visible = false;
            getMsgs.Stop();
            connected = false;
            // Enable Connect button and IP address text box
            button1.Enabled = true;
            IP_TextBox.Enabled = true;
        }

        private async Task HandleServerMessages(NetworkStream clientStream)
        {
            byte[] message = new byte[4096];
            try
            {
                int bytesRead = 0;
                // Read incoming message from server
                if (connected)
                {
                    bytesRead = await clientStream.ReadAsync(message, 0, 4096);
                }
                if (bytesRead == 0)
                {
                    // Server disconnected
                    this.chatTextBox.AppendText("Server disconnected" + Environment.NewLine);
                    Disconnect_Button.PerformClick();
                }

                // Convert incoming message to string
                string data = Encoding.ASCII.GetString(message, 0, bytesRead);

                if (data == "heartbeat")
                {
                        // Send heartbeat reply to server
                        SendData("HEARTBEAT-REPLY");
                }
                else
                {
                    // Handle other messages from server
                    this.Invoke(new Action(() =>
                    {
                        // Update UI with message from server
                        this.chatTextBox.AppendText("Server: " + data + Environment.NewLine);
                    }));
                }
            }
            catch (IOException)
            {

            }
            catch (Exception ex)
            {
                // Handle exception
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
                // Handle exception

            }
        }

        private async void SendData(string data)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] message = Encoding.ASCII.GetBytes(data);
                await stream.WriteAsync(message, 0, message.Length);
                this.chatTextBox.AppendText("Sent: "+ data);
                stream.Flush(); // Flush the stream to ensure all data is sent immediately
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void chatTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}