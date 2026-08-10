namespace Shared.Security
{

        public static class AppPolicies
        {
            public const string AdminOnly = "AdminOnly";
            public const string CustomerOnly = "CustomerOnly";
            public const string DriverOnly = "DriverOnly";           // any driver, regardless of application status
            public const string DriverApproved = "DriverApproved";   // driver AND applicationStatus == Approved
        }
    }

