using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StaffApp
{
    public class StaffLogin : Form
    {
        private const int EM_SETMARGINS = 0xD3, EC_LEFTMARGIN = 0x1;

        [DllImport("User32.dll")]
        static extern IntPtr SendMessage(IntPtr h, int msg, int wp, int lp);

        TextBox txtUser, txtPass;
        Panel loginPanel;
        Label errorLabel;   // <--- added

        public StaffLogin()
        {
            InitializeComponent();
            BuildUI();
        }

        private void InitializeComponent() { }

        private void BuildUI()
        {
            // FORM SETTINGS
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackgroundImage = Properties.Resources.background;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            loginPanel = new Panel()
            {
                Size = new Size(500, 350),
                BackColor = Color.FromArgb(120, 0, 0, 0)
            };
            Round(loginPanel, 30);
            this.Controls.Add(loginPanel);

            this.Load += (s, e) => CenterLoginPanel();
            this.Resize += (s, e) => CenterLoginPanel();

            // USERNAME LABEL
            loginPanel.Controls.Add(new Label()
            {
                Text = "Username",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent,
                Location = new Point(40, 40)
            });

            Panel box1 = new Panel()
            {
                BackColor = Color.White,
                Width = 400,
                Height = 40,
                Location = new Point(40, 70)
            };
            Round(box1, 15);
            loginPanel.Controls.Add(box1);

            txtUser = new TextBox()
            {
                BorderStyle = BorderStyle.None,
                Width = 350,
                Font = new Font("Segoe UI", 15),
                Location = new Point(10, 9)
            };
            box1.Controls.Add(txtUser);
            Pad(txtUser);

            // PASSWORD LABEL
            loginPanel.Controls.Add(new Label()
            {
                Text = "Password",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent,
                Location = new Point(40, 130)
            });

            Panel box2 = new Panel()
            {
                BackColor = Color.White,
                Width = 400,
                Height = 40,
                Location = new Point(40, 160)
            };
            Round(box2, 15);
            loginPanel.Controls.Add(box2);

            txtPass = new TextBox()
            {
                BorderStyle = BorderStyle.None,
                Width = 320,
                Font = new Font("Segoe UI", 15),
                UseSystemPasswordChar = true,
                Location = new Point(10, 9)
            };
            box2.Controls.Add(txtPass);
            Pad(txtPass);

            // SHOW/HIDE PASSWORD ICON
            PictureBox eyeIcon = new PictureBox()
            {
                Image = Properties.Resources.password_visible,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Width = 22,
                Height = 22,
                Location = new Point(box2.Width - 35, 9)
            };
            box2.Controls.Add(eyeIcon);

            bool hidden = true;
            eyeIcon.Click += (s, e) =>
            {
                hidden = !hidden;
                txtPass.UseSystemPasswordChar = hidden;
                Pad(txtPass);

                eyeIcon.Image = hidden ?
                    Properties.Resources.password_visible :
                    Properties.Resources.password_hiden;
            };

            // ================================
            // ERROR LABEL (Below Password)
            // ================================
            errorLabel = new Label()
            {
                Text = "",
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(40, 205),
                Visible = false   // hidden until needed
            };
            loginPanel.Controls.Add(errorLabel);

            // LOGIN BUTTON
            Button btnLogin = new Button()
            {
                Text = "Sign In",
                Width = 300,
                Height = 45,
                BackColor = Color.FromArgb(17, 89, 201),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(100, 240)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            Round(btnLogin, 25);

            loginPanel.Controls.Add(btnLogin);
            btnLogin.Click += LoginAttempt;
        }

        private void CenterLoginPanel()
        {
            if (loginPanel == null) return;

            loginPanel.Left = (this.ClientSize.Width - loginPanel.Width) / 2;
            loginPanel.Top = (this.ClientSize.Height - loginPanel.Height) / 2;
        }

        // ===================================
        // LOGIN ATTEMPT WITH RED ERROR LINE
        // ===================================
        private void LoginAttempt(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(
                @"Data Source=DESKTOP-TSHTFFD\HUISQL;Initial Catalog=Final_Project_DB;Integrated Security=True;TrustServerCertificate=True"))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT Username FROM Staff WHERE Username=@u AND Password=@p", conn);

                cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                cmd.Parameters.AddWithValue("@p", txtPass.Text.Trim());

                conn.Open();
                var user = cmd.ExecuteScalar();

                if (user != null)
                {
                    // clear error message when successful
                    errorLabel.Visible = false;
                    Program.LoginSuccess(user.ToString());
                }
                else
                {
                    // show red line error
                    errorLabel.Text = "❌ Wrong Username or Password";
                    errorLabel.Visible = true;
                }
            }
        }

        private void Round(Control c, int r)
        {
            c.Region = Region.FromHrgn(
                CreateRoundRectRgn(0, 0, c.Width, c.Height, r, r));
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        static extern IntPtr CreateRoundRectRgn(int L, int T, int R, int B, int W, int H);

        void Pad(TextBox T)
        {
            SendMessage(T.Handle, EM_SETMARGINS, EC_LEFTMARGIN, 20);
        }

        public void ClearFields()
        {
            txtUser.Text = "";
            txtPass.Text = "";
            errorLabel.Visible = false; // also reset error
        }
    }
}
