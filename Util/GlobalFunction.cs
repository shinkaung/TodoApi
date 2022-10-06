using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Serilog;
using TodoApi.Models;

namespace TodoApi.Util
{
    public class Globalfunction
    {
        


        public static Claim[] GetClaims(TokenData obj)
        {
            var claims = new Claim[]
            {
                new Claim("UserID",obj.UserID),
                new Claim("LoginType",obj.LoginType),
                new Claim("UserLevelID", obj.UserLevelID),
                new Claim("isAdmin",obj.isAdmin.ToString()),
                new Claim("TicketExpireDate", obj.TicketExpireDate.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, obj.Sub),
                new Claim(JwtRegisteredClaimNames.Jti, obj.Jti),
                new Claim(JwtRegisteredClaimNames.Iat, obj.Iat, ClaimValueTypes.Integer64)
            };
            return claims;
        }

        public static TokenData GetTokenData(JwtSecurityToken tokenS)
        {
            var obj = new TokenData();
            try
            {
                obj.UserID = tokenS.Claims.First(claim => claim.Type == "UserID").Value;
                obj.LoginType = tokenS.Claims.First(claim => claim.Type == "LoginType").Value;
                obj.UserLevelID = tokenS.Claims.First(claim => claim.Type == "UserLevelID").Value;
                obj.isAdmin = Convert.ToBoolean(tokenS.Claims.First(claim => claim.Type == "isAdmin").Value);
                obj.Sub = tokenS.Claims.First(claim => claim.Type == "sub").Value;
                string TicketExpire = tokenS.Claims.First(claim => claim.Type == "TicketExpireDate").Value;
                DateTime TicketExpireDate = DateTime.Parse(TicketExpire);
                obj.TicketExpireDate = TicketExpireDate;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return obj;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}