namespace BLL.Encryption;


public class PassHash
{
    private static string pepper = "PazarPepper";
    public static string GetRandomSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt(12);
    }
    public static string HashPassword(string password)
    {
        string PassWPepper = password + pepper;
        return BCrypt.Net.BCrypt.HashPassword(PassWPepper, GetRandomSalt());
    }
    public static bool ValidatePassword(string password, string correctHash)
    {
        {
            var verify = BCrypt.Net.BCrypt.Verify(password + pepper, correctHash);
            return verify;

        }
    }
}
