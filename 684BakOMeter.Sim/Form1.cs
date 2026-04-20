using System.IO;
using System.IO.Ports;

namespace _685BakOMeter.Sim
{
    /// <summary>
    /// Main form for the Bak-O-Meter hardware simulator.
    /// Allows manual testing of the main application by sending simulated serial messages.
    /// </summary>
    public partial class Form1 : Form
    {
        private SerialPort? _serialPort;
        private readonly Random _random = new();
        private ChugSimulator? _chugSimulator;
        private System.Windows.Forms.Timer? _continuousTimer;
        private bool _isConnecting;

        // Tracks the active preset for continuous mode: (baseValue, margin, label)
        private (int BaseValue, int Margin, string Label) _activePreset = (SimulatorConfig.Nothing, SimulatorConfig.NothingMargin, "Nothing");

        public Form1()
        {
            InitializeComponent();
            InitializeForm();
        }

        /// <summary>
        /// Initializes form defaults and event handlers.
        /// </summary>
        private void InitializeForm()
        {
            // Set default values
            txtComPort.Text = SimulatorConfig.DefaultComPort;
            txtBaudRate.Text = SimulatorConfig.DefaultBaudRate.ToString();
            cmbComPorts.DropDownStyle = ComboBoxStyle.DropDownList;

            // Wire up connection events
            btnConnect.Click += BtnConnect_Click;
            btnDisconnect.Click += BtnDisconnect_Click;
            btnRefreshPorts.Click += (s, e) => RefreshAvailablePorts();
            btnTestPort.Click += BtnTestPort_Click;
            cmbComPorts.SelectedIndexChanged += (s, e) =>
            {
                if (cmbComPorts.SelectedItem is string selectedPort)
                {
                    txtComPort.Text = selectedPort;
                }
            };

            // Wire up NFC tag events
            btnNfc1.Click += (s, e) => SendNfcTag(0);
            btnNfc2.Click += (s, e) => SendNfcTag(1);
            btnNfc3.Click += (s, e) => SendNfcTag(2);
            btnNfc4.Click += (s, e) => SendNfcTag(3);
            btnNfc5.Click += (s, e) => SendNfcTag(4);
            btnNfc6.Click += (s, e) => SendNfcTag(5);
            btnNfc7.Click += (s, e) => SendNfcTag(6);
            btnNfc8.Click += (s, e) => SendNfcTag(7);
            btnNfc9.Click += (s, e) => SendNfcTag(8);
            btnNfc10.Click += (s, e) => SendNfcTag(9);

            // Wire up weight preset events
            btnNothing.Click += (s, e) => SendWeightPreset(SimulatorConfig.Nothing, SimulatorConfig.NothingMargin, "Nothing");
            btnEmptyGlass.Click += (s, e) => SendWeightPreset(SimulatorConfig.EmptyGlass, SimulatorConfig.EmptyGlassMargin, "Empty Glass");
            btnFullGlass.Click += (s, e) => SendWeightPreset(SimulatorConfig.FullGlass, SimulatorConfig.FullGlassMargin, "Full Glass");
            btnEmptyPul.Click += (s, e) => SendWeightPreset(SimulatorConfig.EmptyPul, SimulatorConfig.EmptyPulMargin, "Empty Pul");
            btnFullPul.Click += (s, e) => SendWeightPreset(SimulatorConfig.FullPul, SimulatorConfig.FullPulMargin, "Full Pul");

            // Wire up continuous mode
            chkContinuous.CheckedChanged += ChkContinuous_CheckedChanged;

            // Wire up manual send events
            btnSendCustomWeight.Click += BtnSendCustomWeight_Click;
            btnSendCustomNfc.Click += BtnSendCustomNfc_Click;

            // Wire up chug simulation events
            btnSimulateGlassChug.Click += BtnSimulateGlassChug_Click;
            btnSimulatePulChug.Click += BtnSimulatePulChug_Click;
            btnStopSimulation.Click += BtnStopSimulation_Click;

            // Wire up log events
            btnClearLog.Click += (s, e) => txtLog.Clear();

            // Setup form closing
            FormClosing += Form1_FormClosing;

            RefreshAvailablePorts();
            LogMessage("ℹ️ This simulator writes to an existing Windows COM port. It does not create virtual ports. Use a tool like com0com to create a pair first.", Color.LightBlue);
        }

