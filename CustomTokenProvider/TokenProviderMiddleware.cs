using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using TodoApi.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using TodoApi.Models;
using TodoApi.Util;
using System.ComponentModel.DataAnnotations;
using Serilog;

namespace TodoApi.CustomTokenAuthProvider
{
    public class TokenProviderMiddleware : IMiddleware
    {
        private readonly IRepositoryWrapper _repository;
        private readonly TokenProviderOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        readonly int _minPasswordLength;
        readonly int _loginFailCount;
        private readonly double _tokenExpireMinute;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly SymmetricSecurityKey _tokenencKey;

        public TokenProviderMiddleware(IHttpContextAccessor httpContextAccessor, IRepositoryWrapper repository, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = repository;
            _configuration = configuration;

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            
            _minPasswordLength = int.Parse(_configuration.GetSection("appSettings:minPasswordLength").Value);
            _loginFailCount = int.Parse(_configuration.GetSection("appSettings:loginFailCount").Value);
            
            
            double expiretimespan = Convert.ToDouble(_configuration.GetSection("TokenAuthentication:TokenExpire").Value);
            TimeSpan expiration = TimeSpan.FromMinutes(expiretimespan);

            _tokenExpireMinute = int.Parse(_configuration.GetSection("TokenAuthentication:TokenExpire").Value);
            _tokenencKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("TokenAuthentication:TokenEncKey").Value));
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("TokenAuthentication:SecretKey").Value));
            _options = new TokenProviderOptions
            {
                Path = _configuration.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = _configuration.GetSection("TokenAuthentication:Audience").Value,
                Issuer = _configuration.GetSection("TokenAuthentication:Issuer").Value,
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256),
                Expiration = expiration
            };
        }
        public async Task ResponseMessage(dynamic data, HttpContext context, int code = 400)
        {
            var response = new
            {
               data.status,
               data.message
            };
            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
        }

        private async Task GenerateToken(HttpContext context)
        {
            LoginDataModel? loginData = new();
            string username = "";
            string password = "";
            string _loginType = "";

            try
            {
                using (var reader = new System.IO.StreamReader(context.Request.Body))
                {
                    var request_body = reader.ReadToEnd();
                    loginData = JsonConvert.DeserializeObject<LoginDataModel>(request_body, _serializerSettings);
                    if(loginData == null)
                        throw new Exception("Invalid login data");

                    if (loginData.UserName == null) loginData.UserName = "";
                    if (loginData.Password == null) loginData.Password = "";
                    if (loginData.LoginType == null) loginData.LoginType = "";
                    username = loginData.UserName;
                    password = loginData.Password;
                    _loginType = loginData.LoginType;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                await ResponseMessage(new { status = "fail", message = "The user name or password is incorrect." }, context, 400);
                return;
            }

            try {
                dynamic loginresult;
                string ipaddresslist = "";
                int AdminID;
                string AdminName;
                string AdminLevelName;
                int AdminLevelID;

                if (_loginType == "1")
                {
                    loginresult = await DoAdminTypeloginValidation(username, password);
                    if(loginresult.error == 0) {
                        loginresult = loginresult.data;
                        ipaddresslist = loginresult[0].RestrictIPList;
                        AdminID = loginresult[0].AdminID;
                        AdminName = loginresult[0].AdminName;
                        AdminLevelID = loginresult[0].AdminLevelID;
                        AdminLevelName = loginresult[0].AdminLevelName;
                    }
                    else {
                        await ResponseMessage(new { status = "fail", message = loginresult.message.ToString() }, context, 400);
                        return;
                    }
                }
                else
                {
                    await ResponseMessage(new { status = "fail", message = "Invalid login type" }, context, 400);
                    return;
                }

                var now = DateTime.UtcNow;
                var _tokenData = new TokenData
                {
                    Sub = AdminName,
                    Jti = await _options.NonceGenerator(),
                    Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(),
                    UserID = AdminID.ToString(),
                    UserName = AdminName,
                    UserLevelID = AdminLevelID.ToString(),
                    LoginType = _loginType.ToString(),
                    TicketExpireDate = now.Add(_options.Expiration)
                };
                var claims = Globalfunction.GetClaims(_tokenData);

                var appIdentity = new ClaimsIdentity(claims);
                context.User.AddIdentity(appIdentity); //add custom identity because default identity has delay to get data in EventLogRepository

                string encodedJwt = CreateEncryptedJWTToken(claims);

                var response = new
                {
                    AccessToken = encodedJwt,
                    ExpiresIn = (int)_options.Expiration.TotalSeconds,
                    UserID = AdminID.ToString(),
                    LoginType = _loginType,
                    UserLevelID = AdminLevelID,
                    UserLevelName = AdminLevelName,
                    DisplayName = AdminName,
                    PWDLength = _minPasswordLength.ToString()
                };
                
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
                Log.Information("Successful login for this account UserName : " + AdminName); 
            }
            catch(Exception ex) {
                Log.Error("Generate Token Fail: " + ex.Message); 
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Generate Token Fail");
            }
        }



        async Task<dynamic> DoAdminTypeloginValidation(string username, string password)
        {
            try {
                var result = (await _repository.Admin.GetAdminLoginValidation(username)).ToList();
                if (result.Count <= 0)
                    throw new ValidationException("Login User not found " + username);
                else if(result[0].Inactive)
                    throw new ValidationException("Login User is inactive " + username);
                else if(result[0].IsBlock)
                    throw new ValidationException("Login User is blocked " + username);

                Admin objAdmin = await _repository.Admin.FindByIDAsync(result[0].AdminID);
                if (objAdmin == null)
                    throw new ValidationException("Invalid Login User " + username);

                string oldhash = result[0].Password; 
                string oldsalt = result[0].Salt; 
                bool flag = SaltedHash.Verify(oldsalt, oldhash, password);
                if (flag == false)  //incorrect pwd
                {
                    //increase login_failure count                     
                    bool accLock = false;
                    var newfailcount = result[0].LoginFailCount + 1;

                    //change isblock to true if login_failure_count = 'Allow Login Failure Count' from setting table
                    if (newfailcount == _loginFailCount)
                    {
                        objAdmin.IsBlock = true;
                        accLock = true;
                    }

                    objAdmin.LoginFailCount = newfailcount;
                    await _repository.Admin.UpdateAsync(objAdmin);

                    if (accLock == true)
                        throw new Exception("Login failed limit reach for user account : " + username);
                    else
                        throw new Exception("Incorrect Login info for user account : " + username);
                }
                else
                {
                    //reset login_failure count
                    objAdmin.LoginFailCount = 0;
                    objAdmin.LastLoginDate = DateTime.Now;
                    await _repository.Admin.UpdateAsync(objAdmin);
                    return new {error = 0, data = result};
                }
            }
            catch (ValidationException vex)
            {
                Log.Error(vex.Message);
                return new {error = 1, message = vex.Message};
            }
            catch(Exception ex) {
                Log.Error(ex.Message);
                return new {error = 1, message = "Login Fail"};
            }
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.Headers.Add("server", "");  //added to hide server info for security

            //If public url no need to have token and authorization, add in below list
            if (
                context.Request.Path.ToString().ToLower().Contains("publicrequest/test")
            )
            {
                await next(context);
                return;
            }

            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                string newToken = await ReGenerateToken(context);
                if (newToken == "-1")
                {
                    await ResponseMessage(new { status = "fail", message = "Access Denied" }, context, 401);
                }
                else if (newToken == "-2")
                {
                    await ResponseMessage(new { status = "fail", message = "The Token has expired" }, context, 401);
                }
                else if (newToken == "-3")
                {
                    await ResponseMessage(new { status = "fail", message = "Access Token Invalid" }, context, 401);
                }
                else if (newToken == "-4")
                {
                    await ResponseMessage(new { status = "fail", message = "Force Logout" }, context, 401);
                }
                else if (newToken == "-5")
                {
                    await ResponseMessage(new { status = "fail", message = "Password Expire" }, context, 401);
                }
                else if (newToken == "-6")
                {
                    await ResponseMessage(new { status = "fail", message = "API Access Denied" }, context, 401);
                }
                else if (newToken != "")
                {
                    context.Response.Headers.Add("Access-Control-Expose-Headers", "newToken");
                    context.Response.Headers.Add("newToken", newToken);
                    await next(context);
                }
                else   // return blank
                {
                    await ResponseMessage(new { status = "fail", message = "Token not found" }, context, 401); 
                }
            }
            else if (context.Request.Path.ToString().Contains(_options.Path, StringComparison.Ordinal) && context.Request.Method == HttpMethods.Post)
            {
                // Employee Login Validation & Generate Token when valid.
                await GenerateToken(context);
            }
            else
            {
                await ResponseMessage(new { status = "fail", message = "Method Not Allowed." }, context, 405);
            }
        }

        public async Task<string> ReGenerateToken(HttpContext context)
        {
            try
            {
                string access_token = "";
                TokenData _tokenData;

                var hdtoken = context.Request.Headers["Authorization"];
                if (hdtoken.Count > 0)
                {
                    access_token = hdtoken[0];
                    access_token = access_token.Replace("Bearer ", "");
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        handler.ValidateToken(access_token,   //this will throw exception if token change or fake token.
                            new TokenValidationParameters  //this is necessary in both startup and here.
                            {
                                // The signing key must match!
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = _signingKey,
                                RequireSignedTokens = true,
                                // Validate the JWT Issuer (iss) claim
                                ValidateIssuer = true,
                                ValidIssuer = _options.Issuer,
                                // Validate the JWT Audience (aud) claim
                                ValidateAudience = false,
                                ValidAudience = _options.Audience,
                                // Validate the token expiry
                                ValidateLifetime = true,
                                // If you want to allow a certain amount of clock drift, set that here:
                                ClockSkew = TimeSpan.Zero,
                                TokenDecryptionKey = _tokenencKey
                            }, out SecurityToken tokenS);

                        var tokenJS = (JwtSecurityToken)tokenS;
                        if (tokenJS.SignatureAlgorithm != "A256KW")   //only allow new encryption alg A256KW
                            throw new Exception("Invalid Algorithm " + tokenJS.SignatureAlgorithm);

                        _tokenData = Globalfunction.GetTokenData(tokenJS);
                    }
                    catch (SecurityTokenExpiredException)
                    {
                        return "-2";  // token expired
                    }
                    catch (Exception)
                    {
                        return "-3";  // invalid access token
                    }
                }
                else
                {
                    return "";  //Token not found
                }
                
                //_tokenData.IPAddress = HttpContextExtensions.GetRemoteIPAddress(context).ToString();
                bool allow = false;

                var pathstr = context.Request.Path.ToString();
                
                string[] patharr = pathstr.Split('/');

                // add url which need to allow after login success without menu permission
                if (pathstr.ToLower().Contains("fileservice/profilephoto") || 
                    pathstr.ToLower().Contains("menu/getadminlevelmenudata"))
                {
                    allow = true; //Other File Functions default allow = false
                }
                else
                {
                    //allow = true;
                    string controllername = "";
                    string functionname = "";
                    string subfunctionname = "";
                    if(patharr.Length > 2)
                        controllername = patharr[2];
                    
                    if(patharr.Length > 3)
                        functionname = "/" + patharr[3];

                    if(patharr.Length > 4)
                        subfunctionname = "/" + patharr[4];
                    Log.Information(controllername);
                    Log.Information(functionname);
                    Log.Information(subfunctionname);
                    allow = await CheckURLPermission(_tokenData, controllername, functionname, subfunctionname);
                }
        
              
                if (allow)
                {
                    var userObj = await _repository.Admin.FindByIDAsync(Convert.ToInt32(_tokenData.UserID));
                    if (_tokenData.UserLevelID.Trim() != "")
                    {
                        var exp = DateTime.UtcNow;
                        var expires_in = exp.AddMinutes(_tokenExpireMinute).ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'");
                        var now = DateTime.UtcNow;
                        var _newtokenData = new TokenData()
                        {
                            Sub = userObj.AdminName,
                            Jti = await _options.NonceGenerator(),
                            Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(),
                            UserID = userObj.Id.ToString(),
                            UserLevelID = userObj.AdminLevelId.ToString(),
                            TicketExpireDate = now.Add(_options.Expiration),
                            LoginType = _tokenData.LoginType
                        };
                        var claims = Globalfunction.GetClaims(_newtokenData);
                        var appIdentity = new ClaimsIdentity(claims);
                        context.User.AddIdentity(appIdentity); //add custom identity because default identity has delay to get data in EventLogRepository

                        string newToken = CreateEncryptedJWTToken(claims);
                        return newToken;
                    }
                    else
                    {
                        return "-1";
                    }
                }
                else {
                    return "-6";  //API Access Denied
                }
            }
            catch (Exception)
            {
                return "-3";  //Invalid Access token
            }
        }

        
        async Task<bool> CheckURLPermission(TokenData obj, string controllername, string functionname, string subfunctioname)
        {
            try
            {
                var userlevelid = int.Parse(obj.UserLevelID);

                if (userlevelid != 0)
                {
                    var objAdminLevel = await _repository.AdminLevel.FindByIDAsync(userlevelid);

                    var isadmin = false;
                    // if (objAdminLevel != null)
                    // {
                    //     isadmin = objAdminLevel.IsAdministrator;
                    // }
                    // if (isadmin)
                    //     return true;
                    // else
                    // {
                    //     var checkMenuID = await _repository.AdminLevel.CheckAdminLevelAccess(
                    //             userlevelid,
                    //             controllername, 
                    //             controllername + functionname, 
                    //             controllername + functionname + subfunctioname
                    //         );
                    //     return checkMenuID;
                    // }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "checkURLPermission Fail.");
            }
            return false;
        }
        async Task<bool> CheckURLPermission(TokenData obj, string ServiceURL)
        {
            try
            {
                var userlevelid = int.Parse(obj.UserLevelID);

                if (userlevelid != 0)
                {
                    var objAdminLevel = await _repository.AdminLevel.FindByIDAsync(userlevelid);

                    var isadmin = false;
                    if (objAdminLevel != null)
                    {
                        isadmin = objAdminLevel.IsAdministrator;
                    }
                    if (isadmin)
                        return true;
                    else
                    {
                        var checkMenuID = await _repository.AdminLevel.CheckAdminLevelAccessURL(userlevelid, ServiceURL);
                        return checkMenuID;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "checkURLPermission Fail.");
            }
            return false;
        }

        private string CreateEncryptedJWTToken(Claim[] claims)
        {
            string encodedJwt = "";
            try
            {
                var now = DateTime.UtcNow;
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Audience = _options.Audience,
                    Issuer = _options.Issuer,
                    Subject = new ClaimsIdentity(claims),
                    NotBefore = now,
                    IssuedAt = Globalfunction.UnixTimeStampToDateTime(Int32.Parse(claims.First(claim => claim.Type == "iat").Value)),
                    Expires = now.Add(_options.Expiration),
                    SigningCredentials = _options.SigningCredentials,
                    EncryptingCredentials = new EncryptingCredentials(_tokenencKey, SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512)
                };
                var handler = new JwtSecurityTokenHandler();
                encodedJwt = handler.CreateEncodedJwt(tokenDescriptor);

            }
            catch (Exception ex)
            {
                Log.Error("CreateEncryptedJWTToken: " + ex.Message);
            }
            return encodedJwt;
        }
    }
}