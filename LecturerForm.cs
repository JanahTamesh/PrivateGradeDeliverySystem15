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
    public partial class LecturerForm : Form
    {
        public LecturerForm()
        {
            InitializeComponent();
        }

        public LecturerForm(string userName) : this()
        {
            // تأكدي أن lblWelcome موجود في المصمم

        }
        private void LecturerForm_Load(object sender, EventArgs e)
        {

        }

    }
}
