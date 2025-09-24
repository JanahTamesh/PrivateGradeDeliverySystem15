using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Private_deliver
{
    public partial class StudentForm : Form
    {
        public StudentForm()
        {
            InitializeComponent();
        }
        public StudentForm(string userName) : this()
        {
            // تأكدي أن lblWelcome موجود في المصمم

        }
        private void StudentForm_Load(object sender, EventArgs e)
        {

        }
    }
}
