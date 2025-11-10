using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SalesInventorySytemV3.Models;
using SalesInventorySytemV3.Services.Interfaces;

namespace SalesInventorySytemV3.Forms.Sales
{
    public partial class SalesForm : Form
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        private decimal _subtotal = 0;
        private decimal _discountRate = 0;
        private decimal _cash = 0;



        public SalesForm(IProductService productService, ISalesService salesService)
        {
            _productService = productService;
            _salesService = salesService;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Dock = DockStyle.Fill;
            InitializeComponent();
            LoadProducts();
            timer1.Start();

        }

        private void LoadProducts()
        {

            cbProducts.Items.Clear();
            foreach (var p in _productService.GetAllActive().Where(p => p.Stock > 0))
            {
                cbProducts.Items.Add(new ComboBoxItem
                {
                    Text = $"{p.Name} - ₱{p.Price:F2} (Stock:{p.Stock})",
                    Value = p.Id
                });
            }
            if (cbProducts.Items.Count > 0)
                cbProducts.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cbProducts.SelectedItem == null)
            {
                MessageBox.Show("Select a product first.");
                return;
            }
            var item = (ComboBoxItem)cbProducts.SelectedItem;
            int productId = (int)item.Value;
            int qty = (int)numQty.Value;
            var product = _productService.GetById(productId);

