using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrivateGradeDeliverySystem1
{
    public partial class LectuersForm : Form
    {
        private int lecturerId; // رقم المحاضر (من تسجيل الدخول)
        private DBconnection db; // كائن الاتصال
        public LectuersForm()
        {
            InitializeComponent();
            //this.lecturerId = lecturerId;
            db = new DBconnection(); // false = الاتصال المحلي (true = اونلاين)
            LoadSubjects();

        }
        private void LoadSubjects()
        {

            using (MySqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"SELECT s.Id, s.Name 
                                     FROM Subject s";
                    //INNER JOIN SemesterSubject ss ON s.Id = ss.SubjectID
                    //WHERE ss.LecturerId = @LecturerId";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    // cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBoxSubjects.DisplayMember = "Name"; // يظهر اسم المادة
                    comboBoxSubjects.ValueMember = "Id";     // القيمة = ID
                    comboBoxSubjects.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ خطأ في تحميل المواد: " + ex.Message);
                }
            }
        }

        private void LectuersForm_Load(object sender, EventArgs e)
        {


        }

        private void BtnOpenGrades_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            openFileDialog.Title = "Select Excel File";

            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string excelFilePath = openFileDialog.FileName;
                    Microsoft.Office.Interop.Excel.Workbook excelWorkbook = application.Workbooks.Open(excelFilePath);
                    Microsoft.Office.Interop.Excel.Worksheet excelWorksheet = excelWorkbook.Sheets[1];

                    using (MySqlConnection connection = db.GetConnection())
                    {
                        connection.Open();
                        

                        for (int row = 2; row <= excelWorksheet.UsedRange.Rows.Count; row++) // الصف الأول للعناوين
                        {
                            object studentid = excelWorksheet.Cells[row, 1].Value;
                            object Score = excelWorksheet.Cells[row, 2].Value;

                            // تحقق أن القيم ليست فارغة
                            if (studentid == null || Score == null)
                                continue;

                            int subid = Convert.ToInt32(studentid);
                            int score = Convert.ToInt32(Score);

                            string sqlQuery = "INSERT INTO marks  (StudentID, Score,SemesterId,SemesterSubjectID) VALUES (@stuid, @score,1,1)";

                            using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                            {
                                command.Parameters.AddWithValue("@stuid", studentid);
                                command.Parameters.AddWithValue("@score", Score);
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    excelWorkbook.Close();
                    application.Quit();
                    MessageBox.Show("data saved");
                }
            }
             catch (Exception ex)
            {
                MessageBox.Show( ex.Message);
            }

        }

    }

}



