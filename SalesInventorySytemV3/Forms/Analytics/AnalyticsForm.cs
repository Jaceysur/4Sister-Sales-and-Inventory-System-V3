using LiveCharts;
using LiveCharts.Wpf;
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

namespace SalesInventorySytemV3.Forms.Analytics
{
    public partial class AnalyticsForm : Form
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        public AnalyticsForm(IProductService productService, ISalesService salesService)
        {
            _productService = productService;
            _salesService = salesService;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            InitializeComponent();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time.Text = DateTime.Now.ToString();
        }

        private void Analytics_Load(object sender, EventArgs e)
        {
            SetupGrids();
            LoadAllCharts();
            day.Text = DateTime.Now.DayOfWeek.ToString();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllCharts();
        }
        private void LoadAllCharts(DateTime? from = null, DateTime? to = null)
        {
            var sales = _salesService.GetAll().ToList();

            if (from.HasValue && to.HasValue)
                sales = sales.Where(s => s.Date.Date >= from.Value.Date && s.Date.Date <= to.Value.Date).ToList();

            LoadPieChart(sales);
            LoadLineChart(sales);
            LoadSummaryTables(sales);
        }
        private void LoadPieChart(List<SalesInventorySytemV3.Models.Sale> sales)
        {
            pieChartSalesProducts.Series = new SeriesCollection();

            if (!sales.Any())
            {
                pieChartSalesProducts.Series.Add(new PieSeries
                {
                    Title = "No Sales",
                    Values = new ChartValues<double> { 1 }
                });
                return;
            }

            var categorySales = sales
                .SelectMany(s => s.Items)
                .GroupBy(i => i.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(i => (double)(i.Price * i.Quantity))
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            foreach (var item in categorySales)
            {
                pieChartSalesProducts.Series.Add(new PieSeries
                {
                    Title = item.Category,
                    Values = new ChartValues<double> { item.Total },
                    DataLabels = true
                });
            }
        }
        private double CalculateTotalProfit(List<SalesInventorySytemV3.Models.Sale> sales)
        {
            return sales
                .SelectMany(s => s.Items)
                .Sum(i => (double)((i.Price - i.CostPrice) * i.Quantity));
        }
        private void LoadLineChart(List<SalesInventorySytemV3.Models.Sale> sales)
        {
            double dailyTotal = sales.Where(s => s.Date.Date == DateTime.Today).Sum(s => (double)s.Total);
            double weeklyTotal = sales.Where(s => s.Date >= DateTime.Today.AddDays(-7)).Sum(s => (double)s.Total);
            double monthlyTotal = sales.Where(s => s.Date.Month == DateTime.Today.Month && s.Date.Year == DateTime.Today.Year).Sum(s => (double)s.Total);
            double yearlyTotal = sales.Where(s => s.Date.Year == DateTime.Today.Year).Sum(s => (double)s.Total);
            double totalOverall = sales.Sum(s => (double)s.Total);
            double totalProfit = CalculateTotalProfit(sales);

            string[] labels = { "Daily", "Weekly", "Monthly", "Yearly", "Total" };
            double[] salesValues = { dailyTotal, weeklyTotal, monthlyTotal, yearlyTotal, totalOverall };
            double[] profitValues = { totalProfit, totalProfit, totalProfit, totalProfit, totalProfit };

            chartSalesProfit.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Sales (₱)",
                    Values = new ChartValues<double>(salesValues),
                    PointGeometrySize = 8
                },
                new LineSeries
                {
                    Title = "Profit (₱)",
                    Values = new ChartValues<double>(profitValues),
                    StrokeDashArray = new System.Windows.Media.DoubleCollection { 4 },
                    PointGeometrySize = 6
                }
            };

            chartSalesProfit.AxisX.Clear();
            chartSalesProfit.AxisX.Add(new Axis
            {
                Title = "Period",
                Labels = labels.ToList()
            });

            chartSalesProfit.AxisY.Clear();
            chartSalesProfit.AxisY.Add(new Axis
            {
                Title = "Total (₱)"
            });
        }
        private void SetupGrids()
        {
            gridSalesProducts.AutoGenerateColumns = true;
            gridSalesProfits.AutoGenerateColumns = true;
        }

        private void LoadSummaryTables(List<SalesInventorySytemV3.Models.Sale> sales)
        {
            double daily = sales.Where(s => s.Date.Date == DateTime.Today).Sum(s => (double)s.Total);
            double weekly = sales.Where(s => s.Date >= DateTime.Today.AddDays(-7)).Sum(s => (double)s.Total);
            double monthly = sales.Where(s => s.Date.Month == DateTime.Today.Month && s.Date.Year == DateTime.Today.Year).Sum(s => (double)s.Total);
            double yearly = sales.Where(s => s.Date.Year == DateTime.Today.Year).Sum(s => (double)s.Total);
            double total = sales.Sum(s => (double)s.Total);

            var topProducts = sales
                .SelectMany(s => s.Items)
                .GroupBy(i => i.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalSales = $"₱{g.Sum(i => (double)(i.Price * i.Quantity)):F2}"
                })
                .OrderByDescending(x => double.Parse(x.TotalSales.Replace("₱", "")))
                .Take(5)
                .ToList();

            gridSalesProducts.DataSource = topProducts;

            gridSalesProfits.DataSource = new[]
            {
                new
                {
                    Daily = $"₱{daily:F2}",
                    Weekly = $"₱{weekly:F2}",
                    Monthly = $"₱{monthly:F2}",
                    Yearly = $"₱{yearly:F2}",
                    Total = $"₱{total:F2}"
                }
            };
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            LoadAllCharts(dateFrom.Value, dateTo.Value);
        }
    }
}
