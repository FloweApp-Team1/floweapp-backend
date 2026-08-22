using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AddressCartService.Infrastructure.Persistence
{
    public static class DbExceptionExtensions
    {
       
        private const int UniqueConstraintViolation = 2627;
        private const int UniqueIndexViolation = 2601;

        public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == UniqueConstraintViolation || sqlEx.Number == UniqueIndexViolation);
        }
    }
}
