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
    public partial class UserControlDays : UserControl
    {
        public string DayText => lbDays.Text;
        public UserControlDays()
        {
            InitializeComponent();
        }
    

    public void days(int numday)
        {
            lbDays.Text = numday.ToString();
        }
    }
}
