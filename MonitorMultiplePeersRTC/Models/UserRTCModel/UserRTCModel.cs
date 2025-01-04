using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MonitorMultiplePeersRTC.Data;

namespace MonitorMultiplePeersRTC.Models.UserRTCModel
{
    public class UserRTCModel
    {
        public int Id { get; set; }

        // User properties
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // Token will be set in the controller, no need to initialize it here
        public string? Token { get; set; } // Make Token nullable (optional)
        public bool IsLoggedIn { get; set; } = false;
        public string? my_unique_number { get; set; }

        // Method to generate a 32-character random token
        public string GenerateToken()
        {
            return GenerateRandomToken();
        }

        // Helper method to generate a random token
        private string GenerateRandomToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenBytes = new byte[24]; // 24 bytes will result in a 32-character base64 string
                rng.GetBytes(tokenBytes);

                // Generate the token and replace any '/' character with '_'
                string base64Token = Convert.ToBase64String(tokenBytes);

                // Replace characters to ensure URL safety and remove '/'
                string token = base64Token.Replace("/", "_").Replace("+", "-").Substring(0, 32);

                return token;
            }
        }
        public bool GetUniqueWord(string myword)
        {
            // Initialize the database context
            using (var dbContext = new ApplicationDbContext("Data Source=localhost;Initial Catalog=MonitorRTC;Integrated Security=True;Trust Server Certificate=True"))
            {
                // Check if the table has any rows
                bool hasRows = dbContext.Set<UserRTCModel>().Any();

                if (!hasRows)
                {
                    // If the table is empty, return true as the word cannot exist
                    return true;
                }

                // Check if the provided word exists in the my_unique_number column
                bool exists = dbContext.Set<UserRTCModel>().Any(u => u.my_unique_number == myword);

                // Return true if the word does not exist, false otherwise
                return !exists;
            }
        }

        // Method to generate a unique random number
        public string GenerateUniqueNumber()
        {
            string randomNumber = "";

            // Initialize the database context
            using (var dbContext = new ApplicationDbContext("Data Source=localhost;Initial Catalog=MonitorRTC;Integrated Security=True;Trust Server Certificate=True"))
            {
                // Check if the table has any rows
                bool hasRows = dbContext.Set<UserRTCModel>().Any();

                if (!hasRows)
                {
                    // If the table is empty, generate and return the first random number
                    randomNumber = GenerateRandomNumber();
                    return randomNumber;
                }

                bool isUnique = false;

                // Continue generating random numbers until a unique one is found
                while (!isUnique)
                {
                    randomNumber = GenerateRandomNumber();

                    // Check if the generated random number already exists in the database
                    isUnique = !dbContext.Set<UserRTCModel>().Any(u => u.my_unique_number == randomNumber);
                }
            }

            return randomNumber;
        }


        private string GenerateRandomNumber()
        {
            string randomNumber;

            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenBytes = new byte[5]; // 5 bytes will result in a 10-character number
                rng.GetBytes(tokenBytes);

                // Convert the byte array to a numeric value
                randomNumber = BitConverter.ToUInt32(tokenBytes, 0).ToString();

                // Ensure the number doesn't start with 0
                while (randomNumber.StartsWith("0"))
                {
                    rng.GetBytes(tokenBytes);
                    randomNumber = BitConverter.ToUInt32(tokenBytes, 0).ToString();
                }
            }

            return randomNumber;
        }

        // Placeholder for the unique number property (ensure this exists in your database schema)
    }
}
