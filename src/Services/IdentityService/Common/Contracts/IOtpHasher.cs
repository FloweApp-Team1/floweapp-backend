namespace IdentityService.Common.Contracts
{
    public interface IOtpHasher
    {
        string Hash(string otp);

        bool Verify( string otp, string hash);
    }
}
