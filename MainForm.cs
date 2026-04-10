using System;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Windows.Forms;

namespace Fluke8808AGui
{
    public class MainForm : Form
    {
        private ComboBox cmbPort;
        private ComboBox cmbBaud;
        private TextBox txtCommand;
        private TextBox txtResponse;
        private TextBox txtLog;
        private TextBox txtLowLimit;
        private TextBox txtHighLimit;
        private Label lblReading;
        private Label lblStatus;
        private Button btnRefreshPorts;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnIdn;
        private Button btnMeasure;
        private Button btnDc;
        private Button btnAc;
        private Button btnOhm;
        private Button btnSend;

        private SerialPort serialPort;

        public MainForm()
        {
            InitializeUi();
            LoadPorts();
        }

        private void InitializeUi()
        {
            Text = "Fluke 8808A GUI";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(860, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Controls.Add(new Label { Text = "COM Port:", Location = new Point(20, 20), AutoSize = true });
            cmbPort = new ComboBox { Location = new Point(90, 16), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbPort);

            Controls.Add(new Label { Text = "Baud:", Location = new Point(210, 20), AutoSize = true });
            cmbBaud = new ComboBox { Location = new Point(255, 16), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBaud.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
            cmbBaud.SelectedIndex = 0;
            Controls.Add(cmbBaud);

            btnRefreshPorts = new Button { Text = "Refresh", Location = new Point(380, 14), Size = new Size(80, 30) };
            btnRefreshPorts.Click += (s, e) => LoadPorts();
            Controls.Add(btnRefreshPorts);

            btnConnect = new Button { Text = "Connect", Location = new Point(470, 14), Size = new Size(90, 30) };
            btnConnect.Click += BtnConnect_Click;
            Controls.Add(btnConnect);

            btnDisconnect = new Button { Text = "Disconnect", Location = new Point(570, 14), Size = new Size(90, 30) };
            btnDisconnect.Click += BtnDisconnect_Click;
            Controls.Add(btnDisconnect);

            lblStatus = new Label
            {
                Text = "Status: Disconnected",
                Location = new Point(680, 20),
                AutoSize = true,
                ForeColor = Color.DarkRed
            };
            Controls.Add(lblStatus);

            btnIdn = new Button { Text = "*IDN?", Location = new Point(20, 70), Size = new Size(90, 36) };
            btnIdn.Click += (s, e) => RunQuery("*IDN?");
            Controls.Add(btnIdn);

            btnDc = new Button { Text = "DC Volt", Location = new Point(120, 70), Size = new Size(90, 36) };
            btnDc.Click += (s, e) => RunCommand("VDC");
            Controls.Add(btnDc);

            btnAc = new Button { Text = "AC Volt", Location = new Point(220, 70), Size = new Size(90, 36) };
            btnAc.Click += (s, e) => RunCommand("VAC");
            Controls.Add(btnAc);

            btnOhm = new Button { Text = "Ohm", Location = new Point(320, 70), Size = new Size(90, 36) };
            btnOhm.Click += (s, e) => RunCommand("OHMS");
            Controls.Add(btnOhm);

            btnMeasure = new Button { Text = "VAL?", Location = new Point(420, 70), Size = new Size(90, 36) };
            btnMeasure.Click += (s, e) => ReadMeasurement();
            Controls.Add(btnMeasure);

            lblReading = new Label
            {
                Text = "Reading: --",
                Location = new Point(20, 125),
                Size = new Size(600, 36),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Navy
            };
            Controls.Add(lblReading);

            Controls.Add(new Label { Text = "Low Limit:", Location = new Point(20, 175), AutoSize = true });
            txtLowLimit = new TextBox { Location = new Point(90, 171), Width = 100, Text = "0" };
            Controls.Add(txtLowLimit);

            Controls.Add(new Label { Text = "High Limit:", Location = new Point(210, 175), AutoSize = true });
            txtHighLimit = new TextBox { Location = new Point(285, 171), Width = 100, Text = "10" };
            Controls.Add(txtHighLimit);

            Controls.Add(new Label { Text = "Manual Cmd:", Location = new Point(20, 220), AutoSize = true });
            txtCommand = new TextBox { Location = new Point(110, 216), Width = 420, Text = "VAL?" };
            Controls.Add(txtCommand);

            btnSend = new Button { Text = "Send", Location = new Point(540, 214), Size = new Size(90, 30) };
            btnSend.Click += BtnSend_Click;
            Controls.Add(btnSend);

            Controls.Add(new Label { Text = "Response:", Location = new Point(20, 265), AutoSize = true });
            txtResponse = new TextBox { Location = new Point(20, 290), Width = 790, Height = 60, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
            Controls.Add(txtResponse);

            Controls.Add(new Label { Text = "Log:", Location = new Point(20, 365), AutoSize = true });
            txtLog = new TextBox { Location = new Point(20, 390), Width = 790, Height = 170, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 10) };
            Controls.Add(txtLog);
        }

        private void LoadPorts()
        {
            cmbPort.Items.Clear();
            cmbPort.Items.AddRange(SerialPort.GetPortNames());
            if (cmbPort.Items.Count > 0)
                cmbPort.SelectedIndex = 0;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbPort.SelectedItem == null)
                    throw new Exception("No COM port selected.");

                serialPort = new SerialPort(cmbPort.SelectedItem.ToString(), int.Parse(cmbBaud.Text), Parity.None, 8, StopBits.One);
                serialPort.NewLine = "\r\n";
                serialPort.ReadTimeout = 3000;
                serialPort.WriteTimeout = 3000;
                serialPort.Open();

                lblStatus.Text = "Status: Connected";
                lblStatus.ForeColor = Color.DarkGreen;
                Log("Connected to " + cmbPort.SelectedItem + " @ " + cmbBaud.Text);
            }
            catch (Exception ex)
            {
                Log("Connect error: " + ex.Message);
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                    serialPort.Close();
                lblStatus.Text = "Status: Disconnected";
                lblStatus.ForeColor = Color.DarkRed;
                Log("Disconnected.");
            }
            catch (Exception ex)
            {
                Log("Disconnect error: " + ex.Message);
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                EnsureConnected();
                string cmd = txtCommand.Text.Trim();
                if (string.IsNullOrWhiteSpace(cmd))
                    return;

                if (cmd.EndsWith("?"))
                    txtResponse.Text = Query(cmd);
                else
                {
                    Write(cmd);
                    txtResponse.Text = "Command sent.";
                }
            }
            catch (Exception ex)
            {
                Log("Send error: " + ex.Message);
                MessageBox.Show(ex.Message, "Send Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunCommand(string cmd)
        {
            try
            {
                EnsureConnected();
                Write(cmd);
                txtResponse.Text = "Mode set: " + cmd;
            }
            catch (Exception ex)
            {
                Log("Command error: " + ex.Message);
            }
        }

        private void RunQuery(string cmd)
        {
            try
            {
                EnsureConnected();
                txtResponse.Text = Query(cmd);
            }
            catch (Exception ex)
            {
                Log("Query error: " + ex.Message);
            }
        }

        private void ReadMeasurement()
        {
            try
            {
                EnsureConnected();
                string result = Query("VAL?");
                txtResponse.Text = result;
                lblReading.Text = "Reading: " + result;
                CheckLimits(result);
            }
            catch (Exception ex)
            {
                Log("Measure error: " + ex.Message);
            }
        }

        private void CheckLimits(string result)
        {
            string clean = result.Trim();
            int spaceIndex = clean.IndexOf(' ');
            if (spaceIndex > 0)
                clean = clean.Substring(0, spaceIndex);

            if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                double low = double.Parse(txtLowLimit.Text, CultureInfo.InvariantCulture);
                double high = double.Parse(txtHighLimit.Text, CultureInfo.InvariantCulture);

                if (value >= low && value <= high)
                {
                    lblReading.ForeColor = Color.DarkGreen;
                    Log("PASS: " + value.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    lblReading.ForeColor = Color.Red;
                    Log("FAIL: " + value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void EnsureConnected()
        {
            if (serialPort == null || !serialPort.IsOpen)
                throw new Exception("Connect to Fluke 8808A first.");
        }

        private void Write(string cmd)
        {
            serialPort.WriteLine(cmd);
            Log(">> " + cmd);
        }

        private string Query(string cmd)
        {
            serialPort.DiscardInBuffer();
            serialPort.WriteLine(cmd);
            Log(">> " + cmd);
            string response = serialPort.ReadLine().Trim();
            Log("<< " + response);
            return response;
        }

        private void Log(string text)
        {
            txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + text + Environment.NewLine);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                    serialPort.Close();
            }
            catch { }
            base.OnFormClosing(e);
        }
    }
}
