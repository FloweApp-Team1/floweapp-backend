namespace OrdersService.Infrastructure.Clients
{
   
        public class AuthorizationHeaderForwardingHandler : DelegatingHandler
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public AuthorizationHeaderForwardingHandler(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

                if (!string.IsNullOrWhiteSpace(authHeader))
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader);

                return base.SendAsync(request, cancellationToken);
            }
        }
}
