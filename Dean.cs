using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Data.SqlClient;
//using Microsoft.Office.Interop.Excel;

namespace PrivateGradeDeliverySystem1
{
    public partial class Dean : Form
    {
        private DBconnection db;
        public Dean()
        {
            InitializeComponent();
            db = new DBconnection();
            BuildMenu();
        }

        private void BuildMenu()
        {
            // 1- Academic Years
            DataTable years = db.GetData("SELECT Id, Name FROM AcademicYear");
            foreach (DataRow year in years.Rows)
            {
                int yearId = Convert.ToInt32(year["Id"]);
                string yearName = year["Name"].ToString();

                ToolStripMenuItem yearMenu = new ToolStripMenuItem(yearName);

                // 2- Majors
                DataTable majors = db.GetData("SELECT Id, Name FROM Major");
                foreach (DataRow major in majors.Rows)
                {
                    int majorId = Convert.ToInt32(major["Id"]);
                    string majorName = major["Name"].ToString();

                    ToolStripMenuItem majorMenu = new ToolStripMenuItem(majorName);

                    // 3- Terms
                    DataTable terms = db.GetData($"SELECT Id, Name FROM Terms WHERE YearID = {yearId}");
                    foreach (DataRow term in terms.Rows)
                    {
                        int termId = Convert.ToInt32(term["Id"]);
                        string termName = term["Name"].ToString();

                        ToolStripMenuItem termMenu = new ToolStripMenuItem(termName);

                        // 4- Semesters
                        DataTable semesters = db.GetData($"SELECT Id, Name FROM Semesters WHERE TermID = {termId}");
                        foreach (DataRow sem in semesters.Rows)
                        {
                            int semId = Convert.ToInt32(sem["Id"]);
                            string semName = sem["Name"].ToString();

                            ToolStripMenuItem semMenu = new ToolStripMenuItem(semName);

                            // 5- Subjects (عن طريق SemesterSubject)
                            string subjectQuery = $@"
                                SELECT DISTINCT s.Id, s.Name
                                FROM Subject s
                                INNER JOIN SemesterSubject ss ON ss.SubjectID = s.Id
                                WHERE ss.SemesterID = {semId} AND s.MajorID = {majorId}";

                            DataTable subjects = db.GetData(subjectQuery);

                            foreach (DataRow sub in subjects.Rows)
                            {
                                int subId = Convert.ToInt32(sub["Id"]);
                                string subName = sub["Name"].ToString();

                                ToolStripMenuItem subjectMenu = new ToolStripMenuItem(subName);

                                // عند الضغط على المادة → عرض النتائج في DataGridView
                                subjectMenu.Click += (s, e) =>
                                {
                                    LoadStudentScores(semId, subId);
                                   
                                };

                                semMenu.DropDownItems.Add(subjectMenu);
                            }

                            termMenu.DropDownItems.Add(semMenu);
                        }

                        majorMenu.DropDownItems.Add(termMenu);
                    }

                    yearMenu.DropDownItems.Add(majorMenu);
                }
                accademicYearToolStripMenuItem.DropDownItems.Add(yearMenu);
                // هنا MenuStrip موجود مسبقًا عندك في الفورم باسم menuStrip1
               
            }
        }

        private void LoadStudentScores(int semesterId, int subjectId)
        {
            string query = @"
                SELECT st.Name AS StudentName, m.Score
                FROM Marks m
                INNER JOIN Students st ON st.Id = m.StudentID
                INNER JOIN SemesterSubject ss ON ss.Id = m.SemesterSubjectID
                WHERE m.SemesterID = @semId AND ss.SubjectID = @subId";

            var parameters = new Dictionary<string, object>
            {
                {"@semId", semesterId},
                {"@subId", subjectId}
            };

            DataTable dt = db.GetData(query, parameters);

           
            dataGridView1.DataSource = dt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void accademicYearToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string folderPath = @"C:\Student_Marks";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, "Student_Marks.pdf");

                Document document = new Document();
                PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                Paragraph paragraph = new Paragraph("The Student Marks\n\n");
                document.Add(paragraph);

                PdfPTable table = new PdfPTable(dataGridView1.Columns?.Count ?? 0);

                if (dataGridView1.Columns != null)
                {
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        table.AddCell(col.HeaderText);
                    }
                }