        #region Connection Management

        private void BtnConnect_Click(object? sender, EventArgs e)
        {
            if (_isConnecting)
            {
                LogMessage("⏳ A connection attempt is already in progress.", Color.Goldenrod);
                return;
            }

            if (_serialPort?.IsOpen == true)
            {
                LogMessage("ℹ️ Already connected. Disconnect first to switch ports.", Color.Goldenrod);
                return;
            }

            var selectedPort = GetSelectedPortName();
            if (!TryValidateConnectionInputs(selectedPort, out var normalizedPort, out var baudRate))
            {
                return;
            }

            _isConnecting = true;
            btnConnect.Enabled = false;

            try
            {
                DisconnectSerial();

                if (TryOpenSerialPort(normalizedPort, baudRate, keepOpen: true))
                {
                    UpdateConnectionState(true);

                    // Initialize chug simulator
                    _chugSimulator = new ChugSimulator(SendWeight, msg => LogMessage(msg, Color.Cyan));
                }
            }
            finally
            {
                _isConnecting = false;
                if (_serialPort?.IsOpen != true)
                {
                    UpdateConnectionState(false);
                }
            }
        }

        private void BtnDisconnect_Click(object? sender, EventArgs e)
        {
            DisconnectSerial();
        }

        private void BtnTestPort_Click(object? sender, EventArgs e)
        {
            var selectedPort = GetSelectedPortName();
            if (!TryValidateConnectionInputs(selectedPort, out var normalizedPort, out var baudRate))
            {
                return;
            }

            LogMessage($"🧪 Running Open/Close test for {normalizedPort}...", Color.LightBlue);
            var tested = TryOpenSerialPort(normalizedPort, baudRate, keepOpen: false);
            if (tested)
            {
                LogMessage($"✅ Port test succeeded for {normalizedPort}.", Color.LimeGreen);
            }
        }

        private string GetSelectedPortName()
        {
            var selected = NormalizePortName(cmbComPorts.SelectedItem?.ToString());
            var manual = NormalizePortName(txtComPort.Text);

            if (!string.IsNullOrWhiteSpace(manual) && !manual.Equals(selected, StringComparison.OrdinalIgnoreCase))
            {
                return manual;
            }

            return !string.IsNullOrWhiteSpace(selected)
                ? selected
                : manual;
        }

        private void RefreshAvailablePorts()
        {
            var ports = SerialPort.GetPortNames()
                .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            cmbComPorts.Items.Clear();
            cmbComPorts.Items.AddRange(ports);

            if (ports.Length == 0)
            {
                cmbComPorts.SelectedIndex = -1;
                lblDetectedPorts.Text = "Detected ports: none";
                LogMessage("⚠️ No COM ports detected. Connect a real serial device or create a virtual pair (for example with com0com).", Color.Goldenrod);
                return;
            }

            lblDetectedPorts.Text = $"Detected ports: {string.Join(", ", ports)}";

            var preferredPort = NormalizePortName(txtComPort.Text);
            var preferredMatch = ports.FirstOrDefault(p => p.Equals(preferredPort, StringComparison.OrdinalIgnoreCase));
            var fallbackPort = ports[0];
            var selected = preferredMatch ?? fallbackPort;

            cmbComPorts.SelectedItem = selected;
            txtComPort.Text = selected;

            LogMessage($"🔎 Available COM ports: {string.Join(", ", ports)}", Color.LightBlue);
        }

