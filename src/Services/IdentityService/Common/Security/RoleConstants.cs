namespace IdentityService.Common.Security
{

    public static class RoleConstants
    {
        public const string Admin = "Admin";
        public const string CustomerOnly = "CustomerOnly";
        public const string DriverOnly = "DriverOnly";           // any driver, regardless of application status
        public const string DriverApproved = "DriverApproved";   // driver AND applicationStatus == Approved
    }
}