                if (dataGridView1.Rows != null)
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                table.AddCell(cell.Value?.ToString() ?? "");
                            }
                        }
                    }
                }

                document.Add(table);
                document.Close();
                MessageBox.Show("PDF File Saved successfully at: " + filePath);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                openFileDialog.Title = "Select Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string excelFilePath = openFileDialog.FileName;
                    Microsoft.Office.Interop.Excel.Workbook excelWorkbook = application.Workbooks.Open(excelFilePath);
                    Microsoft.Office.Interop.Excel.Worksheet excelWorksheet = excelWorkbook.Sheets[1];

                    DBconnection db = new DBconnection(); // محلي (لو تبغى أونلاين ضع true)
                    using (MySqlConnection connection = db.GetConnection())
                    {
                        connection.Open();

                        for (int row = 2; row <= excelWorksheet.UsedRange.Rows.Count; row++) // يبدأ من الصف 2 لأن 1 غالبًا العناوين
                        {
                            object nameValue = excelWorksheet.Cells[row, 1].Value;
                            object emailValue = excelWorksheet.Cells[row, 2].Value;
                            object phoneValue = excelWorksheet.Cells[row, 3].Value;

                            // تحقق أن الخلايا ليست فارغة
                            if (nameValue == null || emailValue == null || phoneValue == null)
                                continue;

                            string name = nameValue.ToString();
                            string email = emailValue.ToString();
                            string phone = phoneValue.ToString();

                            string sqlQuery = "INSERT INTO Lecturer (Name, Email, Phone) VALUES (@name, @email, @phone)";

                            using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                            {
                                command.Parameters.AddWithValue("@name", name);
                                command.Parameters.AddWithValue("@email", email);
                                command.Parameters.AddWithValue("@phone", phone);
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    excelWorkbook.Close();
                    application.Quit();
                    MessageBox.Show("Lecturer data imported successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing Lecturer data: " + ex.Message);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                openFileDialog.Title = "Select Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string excelFilePath = openFileDialog.FileName;
                    Microsoft.Office.Interop.Excel.Workbook excelWorkbook = application.Workbooks.Open(excelFilePath);
                    Microsoft.Office.Interop.Excel.Worksheet excelWorksheet = excelWorkbook.Sheets[1];

                    DBconnection db = new DBconnection(); // محلي (لو تبغى أونلاين حط true)
                    using (MySqlConnection connection = db.GetConnection())
                    {
                        connection.Open();

                        for (int row = 2; row <= excelWorksheet.UsedRange.Rows.Count; row++) // يبدأ من الصف 2 لأن الصف 1 للعناوين
                        {
                            object majorValue = excelWorksheet.Cells[row, 1].Value;
                            object nameValue = excelWorksheet.Cells[row, 2].Value;
                            object emailValue = excelWorksheet.Cells[row, 3].Value;
                            object phoneValue = excelWorksheet.Cells[row, 4].Value;

                            // التحقق أن الخلايا ليست فارغة
                            if (majorValue == null || nameValue == null || emailValue == null || phoneValue == null)
                                continue;

                            int majorId = Convert.ToInt32(majorValue);
                            string name = nameValue.ToString();
                            string email = emailValue.ToString();
                            string phone = phoneValue.ToString();

                            string sqlQuery = "INSERT INTO Students (MajorID, Name, Email, Phone) VALUES (@majorId, @name, @email, @phone)";

                            using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                            {
                                command.Parameters.AddWithValue("@majorId", majorId);
                                command.Parameters.AddWithValue("@name", name);
                                command.Parameters.AddWithValue("@email", email);
                                command.Parameters.AddWithValue("@phone", phone);
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    excelWorkbook.Close();
                    application.Quit();
                    MessageBox.Show("Student data imported successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing Student data: " + ex.Message);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                openFileDialog.Title = "Select Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string excelFilePath = openFileDialog.FileName;
                    Microsoft.Office.Interop.Excel.Workbook excelWorkbook = application.Workbooks.Open(excelFilePath);
                    Microsoft.Office.Interop.Excel.Worksheet excelWorksheet = excelWorkbook.Sheets[1];

                    DBconnection db = new DBconnection(); // محلي (لو تريد أونلاين ضع true)
                    using (MySqlConnection connection = db.GetConnection())
                    {
                        connection.Open();

                        for (int row = 2; row <= excelWorksheet.UsedRange.Rows.Count; row++) // الصف الأول للعناوين
                        {
                            object majorValue = excelWorksheet.Cells[row, 1].Value;
                            object nameValue = excelWorksheet.Cells[row, 2].Value;

                            // تحقق أن القيم ليست فارغة
                            if (majorValue == null || nameValue == null)
                                continue;

                            int majorId = Convert.ToInt32(majorValue);
                            string name = nameValue.ToString();

                            string sqlQuery = "INSERT INTO Subject (MajorID, Name) VALUES (@majorId, @name)";

                            using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                            {
                                command.Parameters.AddWithValue("@majorId", majorId);
                                command.Parameters.AddWithValue("@name", name);
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    excelWorkbook.Close();
                    application.Quit();
                    MessageBox.Show("Subject data imported successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing Subject data: " + ex.Message);
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            panel4.Visible = true;
            LoadAcademicYears();
            LoadMajors();
            LoadLecturers();
         


        }

        // تحميل السنوات الأكاديمية
        private void LoadAcademicYears()
        {
            string query = "SELECT Id, Name FROM academicyear";
            using (var conn = db.GetConnection())
            using (var da = new MySqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                comboBox1.DataSource = dt;
                comboBox1.DisplayMember = "Name";
                comboBox1.ValueMember = "Id";
            }
        }

        // تحميل التخصصات (Majors)
        private void LoadMajors()
        {
            string query = "SELECT Id, Name FROM major";
            using (var conn = db.GetConnection())
            using (var da = new MySqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                comboBox2.DataSource = dt;
                comboBox2.DisplayMember = "Name";
                comboBox2.ValueMember = "Id";
            }
        }

        // عند اختيار السنة -> تحميل الترم
        private void Loadterm(int yearId)
        {
           
            string query = "SELECT Id, Name FROM terms WHERE YearId = @YearId";
            using (var conn = db.GetConnection())
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@YearId", yearId);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                comboBox3.DataSource = dt;
                comboBox3.DisplayMember = "Name";
                comboBox3.ValueMember = "Id";
            }
        }

        // عند اختيار الترم -> تحميل السمستر
        private void Loadsemester(int termId)
        {
           
            string query = "SELECT Id, Name FROM semesters WHERE TermID = @TermId";
            using (var conn = db.GetConnection())
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TermId", termId);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                comboBox4.DataSource = dt;
                comboBox4.DisplayMember = "Name";
                comboBox4.ValueMember = "Id";
            }
        }

        // عند اختيار السمستر + التخصص -> تحميل المواد
        private void LoadSubjects(int majorId)
        {
            
            string query = @"SELECT Id, Name 
                     FROM subject
                     WHERE MajorID = @MajorId";
            using (var conn = db.GetConnection())
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MajorId", majorId);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                comboBox5.DataSource = dt;
                comboBox5.DisplayMember = "Name";
                comboBox5.ValueMember = "Id";
            }
        }

        // تحميل المحاضرين
        private void LoadLecturers()
        {
            string query = "SELECT Id, Name FROM lecturer";
            using (var conn = db.GetConnection())
            using (var da = new MySqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                comboBox6.DataSource = dt;
                comboBox6.DisplayMember = "Name";
                comboBox6.ValueMember = "Id";
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox4.SelectedValue != null && comboBox5.SelectedValue != null && comboBox6.SelectedValue != null)
                {
                    int semesterId = Convert.ToInt32(comboBox4.SelectedValue);
                    int subjectId = Convert.ToInt32(comboBox5.SelectedValue);
                    int lecturerId = Convert.ToInt32(comboBox6.SelectedValue);

                    string query = @"INSERT INTO semestersubject (Semesterid, SubjectID, Lecturerid) 
                         VALUES (@SemesterId, @SubjectId, @LecturerId)";
                    using (var conn = db.GetConnection())
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SemesterId", semesterId);
                        cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("Saved");
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null || comboBox1.SelectedValue is DataRowView)
                return;

            try
            {
                int yearId = Convert.ToInt32(comboBox1.SelectedValue);
                Loadterm(yearId);
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message);
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedValue == null || comboBox3.SelectedValue is DataRowView)
                return;

            try
            {
                int termId = Convert.ToInt32(comboBox3.SelectedValue);
                Loadsemester(termId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedValue == null || comboBox2.SelectedValue is DataRowView)
                return;

            try
            {
                int termId = Convert.ToInt32(comboBox2.SelectedValue);
                LoadSubjects(termId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