        private bool TryValidateConnectionInputs(string rawPortName, out string normalizedPortName, out int baudRate)
        {
            normalizedPortName = NormalizePortName(rawPortName);

            if (string.IsNullOrWhiteSpace(normalizedPortName))
            {
                LogMessage("❌ Select a COM port first.", Color.Red);
                baudRate = 0;
                return false;
            }

            if (!normalizedPortName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                LogMessage($"❌ Invalid port name '{normalizedPortName}'. Use a COM-style name such as COM3.", Color.Red);
                baudRate = 0;
                return false;
            }

            if (!IsValidComPortName(normalizedPortName))
            {
                LogMessage($"❌ Invalid COM port format '{normalizedPortName}'. Expected COM followed by a positive number (for example COM3).", Color.Red);
                baudRate = 0;
                return false;
            }

            if (!int.TryParse(txtBaudRate.Text, out baudRate) || baudRate <= 0)
            {
                LogMessage("❌ Invalid baud rate.", Color.Red);
                return false;
            }

            var availablePorts = SerialPort.GetPortNames();
            var portToFind = normalizedPortName;
            var exists = availablePorts.Any(p => p.Equals(portToFind, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                LogMessage($"❌ {normalizedPortName} is not currently available on this machine.", Color.Red);
                if (availablePorts.Length == 0)
                {
                    LogMessage("ℹ️ No COM ports are currently detected. A real port or virtual pair may not exist yet.", Color.Goldenrod);
                }
                else
                {
                    LogMessage($"ℹ️ Detected ports: {string.Join(", ", availablePorts.OrderBy(static p => p))}", Color.Goldenrod);
                }

                return false;
            }

            return true;
        }

        private static bool IsValidComPortName(string normalizedPortName)
        {
            if (!normalizedPortName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var suffix = normalizedPortName[3..];
            return int.TryParse(suffix, out var portNumber) && portNumber > 0;
        }

        private static string NormalizePortName(string? portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
            {
                return string.Empty;
            }

            var trimmed = portName.Trim();

            if (trimmed.StartsWith("\\\\.\\", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[4..];
            }

            return trimmed.ToUpperInvariant();
        }

        private bool TryOpenSerialPort(string portName, int baudRate, bool keepOpen)
        {
            var serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                DtrEnable = true,
                RtsEnable = true,
                NewLine = "\n"
            };

            LogSerialSettings(serialPort, keepOpen ? "🔌 Attempting connection" : "🧪 Attempting test open/close");

            try
            {
                serialPort.Open();

                if (keepOpen)
                {
                    _serialPort = serialPort;
                    LogMessage($"✅ Connected to {portName} at {baudRate} baud.", Color.LimeGreen);
                }
                else
                {
                    LogMessage($"✅ Test open succeeded for {portName}. Closing test port...", Color.LimeGreen);
                    serialPort.Close();
                    serialPort.Dispose();
                }

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogConnectionFailure(serialPort, ex, "Port access denied. Another process may already own this port.");
            }
            catch (IOException ex)
            {
                LogConnectionFailure(serialPort, ex, "I/O error while opening the port. The port may not exist, may be unplugged, or may be unstable.");
            }
            catch (ArgumentException ex)
            {
                LogConnectionFailure(serialPort, ex, "Invalid port configuration. Check COM name and serial settings.");
            }
            catch (InvalidOperationException ex)
            {
                LogConnectionFailure(serialPort, ex, "Port is already open or in an invalid state.");
            }
            catch (Exception ex)
            {
                LogConnectionFailure(serialPort, ex, "Unexpected error while opening serial port.");
            }

            serialPort.Dispose();
            return false;
        }

        private void LogSerialSettings(SerialPort serialPort, string prefix)
        {
            LogMessage($"{prefix}: Port={serialPort.PortName}, Baud={serialPort.BaudRate}, Parity={serialPort.Parity}, DataBits={serialPort.DataBits}, StopBits={serialPort.StopBits}, Handshake={serialPort.Handshake}", Color.LightBlue);
        }

        private void LogConnectionFailure(SerialPort serialPort, Exception ex, string friendlyReason)
        {
            LogMessage($"❌ Connection failed ({ex.GetType().Name}): {friendlyReason}", Color.Red);
            LogSerialSettings(serialPort, "🔎 Attempted settings");
            LogMessage($"   Exception: {ex.Message}", Color.Red);
            if (ex.InnerException is not null)
            {
                LogMessage($"   Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}", Color.Red);
            }

            LogMessage("💡 Likely causes: selected port does not exist, virtual pair not created yet, wrong side of pair selected, port already in use, or invalid serial settings.", Color.Goldenrod);
        }

        private void DisconnectSerial()
        {
            StopContinuousMode();
            _chugSimulator?.Stop();

            if (_serialPort?.IsOpen == true)
            {
                var connectedPort = _serialPort.PortName;
                _serialPort.Close();
                LogMessage($"⏸️ Disconnected from {connectedPort}.", Color.Yellow);
            }

            _serialPort?.Dispose();
            _serialPort = null;
            _chugSimulator = null;
            UpdateConnectionState(false);
        }

        private void UpdateConnectionState(bool connected)
        {
            // Update status label
            lblConnectionStatus.Text = connected ? "Connected" : "Disconnected";
            lblConnectionStatus.ForeColor = connected ? Color.LimeGreen : Color.Red;

            // Update connection buttons
            btnConnect.Enabled = !connected && !_isConnecting;
            btnDisconnect.Enabled = connected;
            cmbComPorts.Enabled = !connected && !_isConnecting;
            btnRefreshPorts.Enabled = !connected && !_isConnecting;
            btnTestPort.Enabled = !connected && !_isConnecting;
            txtComPort.Enabled = !connected && !_isConnecting;
            txtBaudRate.Enabled = !connected && !_isConnecting;

            // Update all send buttons
            var sendEnabled = connected;
            btnNfc1.Enabled = sendEnabled;
            btnNfc2.Enabled = sendEnabled;
            btnNfc3.Enabled = sendEnabled;
            btnNfc4.Enabled = sendEnabled;
            btnNfc5.Enabled = sendEnabled;
            btnNfc6.Enabled = sendEnabled;
            btnNfc7.Enabled = sendEnabled;
            btnNfc8.Enabled = sendEnabled;
            btnNfc9.Enabled = sendEnabled;
            btnNfc10.Enabled = sendEnabled;

            btnNothing.Enabled = sendEnabled;
            btnEmptyGlass.Enabled = sendEnabled;
            btnFullGlass.Enabled = sendEnabled;
            btnEmptyPul.Enabled = sendEnabled;
            btnFullPul.Enabled = sendEnabled;
            chkContinuous.Enabled = sendEnabled;

            btnSendCustomWeight.Enabled = sendEnabled;
            numCustomWeight.Enabled = sendEnabled;
            btnSendCustomNfc.Enabled = sendEnabled;
            txtCustomNfc.Enabled = sendEnabled;

            btnSimulateGlassChug.Enabled = sendEnabled;
            btnSimulatePulChug.Enabled = sendEnabled;
        }

        #endregion

        #region NFC Tag Simulation

        private void SendNfcTag(int tagIndex)
        {
            if (tagIndex < 0 || tagIndex >= SimulatorConfig.NfcTags.Length)
            {
                LogMessage($"❌ Invalid NFC tag index: {tagIndex}", Color.Red);
                return;
            }

            var tag = SimulatorConfig.NfcTags[tagIndex];
            var message = SimulatorConfig.FormatMessage(SimulatorConfig.RfidPrefix, tag);
            SendSerialMessage(message);
            LogMessage($"📡 NFC {tagIndex + 1}: {tag}", Color.Cyan);
        }

        #endregion

        #region Weight Preset Simulation

        private void SendWeightPreset(int baseValue, int margin, string label)
        {
            _activePreset = (baseValue, margin, label);
            var randomOffset = _random.Next(-margin, margin + 1);
            var value = baseValue + randomOffset;
            SendWeight(value);
            LogMessage($"⚖️ {label}: {value} (base: {baseValue}, offset: {randomOffset:+#;-#;0})", Color.Yellow);
        }

        private void SendWeight(int value)
        {
            var message = SimulatorConfig.FormatMessage(SimulatorConfig.ScalePrefix, value.ToString());
            SendSerialMessage(message);
        }

        #endregion

        #region Continuous Mode

        private void ChkContinuous_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkContinuous.Checked)
            {
                StartContinuousMode();
            }
            else
            {
                StopContinuousMode();
            }
        }

