using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace HidSharpTest
{
    public partial class MainForm : Form
    {
        private CustomGamepad gamepad;
        private Button[] buttonControls = new Button[32];
        private bool isInitialized = false;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "32 Button Gamepad";
            this.Size = new Size(600, 400);
            SetupGamepad();
            CreateButtonGrid();
        }

        private void SetupGamepad()
        {
            try
            {
                gamepad = new CustomGamepad();
                gamepad.Initialize();
                isInitialized = true;

                Thread gamepadThread = new Thread(() => gamepad.StartReporting());
                gamepadThread.IsBackground = true;
                gamepadThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gamepad başlatılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateButtonGrid()
        {
            int buttonSize = 60;
            int spacing = 10;

            for (int i = 0; i < 32; i++)
            {
                Button btn = new Button
                {
                    Size = new Size(buttonSize, buttonSize),
                    Location = new Point(
                        spacing + (i % 8) * (buttonSize + spacing),
                        spacing + (i / 8) * (buttonSize + spacing)
                    ),
                    Text = $"Btn {i + 1}",
                    Tag = i
                };

                btn.MouseDown += (s, e) => ButtonPressed(btn);
                btn.MouseUp += (s, e) => ButtonReleased(btn);

                buttonControls[i] = btn;
                this.Controls.Add(btn);
            }
        }

        private void ButtonPressed(Button btn)
        {
            if (!isInitialized) return;

            int index = (int)btn.Tag;
            gamepad.SetButton(index, true);
            btn.BackColor = Color.LightGreen;
        }

        private void ButtonReleased(Button btn)
        {
            if (!isInitialized) return;

            int index = (int)btn.Tag;
            gamepad.SetButton(index, false);
            btn.BackColor = SystemColors.Control;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isInitialized)
            {
                gamepad.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}
