namespace MonitorMultiplePeersRTC.Data
{
    public class Setter_Getter
    {
        // Private static nullable field to store the email
        private static string? _email;

        // Public static property to get and set the email with encapsulation
        public static string? Email
        {
            get { return _email; }
            set
            {
                // You can add validation logic here if necessary
                if (!string.IsNullOrEmpty(value) && value.Contains("@"))
                {
                    _email = value;
                }
                else
                {
                    // You could throw an exception or handle invalid email input
                    throw new ArgumentException("Invalid email address.");
                }
            }
        }
    }
}
