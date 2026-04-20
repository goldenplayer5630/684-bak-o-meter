namespace _685BakOMeter.Sim
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
            grpConnection = new GroupBox();
            lblVirtualPortHint = new Label();
            btnTestPort = new Button();
            btnRefreshPorts = new Button();
            cmbComPorts = new ComboBox();
            lblDetectedPorts = new Label();
            lblConnectionStatus = new Label();
            btnDisconnect = new Button();
            btnConnect = new Button();
            txtBaudRate = new TextBox();
            lblBaudRate = new Label();
            txtComPort = new TextBox();
            lblComPort = new Label();
            grpNfcTags = new GroupBox();
            btnNfc10 = new Button();
            btnNfc9 = new Button();
            btnNfc8 = new Button();
            btnNfc7 = new Button();
            btnNfc6 = new Button();
            btnNfc5 = new Button();
            btnNfc4 = new Button();
            btnNfc3 = new Button();
            btnNfc2 = new Button();
            btnNfc1 = new Button();
            grpWeightPresets = new GroupBox();
            chkContinuous = new CheckBox();
            btnFullPul = new Button();
            btnEmptyPul = new Button();
            btnFullGlass = new Button();
            btnEmptyGlass = new Button();
            btnNothing = new Button();
            grpManualSend = new GroupBox();
            btnSendCustomNfc = new Button();
            txtCustomNfc = new TextBox();
            lblCustomNfc = new Label();
            btnSendCustomWeight = new Button();
            numCustomWeight = new NumericUpDown();
            lblCustomWeight = new Label();
            grpChugSimulation = new GroupBox();
            btnStopSimulation = new Button();
            btnSimulatePulChug = new Button();
            btnSimulateGlassChug = new Button();
            grpLog = new GroupBox();
            btnClearLog = new Button();
            txtLog = new TextBox();
            grpConnection.SuspendLayout();
            grpNfcTags.SuspendLayout();
            grpWeightPresets.SuspendLayout();
            grpManualSend.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCustomWeight).BeginInit();
            grpChugSimulation.SuspendLayout();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpConnection
            // 
            grpConnection.Controls.Add(lblVirtualPortHint);
            grpConnection.Controls.Add(btnTestPort);
            grpConnection.Controls.Add(btnRefreshPorts);
            grpConnection.Controls.Add(cmbComPorts);
            grpConnection.Controls.Add(lblDetectedPorts);
            grpConnection.Controls.Add(lblConnectionStatus);
            grpConnection.Controls.Add(btnDisconnect);
            grpConnection.Controls.Add(btnConnect);
            grpConnection.Controls.Add(txtBaudRate);
            grpConnection.Controls.Add(lblBaudRate);
            grpConnection.Controls.Add(txtComPort);
            grpConnection.Controls.Add(lblComPort);
            grpConnection.Location = new Point(12, 12);
            grpConnection.Name = "grpConnection";
            grpConnection.Size = new Size(350, 250);
            grpConnection.TabIndex = 0;
            grpConnection.TabStop = false;
            grpConnection.Text = "Serial Connection";
            // 
            // lblVirtualPortHint
            // 
            lblVirtualPortHint.ForeColor = SystemColors.GrayText;
            lblVirtualPortHint.Location = new Point(15, 202);
            lblVirtualPortHint.Name = "lblVirtualPortHint";
            lblVirtualPortHint.Size = new Size(320, 18);
            lblVirtualPortHint.TabIndex = 11;
            lblVirtualPortHint.Text = "Create virtual pairs externally (e.g. com0com).";
            // 
            // btnTestPort
            // 
            btnTestPort.Location = new Point(195, 169);
            btnTestPort.Name = "btnTestPort";
            btnTestPort.Size = new Size(140, 30);
            btnTestPort.TabIndex = 10;
            btnTestPort.Text = "Test Open/Close";
            btnTestPort.UseVisualStyleBackColor = true;
            // 
            // btnRefreshPorts
            // 
            btnRefreshPorts.Location = new Point(195, 45);
            btnRefreshPorts.Name = "btnRefreshPorts";
            btnRefreshPorts.Size = new Size(140, 27);
            btnRefreshPorts.TabIndex = 2;
            btnRefreshPorts.Text = "Refresh Ports";
            btnRefreshPorts.UseVisualStyleBackColor = true;
            // 
            // cmbComPorts
            // 
            cmbComPorts.FormattingEnabled = true;
            cmbComPorts.Location = new Point(15, 45);
            cmbComPorts.Name = "cmbComPorts";
            cmbComPorts.Size = new Size(170, 28);
            cmbComPorts.TabIndex = 1;
            // 
            // lblDetectedPorts
            // 
            lblDetectedPorts.AutoSize = true;
            lblDetectedPorts.Location = new Point(15, 22);
            lblDetectedPorts.Name = "lblDetectedPorts";
            lblDetectedPorts.Size = new Size(149, 20);
            lblDetectedPorts.TabIndex = 0;
            lblDetectedPorts.Text = "Detected ports: none";
            // 
            // lblConnectionStatus
            // 
            lblConnectionStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblConnectionStatus.ForeColor = Color.Red;
            lblConnectionStatus.Location = new Point(15, 221);
            lblConnectionStatus.Name = "lblConnectionStatus";
            lblConnectionStatus.Size = new Size(320, 23);
            lblConnectionStatus.TabIndex = 12;
            lblConnectionStatus.Text = "Disconnected";
            lblConnectionStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Enabled = false;
            btnDisconnect.Location = new Point(15, 136);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(170, 30);
            btnDisconnect.TabIndex = 9;
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(195, 136);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(140, 30);
            btnConnect.TabIndex = 8;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // txtBaudRate
            // 
            txtBaudRate.Location = new Point(195, 103);
            txtBaudRate.Name = "txtBaudRate";
            txtBaudRate.Size = new Size(140, 27);
            txtBaudRate.TabIndex = 7;
            // 
            // lblBaudRate
            // 
            lblBaudRate.AutoSize = true;
            lblBaudRate.Location = new Point(195, 80);
            lblBaudRate.Name = "lblBaudRate";
            lblBaudRate.Size = new Size(80, 20);
            lblBaudRate.TabIndex = 6;
            lblBaudRate.Text = "Baud Rate:";
            // 
            // txtComPort
            // 
            txtComPort.Location = new Point(15, 103);
            txtComPort.Name = "txtComPort";
            txtComPort.PlaceholderText = "Optional override, e.g. COM3";
            txtComPort.Size = new Size(170, 27);
            txtComPort.TabIndex = 5;
            // 
            // lblComPort
            // 
            lblComPort.AutoSize = true;
            lblComPort.Location = new Point(15, 80);
            lblComPort.Name = "lblComPort";
            lblComPort.Size = new Size(112, 20);
            lblComPort.TabIndex = 4;
            lblComPort.Text = "Manual override";
            // 
            // grpNfcTags
            // 
            grpNfcTags.Controls.Add(btnNfc10);
            grpNfcTags.Controls.Add(btnNfc9);
            grpNfcTags.Controls.Add(btnNfc8);
            grpNfcTags.Controls.Add(btnNfc7);
            grpNfcTags.Controls.Add(btnNfc6);
            grpNfcTags.Controls.Add(btnNfc5);
            grpNfcTags.Controls.Add(btnNfc4);
            grpNfcTags.Controls.Add(btnNfc3);
            grpNfcTags.Controls.Add(btnNfc2);
            grpNfcTags.Controls.Add(btnNfc1);
            grpNfcTags.Location = new Point(12, 268);
            grpNfcTags.Name = "grpNfcTags";
            grpNfcTags.Size = new Size(350, 150);
            grpNfcTags.TabIndex = 1;
            grpNfcTags.TabStop = false;
            grpNfcTags.Text = "Simulated NFC Tags";
            // 
            // btnNfc10
            // 
            btnNfc10.Enabled = false;
            btnNfc10.Location = new Point(290, 105);
            btnNfc10.Name = "btnNfc10";
            btnNfc10.Size = new Size(60, 30);
            btnNfc10.TabIndex = 9;
            btnNfc10.Text = "NFC 10";
            btnNfc10.UseVisualStyleBackColor = true;
            // 
            // btnNfc9
            // 
            btnNfc9.Enabled = false;
            btnNfc9.Location = new Point(225, 105);
            btnNfc9.Name = "btnNfc9";
            btnNfc9.Size = new Size(60, 30);
            btnNfc9.TabIndex = 8;
            btnNfc9.Text = "NFC 9";
            btnNfc9.UseVisualStyleBackColor = true;
            // 
            // btnNfc8
            // 
            btnNfc8.Enabled = false;
            btnNfc8.Location = new Point(160, 105);
            btnNfc8.Name = "btnNfc8";
            btnNfc8.Size = new Size(60, 30);
            btnNfc8.TabIndex = 7;
            btnNfc8.Text = "NFC 8";
            btnNfc8.UseVisualStyleBackColor = true;
            // 
            // btnNfc7
            // 
            btnNfc7.Enabled = false;
            btnNfc7.Location = new Point(95, 105);
            btnNfc7.Name = "btnNfc7";
            btnNfc7.Size = new Size(60, 30);
            btnNfc7.TabIndex = 6;
            btnNfc7.Text = "NFC 7";
            btnNfc7.UseVisualStyleBackColor = true;
            // 
            // btnNfc6
            // 
            btnNfc6.Enabled = false;
            btnNfc6.Location = new Point(30, 105);
            btnNfc6.Name = "btnNfc6";
            btnNfc6.Size = new Size(60, 30);
            btnNfc6.TabIndex = 5;
            btnNfc6.Text = "NFC 6";
            btnNfc6.UseVisualStyleBackColor = true;
            // 
            // btnNfc5
            // 
            btnNfc5.Enabled = false;
            btnNfc5.Location = new Point(290, 65);
            btnNfc5.Name = "btnNfc5";
            btnNfc5.Size = new Size(60, 30);
            btnNfc5.TabIndex = 4;
            btnNfc5.Text = "NFC 5";
            btnNfc5.UseVisualStyleBackColor = true;
            // 
            // btnNfc4
            // 
            btnNfc4.Enabled = false;
            btnNfc4.Location = new Point(225, 65);
            btnNfc4.Name = "btnNfc4";
            btnNfc4.Size = new Size(60, 30);
            btnNfc4.TabIndex = 3;
            btnNfc4.Text = "NFC 4";
            btnNfc4.UseVisualStyleBackColor = true;
            // 
            // btnNfc3
            // 
            btnNfc3.Enabled = false;
            btnNfc3.Location = new Point(160, 65);
            btnNfc3.Name = "btnNfc3";
            btnNfc3.Size = new Size(60, 30);
            btnNfc3.TabIndex = 2;
            btnNfc3.Text = "NFC 3";
            btnNfc3.UseVisualStyleBackColor = true;
            // 
            // btnNfc2
            // 
            btnNfc2.Enabled = false;
            btnNfc2.Location = new Point(95, 65);
            btnNfc2.Name = "btnNfc2";
            btnNfc2.Size = new Size(60, 30);
            btnNfc2.TabIndex = 1;
            btnNfc2.Text = "NFC 2";
            btnNfc2.UseVisualStyleBackColor = true;
            // 
            // btnNfc1
            // 
            btnNfc1.Enabled = false;
            btnNfc1.Location = new Point(30, 65);
            btnNfc1.Name = "btnNfc1";
            btnNfc1.Size = new Size(60, 30);
            btnNfc1.TabIndex = 0;
            btnNfc1.Text = "NFC 1";
            btnNfc1.UseVisualStyleBackColor = true;
            // 
            // grpWeightPresets
            // 
            grpWeightPresets.Controls.Add(chkContinuous);
            grpWeightPresets.Controls.Add(btnFullPul);
            grpWeightPresets.Controls.Add(btnEmptyPul);
            grpWeightPresets.Controls.Add(btnFullGlass);
            grpWeightPresets.Controls.Add(btnEmptyGlass);
            grpWeightPresets.Controls.Add(btnNothing);
            grpWeightPresets.Location = new Point(12, 424);
            grpWeightPresets.Name = "grpWeightPresets";
            grpWeightPresets.Size = new Size(350, 190);
            grpWeightPresets.TabIndex = 2;
            grpWeightPresets.TabStop = false;
            grpWeightPresets.Text = "Weight Presets";
            // 
            // chkContinuous
            // 
            chkContinuous.AutoSize = true;
            chkContinuous.Enabled = false;
            chkContinuous.Location = new Point(15, 155);
            chkContinuous.Name = "chkContinuous";
            chkContinuous.Size = new Size(166, 24);
            chkContinuous.TabIndex = 5;
            chkContinuous.Text = "Send Continuously";
            chkContinuous.UseVisualStyleBackColor = true;
            // 
            // btnFullPul
            // 
            btnFullPul.Enabled = false;
            btnFullPul.Location = new Point(180, 110);
            btnFullPul.Name = "btnFullPul";
            btnFullPul.Size = new Size(155, 35);
            btnFullPul.TabIndex = 4;
            btnFullPul.Text = "Full Pul";
            btnFullPul.UseVisualStyleBackColor = true;
            // 
            // btnEmptyPul
            // 
            btnEmptyPul.Enabled = false;
            btnEmptyPul.Location = new Point(15, 110);
            btnEmptyPul.Name = "btnEmptyPul";
            btnEmptyPul.Size = new Size(155, 35);
            btnEmptyPul.TabIndex = 3;
            btnEmptyPul.Text = "Empty Pul";
            btnEmptyPul.UseVisualStyleBackColor = true;
            // 
            // btnFullGlass
            // 
            btnFullGlass.Enabled = false;
            btnFullGlass.Location = new Point(180, 68);
            btnFullGlass.Name = "btnFullGlass";
            btnFullGlass.Size = new Size(155, 35);
            btnFullGlass.TabIndex = 2;
            btnFullGlass.Text = "Full Glass";
            btnFullGlass.UseVisualStyleBackColor = true;
            // 
            // btnEmptyGlass
            // 
            btnEmptyGlass.Enabled = false;
            btnEmptyGlass.Location = new Point(15, 68);
            btnEmptyGlass.Name = "btnEmptyGlass";
            btnEmptyGlass.Size = new Size(155, 35);
            btnEmptyGlass.TabIndex = 1;
            btnEmptyGlass.Text = "Empty Glass";
            btnEmptyGlass.UseVisualStyleBackColor = true;
            // 
            // btnNothing
            // 
            btnNothing.Enabled = false;
            btnNothing.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNothing.Location = new Point(15, 26);
            btnNothing.Name = "btnNothing";
            btnNothing.Size = new Size(320, 35);
            btnNothing.TabIndex = 0;
            btnNothing.Text = "Nothing";
            btnNothing.UseVisualStyleBackColor = true;
            // 
            // grpManualSend
            // 
            grpManualSend.Controls.Add(btnSendCustomNfc);
            grpManualSend.Controls.Add(txtCustomNfc);
            grpManualSend.Controls.Add(lblCustomNfc);
            grpManualSend.Controls.Add(btnSendCustomWeight);
            grpManualSend.Controls.Add(numCustomWeight);
            grpManualSend.Controls.Add(lblCustomWeight);
            grpManualSend.Location = new Point(12, 620);
            grpManualSend.Name = "grpManualSend";
            grpManualSend.Size = new Size(350, 150);
            grpManualSend.TabIndex = 3;
            grpManualSend.TabStop = false;
            grpManualSend.Text = "Manual Send";
            // 
            // btnSendCustomNfc
            // 
            btnSendCustomNfc.Enabled = false;
            btnSendCustomNfc.Location = new Point(225, 105);
            btnSendCustomNfc.Name = "btnSendCustomNfc";
            btnSendCustomNfc.Size = new Size(110, 30);
            btnSendCustomNfc.TabIndex = 5;
            btnSendCustomNfc.Text = "Send";
            btnSendCustomNfc.UseVisualStyleBackColor = true;
            // 
            // txtCustomNfc
            // 
            txtCustomNfc.Enabled = false;
            txtCustomNfc.Location = new Point(15, 107);
            txtCustomNfc.Name = "txtCustomNfc";
            txtCustomNfc.PlaceholderText = "e.g., 04A1B2C3D4E5F0";
            txtCustomNfc.Size = new Size(200, 27);
            txtCustomNfc.TabIndex = 4;
            // 
            // lblCustomNfc
            // 
            lblCustomNfc.AutoSize = true;
            lblCustomNfc.Location = new Point(15, 84);
            lblCustomNfc.Name = "lblCustomNfc";
            lblCustomNfc.Size = new Size(116, 20);
            lblCustomNfc.TabIndex = 3;
            lblCustomNfc.Text = "Custom NFC tag:";
            // 
            // btnSendCustomWeight
            // 
            btnSendCustomWeight.Enabled = false;
            btnSendCustomWeight.Location = new Point(225, 45);
            btnSendCustomWeight.Name = "btnSendCustomWeight";
            btnSendCustomWeight.Size = new Size(110, 30);
            btnSendCustomWeight.TabIndex = 2;
            btnSendCustomWeight.Text = "Send";
            btnSendCustomWeight.UseVisualStyleBackColor = true;
            // 
            // numCustomWeight
            // 
            numCustomWeight.Enabled = false;
            numCustomWeight.Location = new Point(15, 47);
            numCustomWeight.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numCustomWeight.Name = "numCustomWeight";
            numCustomWeight.Size = new Size(200, 27);
            numCustomWeight.TabIndex = 1;
            // 
            // lblCustomWeight
            // 
            lblCustomWeight.AutoSize = true;
            lblCustomWeight.Location = new Point(15, 24);
            lblCustomWeight.Name = "lblCustomWeight";
            lblCustomWeight.Size = new Size(116, 20);
            lblCustomWeight.TabIndex = 0;
            lblCustomWeight.Text = "Custom weight:";
            // 
            // grpChugSimulation
            // 
            grpChugSimulation.Controls.Add(btnStopSimulation);
            grpChugSimulation.Controls.Add(btnSimulatePulChug);
            grpChugSimulation.Controls.Add(btnSimulateGlassChug);
            grpChugSimulation.Location = new Point(12, 776);
            grpChugSimulation.Name = "grpChugSimulation";
            grpChugSimulation.Size = new Size(350, 100);
            grpChugSimulation.TabIndex = 4;
            grpChugSimulation.TabStop = false;
            grpChugSimulation.Text = "Chug Simulation";
            // 
            // btnStopSimulation
            // 
            btnStopSimulation.BackColor = Color.IndianRed;
            btnStopSimulation.Enabled = false;
            btnStopSimulation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStopSimulation.ForeColor = Color.White;
            btnStopSimulation.Location = new Point(97, 60);
            btnStopSimulation.Name = "btnStopSimulation";
            btnStopSimulation.Size = new Size(155, 30);
            btnStopSimulation.TabIndex = 2;
            btnStopSimulation.Text = "⏹ Stop";
            btnStopSimulation.UseVisualStyleBackColor = false;
            // 
            // btnSimulatePulChug
            // 
            btnSimulatePulChug.Enabled = false;
            btnSimulatePulChug.Location = new Point(180, 26);
            btnSimulatePulChug.Name = "btnSimulatePulChug";
            btnSimulatePulChug.Size = new Size(155, 30);
            btnSimulatePulChug.TabIndex = 1;
            btnSimulatePulChug.Text = "🍺 Pul Chug";
            btnSimulatePulChug.UseVisualStyleBackColor = true;
            // 
            // btnSimulateGlassChug
            // 
            btnSimulateGlassChug.Enabled = false;
            btnSimulateGlassChug.Location = new Point(15, 26);
            btnSimulateGlassChug.Name = "btnSimulateGlassChug";
            btnSimulateGlassChug.Size = new Size(155, 30);
            btnSimulateGlassChug.TabIndex = 0;
            btnSimulateGlassChug.Text = "🍺 Glass Chug";
            btnSimulateGlassChug.UseVisualStyleBackColor = true;
            // 
            // grpLog
            // 
            grpLog.Controls.Add(btnClearLog);
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new Point(368, 12);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(620, 876);
            grpLog.TabIndex = 5;
            grpLog.TabStop = false;
            grpLog.Text = "Message Log";
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(515, 22);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(90, 30);
            btnClearLog.TabIndex = 1;
            btnClearLog.Text = "Clear";
            btnClearLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            txtLog.BackColor = Color.Black;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.ForeColor = Color.Lime;
            txtLog.Location = new Point(15, 58);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(590, 802);
            txtLog.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 900);
            Controls.Add(grpLog);
            Controls.Add(grpChugSimulation);
            Controls.Add(grpManualSend);
            Controls.Add(grpWeightPresets);
            Controls.Add(grpNfcTags);
            Controls.Add(grpConnection);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bak-O-Meter Hardware Simulator";
            grpConnection.ResumeLayout(false);
            grpConnection.PerformLayout();
            grpNfcTags.ResumeLayout(false);
            grpWeightPresets.ResumeLayout(false);
            grpWeightPresets.PerformLayout();
            grpManualSend.ResumeLayout(false);
            grpManualSend.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCustomWeight).EndInit();
            grpChugSimulation.ResumeLayout(false);
            grpLog.ResumeLayout(false);
            grpLog.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpConnection;
        private Label lblComPort;
        private TextBox txtComPort;
        private TextBox txtBaudRate;
        private Label lblBaudRate;
        private Button btnConnect;
        private Button btnDisconnect;
        private Label lblConnectionStatus;
        private Label lblDetectedPorts;
        private ComboBox cmbComPorts;
        private Button btnRefreshPorts;
        private Button btnTestPort;
        private Label lblVirtualPortHint;
        private GroupBox grpNfcTags;
        private Button btnNfc1;
        private Button btnNfc2;
        private Button btnNfc3;
        private Button btnNfc4;
        private Button btnNfc5;
        private Button btnNfc6;
        private Button btnNfc7;
        private Button btnNfc8;
        private Button btnNfc9;
        private Button btnNfc10;
        private GroupBox grpWeightPresets;
        private Button btnNothing;
        private Button btnEmptyGlass;
        private Button btnFullGlass;
        private Button btnEmptyPul;
        private Button btnFullPul;
        private CheckBox chkContinuous;
        private GroupBox grpManualSend;
        private Label lblCustomWeight;
        private NumericUpDown numCustomWeight;
        private Button btnSendCustomWeight;
        private Label lblCustomNfc;
        private TextBox txtCustomNfc;
        private Button btnSendCustomNfc;
        private GroupBox grpChugSimulation;
        private Button btnSimulateGlassChug;
        private Button btnSimulatePulChug;
        private Button btnStopSimulation;
        private GroupBox grpLog;
        private TextBox txtLog;
        private Button btnClearLog;
    }
}