        private void StartContinuousMode()
        {
            if (_continuousTimer != null) return;

            _continuousTimer = new System.Windows.Forms.Timer
            {
                Interval = SimulatorConfig.SimulationUpdateIntervalMs
            };
            _continuousTimer.Tick += ContinuousTimer_Tick;
            _continuousTimer.Start();

            LogMessage($"🔄 Continuous mode started (sending {_activePreset.Label})", Color.Magenta);
        }

        private void StopContinuousMode()
        {
            if (_continuousTimer != null)
            {
                _continuousTimer.Stop();
                _continuousTimer.Dispose();
                _continuousTimer = null;
                LogMessage("⏸️ Continuous mode stopped", Color.Magenta);
            }
        }

        private void ContinuousTimer_Tick(object? sender, EventArgs e)
        {
            // Send randomized value for the last selected preset
            var randomOffset = _random.Next(-_activePreset.Margin, _activePreset.Margin + 1);
            var value = _activePreset.BaseValue + randomOffset;
            SendWeight(value);
        }

        #endregion

        #region Manual Send

        private void BtnSendCustomWeight_Click(object? sender, EventArgs e)
        {
            var value = (int)numCustomWeight.Value;
            SendWeight(value);
            LogMessage($"⚖️ Custom weight: {value}", Color.Orange);
        }

