using auth.Config;
using auth.Extensions;
using auth.Repositories;
using JwtAuth.Constants;
using JwtAuth.Database;
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

        void Logout(UserJwtTokenInfo user);
    }
    public class AuthService(
            IBaseRepository baseRepository
            //ILogger<AuthService> logger
        ) : IAuthService
    {
        public void InitAdminAccount()
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
                    .FirstOrDefault(acc => acc.Username == request.Username)
                    ?? throw new ErrorException(ErrorCode.ACCOUNT_NOT_FOUND);

                if (user.Status == EntityStatus.Inactive)
                {
                    throw new ErrorException(ErrorCode.ACCOUNT_NOT_ACTIVE, "Tài khoản chưa được kích hoạt.");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    throw new ErrorException(ErrorCode.USERNAME_OR_PASSWORD_INCORRECT);
                }

                user.RefreshRootExpireAt = DateTime.Now.AddSeconds(JwtToken.RefreshRootTokenLifetime);

                var userTokenInfo = new UserJwtTokenInfo
                {
                    UserUuid = user.Uuid,
                    UserName = user.FullName ?? "Unknown User",
                    TokenTimes = user.TokenTimes,
                    AccountType = user.Type,
                    PhoneNumber = user.PhoneNumber ?? "",
                    RefreshRootExpireAt = user.RefreshRootExpireAt
                };
                return userTokenInfo;
            });
        }

        public JwtTokenResponse Login(LoginRequest request, JwtTokenAudience jwtTokenAudience)
        {
            var userTokenInfo = GetUserTokenInfoLogin(request);
            List<KeyValuePair<string, object>> userTokenInfoClaims = [];
            var rootRefreshExpireAt = userTokenInfo.RefreshRootExpireAt;
            userTokenInfoClaims.AddRange([
                new KeyValuePair<string, object>("user_token_info", userTokenInfo.ToStringJson())
            ]);
            var accessToken = JwtManager.GenerateAccessToken(
                claims: userTokenInfoClaims, 
                audience: GlobalSettings.AppSettings.JwtAudience[(int)jwtTokenAudience], 
                mustVerifySignature: true, 
                tokenTimes: userTokenInfo.TokenTimes);
            
            var refreshToken = JwtManager.GenerateRefreshToken(
                accessToken: accessToken, 
                userClaims: userTokenInfoClaims, 
                audience: GlobalSettings.AppSettings.JwtAudience[(int)jwtTokenAudience], 
                refreshRootExpireAt: rootRefreshExpireAt,
                mustVerifySignature: true, 
                tokenTimes: userTokenInfo.TokenTimes);

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
                RefreshExpiresAt = token.RefreshExpiresAt,
                RefreshRootExpireAt = userTokenInfo.RefreshRootExpireAt
            };
        }

        public void Logout(UserJwtTokenInfo user)
        {
            baseRepository.ExecuteTransaction(db =>
            {
                var userEntity = db.Users.FirstOrDefault(u => u.Uuid == user.UserUuid);
                if (userEntity != null)
                {
                    userEntity.TokenTimes += 1;
                }
                return 0;
            });
        }

        public JwtTokenResponse RefreshToken(string refreshToken)
        {
            var db = DatabaseExtensions.GetDbContext<AppDbContext>();
            var refreshClaim = JwtManager.ClaimTokens(refreshToken, false);
            var dbUser = db.Users.First(u => u.Uuid == refreshClaim.UserUuid);
            if (refreshClaim.Type != JwtTokenType.Refresh)
            {
                throw new ErrorException(ErrorCode.INVALID_TOKEN, "Loại token không hợp lệ");
            }

            if (dbUser.RefreshRootExpireAt <= DateTime.UtcNow) 
                throw new ErrorException(ErrorCode.SESSION_EXPIRED, "Phiên đăng nhập hết hạn");
            var tokenTimes = dbUser.TokenTimes;
            JwtManager.ValidateToken(refreshToken, refreshClaim.Audience, JwtTokenType.Refresh, tokenTimes: tokenTimes);

            var userTokenInfo = JwtManager.ClaimUserTokenInfo(refreshToken, false)
                ?? throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Refresh token không hợp lệ");

            // Tạo mới access token và refresh token
            List<KeyValuePair<string, object>> userTokenInfoClaims = [];
            userTokenInfoClaims.AddRange([
                new KeyValuePair<string, object>("user_token_info", userTokenInfo.ToStringJson())
            ]);
            var newAccessToken = JwtManager.GenerateAccessToken(
                claims: userTokenInfoClaims, 
                audience: refreshClaim.Audience, 
                mustVerifySignature: true, 
                tokenTimes: userTokenInfo.TokenTimes);

            var newRefreshToken = JwtManager.GenerateRefreshToken(
                accessToken: newAccessToken, 
                userClaims: userTokenInfoClaims, 
                audience: refreshClaim.Audience, 
                mustVerifySignature: true, 
                refreshRootExpireAt: userTokenInfo.RefreshRootExpireAt, 
                tokenTimes: userTokenInfo.TokenTimes);

            return new JwtTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessExpiresAt = DateTime.Now.AddSeconds(JwtToken.AccessTokenLifetime),
                RefreshExpiresAt = DateTime.Now.AddSeconds(JwtToken.RefreshTokenLifetime),
                RefreshRootExpireAt = userTokenInfo.RefreshRootExpireAt
            };
        }
    }
}
