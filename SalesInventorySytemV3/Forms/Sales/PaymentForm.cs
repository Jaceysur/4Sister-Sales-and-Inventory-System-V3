using SalesInventorySytemV3.Models;
using SalesInventorySytemV3.Services.Implementations;
using SalesInventorySytemV3.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalesInventorySytemV3.Forms.Sales
{
    
    public partial class PaymentForm : Form
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        private readonly decimal _totalDue;
        private readonly ListView _lvItems;
        public Sale CompletedSale { get; private set; }
        public decimal Cash { get; private set; }
        public decimal Change { get; private set; }
        public PaymentForm(IProductService productService, ISalesService salesService, decimal totalAfterDiscount, ListView lvItem)
        {
            _productService = productService;
            _salesService = salesService;
            _totalDue = totalAfterDiscount;
            _lvItems = lvItem;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            InitializeComponent();
            
        }

        private void PaymentForm_Load(object sender, EventArgs e)
        {
            lblSubTotal.Text = $"₱{_totalDue:F2}";
            lblChange.Text = "₱0.00";

            cbPaymentMethod.Items.AddRange(new string[] { "Cash", "Card", "GCash" });
            cbPaymentMethod.SelectedIndex = 0;

            ToggleFieldsVisibility("Cash");
        }

        private void cbPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            string method = cbPaymentMethod.SelectedItem?.ToString() ?? "Cash";
           
                ToggleFieldsVisibility(method);
        }
        private void ToggleFieldsVisibility(string method)
        {
            bool isCash = method == "Cash";
            bool needsReference = method == "Card" || method == "GCash";

            if (needsReference)
            {
                gcashQR.Visible = true;
            } else
            {
                gcashQR.Visible = false;
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string paymentMethod = cbPaymentMethod.SelectedItem.ToString();
            if (paymentMethod == "Cash" && Cash < _totalDue)
            {
                MessageBox.Show("Insufficient cash amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sale = new Sale
            {
                Items = new List<SaleItem>(),
                PaymentMethod = paymentMethod,
                Total = _totalDue
            };

            foreach (ListViewItem li in _lvItems.Items)
            {
                var p = _productService.GetByName(li.SubItems[1].Text);
                int qty = int.Parse(li.SubItems[2].Text);

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
            CompletedSale = sale;

            MessageBox.Show($"Payment successful!\nTotal: ₱{_totalDue:F2}\nChange: ₱{Change:F2}",
                            "Transaction Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
