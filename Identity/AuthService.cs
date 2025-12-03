using auth.Config;
using auth.Repositories;
using JwtAuth.Constants;
using JwtAuth.ExceptionHandling;
using JwtAuth.Identity.Jwts;
using JwtAuth.Identity.Models;
using JwtAuth.Security.Jwts;
using JwtAuth.Utilities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Identity
{
    public interface IAuthService
    {
        void InitAdminAccount();

        JwtTokenResponse Login(LoginRequest request, JwtTokenAudience jwtTokenAudience);

        JwtTokenResponse RefreshToken(string refreshToken);

        void Logout(string accessToken);
    }
    public class AuthService(
            IBaseRepository baseRepository,
            ILogger<AuthService> logger
        ) : IAuthService
    {
        public void InitAdminAccount()
        {
            throw new NotImplementedException();
        }

        public JwtTokenResponse Login(LoginRequest request, JwtTokenAudience jwtTokenAudience)
        {
            var userTokenInfo = GetUserTokenInfoLogin(request);
            List<KeyValuePair<string, object>> userTokenInfoClaims = [];
            userTokenInfoClaims.AddRange([
                new KeyValuePair<string, object>("user_token_info", userTokenInfo.ToStringJson())
            ]);
            var accessToken = JwtManager.GenerateAccessToken(userTokenInfoClaims, GlobalSettings.AppSettings.JwtAudience[(int)jwtTokenAudience], true, userTokenInfo.TokenTimes);
            var refreshToken = JwtManager.GenerateRefreshToken(accessToken, userTokenInfoClaims, GlobalSettings.AppSettings.JwtAudience[(int)jwtTokenAudience],
                mustVerifySignature: true, tokenTimes: userTokenInfo.TokenTimes);
            var token = new JwtToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessExpiresAt = DateTime.Now.AddSeconds(JwtToken.AccessTokenLifetime),
                RefreshExpiresAt = DateTime.Now.AddSeconds(JwtToken.RefreshTokenLifetime)
            };
            return new JwtTokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                AccessExpiresAt = token.AccessExpiresAt,
                RefreshExpiresAt = token.RefreshExpiresAt
            };
        }

        public void Logout(string accessToken)
        {
            throw new NotImplementedException();
        }

        public JwtTokenResponse RefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }

        private UserJwtTokenInfo GetUserTokenInfoLogin(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                throw new ErrorException(ErrorCode.MISSING_USERNAME_OR_PASSWORD);
            }

            return baseRepository.ExecuteTransaction(db =>
            {
                var user = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(acc => acc.Username == request.Username);

                if (user == null)
                {
                    throw new ErrorException(ErrorCode.ACCOUNT_NOT_FOUND);
                }

                if (user.Status == EntityStatus.Inactive)
                {
                    throw new ErrorException(ErrorCode.ACCOUNT_NOT_ACTIVE, "Tài khoản chưa được kích hoạt.");
                }

                if (BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    throw new ErrorException(ErrorCode.USERNAME_OR_PASSWORD_INCORRECT);
                }

                var userTokenInfo = new UserJwtTokenInfo
                {
                    UserUuid = user.Uuid,
                    UserName = user.FullName ?? "Unknown User",
                    TokenTimes = user.TokenTimes,
                    AccountType = user.Type,
                    PhoneNumber = user.PhoneNumber ?? "",
                };
                return userTokenInfo;
            });
        }
    }
}
