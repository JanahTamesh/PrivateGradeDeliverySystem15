using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Net.Mail;

namespace Private_deliver
{
    public partial class SendEmail : Form
    {
        public SendEmail()
        {
            InitializeComponent();
        }

        private void TxtToEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void SendEmail_Load(object sender, EventArgs e)
        {

        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("yourEmail@example.com"); // ضع بريدك هنا
                mail.To.Add(txtTo.Text);
                mail.Subject = txtSubject.Text;
                mail.Body = txtMessage.Text;

                if (!string.IsNullOrEmpty(txtFilePath.Text))
                {
                    Attachment attachment = new Attachment(txtFilePath.Text);
                    mail.Attachments.Add(attachment);
                }

                SmtpClient smtp = new SmtpClient("wlaamohammed60gmail.com", 587); // عدلي حسب سيرفرك
                smtp.Credentials = new System.Net.NetworkCredential("wlaamohammed60gmail.com", "dczx elvh oiuz zuqw"); // بريدك وكلمة المرور
                smtp.EnableSsl = true;

                smtp.Send(mail);
                MessageBox.Show("تم إرسال الرسالة بنجاح!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الإرسال: " + ex.Message);
            }
        }

        private void TxtBrowsettachment_TextChanged(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog1.FileName;
            }
        }

        private void BtnAttachFile_Click(object sender, EventArgs e)
        {
            // ننشئ أداة اسمها OpenFileDialog. وظيفتها تفتح نافذة اختيار الملفات.
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // هنا نحدد أنواع الملفات اللي ممكن المستخدم يختارها.
            // Filter معناها "تصفية".
            // "|" معناها "أو".
            // يعني: ملفات PDF أو ملفات Excel أو كل أنواع الملفات.
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*";

            // هنا نشوف إذا المستخدم اختار ملف و ضغط على "Open".
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // إذا اختار ملف، ناخذ مسار الملف (مكانه في الكمبيوتر).
                string filePath = openFileDialog.FileName;

                // الآن نعرض مسار الملف في صندوق النص اللي سويناه.
                // تأكدي أن اسم صندوق النص هو txtFilePath (اللي سميناه في الخطوة اللي فاتت).
                // في الصورة اللي أرسلتها، صندوق النص ما عنده اسم، فإذا حابة، سميه الآن.
                txtFilePath.Text = filePath;
            }
        }
    }
}