        private void BtnSendCustomNfc_Click(object? sender, EventArgs e)
        {
            var tag = txtCustomNfc.Text.Trim();
            if (string.IsNullOrEmpty(tag))
            {
                LogMessage("❌ Please enter a custom NFC tag.", Color.Red);
                return;
            }

            var message = SimulatorConfig.FormatMessage(SimulatorConfig.RfidPrefix, tag);
            SendSerialMessage(message);
            LogMessage($"📡 Custom NFC: {tag}", Color.Orange);
        }

        #endregion

        #region Chug Simulation

        private async void BtnSimulateGlassChug_Click(object? sender, EventArgs e)
        {
            if (_chugSimulator == null) return;

            btnSimulateGlassChug.Enabled = false;
            btnSimulatePulChug.Enabled = false;
            btnStopSimulation.Enabled = true;

            try
            {
                await _chugSimulator.SimulateGlassChugAsync();
            }
            finally
            {
                if (_serialPort?.IsOpen == true)
                {
                    btnSimulateGlassChug.Enabled = true;
                    btnSimulatePulChug.Enabled = true;
                }
                btnStopSimulation.Enabled = false;
            }
        }

        private async void BtnSimulatePulChug_Click(object? sender, EventArgs e)
        {
            if (_chugSimulator == null) return;

            btnSimulateGlassChug.Enabled = false;
            btnSimulatePulChug.Enabled = false;
            btnStopSimulation.Enabled = true;

            try
            {
                await _chugSimulator.SimulatePulChugAsync();
            }
            finally
            {
                if (_serialPort?.IsOpen == true)
                {
                    btnSimulateGlassChug.Enabled = true;
                    btnSimulatePulChug.Enabled = true;
                }
                btnStopSimulation.Enabled = false;
            }
        }

        private void BtnStopSimulation_Click(object? sender, EventArgs e)
        {
            _chugSimulator?.Stop();
            btnSimulateGlassChug.Enabled = true;
            btnSimulatePulChug.Enabled = true;
            btnStopSimulation.Enabled = false;
        }

        #endregion

        #region Serial Communication

        private void SendSerialMessage(string message)
        {
            if (_serialPort?.IsOpen != true)
            {
                LogMessage("❌ Serial port not connected.", Color.Red);
                return;
            }

            try
            {
                _serialPort.WriteLine(message);
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Send error: {ex.Message}", Color.Red);
            }
        }

        #endregion

        #region Logging

        private void LogMessage(string message, Color? color = null)
        {
            if (InvokeRequired)
            {
                Invoke(() => LogMessage(message, color));
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";

            txtLog.AppendText(logEntry + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        #endregion

        #region Cleanup

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DisconnectSerial();
        }

        #endregion
    }
}