            if (product.Stock < qty)
            {
                MessageBox.Show("Insufficient stock.");
                return;
            }
            // Check if product already in list
            foreach (ListViewItem li in lvItems.Items)
            {
                if (li.SubItems[1].Text == product.Name)
                {
                    int currentQty = int.Parse(li.SubItems[2].Text);
                    currentQty += qty;
                    li.SubItems[2].Text = currentQty.ToString();
                    li.SubItems[4].Text = $"₱{product.Price * currentQty:F2}";
                    UpdateTotal();
                    return;
                }
            }
            // Add new item
            int rowNumber = lvItems.Items.Count + 1;
            var newItem = new ListViewItem($"P-{rowNumber:000}");
            newItem.SubItems.Add(product.Name);
            newItem.SubItems.Add(qty.ToString());
            newItem.SubItems.Add($"₱{product.Price:F2}");
            newItem.SubItems.Add($"₱{product.Price * qty:F2}");
            lvItems.Items.Add(newItem);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            _subtotal = 0;
            foreach (ListViewItem li in lvItems.Items)
            {
                string t = li.SubItems[4].Text.Replace("₱", "");
                if (decimal.TryParse(t, out decimal v))
                    _subtotal += v;
            }
            ApplyDiscountAndRefreshUI();
        }

        private void btnComplete_Click(object sender, EventArgs e)
        {
            if (lvItems.Items.Count == 0)
            {
                MessageBox.Show("Add items first before completing the sale.");
                return;
            }

            if (!decimal.TryParse(txtCash.Text.Trim(), out _cash))
            {
                MessageBox.Show("Please enter valid cash amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal discountAmount = _subtotal * _discountRate;
            decimal totalAfterDiscount = _subtotal - discountAmount;

            if (_cash < totalAfterDiscount)
            {
                MessageBox.Show("Insufficient cash payment.");
                return;
            }

            decimal change = _cash - totalAfterDiscount;

            var sale = new Sale
            {
                Items = new List<SaleItem>(),
                PaymentMethod = "Cash",
                Total = totalAfterDiscount
            };

            try
            {
                foreach (ListViewItem li in lvItems.Items)
                {
                    string productName = li.SubItems[1].Text;
                    int qty = int.Parse(li.SubItems[2].Text);
                    var p = _productService.GetByName(productName);
                    if (p == null)
                        throw new InvalidOperationException($"Product '{productName}' not found.");

                    sale.Items.Add(new SaleItem
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        Quantity = qty,
                        Price = p.Price,
                        CostPrice = p.CostPrice
                    });

                    p.Stock -= qty;
                    _productService.Save(p);
                }

                _salesService.CreateSale(sale);

                MessageBox.Show(
                    $"Sale completed successfully!\n\nTotal: ₱{totalAfterDiscount:F2}\nChange: ₱{change:F2}",
                    "Transaction Done", MessageBoxButtons.OK, MessageBoxIcon.Information
                );

                DisplayReceipt(sale, _cash, change, discountAmount);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing sale: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ApplyDiscountAndRefreshUI()
        {
            decimal discountAmount = _subtotal * _discountRate;
            decimal totalAfterDiscount = _subtotal - discountAmount;

            lblSubTotal.Text = $"₱{_subtotal:F2}";
            lblDiscount.Text = $"₱{discountAmount:F2}";
            lblChange.Text = "₱0.00";
            lblAmount.Text = $"₱{Math.Max(totalAfterDiscount, 0):F2}";
        }
        private void cbDiscount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbDiscount.SelectedItem == null) return;

            string text = cbDiscount.SelectedItem.ToString().Replace("%", "");
            if (decimal.TryParse(text, out decimal rate))
                _discountRate = rate / 100;
            else
                _discountRate = 0;
            ApplyDiscountAndRefreshUI();
        }
        private void txtCash_TextChanged(object sender, EventArgs e)
        {

        }



        // helper
        private class ComboBoxItem { public string Text { get; set; } public object Value { get; set; } public override string ToString() => Text; }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime datetime = DateTime.Now;
            this.time.Text = datetime.ToString();
        }

        private void SalesForm_Load(object sender, EventArgs e)
        {
            day.Text = System.DateTime.Now.DayOfWeek.ToString();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lvItems.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select an item to remove.");
                return;
            }
            lvItems.Items.Remove(lvItems.SelectedItems[0]);
            UpdateTotal();
        }

        private void cbDiscount_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (cbDiscount.SelectedItem == null) return;

            string text = cbDiscount.SelectedItem.ToString().Replace("%", "");
            if (decimal.TryParse(text, out decimal rate))
                _discountRate = rate / 100;
            else
                _discountRate = 0;

            ApplyDiscountAndRefreshUI();
        }


        private void btnPay_Click(object sender, EventArgs e)
        {
            if (lvItems.Items.Count == 0)
            {
                MessageBox.Show("Add items before proceeding to payment.");
                return;
            }

            decimal discountAmount = _subtotal * _discountRate;
            decimal totalAfterDiscount = _subtotal - discountAmount;

            var paymentForm = new PaymentForm(_productService, _salesService, totalAfterDiscount, lvItems);
            paymentForm.ShowDialog();
        }


        private void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Receipt printed successfully!", "Print Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPreviewReceipt_Click(object sender, EventArgs e)
        {
            string preview = $"--- SALES RECEIPT PREVIEW ---\n\nItems:\n";

            foreach (ListViewItem li in lvItems.Items)
                preview += $"{li.SubItems[1].Text} x{li.SubItems[2].Text} = {li.SubItems[4].Text}\n";

            decimal discountAmount = _subtotal * _discountRate;
            decimal totalAfterDiscount = _subtotal - discountAmount;

            preview += $"\nSubtotal: ₱{_subtotal:F2}\nDiscount: ₱{discountAmount:F2}\n" +
                       $"Total: ₱{totalAfterDiscount:F2}\nCash: ₱{_cash:F2}\nChange: ₱{(_cash - totalAfterDiscount):F2}";

            MessageBox.Show(preview, "Receipt Preview");
        }
        private void DisplayReceipt(Sale sale, decimal cash, decimal change, decimal discountAmount)
        {
            lvReceipt.Items.Clear();

            foreach (var item in sale.Items)
            {
                var li = new ListViewItem(item.Name);
                li.SubItems.Add(item.Quantity.ToString());
                li.SubItems.Add($"₱{item.Price * item.Quantity:F2}");
                lvReceipt.Items.Add(li);
            }

            var sep = new ListViewItem("------------------------------");
            lvReceipt.Items.Add(sep);

            var subtotalItem = new ListViewItem("Subtotal");
            subtotalItem.SubItems.Add("");
            subtotalItem.SubItems.Add($"₱{_subtotal:F2}");
            lvReceipt.Items.Add(subtotalItem);

            var discountItem = new ListViewItem("Discount");
            discountItem.SubItems.Add("");
            discountItem.SubItems.Add($"₱{discountAmount:F2}");
            lvReceipt.Items.Add(discountItem);

            var totalItem = new ListViewItem("Total");
            totalItem.SubItems.Add("");
            totalItem.SubItems.Add($"₱{sale.Total:F2}");
            lvReceipt.Items.Add(totalItem);

            var cashItem = new ListViewItem("Cash");
            cashItem.SubItems.Add("");
            cashItem.SubItems.Add($"₱{cash:F2}");
            lvReceipt.Items.Add(cashItem);

            var changeItem = new ListViewItem("Change");
            changeItem.SubItems.Add("");
            changeItem.SubItems.Add($"₱{change:F2}");
            lvReceipt.Items.Add(changeItem);
        }


        private void txtCash_TextChanged_1(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtCash.Text.Trim(), out decimal cash))
            {
                decimal discountAmount = _subtotal * _discountRate;
                decimal totalAfterDiscount = _subtotal - discountAmount;

                if (cash >= totalAfterDiscount)
                {
                    lblChange.Text = $"₱{(cash - totalAfterDiscount):F2}";
                }
                else
                {
                    lblChange.Text = "₱0.00";
                }
            }
            else
            {
                lblChange.Text = "₱0.00";
            }
        }
    }
}
