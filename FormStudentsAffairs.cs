using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PrivateGradeDeliverySystem1
{
    public partial class FormStudentsAffairs : Form
    {
        DBconnection db = new DBconnection(false); // true = use online, false = use local

        public FormStudentsAffairs()
        {
            InitializeComponent();

        }

        // ✅ التحقق من البريد الإلكتروني
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        // ✅ التحقق من رقم الهاتف (يبدأ بـ 7 + 9 أرقام)
        private bool IsValidPhone(string phone)
        {
            string pattern = @"^7\d{8}$";
            return Regex.IsMatch(phone, pattern);
        }
        // Load lecturers into combo box (for selecting dean)
        private void LoadDeans()
        {
            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    string query = "SELECT Id, Name FROM Lecturer";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboDean.DataSource = dt;
                    comboDean.DisplayMember = "Name";
                    comboDean.ValueMember = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to load deans: " + ex.Message);
            }


        }

        // Load colleges with dean name into DataGridView
        private void LoadColleges()
        {
            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    string query = @"SELECT c.Id, c.Name AS CollegeName, l.Name AS DeanName 
                                     FROM College c 
                                     LEFT JOIN Lecturer l ON c.DeanId = l.Id";

                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvColleges.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to load colleges: " + ex.Message);
            }
        }

        private void LoadCollegesCombo()
        {
            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT Id, Name FROM College";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader dr = cmd.ExecuteReader();

                    comboCollege.Items.Clear();
                    while (dr.Read())
                    {
                        comboCollege.Items.Add(new
                        {
                            Id = dr["Id"],
                            Name = dr["Name"].ToString()
                        });
                    }
                    comboCollege.DisplayMember = "Name";
                    comboCollege.ValueMember = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to load colleges: " + ex.Message);
            }
        }

        private void LoadMajors()
        {
            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    string query = @"SELECT m.Id, m.Name AS Major, c.Name AS College
                             FROM Major m
                             INNER JOIN College c ON m.CollegeID = c.Id";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvMajors.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to load majors: " + ex.Message);
            }
        }
        private void LoadLecture()
        {
            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    string query = "SELECT Id, Name, Email, Phone FROM Lecturer";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvDeans.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to load deans: " + ex.Message);
            }

        }



        private void BtnAddCollege_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCollegeName.Text))
            {
                MessageBox.Show("⚠ Please enter a college name.");
                return;
            }
            if (comboDean.SelectedIndex == -1)
            {

                MessageBox.Show("⚠ Please select a dean.");
                return;
            }

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "INSERT INTO College (Name, DeanId) VALUES (@Name,@DeanId)", conn);
                    cmd.Parameters.AddWithValue("@Name", txtCollegeName.Text.Trim());
                    cmd.Parameters.AddWithValue("@DeanId", comboDean.SelectedValue);
                    cmd.ExecuteNonQuery();
                }
                LoadColleges();
                txtCollegeName.Clear();
                comboDean.SelectedIndex = -1;
                MessageBox.Show("✅ College added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error adding college: " + ex.Message);
            }

        }

        private void BtnUpdateCollege_Click(object sender, EventArgs e)
        {
            if (dgvColleges.CurrentRow == null)
            {
                MessageBox.Show("⚠ Please select a college to update.");
                return;
            }

            int id = Convert.ToInt32(dgvColleges.CurrentRow.Cells["Id"].Value);

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "UPDATE College SET Name=@Name, DeanId=@DeanId WHERE Id=@Id", conn);
                    cmd.Parameters.AddWithValue("@Name", txtCollegeName.Text.Trim());
                    cmd.Parameters.AddWithValue("@DeanId", comboDean.SelectedValue);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
                LoadColleges();
                MessageBox.Show("✅ College updated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error updating college: " + ex.Message);
            }

        }

        private void BtnDeleteCollege_Click(object sender, EventArgs e)
        {
            if (dgvColleges.CurrentRow == null)
            {
                MessageBox.Show("⚠ Please select a college to delete.");
                return;
            }

            int id = Convert.ToInt32(dgvColleges.CurrentRow.Cells["Id"].Value);

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM College WHERE Id=@Id", conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
                LoadColleges();
                MessageBox.Show("✅ College deleted successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error deleting college: " + ex.Message);
            }

        }

        private void FormStudentsAffairs_Load(object sender, EventArgs e)
        {
            LoadDeans();
            LoadColleges();
            LoadCollegesCombo();
            LoadMajors();
            LoadLecture();
            LoadAcademicData();




        }

        private void DgvColleges_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtCollegeName.Text = dgvColleges.Rows[e.RowIndex].Cells["CollegeName"].Value.ToString();
                comboDean.Text = dgvColleges.Rows[e.RowIndex].Cells["DeanName"].Value.ToString();
            }

        }

        private void BtnAddMajor_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMajorName.Text))
            {
                MessageBox.Show("⚠️ Please enter a major name.");
                return;
            }
            if (comboCollege.SelectedIndex == -1)
            {
                MessageBox.Show("⚠️ Please select a college.");
                return;
            }

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Major (Name, CollegeID) VALUES (@Name, @CollegeID)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", txtMajorName.Text.Trim());
                    cmd.Parameters.AddWithValue("@CollegeID", ((dynamic)comboCollege.SelectedItem).Id);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Major added successfully.");
                LoadMajors();
                txtMajorName.Clear();
                comboCollege.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to add major: " + ex.Message);
            }

        }
        private int selectedMajorId = -1;
        private void DgvMajors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedMajorId = Convert.ToInt32(dgvMajors.Rows[e.RowIndex].Cells["Id"].Value);
                txtMajorName.Text = dgvMajors.Rows[e.RowIndex].Cells["Major"].Value.ToString();
                comboCollege.Text = dgvMajors.Rows[e.RowIndex].Cells["College"].Value.ToString();
            }

        }

        private void BtnUpdateMajor_Click(object sender, EventArgs e)
        {
            if (selectedMajorId == -1)
            {
                MessageBox.Show("⚠️ Please select a major to update.");
                return;
            }

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Major SET Name=@Name, CollegeID=@CollegeID WHERE Id=@Id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", txtMajorName.Text.Trim());
                    cmd.Parameters.AddWithValue("@CollegeID", ((dynamic)comboCollege.SelectedItem).Id);
                    cmd.Parameters.AddWithValue("@Id", selectedMajorId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Major updated successfully.");
                LoadMajors();
                txtMajorName.Clear();
                comboCollege.SelectedIndex = -1;
                selectedMajorId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to update major: " + ex.Message);
            }

        }

        private void BtnDeleteMajor_Click(object sender, EventArgs e)
        {
            if (selectedMajorId == -1)
            {
                MessageBox.Show("⚠️ Please select a major to delete.");
                return;
            }

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Major WHERE Id=@Id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", selectedMajorId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Major deleted successfully.");
                LoadMajors();
                txtMajorName.Clear();
                comboCollege.SelectedIndex = -1;
                selectedMajorId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Failed to delete major: " + ex.Message);
            }


        }

        private void BtnAddDean_Click(object sender, EventArgs e)
        {
            string name = txtDeanName.Text.Trim();
            string email = txtDeanEmail.Text.Trim();
            string phone = txtDeanPhone.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("⚠️ Please enter the dean's name.");
                return;
            }
            if (!IsValidEmail(email))
            {
                MessageBox.Show("⚠️ Invalid email format.");
                return;
            }
            if (!IsValidPhone(phone))
            {
                MessageBox.Show("⚠️ Invalid phone number. It must start with 7 and be 9 digits.");
                return;
            }

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Lecturer (Name, Email, Phone) VALUES (@Name, @Email, @Phone)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Dean added successfully.");
                LoadDeans();
                txtDeanName.Clear();
                txtDeanEmail.Clear();
                txtDeanPhone.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Failed to add dean: " + ex.Message);
            }

        }
        private int selectedDeanId = -1;

        private void DgvDeans_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedDeanId = Convert.ToInt32(dgvDeans.Rows[e.RowIndex].Cells["Id"].Value);
                txtDeanName.Text = dgvDeans.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                txtDeanEmail.Text = dgvDeans.Rows[e.RowIndex].Cells["Email"].Value.ToString();
                txtDeanPhone.Text = dgvDeans.Rows[e.RowIndex].Cells["Phone"].Value.ToString();
            }
        }

        private void BtnUpdateDean_Click(object sender, EventArgs e)
        {

            if (selectedDeanId == -1)
            {
                MessageBox.Show("⚠️ Please select a dean to update.");
                return;
            }

            if (!IsValidEmail(txtDeanEmail.Text.Trim()))
            {
                MessageBox.Show("⚠️ Invalid email format.");
                return;
            }
            if (!IsValidPhone(txtDeanPhone.Text.Trim()))
            {
                MessageBox.Show("⚠️ Invalid phone number.");
                return;
            }

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Lecturer SET Name=@Name, Email=@Email, Phone=@Phone WHERE Id=@Id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", txtDeanName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtDeanEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phone", txtDeanPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@Id", selectedDeanId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Dean updated successfully.");
                LoadDeans();
                selectedDeanId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Failed to update dean: " + ex.Message);
            }

        }

        private void BtnDeleteDean_Click(object sender, EventArgs e)
        {
            if (selectedDeanId == -1)
            {
                MessageBox.Show("⚠️ Please select a dean to delete.");
                return;
            }

            try
            {
                using (MySqlConnection conn = db.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Lecturer WHERE Id=@Id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", selectedDeanId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Dean deleted successfully.");
                LoadDeans();
                txtDeanName.Clear();
                txtDeanEmail.Clear();
                txtDeanPhone.Clear();
                selectedDeanId = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Failed to delete dean: " + ex.Message);
            }
        }

        private void BtnAddYear_Click(object sender, EventArgs e)
        {
            string yearName = txtYearName.Text.Trim();

            if (string.IsNullOrEmpty(yearName))
            {
                MessageBox.Show("⚠️ Please enter Academic Year (e.g., 2025/2026).");
                return;
            }

            if (!radioOdd.Checked && !radioEven.Checked)
            {
                MessageBox.Show("⚠️ Please select a Term type (Odd or Even).");
                return;
            }

            try
            {
                using (var conn = new DBconnection().GetConnection())
                {
                    conn.Open();
                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // ✅ إدخال السنة
                        string insertYear = "INSERT INTO AcademicYear (Name) VALUES (@YearName)";
                        MySqlCommand cmdYear = new MySqlCommand(insertYear, conn, transaction);
                        cmdYear.Parameters.AddWithValue("@YearName", yearName);
                        cmdYear.ExecuteNonQuery();
                        long yearId = cmdYear.LastInsertedId;

                        if (radioOdd.Checked)
                        {
                            // ترم فردي فقط
                            string insertTerm = "INSERT INTO Terms (Name, TermType, YearId) VALUES ('Term 1', 'Odd', @YearId)";
                            MySqlCommand cmdTerm = new MySqlCommand(insertTerm, conn, transaction);
                            cmdTerm.Parameters.AddWithValue("@YearId", yearId);
                            cmdTerm.ExecuteNonQuery();
                            long termId = cmdTerm.LastInsertedId;

                            // سمسترات فردية
                            int[] oddSemesters = { 1, 3, 5, 7 };
                            AddSemesters(conn, (int)termId, oddSemesters);
                        }
                        else if (radioEven.Checked)
                        {
                            // ترم زوجي فقط
                            string insertTerm = "INSERT INTO Terms (Name, TermType, YearId) VALUES ('Term 2', 'Even', @YearId)";
                            MySqlCommand cmdTerm = new MySqlCommand(insertTerm, conn, transaction);
                            cmdTerm.Parameters.AddWithValue("@YearId", yearId);
                            cmdTerm.ExecuteNonQuery();
                            long termId = cmdTerm.LastInsertedId;

                            // سمسترات زوجية
                            int[] evenSemesters = { 2, 4, 6, 8 };
                            AddSemesters(conn, (int)termId, evenSemesters);
                        }

                        // حفظ كل العمليات
                        transaction.Commit();

                        MessageBox.Show("✅ Academic Year and selected Term & Semesters added successfully.");
                        LoadAcademicData();
                        txtYearName.Clear();
                        radioOdd.Checked = false;
                        radioEven.Checked = false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("❌ Error adding year: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Connection error: " + ex.Message);
            }


        }
        // ✅ دالة لإضافة السمسترات
        private void AddSemesters(MySqlConnection conn, int termId, int[] semesters)
        {
            string querySemester = "INSERT INTO Semesters (TermId, Name) VALUES (@TermId, @SemesterName)";
            MySqlCommand cmdSemester = new MySqlCommand(querySemester, conn);

            foreach (int sem in semesters)
            {
                cmdSemester.Parameters.Clear();
                cmdSemester.Parameters.AddWithValue("@TermId", termId);
                cmdSemester.Parameters.AddWithValue("@SemesterName", "Semester " + sem);
                cmdSemester.ExecuteNonQuery();
            }
        }

        // ✅ تحميل البيانات لعرضها في DataGridView
        private void LoadAcademicData()
        {
            try
            {
                using (var conn = new DBconnection().GetConnection())
                {
                    conn.Open();

                    string query = @"
                SELECT 
                    ay.Id AS AcademicYearId,
                    ay.Name AS AcademicYear,
                    t.Id AS TermId,
                    t.Name AS TermName,
                    t.TermType,
                    s.Id AS SemesterId,
                    s.Name AS SemesterName
                FROM AcademicYear ay
                JOIN Terms t ON t.YearId = ay.Id
                JOIN Semesters s ON s.TermId = t.Id
                ORDER BY ay.Name, t.Id, s.Id;
            ";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvAcademicData.DataSource = dt;

                    // تحسين شكل الجدول
                    dgvAcademicData.Columns["AcademicYearId"].Visible = false;
                    dgvAcademicData.Columns["TermId"].Visible = false;
                    dgvAcademicData.Columns["SemesterId"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error loading data: " + ex.Message);
            }

        }

        private void BtnUpdateYear_Click(object sender, EventArgs e)
        {
            if (selectedYearId == -1)
            {
                MessageBox.Show("⚠️ Please select an Academic Year to update.");
                return;
            }

            string yearName = txtYearName.Text.Trim();
            if (string.IsNullOrEmpty(yearName))
            {
                MessageBox.Show("⚠️ Academic Year name cannot be empty.");
                return;
            }

            try
            {
                using (var conn = new DBconnection().GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE AcademicYear SET Name = @YearName WHERE Id = @YearId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@YearName", yearName);
                    cmd.Parameters.AddWithValue("@YearId", selectedYearId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("✅ Academic Year updated successfully.");
                    LoadAcademicData();
                    txtYearName.Clear();
                    selectedYearId = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error updating year: " + ex.Message);
            }

        }
        private int selectedYearId = -1;

        private void DgvAcademicData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedYearId = Convert.ToInt32(dgvAcademicData.Rows[e.RowIndex].Cells["AcademicYearId"].Value);
                txtYearName.Text = dgvAcademicData.Rows[e.RowIndex].Cells["AcademicYear"].Value.ToString();
            }

        }

        private void BtnDeleteYear_Click(object sender, EventArgs e)
        {
            if (selectedYearId == -1)
            {
                MessageBox.Show("⚠️ Please select an Academic Year to delete.");
                return;
            }

            DialogResult dr = MessageBox.Show("⚠️ Are you sure you want to delete this Academic Year and all related Terms & Semesters?",
                                              "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr == DialogResult.No) return;

            try
            {
                using (var conn = new DBconnection().GetConnection())
                {
                    conn.Open();
                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // 1. حذف السمسترات
                        string deleteSemesters = @"DELETE s 
                                           FROM Semesters s
                                           JOIN Terms t ON s.TermId = t.Id
                                           WHERE t.YearId = @YearId";
                        MySqlCommand cmdSem = new MySqlCommand(deleteSemesters, conn, transaction);
                        cmdSem.Parameters.AddWithValue("@YearId", selectedYearId);
                        cmdSem.ExecuteNonQuery();

                        // 2. حذف الترمات
                        string deleteTerms = "DELETE FROM Terms WHERE YearId = @YearId";
                        MySqlCommand cmdTerms = new MySqlCommand(deleteTerms, conn, transaction);
                        cmdTerms.Parameters.AddWithValue("@YearId", selectedYearId);
                        cmdTerms.ExecuteNonQuery();

                        // 3. حذف السنة
                        string deleteYear = "DELETE FROM AcademicYear WHERE Id = @YearId";
                        MySqlCommand cmdYear = new MySqlCommand(deleteYear, conn, transaction);
                        cmdYear.Parameters.AddWithValue("@YearId", selectedYearId);
                        cmdYear.ExecuteNonQuery();

                        transaction.Commit();

                        MessageBox.Show("✅ Academic Year and related Terms & Semesters deleted successfully.");
                        LoadAcademicData();
                        txtYearName.Clear();
                        selectedYearId = -1;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("❌ Error deleting year: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Connection error: " + ex.Message);
            }

        }

        
    }
}




