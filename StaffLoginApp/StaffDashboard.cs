using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StaffApp
{
    public class Dashboard : Form
    {
        private string StaffUsername; // Renamed to be clear this is the Username
        private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["StaffDB"].ConnectionString;

        // UI Containers
        Panel sidebar, topbar, content, center;

        // Inventory Variables
        DataGridView dgvProducts;
        TextBox txtSearch, txtName, txtCategory, txtPrice, txtQty, txtReorder;

        // Staff Management Variables
        DataGridView dgvStaff;
        TextBox txtStaffFullName, txtStaffUser, txtStaffPass;

        public Dashboard(string username)
        {
            StaffUsername = username;
            InitializeComponent();
            BuildUI();
        }

        private void InitializeComponent() { }

        //====================================================================
        //=======================  UI LAYOUT BUILDER  ========================
        //====================================================================
        private void BuildUI()
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            // ---------- SIDEBAR ----------
            sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.Gainsboro,
                Padding = new Padding(10, 20, 0, 0)
            };
            this.Controls.Add(sidebar);

            sidebar.Controls.Add(new Label()
            {
                Text = "📌 Navigation",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 20)
            });

            AddMenuButton("🏠 Dashboard", 80, ShowDashboardHomepage);
            AddMenuButton("📦 Inventory", 130, () => LoadPage("Inventory"));
            AddMenuButton("📥 GRN", 180, () => LoadPage("GRN"));
            AddMenuButton("📤 GIN", 230, () => LoadPage("GIN"));
            AddMenuButton("👥 Staff", 280, () => LoadPage("Staff"));
            AddMenuButton("🚪 Logout", 330, () => Program.Logout());
            AddMenuButton("❌ Exit", 380, () => Application.Exit());

            // ---------- TOPBAR ----------
            topbar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.RoyalBlue };
            this.Controls.Add(topbar);

            topbar.Controls.Add(new Label()
            {
                Text = "User: " + StaffUsername,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 14)
            });

            // ---------- CONTENT ----------
            content = new Panel()
            {
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.White,
                Left = sidebar.Width,
                Top = topbar.Height,
                Width = this.ClientSize.Width - sidebar.Width,
                Height = this.ClientSize.Height - topbar.Height
            };
            this.Controls.Add(content);

            this.Resize += (s, e) => ResizePanel();
            ShowDashboardHomepage();
        }

        private void ResizePanel()
        {
            if (content == null) return;
            content.Left = sidebar.Width;
            content.Top = topbar.Height;
            content.Width = this.ClientSize.Width - sidebar.Width;
            content.Height = this.ClientSize.Height - topbar.Height;
        }

        private void AddMenuButton(string text, int top, Action action)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 180,
                Height = 40,
                Left = 10,
                Top = top,
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action();
            sidebar.Controls.Add(btn);
        }

        //====================================================================
        //=====================  PAGE ROUTING SYSTEM  ========================
        //====================================================================
        private void LoadPage(string p)
        {
            content.Controls.Clear();
            if (p == "Inventory") { BuildInventoryUI(); return; }
            if (p == "GRN") { BuildGRN_UI(); return; }
            if (p == "GIN") { BuildGIN_UI(); return; }
            if (p == "Staff") { BuildStaffUI(); return; }

            content.Controls.Add(new Label() { Text = p + " coming soon...", Font = new Font("Segoe UI", 18), Location = new Point(40, 40) });
        }

        //====================================================================
        //=====================  DASHBOARD HOMEPAGE  =========================
        //====================================================================
        private void ShowDashboardHomepage()
        {
            content.Controls.Clear();
            center = new Panel { AutoSize = true, Width = 700 };
            content.Controls.Add(center);

            FlowLayoutPanel cards = new FlowLayoutPanel { Width = center.Width, AutoSize = true };
            center.Controls.Add(cards);

            cards.Controls.Add(Card("📦 Products", GetTotalProducts().ToString()));
            cards.Controls.Add(Card("⚠ Low Stock", GetLowStockItems().ToString()));
            cards.Controls.Add(Card("📥 GRN Month", GetGRNThisMonth().ToString()));
            cards.Controls.Add(Card("📤 GIN Month", GetGINThisMonth().ToString()));

            center.Controls.Add(new Label()
            {
                Text = "📈 Inventory Trend",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Top = cards.Bottom + 15,
                AutoSize = true
            });

            Chart c1 = TrendChart();
            c1.Top = cards.Bottom + 50;
            center.Controls.Add(c1);

            Label t2 = new Label()
            {
                Text = "📋 Recent GRN / GIN",
                Top = c1.Bottom + 25,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };
            center.Controls.Add(t2);

            DataGridView grid = Table();
            grid.Top = t2.Bottom + 5;
            center.Controls.Add(grid);
        }

        private Panel Card(string title, string number)
        {
            Panel p = new Panel()
            {
                Width = 150,
                Height = 80,
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(10)
            };
            p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10), Location = new Point(10, 5) });
            p.Controls.Add(new Label { Text = number, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(10, 30) });
            return p;
        }

        private DataGridView Table()
        {
            DataGridView g = new DataGridView()
            {
                DataSource = GetRecentActivities(),
                Width = 600,
                Height = 200,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            return g;
        }

        private Chart TrendChart()
        {
            Chart c = new Chart() { Width = 600, Height = 200 };
            c.ChartAreas.Add(new ChartArea());

            Series s = new Series("Inventory") { ChartType = SeriesChartType.Line };
            foreach (DataRow r in GetInventoryTrend().Rows)
                s.Points.AddXY(Convert.ToDateTime(r["Date"]), Convert.ToInt32(r["TotalQty"]));

            c.Series.Add(s);
            return c;
        }

        //====================================================================
        //======================== INVENTORY UI =============================
        //====================================================================
        private void BuildInventoryUI()
        {
            Label ti = new Label() { Text = "📦 Inventory Management", Font = new Font("Segoe UI", 22), Location = new Point(20, 20), AutoSize = true };
            content.Controls.Add(ti);

            // --- SEARCH ---
            content.Controls.Add(new Label() { Text = "Search:", Font = new Font("Segoe UI", 10), Location = new Point(20, 80) });

            txtSearch = new TextBox() { Width = 250, Location = new Point(80, 76) };
            txtSearch.TextChanged += (s, e) => SearchProducts();
            content.Controls.Add(txtSearch);

            // --- TABLE ---
            dgvProducts = new DataGridView()
            {
                Width = content.Width - 60,
                Height = 320,
                Top = 120,
                Left = 20,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvProducts.CellClick += GridClick;
            content.Controls.Add(dgvProducts);

            // --- PRODUCT FORM ---
            Panel p = new Panel()
            {
                Width = dgvProducts.Width,
                Height = 200,
                Left = 20,
                Top = 460,
                BackColor = Color.WhiteSmoke
            };
            content.Controls.Add(p);

            Field(p, "Name:", out txtName, 20, 20);
            Field(p, "Category:", out txtCategory, 330, 20);
            Field(p, "Price:", out txtPrice, 640, 20);
            Field(p, "Qty:", out txtQty, 20, 75);
            Field(p, "Reorder:", out txtReorder, 330, 75);

            Btn(p, "Add", 640, 75, (s, e) => AddProduct());
            Btn(p, "Update", 740, 75, (s, e) => UpdateProduct());
            Btn(p, "Delete", 840, 75, (s, e) => DeleteProduct());
            Btn(p, "Refresh", 740, 120, (s, e) => LoadProducts());

            LoadProducts();
        }

        //====================================================================
        //======================== GRN UI & LOGIC ============================
        //====================================================================
        private void BuildGRN_UI()
        {
            content.Controls.Clear();
            Label title = new Label() { Text = "📥 Goods Receipt Note (GRN)", Font = new Font("Segoe UI", 22), Location = new Point(20, 20), AutoSize = true };
            content.Controls.Add(title);

            // INPUT PANEL
            Panel p = new Panel() { Width = content.Width - 60, Height = 150, Left = 20, Top = 80, BackColor = Color.WhiteSmoke };
            content.Controls.Add(p);

            Label lbProduct = new Label() { Text = "Product:", Location = new Point(20, 20), Font = new Font("Segoe UI", 10) };
            ComboBox cbProduct = new ComboBox() { Width = 300, Location = new Point(100, 18), DropDownStyle = ComboBoxStyle.DropDownList };
            LoadProductDropdown(cbProduct);
            p.Controls.Add(lbProduct); p.Controls.Add(cbProduct);

            Label lbQty = new Label() { Text = "Quantity:", Location = new Point(420, 20), Font = new Font("Segoe UI", 10) };
            TextBox txtGRNQty = new TextBox() { Width = 150, Location = new Point(500, 18) };
            p.Controls.Add(lbQty); p.Controls.Add(txtGRNQty);

            DataGridView grid = new DataGridView()
            {
                Width = content.Width - 60,
                Height = 350,
                Top = 250,
                Left = 20,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            content.Controls.Add(grid);
            LoadGRNTable(grid);

            Button btnSave = new Button() { Text = "Add GRN", Width = 120, Height = 40, BackColor = Color.Silver, Location = new Point(680, 18) };
            p.Controls.Add(btnSave);

            btnSave.Click += (s, e) =>
            {
                if (cbProduct.SelectedIndex == -1) { MessageBox.Show("Please select a product."); return; }
                if (!int.TryParse(txtGRNQty.Text, out int qty) || qty <= 0) { MessageBox.Show("Positive Quantity required."); return; }

                // ➤ FIX 1: Use SelectedValue for ID
                int productID = Convert.ToInt32(cbProduct.SelectedValue);

                using (SqlConnection c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO GRN(ProductID, Quantity, StaffID) VALUES(@p,@q,@s)", c);
                    cmd.Parameters.AddWithValue("@p", productID);
                    cmd.Parameters.AddWithValue("@q", qty);
                    // ➤ FIX 2: GetStaffID now works correctly with Username
                    cmd.Parameters.AddWithValue("@s", GetStaffID(StaffUsername));
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("GRN added successfully!");
                LoadGRNTable(grid);
            };
        }

        //====================================================================
        //======================== GIN UI & LOGIC ============================
        //====================================================================
        private void BuildGIN_UI()
        {
            content.Controls.Clear();
            Label title = new Label() { Text = "📤 Goods Issue Note (GIN)", Font = new Font("Segoe UI", 22), Location = new Point(20, 20), AutoSize = true };
            content.Controls.Add(title);

            Panel p = new Panel() { Width = content.Width - 60, Height = 150, Left = 20, Top = 80, BackColor = Color.WhiteSmoke };
            content.Controls.Add(p);

            ComboBox cbProduct = new ComboBox() { Width = 300, Location = new Point(100, 18), DropDownStyle = ComboBoxStyle.DropDownList };
            LoadProductDropdown(cbProduct);
            p.Controls.Add(new Label() { Text = "Product:", Location = new Point(20, 20), Font = new Font("Segoe UI", 10) });
            p.Controls.Add(cbProduct);

            TextBox txtGINQty = new TextBox() { Width = 150, Location = new Point(500, 18) };
            p.Controls.Add(new Label() { Text = "Quantity:", Location = new Point(420, 20), Font = new Font("Segoe UI", 10) });
            p.Controls.Add(txtGINQty);

            DataGridView grid = new DataGridView()
            {
                Width = content.Width - 60,
                Height = 350,
                Top = 250,
                Left = 20,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            content.Controls.Add(grid);
            LoadGINTable(grid);

            Button btnSave = new Button() { Text = "Add GIN", Width = 120, Height = 40, BackColor = Color.Silver, Location = new Point(680, 18) };
            p.Controls.Add(btnSave);

            btnSave.Click += (s, e) =>
            {
                if (cbProduct.SelectedIndex == -1) { MessageBox.Show("Select a product."); return; }
                if (!int.TryParse(txtGINQty.Text, out int qty) || qty <= 0) { MessageBox.Show("Positive Quantity required."); return; }

                // ➤ FIX 1: Use SelectedValue for ID
                int productID = Convert.ToInt32(cbProduct.SelectedValue);

                if (qty > GetProductQty(productID)) { MessageBox.Show("Not enough stock!"); return; }

                using (SqlConnection c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO GIN(ProductID, Quantity, StaffID) VALUES(@p,@q,@s)", c);
                    cmd.Parameters.AddWithValue("@p", productID);
                    cmd.Parameters.AddWithValue("@q", qty);
                    // ➤ FIX 2: GetStaffID now works correctly with Username
                    cmd.Parameters.AddWithValue("@s", GetStaffID(StaffUsername));
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("GIN added successfully!");
                LoadGINTable(grid);
            };
        }

        //====================================================================
        //======================== STAFF MANAGEMENT UI =======================
        //====================================================================
        private void BuildStaffUI()
        {
            Label title = new Label() { Text = "👥 Staff Management", Font = new Font("Segoe UI", 22), Location = new Point(20, 20), AutoSize = true };
            content.Controls.Add(title);

            dgvStaff = new DataGridView()
            {
                Width = content.Width - 60,
                Height = 350,
                Top = 80,
                Left = 20,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            dgvStaff.CellClick += StaffGridClick;
            content.Controls.Add(dgvStaff);

            Panel p = new Panel() { Width = dgvStaff.Width, Height = 150, Left = 20, Top = 450, BackColor = Color.WhiteSmoke };
            content.Controls.Add(p);

            Field(p, "Full Name:", out txtStaffFullName, 20, 20);
            Field(p, "Username:", out txtStaffUser, 330, 20);
            Field(p, "Password:", out txtStaffPass, 20, 75);
            txtStaffPass.PasswordChar = '*';

            Btn(p, "Add Staff", 640, 30, (s, e) => AddStaff());
            Btn(p, "Update", 640, 75, (s, e) => UpdateStaff());
            Btn(p, "Delete", 750, 75, (s, e) => DeleteStaff());
            Btn(p, "Clear", 750, 30, (s, e) => ClearStaffInputs());

            LoadStaffList();
        }

        private void LoadStaffList()
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                try
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT StaffID, FullName, Username, Password FROM Staff", c);
                    DataTable dt = new DataTable(); da.Fill(dt);
                    dgvStaff.DataSource = dt;
                    if (dgvStaff.Columns["Password"] != null) dgvStaff.Columns["Password"].Visible = false;
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void AddStaff()
        {
            if (string.IsNullOrWhiteSpace(txtStaffFullName.Text) || string.IsNullOrWhiteSpace(txtStaffUser.Text)) { MessageBox.Show("Full Name and Username required."); return; }
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                try
                {
                    c.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Staff (FullName, Username, Password) VALUES (@n, @u, @p)", c);
                    cmd.Parameters.AddWithValue("@n", txtStaffFullName.Text);
                    cmd.Parameters.AddWithValue("@u", txtStaffUser.Text);
                    cmd.Parameters.AddWithValue("@p", txtStaffPass.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Staff added."); LoadStaffList(); ClearStaffInputs();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void UpdateStaff()
        {
            if (dgvStaff.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvStaff.SelectedRows[0].Cells["StaffID"].Value);
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                try
                {
                    c.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE Staff SET FullName=@n, Username=@u, Password=@p WHERE StaffID=@id", c);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@n", txtStaffFullName.Text);
                    cmd.Parameters.AddWithValue("@u", txtStaffUser.Text);
                    cmd.Parameters.AddWithValue("@p", txtStaffPass.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Staff updated."); LoadStaffList(); ClearStaffInputs();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void DeleteStaff()
        {
            if (dgvStaff.SelectedRows.Count == 0) return;
            if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No) return;
            int id = Convert.ToInt32(dgvStaff.SelectedRows[0].Cells["StaffID"].Value);
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                try
                {
                    c.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Staff WHERE StaffID=@id", c);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Staff deleted."); LoadStaffList(); ClearStaffInputs();
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 547) MessageBox.Show("Cannot delete staff with existing GRN/GIN records.");
                    else MessageBox.Show("SQL Error: " + sqlEx.Message);
                }
            }
        }

        private void StaffGridClick(object s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            txtStaffFullName.Text = dgvStaff["FullName", e.RowIndex].Value.ToString();
            txtStaffUser.Text = dgvStaff["Username", e.RowIndex].Value.ToString();
            txtStaffPass.Text = dgvStaff["Password", e.RowIndex].Value.ToString();
        }

        private void ClearStaffInputs()
        {
            txtStaffFullName.Clear(); txtStaffUser.Clear(); txtStaffPass.Clear(); dgvStaff.ClearSelection();
        }

        //====================================================================
        //======================== HELPERS & DATA ============================
        //====================================================================
        private void LoadProductDropdown(ComboBox cb)
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT ProductID, ProductName FROM Products", c);
                DataTable dt = new DataTable(); da.Fill(dt);
                cb.DataSource = dt; cb.DisplayMember = "ProductName"; cb.ValueMember = "ProductID";
            }
        }

        private void LoadStaffDropdown(ComboBox cb)
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT StaffID, FullName FROM Staff", c);
                DataTable dt = new DataTable(); da.Fill(dt);
                cb.DataSource = dt; cb.DisplayMember = "FullName"; cb.ValueMember = "StaffID";
            }
        }

        // ➤ FIX: This now checks USERNAME (which is what Login sends) instead of FullName
        private int GetStaffID(string username)
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("SELECT StaffID FROM Staff WHERE Username=@u", c);
                cmd.Parameters.AddWithValue("@u", username);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetProductQty(int id)
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("SELECT Quantity FROM Products WHERE ProductID=@id", c);
                cmd.Parameters.AddWithValue("@id", id);
                object o = cmd.ExecuteScalar();
                return o != null ? Convert.ToInt32(o) : 0;
            }
        }

        private void LoadGRNTable(DataGridView grid)
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlDataAdapter da = new SqlDataAdapter(@"SELECT G.GRN_ID, P.ProductName, G.Quantity, S.FullName AS Staff, G.DateReceived FROM GRN G JOIN Products P ON G.ProductID=P.ProductID JOIN Staff S ON G.StaffID=S.StaffID ORDER BY G.GRN_ID DESC", c);
                DataTable dt = new DataTable(); da.Fill(dt);
                grid.DataSource = dt;
            }
        }

        private void LoadGINTable(DataGridView grid)
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlDataAdapter da = new SqlDataAdapter(@"SELECT G.GIN_ID, P.ProductName, G.Quantity, S.FullName AS Staff, G.DateIssued FROM GIN G JOIN Products P ON G.ProductID=P.ProductID JOIN Staff S ON G.StaffID=S.StaffID ORDER BY G.GIN_ID DESC", c);
                DataTable dt = new DataTable(); da.Fill(dt);
                grid.DataSource = dt;
            }
        }

        private void LoadProducts()
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Products", c);
                DataTable dt = new DataTable(); da.Fill(dt);
                dgvProducts.DataSource = dt;
                foreach (DataGridViewRow r in dgvProducts.Rows)
                {
                    int q = Convert.ToInt32(r.Cells["Quantity"].Value);
                    int re = Convert.ToInt32(r.Cells["ReorderLevel"].Value);
                    if (q <= re) r.DefaultCellStyle.BackColor = Color.LightCoral;
                }
            }
        }

        private void SearchProducts()
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Products WHERE ProductName LIKE @s", c);
                da.SelectCommand.Parameters.AddWithValue("@s", "%" + txtSearch.Text + "%");
                DataTable dt = new DataTable(); da.Fill(dt);
                dgvProducts.DataSource = dt;
            }
        }

        private void AddProduct()
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Products(ProductName,Category,Quantity,ReorderLevel,UnitPrice) VALUES(@n,@c,@q,@r,@p)", c);
                cmd.Parameters.AddWithValue("@n", txtName.Text); cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                cmd.Parameters.AddWithValue("@q", txtQty.Text); cmd.Parameters.AddWithValue("@r", txtReorder.Text);
                cmd.Parameters.AddWithValue("@p", txtPrice.Text);
                cmd.ExecuteNonQuery(); LoadProducts();
            }
        }

        private void UpdateProduct()
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Products SET ProductName=@n,Category=@c,Quantity=@q,ReorderLevel=@r,UnitPrice=@p WHERE ProductID=@id", c);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@n", txtName.Text); cmd.Parameters.AddWithValue("@c", txtCategory.Text);
                cmd.Parameters.AddWithValue("@q", txtQty.Text); cmd.Parameters.AddWithValue("@r", txtReorder.Text);
                cmd.Parameters.AddWithValue("@p", txtPrice.Text);
                cmd.ExecuteNonQuery(); LoadProducts();
            }
        }

        private void DeleteProduct()
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Products WHERE ProductID=@id", c);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery(); LoadProducts();
            }
        }

        private void GridClick(object s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            txtName.Text = dgvProducts["ProductName", e.RowIndex].Value.ToString();
            txtCategory.Text = dgvProducts["Category", e.RowIndex].Value.ToString();
            txtPrice.Text = dgvProducts["UnitPrice", e.RowIndex].Value.ToString();
            txtQty.Text = dgvProducts["Quantity", e.RowIndex].Value.ToString();
            txtReorder.Text = dgvProducts["ReorderLevel", e.RowIndex].Value.ToString();
        }

        private int GetTotalProducts() => Scalar("SELECT COUNT(*) FROM Products");
        private int GetLowStockItems() => Scalar("SELECT COUNT(*) FROM Products WHERE Quantity <= ReorderLevel");
        private int GetGRNThisMonth() => Scalar("SELECT COUNT(*) FROM GRN WHERE MONTH(DateReceived)=MONTH(GETDATE())");
        private int GetGINThisMonth() => Scalar("SELECT COUNT(*) FROM GIN WHERE MONTH(DateIssued)=MONTH(GETDATE())");

        private int Scalar(string sql)
        {
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open(); SqlCommand cmd = new SqlCommand(sql, c);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private DataTable GetRecentActivities()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Type"); dt.Columns.Add("Item"); dt.Columns.Add("Qty"); dt.Columns.Add("Date");
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlCommand a = new SqlCommand("SELECT TOP 5 'GRN', P.ProductName, G.Quantity, G.DateReceived FROM GRN G JOIN Products P ON G.ProductID=P.ProductID ORDER BY G.DateReceived DESC", c);
                SqlDataReader r = a.ExecuteReader(); while (r.Read()) dt.Rows.Add(r[0], r[1], r[2], r[3]); r.Close();
                a = new SqlCommand("SELECT TOP 5 'GIN', P.ProductName, G.Quantity, G.DateIssued FROM GIN G JOIN Products P ON G.ProductID=P.ProductID ORDER BY G.DateIssued DESC", c);
                r = a.ExecuteReader(); while (r.Read()) dt.Rows.Add(r[0], r[1], r[2], r[3]);
            }
            return dt;
        }

        private DataTable GetInventoryTrend()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Date", typeof(DateTime)); dt.Columns.Add("TotalQty", typeof(int));
            Dictionary<DateTime, int> map = new Dictionary<DateTime, int>();
            using (SqlConnection c = new SqlConnection(connectionString))
            {
                c.Open();
                SqlCommand cmd = new SqlCommand("SELECT DateReceived, Quantity FROM GRN UNION ALL SELECT DateIssued, -Quantity FROM GIN", c);
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    DateTime d = Convert.ToDateTime(r[0]);
                    int q = Convert.ToInt32(r[1]);
                    if (!map.ContainsKey(d)) map[d] = 0; map[d] += q;
                }
            }
            int sum = 0;
            foreach (var x in map.OrderBy(z => z.Key)) { sum += x.Value; dt.Rows.Add(x.Key, sum); }
            return dt;
        }

        private void Field(Panel p, string text, out TextBox box, int x, int y)
        {
            p.Controls.Add(new Label() { Text = text, Location = new Point(x, y), Font = new Font("Segoe UI", 10) });
            box = new TextBox() { Width = 250, Location = new Point(x, y + 22) };
            p.Controls.Add(box);
        }

        private void Btn(Panel p, string text, int x, int y, EventHandler click)
        {
            Button b = new Button() { Text = text, Width = 90, Height = 35, Location = new Point(x, y), BackColor = Color.Silver };
            b.Click += click;
            p.Controls.Add(b);
        }

        // KILL APP ON CLOSE
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}