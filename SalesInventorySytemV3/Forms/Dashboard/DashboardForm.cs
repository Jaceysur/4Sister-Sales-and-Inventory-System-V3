using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SalesInventorySytemV3.Forms.Main;
using SalesInventorySytemV3.Services.Interfaces;

namespace SalesInventorySytemV3.Forms.Dashboard
{
    public partial class DashboardForm : Form
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        private DateTime _currentDate = DateTime.Now;
        int month, year;
        private UserControlDays _selectedDayControl;

        public DashboardForm(IProductService productService, ISalesService salesService)
        {
            _productService = productService;
            _salesService = salesService;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Dock = DockStyle.Fill;
            InitializeComponent();
            LoadDashboard();
            timer1.Start();
            DisplayDays();
        }

        public void LoadDashboard()
        {
            var today = DateTime.Now.Date;
            var todaySales = _salesService.GetAll().Where(s => s.Date.Date == today).ToList();
            var totalToday = todaySales.Sum(s => s.Total);
            var lowStock = _productService.GetAllActive().Count(p => p.Stock <= 10 && p.Stock > 0);
            var outOfStock = _productService.GetAllActive().Count(p => p.Stock == 0);

            lblTodaySales.Text = $"₱{totalToday:F2}";
            lblTotalProducts.Text = _productService.GetAllActive().Count().ToString();
            lblLowStock.Text = (lowStock + outOfStock).ToString();
            lblTodayOrders.Text = todaySales.Count.ToString();

            lvRecent.Items.Clear();

            var recent = _salesService.GetAll()
                .OrderBy(s => s.Date)
                .Take(5)
                .ToList();

            if (!recent.Any())
            {
                lvRecent.Items.Add(new ListViewItem(new[] { "-", "No transactions yet", "-" }));
            }
            else
            {
                foreach (var s in recent)
                {
                    var itemsText = string.Join(", ",
                        s.Items.Select(i => $"{i.Name}({i.Quantity})"));

                    var lvi = new ListViewItem(new[]
                    {
                s.Date.ToString("g"),
                itemsText,
                $"₱{s.Total:F2}"
            });

                    lvRecent.Items.Add(lvi);
                }
            }

        }

        private void lvRecent_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lblTodaySalesLabel_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime datetime = DateTime.Now;
            this.time.Text = datetime.ToString();
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            day.Text = System.DateTime.Now.DayOfWeek.ToString();
        }

        private void DisplayDays()
        {
            daycontainer.Controls.Clear();

            DateTime now = DateTime.Now;
            month = now.Month;
            year = now.Year;

            DateTime startOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);
            int dayOfWeek = (int)startOfMonth.DayOfWeek;

            DateTime prevMonth = _currentDate.AddMonths(-1);
            DateTime nextMonth = _currentDate.AddMonths(1);
            int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            lbMonthYear.Text = _currentDate.ToString("MMMM - yyyy", CultureInfo.InvariantCulture);

            for (int i = dayOfWeek ; i > 0; i--)
            {
                UserControlBlankCalendar prevDay = new UserControlBlankCalendar();
                int dayNum = daysInPrevMonth - i + 1;
                prevDay.days(dayNum);
                daycontainer.Controls.Add(prevDay);
            }

            for (int i = 1; i <= daysInMonth; i++)
            {
                UserControlDays ucdays = new UserControlDays();
                ucdays.days(i);
                daycontainer.Controls.Add(ucdays);
                ucdays.Cursor = Cursors.Hand;

                if (_currentDate.Year == DateTime.Now.Year &&
                    _currentDate.Month == DateTime.Now.Month &&
                    i == DateTime.Now.Day)
                {
                    ucdays.BackColor = Color.LightBlue;
                }

                ucdays.Click += (s, e) => SelectDay(ucdays);

                daycontainer.Controls.Add(ucdays);
            }

            int totalCells = daycontainer.Controls.Count;
            for (int i = 1; i <= 42 - totalCells; i++)
            {
                UserControlBlankCalendar nextDay = new UserControlBlankCalendar();
                nextDay.days(i);
                daycontainer.Controls.Add(nextDay);
            }
        }


        private void btnPrev_Click(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            DisplayDays();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            DisplayDays();
        }

        private void SelectDay(UserControlDays dayControl)
        {
            if (_selectedDayControl != null)
            {
                if (_selectedDayControl.BackColor == Color.LightBlue)
                    _selectedDayControl.BackColor = Color.White;
            }

            _selectedDayControl = dayControl;
            _selectedDayControl.BackColor = Color.LightBlue;

            int selectedDay = int.Parse(dayControl.DayText);
            DateTime selectedDate = new DateTime(_currentDate.Year, _currentDate.Month, selectedDay);
            Console.WriteLine($"Selected date: {selectedDate.ToShortDateString()}");
        }

    }
}