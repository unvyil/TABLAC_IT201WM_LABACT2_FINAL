using MySql.Data.MySqlClient;
using QRCoder;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace LostFoundQRSystem
{
    public partial class Form1 : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        int selectedItemID = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DefaultCellStyle.Font = new Font("Inter", 10);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Inter", 10, FontStyle.Regular);

            string guideText = "Welcome to the Smart Lost & Found System!\n\n" +
                       "1. Open XAMPP and click Start on MySQL to integrate the localhost to the WinForms.\n" +
                       "2. Click 'Add' to clear fields and get a new Item Code.\n" +
                       "3. Fill in the Item Details and Status.\n" +
                       "4. Click 'Generate QR Code' to create the tracking image.\n" +
                       "5. Click 'Save' to add the item to the database (Note: QR must be generated first).\n\n" +
                       "Use 'Search by QR' or the Search Bar to find specific items in the history.";

        MessageBox.Show(guideText, "System User Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
            UpdateDashboardCounters();
        }

        private void LoadData()
        {
            string query = "SELECT * FROM LostItems";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                        dataGridView1.RowTemplate.Height = 30;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
            }
        }

        private void UpdateDashboardCounters()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmdLost = new MySqlCommand("SELECT COUNT(*) FROM LostItems WHERE Status = 'Lost'", conn);
                    lbl_lostItems.Text = cmdLost.ExecuteScalar().ToString();

                    MySqlCommand cmdFound = new MySqlCommand("SELECT COUNT(*) FROM LostItems WHERE Status = 'Found'", conn);
                    lbl_foundItems.Text = cmdFound.ExecuteScalar().ToString();

                    MySqlCommand cmdClaimed = new MySqlCommand("SELECT COUNT(*) FROM LostItems WHERE Status = 'Claimed'", conn);
                    lbl_claimedItems.Text = cmdClaimed.ExecuteScalar().ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating dashboards: " + ex.Message);
                }
            }
        }

        private void ClearFormFields()
        {
            txtItemCode.Clear();
            txtItemName.Clear();
            txtDescription.Clear();
            txtLocFound.Clear();
            dateTimePicker1.Value = DateTime.Now;
            cbStatus.SelectedIndex = -1;
            txtQRItemCode.Clear();
            txtSearchBar.Clear();
            selectedItemID = 0;
        }

        private void GenerateNewItemCode()
        {
            txtItemCode.Text = "LF-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ClearFormFields();
            GenerateNewItemCode();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string query = "INSERT INTO LostItems (Item_Code, Item_Name, Description, Location_Found, Date_Reported, Status, QR_Code_Data) " +
                           "VALUES (@Code, @Name, @Desc, @Loc, @Date, @Status, @QR)";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", txtItemCode.Text);
                    cmd.Parameters.AddWithValue("@Name", txtItemName.Text);
                    cmd.Parameters.AddWithValue("@Desc", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@Loc", txtLocFound.Text);
                    cmd.Parameters.AddWithValue("@Date", dateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@Status", cbStatus.Text);
                    cmd.Parameters.AddWithValue("@QR", txtQRItemCode.Text);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();

                        LoadData();
                        UpdateDashboardCounters();
                        ClearFormFields();
                        MessageBox.Show("Successfully Saved!");

                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        private void btnLoadRec_Click(object sender, EventArgs e)
        {
            LoadData();
            UpdateDashboardCounters();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                selectedItemID = Convert.ToInt32(row.Cells["Item_ID"].Value);
                txtItemCode.Text = row.Cells["Item_Code"].Value.ToString();
                txtItemName.Text = row.Cells["Item_Name"].Value.ToString();
                txtDescription.Text = row.Cells["Description"].Value.ToString();
                txtLocFound.Text = row.Cells["Location_Found"].Value.ToString();
                dateTimePicker1.Value = Convert.ToDateTime(row.Cells["Date_Reported"].Value);
                cbStatus.Text = row.Cells["Status"].Value.ToString();
                txtQRItemCode.Text = row.Cells["QR_Code_Data"].Value.ToString();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedItemID == 0) return;

            string query = "UPDATE LostItems SET Item_Code=@ItemCode, Item_Name=@ItemName, Description=@Description, " +
                           "Location_Found=@LocFound, Date_Reported=@DateReported, Status=@Status, QR_Code_Data=@QRData " +
                           "WHERE Item_ID=@ItemID";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ItemID", selectedItemID);
                    cmd.Parameters.AddWithValue("@ItemCode", txtItemCode.Text);
                    cmd.Parameters.AddWithValue("@ItemName", txtItemName.Text);
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@LocFound", txtLocFound.Text);
                    cmd.Parameters.AddWithValue("@DateReported", dateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@Status", cbStatus.Text);
                    cmd.Parameters.AddWithValue("@QRData", txtQRItemCode.Text);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Successfully Updated!");

                        // Trigger the refresh
                        LoadData();
                        UpdateDashboardCounters();
                        ClearFormFields();
                    }
                    catch (Exception ex) { MessageBox.Show("Update Error: " + ex.Message); }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedItemID == 0) return;
            string query = "DELETE FROM LostItems WHERE Item_ID=@ItemID";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ItemID", selectedItemID);
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        LoadData();
                        UpdateDashboardCounters();
                        ClearFormFields();
                        MessageBox.Show("Successfully Deleted!");
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtItemCode.Text))
            {
                MessageBox.Show("Please generate an Item Code first!");
                return;
            }

            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtItemCode.Text, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        Bitmap qrCodeImage = qrCode.GetGraphic(5, Color.Black, Color.White, true);

                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox1.Image = qrCodeImage;

                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "PNG|*.png";
                        sfd.FileName = txtItemCode.Text + "_QR";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            qrCodeImage.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            MessageBox.Show("QR Saved!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("QR Generation Error: " + ex.Message);
            }
        }

        private void txtSearchBar_TextChanged(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT * FROM LostItems WHERE Item_Code LIKE @Search OR Item_Name LIKE @Search";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + txtSearchBar.Text + "%");
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }

        private void btnQRSearch_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT * FROM LostItems WHERE QR_Code_Data = @QRSearch OR Item_Code = @QRSearch";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@QRSearch", txtItemCode.Text);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dataGridView1.DataSource = dt;

                        if (dt.Rows.Count == 0)
                        {
                            MessageBox.Show("No item found with the code: " + txtItemCode.Text);
                            LoadData(); 
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Search Error: " + ex.Message); }
            }
        }
    }
}