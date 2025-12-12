using auth.Config;
using auth.Extensions;
using auth.Repositories;
using JwtAuth.Constants;
using JwtAuth.Database;
using JwtAuth.Database.Entity;
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

        void LogoutAll(UserJwtTokenInfo user);

        void LogoutLocally(UserJwtTokenInfo user, string deviceUuid);
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
                throw new ErrorException(ErrorCode.MISSING_USERNAME_OR_PASSWORD);

            return baseRepository.ExecuteTransaction(db =>
            {
                // 1. Lấy user
                var user = db.Users.FirstOrDefault(acc => acc.Username == request.Username)
                    ?? throw new ErrorException(ErrorCode.ACCOUNT_NOT_FOUND);

                if (user.Status == EntityStatus.Inactive)
                    throw new ErrorException(ErrorCode.ACCOUNT_NOT_ACTIVE);

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    throw new ErrorException(ErrorCode.USERNAME_OR_PASSWORD_INCORRECT);

                // 2. Xử lý Device logic
                string deviceUuid;
                var rqDevice = request.Device;

                if (string.IsNullOrEmpty(rqDevice.DeviceUuid)) //login from new device
                {
                    deviceUuid = Guid.NewGuid().ToString();

                    var newDevice = new Device
                    {
                        Uuid = deviceUuid.ToString(),
                        DeviceType = rqDevice.DeviceType,
                        DeviceName = rqDevice.DeviceName,
                        OS = rqDevice.OS,
                        Browser = rqDevice.Browser
                    };
                    db.Devices.Add(newDevice);

                    var userDevice = new UserDevice
                    {
                        UserUuid = user.Uuid,
                        DeviceUuid = newDevice.Uuid,
                        RefreshRootExpireAt = DateTime.UtcNow.AddSeconds(JwtToken.RefreshRootTokenLifetime)
                    };
                    db.UserDevices.Add(userDevice);

                    db.SaveChanges();
                }
                else // old device
                {
                    var existingDevice = db.Devices.FirstOrDefault(d => d.Uuid == rqDevice.DeviceUuid);

                    if (existingDevice == null)
                    {
                        // FE gửi DeviceUuid nhưng BE không có -> xem như thiết bị mới
                        deviceUuid = Guid.NewGuid().ToString();
                        var newDevice = new Device
                        {
                            Uuid = deviceUuid.ToString(),
                            DeviceType = rqDevice.DeviceType,
                            DeviceName = rqDevice.DeviceName,
                            OS = rqDevice.OS,
                            Browser = rqDevice.Browser
                        };
                        db.Devices.Add(newDevice);

                        var userDevice = new UserDevice
                        {
                            UserUuid = user.Uuid,
                            DeviceUuid = newDevice.Uuid,
                            RefreshRootExpireAt = DateTime.UtcNow.AddSeconds(JwtToken.RefreshRootTokenLifetime)
                        };
                        db.UserDevices.Add(userDevice);
                    }
                    else
                    {
                        // Thiết bị đã đăng ký -> lấy đúng UserDevice
                        var userDevice = db.UserDevices
                            .FirstOrDefault(ud => ud.UserUuid == user.Uuid && ud.DeviceUuid == existingDevice.Uuid);

                        // Nếu user từng dùng device này
                        if (userDevice == null)
                        {
                            // user chưa từng login device này -> tạo liên kết mới
                            userDevice = new UserDevice
                            {
                                UserUuid = user.Uuid,
                                DeviceUuid = existingDevice.Uuid,
                                RefreshRootExpireAt = DateTime.UtcNow.AddSeconds(JwtToken.RefreshRootTokenLifetime)
                            };
                            db.UserDevices.Add(userDevice);
                        }
                        else
                        {
                            // Cập nhật RefreshRootExpireAt nếu cần (reset khi login mới)
                            userDevice.RefreshRootExpireAt =
                                DateTime.UtcNow.AddSeconds(JwtToken.RefreshRootTokenLifetime);

                            db.UserDevices.Update(userDevice);
                        }

                        deviceUuid = existingDevice.Uuid;
                    }
                }

                // 3. Build token info
                var finalUserDevice = db.UserDevices
                    .First(ud => ud.UserUuid == user.Uuid && ud.DeviceUuid == deviceUuid.ToString());

                return new UserJwtTokenInfo
                {
                    UserUuid = user.Uuid,
                    UserName = user.FullName ?? "",
                    TokenTimes = user.TokenTimes,
                    AccountType = user.Type,
                    PhoneNumber = user.PhoneNumber ?? "",
                    DeviceUuid = deviceUuid.ToString(),
                    RefreshRootExpireAt = finalUserDevice.RefreshRootExpireAt
                };
            });
        }

        public JwtTokenResponse Login(LoginRequest request, JwtTokenAudience jwtTokenAudience)
        {
            var info = GetUserTokenInfoLogin(request);

            var db = DatabaseExtensions.GetDbContext<AppDbContext>();

            var claims = new List<KeyValuePair<string, object>>
                {
                    new("user_token_info", info.ToStringJson())
                };

            string audience = GlobalSettings.AppSettings.JwtAudience[(int)jwtTokenAudience];

            var accessToken = JwtManager.GenerateAccessToken(
                claims: claims, 
                audience: audience, 
                mustVerifySignature: true, 
                tokenTimes: info.TokenTimes);

            var refreshToken = JwtManager.GenerateRefreshToken(
                accessToken: accessToken, 
                userClaims: claims, 
                audience: audience, 
                mustVerifySignature: true, 
                refreshRootExpireAt: info.RefreshRootExpireAt,
                tokenTimes: info.TokenTimes);

            var hashedRefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);

            var userDevice = db.UserDevices
                  .First(x => x.UserUuid == info.UserUuid && x.DeviceUuid == info.DeviceUuid);

            userDevice.RefreshToken = hashedRefreshToken;
            userDevice.ExpiresAt = DateTime.Now.AddSeconds(JwtToken.RefreshTokenLifetime);

            db.SaveChanges();

            return new JwtTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessExpiresAt = DateTime.UtcNow.AddSeconds(JwtToken.AccessTokenLifetime),
                RefreshExpiresAt = DateTime.UtcNow.AddSeconds(JwtToken.RefreshTokenLifetime),
                RefreshRootExpireAt = info.RefreshRootExpireAt,
                DeviceUuid = info.DeviceUuid
            };
        }

        public void LogoutAll(UserJwtTokenInfo user)
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

        public void LogoutLocally(UserJwtTokenInfo user, string token)
        {
            baseRepository.ExecuteTransaction(db =>
            {
                var tokenClaims = JwtManager.ClaimTokens(token, false);

                var userDevice = db.UserDevices
                    .FirstOrDefault(ud => ud.UserUuid == user.UserUuid && ud.DeviceUuid == tokenClaims.DeviceUuid);
                if (userDevice != null)
                {
                    userDevice.IsRevoked = 1;
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
            //if (dbUser.RefreshRootExpireAt <= DateTime.UtcNow) 
            //    throw new ErrorException(ErrorCode.SESSION_EXPIRED, "Phiên đăng nhập hết hạn");

            var tokenTimes = dbUser.TokenTimes;
            JwtManager.ValidateToken(refreshToken, isRevoked: refreshClaim.IsRevoked, refreshClaim.Audience, JwtTokenType.Refresh, tokenTimes: tokenTimes);

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
