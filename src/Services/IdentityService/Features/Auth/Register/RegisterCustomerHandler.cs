using FluentValidation;
using IdentityService.Common.Contracts;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Responses;
using IdentityService.Common.Security;
using IdentityService.Common.Settings;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace IdentityService.Features.Auth.Register
{
    public class RegisterCustomerHandler : IRequestHandler<RegisterCustomerCommand, ApiResponse<RegisterCustomerResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        private readonly JwtSettings _jwtSettings;

        public RegisterCustomerHandler(IUnitOfWork unitOfWork , IPasswordHasher passwordHasher 
            , IJwtService jwtService  , IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _jwtSettings = jwtOptions.Value;

        }
        public async Task<ApiResponse<RegisterCustomerResponse>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
        { 
            var userRepo = _unitOfWork.Repository<User>();
            var emailTaken = await userRepo.FirstOrDefaultAsync(x=> x.Email == request.Email);
            if(emailTaken is not null)
                return ApiResponse<RegisterCustomerResponse>.Fail("Email already registered" , StatusCodes.Status409Conflict);
            var phoneTaken = await userRepo.FirstOrDefaultAsync(x=> x.PhoneNumber == request.Phone);
            if(phoneTaken is not null)
                return ApiResponse<RegisterCustomerResponse>.Fail("Phone number already registered" , StatusCodes.Status409Conflict);

            var customerRole = await _unitOfWork.Repository<Role>().FirstOrDefaultAsync(r => r.Name == AppRoles.Customer)
           ?? throw new ValidationException("Role 'CUSTOMER' not found in the database. Please ensure that the role exists before registering a customer.");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.Phone,
                Gender = request.Gender,
                PasswordHash = _passwordHasher.Hash(request.Password),
                FcmToken = request.FcmToken,
                NotifcationStatus = request.NotificationStatus,
                CreatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
            };

            customer.UserRoles.Add(new UserRole { UserId = customer.Id, RoleId = customerRole.Id });

            await userRepo.AddAsync(customer);

            var roleNames = new List<string> { customerRole.Name };
            var accessToken = _jwtService.GenerateAccessToken(customer, roleNames);
            var refreshTokenValue = _jwtService.GenerateRefreshTokenValue();

            var refreshToken = new RefreshToken
            {
                Token = _jwtService.HashRefreshTokenValue(refreshTokenValue),
                FamilyId = Guid.NewGuid(), // new login session — starts a fresh token family
                UserId = customer.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                
            };

            await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);


            var registerResponse = new RegisterCustomerResponse
            {
                User = new UserDto
                {
                    Id = customer.Id,
                    Name = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.PhoneNumber,
                    Gender = customer.Gender.ToString(),
                    NotificationStatus = customer.NotifcationStatus.ToString(),
                    CreatedAt = customer.CreatedAt,
                    Roles = roleNames,

                },
                Token = accessToken,
                RefreshToken = refreshTokenValue
            };

            return ApiResponse<RegisterCustomerResponse>.Created(registerResponse, "Customer registered successfully");



        }
    }
}
