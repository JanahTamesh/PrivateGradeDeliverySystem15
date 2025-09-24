using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net.Mail;

namespace Private_deliver
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
    }
    class AuthService
    {
        private DBconnection db;
        // false => local, true => online (AlwaysData) حسب ما تريده
        public AuthService(bool useOnline = false)
        {
            db = new DBconnection(useOnline);
        }

        // ------------ LOGIN -------------
        public UserDto Login(string usernameOrEmail, string passwordHash, out string message)
        {
            message = null;
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    // COALESCE يتعامل مع وجود PasswordHash أو Password
                    string sql = @"SELECT Id, Name, Email, RoleID,
                              COALESCE(Password, Password) AS password_hash
                              FROM Users
                              WHERE Email = @u OR Name = @u
                              LIMIT 1;";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", usernameOrEmail);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                message = "Invalid username or password";
                                return null;
                            }

                            int id = Convert.ToInt32(reader["Id"]);
                            string name = reader["Name"].ToString();
                            string email = reader["Email"].ToString();
                            int roleId = reader["RoleID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleID"]);
                            string stored = reader["password_hash"] == DBNull.Value ? "" : reader["password_hash"].ToString();

                            reader.Close();

                            bool verified = false;
                            bool needsRehash = false;
                          
              if (!string.IsNullOrEmpty(stored) && stored.StartsWith("$2")) // bcrypt marker
                            {
                                verified = BCrypt.Net.BCrypt.Verify(passwordHash, stored);
                            }
                            else
                            {
                                // backward compatibility: إذا كانت كلمة السر مخزنة نصيًا (غير مشفّرة)
                                if (passwordHash == stored)
                                {
                                    verified = true;
                                    needsRehash = true; // نعيد تشفيرها الآن
                                }
                            }

                            if (!verified)
                            {
                                message = "Invalid username or password";
                                return null;
                            }

                            // if legacy plain password -> re-hash and update DB
                            if (needsRehash)
                            {
                                try
                                {
                                    string newHash = BCrypt.Net.BCrypt.HashPassword(passwordHash);
                                    string up = "UPDATE Users SET Password=@h WHERE Id=@id";
                                    using (var upcmd = new MySqlCommand(up, conn))
                                    {
                                        upcmd.Parameters.AddWithValue("@h", newHash);
                                        upcmd.Parameters.AddWithValue("@id", id);
                                        upcmd.ExecuteNonQuery();
                                    }
                                }
                                catch { /* ignore non-fatal */ }
                            }

                            return new UserDto { Id = id, Name = name, Email = email, RoleID = roleId };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
                return null;
            }
        }

        // ------------ GENERATE RESET CODE & SEND EMAIL -------------
        public bool SendResetCode(string email, out string outMessage)
        {
            outMessage = null;
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();

                    // get user
                    string q = "SELECT Id FROM Users WHERE Email = @e LIMIT 1";
                    int userId;
                    using (var cmd = new MySqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@e", email);
                        var r = cmd.ExecuteScalar();
                        if (r == null)
                        {
                            outMessage = "Email not found";
                            return false;
                        }
                        userId = Convert.ToInt32(r);
                    }

                    // generate code
                    string code = GenerateResetCode(6);
                    DateTime expires = DateTime.Now.AddMinutes(30);

                    // insert into password_resets
                    string ins = "INSERT INTO password_resets (user_id, reset_code, expires_at, used) VALUES (@uid, @code, @exp, 0)";
                    using (var icmd = new MySqlCommand(ins, conn))
                    {
                        icmd.Parameters.AddWithValue("@uid", userId);
                        icmd.Parameters.AddWithValue("@code", code);
                        icmd.Parameters.AddWithValue("@exp", expires);
                        icmd.ExecuteNonQuery();
                    }

                    // send email (تعديل بيانات SMTP أدناه)
                    if (!SendResetEmail(email, code, out string err))
                    {
                        outMessage = "Failed to send email: " + err;
                        return false;
                    }

                    outMessage = "Reset code sent to your email (expires in 30 min)";
                    return true;
                }
            }
            catch (Exception ex)
            {
                outMessage = "Error: " + ex.Message;
                return false;
            }
        }

// ------------ RESET PASSWORD -------------
    public bool ResetPassword(string email, string code, string newPassword, out string outMessage)
        {
            outMessage = null;
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    // user id
                    string q = "SELECT Id FROM Users WHERE Email = @e LIMIT 1";
                    object uidObj;
                    using (var cmd = new MySqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@e", email);
                        uidObj = cmd.ExecuteScalar();
                        if (uidObj == null)
                        {
                            outMessage = "Email not found";
                            return false;
                        }
                    }
                    int uid = Convert.ToInt32(uidObj);

                    // get latest reset record
                    string get = "SELECT id, expires_at, used FROM password_resets WHERE user_id=@uid AND reset_code=@code ORDER BY created_at DESC LIMIT 1";
                    int resetId = 0;
                    bool used = false;
                    DateTime expires = DateTime.MinValue;
                    using (var gcmd = new MySqlCommand(get, conn))
                    {
                        gcmd.Parameters.AddWithValue("@uid", uid);
                        gcmd.Parameters.AddWithValue("@code", code);
                        using (var reader = gcmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                outMessage = "Invalid reset code";
                                return false;
                            }
                            resetId = Convert.ToInt32(reader["id"]);
                            used = Convert.ToBoolean(reader["used"]);
                            expires = Convert.ToDateTime(reader["expires_at"]);
                            reader.Close();
                        }
                    }

                    if (used)
                    {
                        outMessage = "This reset code has already been used";
                        return false;
                    }
                    if (DateTime.Now > expires)
                    {
                        outMessage = "Reset code expired";
                        return false;
                    }

                    // update user's password (hashed)
                    string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    string up = "UPDATE Users SET PasswordHash=@h WHERE Id=@uid";
                    using (var upcmd = new MySqlCommand(up, conn))
                    {
                        upcmd.Parameters.AddWithValue("@h", newHash);
                        upcmd.Parameters.AddWithValue("@uid", uid);
                        upcmd.ExecuteNonQuery();
                    }

                    // mark reset as used
                    string mark = "UPDATE password_resets SET used=1 WHERE id=@rid";
                    using (var mcmd = new MySqlCommand(mark, conn))
                    {
                        mcmd.Parameters.AddWithValue("@rid", resetId);
                        mcmd.ExecuteNonQuery();
                    }

                    outMessage = "Password has been reset successfully";
                    return true;
                }
            }
            catch (Exception ex)
            {
                outMessage = "Error: " + ex.Message;
                return false;
            }
        }

        // --------- helpers ----------
        private string GenerateResetCode(int length = 6)
        {
            var rng = new Random();
            string s = "";
            for (int i = 0; i < length; i++) s += rng.Next(0, 10).ToString();
            return s;
        }

        private bool SendResetEmail(string toEmail, string code, out string error)
        {
            error = null;
            try
            {
                // * عدّل هذه القيم إلى بيانات SMTP الحقيقية لديك *
                var smtpHost = "smtp.example.com";
                var smtpPort = 587;
                var smtpUser = "your@email.com";
                var smtpPass = "your-email-password";
                var msg = new MailMessage();
                msg.From = new MailAddress(smtpUser, "Your App Name");
                msg.To.Add(toEmail);
                msg.Subject = "Password Reset Code";
                msg.IsBodyHtml = true;
                msg.Body = $"<p>Your password reset code: <b>{code}</b></p><p>It expires in 30 minutes.</p>";

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                    client.Send(msg);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
