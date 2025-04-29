using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using EkzamenADO.Models;

namespace EkzamenADO.DataAccess
{
    public class DbManager
    {
        private readonly string _connectionString;

        public DbManager()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public bool RegisterUser(User user, string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            byte[] passwordHash = new Rfc2898DeriveBytes(password, saltBytes, 100000).GetBytes(32);

            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @em", conn);
            checkCmd.Parameters.AddWithValue("@em", user.Email);
            int exists = (int)checkCmd.ExecuteScalar();
            if (exists > 0)
                return false;

            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Users (Name, Email, Phone, HashedPassword, Salt)
                VALUES (@name, @em, @ph, @hash, @salt)", conn);

            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@em", user.Email);
            cmd.Parameters.AddWithValue("@ph", user.Phone);
            cmd.Parameters.AddWithValue("@hash", passwordHash);
            cmd.Parameters.AddWithValue("@salt", saltBytes);

            cmd.ExecuteNonQuery();
            return true;
        }

        public User? Login(string email, string password)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @em", conn);
            cmd.Parameters.AddWithValue("@em", email);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            byte[] saltBytes = (byte[])reader["Salt"];
            byte[] storedHashBytes = (byte[])reader["HashedPassword"];

            byte[] inputHashBytes = new Rfc2898DeriveBytes(password, saltBytes, 100000).GetBytes(32);

            if (!CompareByteArrays(inputHashBytes, storedHashBytes))
                return null;

            return new User
            {
                Id = (int)reader["Id"],
                Name = reader["Name"].ToString()!,
                Email = reader["Email"].ToString()!,
                Phone = reader["Phone"].ToString()!
            };
        }

        private bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        public List<Category> GetAllCategories()
        {
            List<Category> list = new();
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Categories", conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString()!
                });
            }

            return list;
        }

        public void AddAd(Ad ad)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Ads (Title, Description, Price, CategoryId, ImageFileName, UserId, CreatedAt)
                VALUES (@t, @d, @p, @cat, @img, @uid, @c)", conn);

            cmd.Parameters.AddWithValue("@t", ad.Title);
            cmd.Parameters.AddWithValue("@d", ad.Description);
            cmd.Parameters.AddWithValue("@p", ad.Price);
            cmd.Parameters.AddWithValue("@cat", ad.CategoryId);
            cmd.Parameters.AddWithValue("@img", ad.ImageFileName ?? "");
            cmd.Parameters.AddWithValue("@uid", ad.UserId);
            cmd.Parameters.AddWithValue("@c", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

        public void UpdateAd(Ad ad)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand(@"
                UPDATE Ads SET 
                    Title = @title,
                    Description = @desc,
                    Price = @price,
                    CategoryId = @cat,
                    ImageFileName = @img
                WHERE Id = @id", conn);

            cmd.Parameters.AddWithValue("@title", ad.Title);
            cmd.Parameters.AddWithValue("@desc", ad.Description);
            cmd.Parameters.AddWithValue("@price", ad.Price);
            cmd.Parameters.AddWithValue("@cat", ad.CategoryId);
            cmd.Parameters.AddWithValue("@img", ad.ImageFileName ?? "");
            cmd.Parameters.AddWithValue("@id", ad.Id);

            cmd.ExecuteNonQuery();
        }

        public void DeleteAd(int adId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand("DELETE FROM Ads WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", adId);
            cmd.ExecuteNonQuery();
        }

        public Ad GetAdById(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Ads WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Ad
                {
                    Id = (int)reader["Id"],
                    Title = reader["Title"].ToString()!,
                    Description = reader["Description"].ToString()!,
                    Price = (decimal)reader["Price"],
                    CategoryId = (int)reader["CategoryId"],
                    ImageFileName = reader["ImageFileName"].ToString()!,
                    UserId = (int)reader["UserId"],
                    CreatedAt = (DateTime)reader["CreatedAt"]
                };
            }

            throw new Exception("Ad not found");
        }

        public List<AdWithCategory> GetAdsByUser(int userId)
        {
            List<AdWithCategory> list = new();
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand(@"
        SELECT Ads.Id, Ads.Title, Ads.Description, Ads.Price, Ads.ImageFileName, Categories.Name AS CategoryName
        FROM Ads
        JOIN Categories ON Ads.CategoryId = Categories.Id
        WHERE Ads.UserId = @uid", conn);

            cmd.Parameters.AddWithValue("@uid", userId);

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AdWithCategory
                {
                    Id = (int)reader["Id"],
                    Title = reader["Title"].ToString()!,
                    Description = reader["Description"].ToString()!,
                    Price = (decimal)reader["Price"],
                    ImageFileName = reader["ImageFileName"].ToString()!,   
                    CategoryName = reader["CategoryName"].ToString()!
                });
            }

            return list;
        }



        public void UpdateUser(User user)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand(@"
                UPDATE Users
                SET Name = @name, Phone = @phone
                WHERE Id = @id", conn);

            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@phone", user.Phone);
            cmd.Parameters.AddWithValue("@id", user.Id);

            cmd.ExecuteNonQuery();
        }

        public void DeleteUser(int userId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand deleteAds = new SqlCommand("DELETE FROM Ads WHERE UserId = @id", conn);
            deleteAds.Parameters.AddWithValue("@id", userId);
            deleteAds.ExecuteNonQuery();

            SqlCommand deleteUser = new SqlCommand("DELETE FROM Users WHERE Id = @id", conn);
            deleteUser.Parameters.AddWithValue("@id", userId);
            deleteUser.ExecuteNonQuery();
        }
        public List<AdWithCategory> GetAllAds()
        {
            List<AdWithCategory> list = new();
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand(@"
        SELECT Ads.*, Categories.Name AS CategoryName
        FROM Ads
        JOIN Categories ON Ads.CategoryId = Categories.Id", conn);

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AdWithCategory
                {
                    Id = (int)reader["Id"],
                    Title = reader["Title"].ToString()!,
                    Description = reader["Description"].ToString()!,
                    Price = (decimal)reader["Price"],
                    CategoryName = reader["CategoryName"].ToString()!,
                    ImageFileName = reader["ImageFileName"].ToString()!
                });
            }
            return list;
        }
        public List<AdWithCategory> GetAllAdsExceptUser(int userId)
        {
            List<AdWithCategory> list = new();
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            SqlCommand cmd = new SqlCommand(@"
        SELECT Ads.*, Categories.Name AS CategoryName
        FROM Ads
        JOIN Categories ON Ads.CategoryId = Categories.Id
        WHERE Ads.UserId <> @uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AdWithCategory
                {
                    Id = (int)reader["Id"],
                    Title = reader["Title"].ToString()!,
                    Description = reader["Description"].ToString()!,
                    Price = (decimal)reader["Price"],
                    CategoryName = reader["CategoryName"].ToString()!,
                    ImageFileName = reader["ImageFileName"].ToString()!
                });
            }

            return list;
        }

    }
}
