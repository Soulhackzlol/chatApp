namespace chatClient
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            button1 = new Button();
            IP_TextBox = new TextBox();
            Disconnect_Button = new Button();
            SendTestMsg_Button = new Button();
            chatTextBox = new TextBox();
            getMsgs = new System.Windows.Forms.Timer(components);
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(191, 12);
            button1.Name = "button1";
            button1.Size = new Size(99, 24);
            button1.TabIndex = 0;
            button1.Text = "Connect";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // IP_TextBox
            // 
            IP_TextBox.Location = new Point(12, 12);
            IP_TextBox.Name = "IP_TextBox";
            IP_TextBox.Size = new Size(173, 23);
            IP_TextBox.TabIndex = 1;
            // 
            // Disconnect_Button
            // 
            Disconnect_Button.Location = new Point(434, 312);
            Disconnect_Button.Name = "Disconnect_Button";
            Disconnect_Button.Size = new Size(99, 24);
            Disconnect_Button.TabIndex = 2;
            Disconnect_Button.Text = "Disconnect";
            Disconnect_Button.UseVisualStyleBackColor = true;
            Disconnect_Button.Visible = false;
            Disconnect_Button.Click += Disconnect_Button_Click;
            // 
            // SendTestMsg_Button
            // 
            SendTestMsg_Button.Location = new Point(12, 312);
            SendTestMsg_Button.Name = "SendTestMsg_Button";
            SendTestMsg_Button.Size = new Size(99, 24);
            SendTestMsg_Button.TabIndex = 3;
            SendTestMsg_Button.Text = "Send MSG";
            SendTestMsg_Button.UseVisualStyleBackColor = true;
            SendTestMsg_Button.Visible = false;
            SendTestMsg_Button.Click += SendTestMsg_Button_Click;
            // 
            // chatTextBox
            // 
            chatTextBox.Location = new Point(12, 56);
            chatTextBox.Multiline = true;
            chatTextBox.Name = "chatTextBox";
            chatTextBox.Size = new Size(521, 226);
            chatTextBox.TabIndex = 4;
            chatTextBox.TextChanged += chatTextBox_TextChanged;
            // 
            // getMsgs
            // 
            getMsgs.Interval = 10;
            getMsgs.Tick += getMsgs_Tick;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(117, 312);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(292, 23);
            textBox1.TabIndex = 5;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(545, 348);
            Controls.Add(textBox1);
            Controls.Add(chatTextBox);
            Controls.Add(SendTestMsg_Button);
            Controls.Add(Disconnect_Button);
            Controls.Add(IP_TextBox);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox IP_TextBox;
        private Button Disconnect_Button;
        private Button SendTestMsg_Button;
        private TextBox chatTextBox;
        private System.Windows.Forms.Timer getMsgs;
        private TextBox textBox1;
    }
}