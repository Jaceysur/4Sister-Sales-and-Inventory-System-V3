using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalesInventorySytemV3.Forms.Main
{
    public partial class UserControlBlankCalendar : UserControl
    {
        public string DayText => lbDays1.Text;
        public UserControlBlankCalendar()
        {
            InitializeComponent();
        }

        private void UserControlBlankCalendar_Load(object sender, EventArgs e)
        {

        }
        public void days(int numday)
        {
            lbDays1.Text = numday.ToString();
        }
    }
}