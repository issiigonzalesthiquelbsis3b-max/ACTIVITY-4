using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Individual_Activity
{
    public partial class InventoryPage : Form
    {
        private int selectedProductID = 0;

        public InventoryPage()
        {
            InitializeComponent();

            btnAdd.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnClear.Click += btnClear_Click;

            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
           // txtSearch.TextChanged += txtSearch_TextChanged;
           dgvProducts.CellClick += dgvProducts_CellClick;

            LoadProducts(); 
            LoadInventoryStats();
           
            
        }

        private void LoadInventoryStats()
        {
            try
            {
                using (OleDbConnection con = Database.GetProductConnection())
                {
                    con.Open();

                    string query = @"
            SELECT
                COUNT(*) AS TotalProducts,
                SUM(IIF([Quantity] > [Reorder Level], 1, 0)) AS InStock,
                SUM(IIF([Quantity] > 0 AND [Quantity] <= [Reorder Level], 1, 0)) AS LowStock,
                SUM(IIF([Quantity] = 0, 1, 0)) AS OutOfStock
            FROM Products";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblTotalCount.Text =
                                    reader["TotalProducts"].ToString();

                                lblInStocks.Text =
                                    reader["InStock"] == DBNull.Value
                                        ? "0"
                                        : reader["InStock"].ToString();

                                lblLowStocks.Text =
                                    reader["LowStock"] == DBNull.Value
                                        ? "0"
                                        : reader["LowStock"].ToString();

                                lblOutofStocks.Text =
                                    reader["OutOfStock"] == DBNull.Value
                                        ? "0"
                                        : reader["OutOfStock"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Statistics Error");
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (OleDbConnection con = Database.GetProductConnection())
                {
                    con.Open();
                     
                    string query = @"SELECT * FROM Products ORDER BY ProductID DESC";
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProducts.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load Products Error: " + ex.Message);
            }
        }
        
        private bool ValidateProduct()
        {
            if (string.IsNullOrEmpty(txtProductName.Text))
            {
                MessageBox.Show("Please enter a Product Name.");
                txtProductName.Focus();
                return false;
            }
           

            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Please enter a valid Price.");
                txtPrice.Focus();
                return false;
            }
         

            int quantity;
            if (!int.TryParse(txtQuantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter a valid Quantity.");
                txtQuantity.Focus();
                return false;
            }
         

            int reorder;
            if (!int.TryParse(txtReorder.Text, out reorder) || reorder < 0)
            {
                MessageBox.Show("Please enter a valid Reorder Level.");
                txtReorder.Focus();
                return false;
            }
            return true;
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateProduct())
            {
                return;
            }

            try
            {
                using (OleDbConnection con = Database.GetProductConnection())
                {
                    con.Open();

                    string query = @"INSERT INTO Products ( [Product Name],[Category],[Product Price],[Quantity],[Reorder Level] ) VALUES ( ?, ?, ?, ?, ? )";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarChar).Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarChar).Value = cmbCategory.Text;
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = decimal.Parse(txtPrice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtQuantity.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtReorder.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product added successfully!");

                ClearField();
                LoadProducts();
                LoadInventoryStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product to update.");
                return;
            }

            if (!ValidateProduct())
            {
                return;
            }

            try
            {
                using (OleDbConnection con = Database.GetProductConnection())
                {
                    con.Open();

                    string query = @"UPDATE Products SET [Product Name] = ?, [Category] = ?,[Product Price] = ?, [Quantity] = ?, [Reorder Level] = ? WHERE ProductID = ?";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarChar).Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarChar).Value = cmbCategory.Text;
                        cmd.Parameters.Add("?", OleDbType.Currency).Value = decimal.Parse(txtPrice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtQuantity.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtReorder.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Product updated successfully!", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearField();
                            LoadProducts();
                            LoadInventoryStats();
                        }
                        else
                        {
                            MessageBox.Show("No product was updated.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Update Error",MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product to delete.");
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection con = Database.GetProductConnection())
                    {
                        con.Open();
                        string query = @"DELETE FROM Products WHERE ProductID=?";
                        using (OleDbCommand cmd = new OleDbCommand(query, con))
                        {
                            cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Product deleted successfully!");
                    ClearField();
                    LoadProducts();
                    LoadInventoryStats();
                }
                catch
                {
                    MessageBox.Show("Error deleting product.");
                }
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

                selectedProductID = Convert.ToInt32(row.Cells[0].Value);

                txtProductName.Text = row.Cells[1].Value.ToString();

                cmbCategory.Text = row.Cells[2].Value.ToString();

                txtPrice.Text = row.Cells[3].Value.ToString();

                txtQuantity.Text = row.Cells[4].Value.ToString();

                txtReorder.Text = row.Cells[5].Value.ToString();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearField();
        }

        private void ClearField()
        {
            txtProductName.Clear();
            cmbCategory.SelectedIndex = -1;
            txtPrice.Clear();
            txtQuantity.Clear();
            txtReorder.Clear();

            selectedProductID = 0;

            dgvProducts.ClearSelection();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Are you sure you want to logout?.", "Logout", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Hide();
        }



        private void InventoryPage_Load(object sender, EventArgs e)
        { 
          this.productsTableAdapter.Fill(this.pawfectItemsDataSet.Products);
        }



        private void label33_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void InventoryPage_Load_1(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'pawfectItemsDataSet1.Products' table. You can move, or remove it, as needed.
            this.productsTableAdapter1.Fill(this.pawfectItemsDataSet1.Products);
            // TODO: This line of code loads data into the 'pawfectItemsDataSet.Products' table. You can move, or remove it, as needed.
            this.productsTableAdapter.Fill(this.pawfectItemsDataSet.Products);

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
