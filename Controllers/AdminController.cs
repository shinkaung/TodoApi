using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Configuration;
using TodoApi.Repositories;
using Kendo.Mvc.UI;
using System.ComponentModel.DataAnnotations;
using TodoApi.Models;
using TodoApi.Payloads;
using TodoApi.Util;
using Serilog;

namespace TodoApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : BaseController
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly int _minPasswordLength;
        private static readonly IConfiguration _configuration = Startup.StaticConfiguration!;

        public AdminController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
            _minPasswordLength = int.Parse(_configuration.GetSection("PasswordPolicy:MinPasswordLength").Value);
        }

        [HttpPost("GetAdminList", Name = "GetAdminList")]
        public async Task<dynamic> GetAdminList([DataSourceRequest]DataSourceRequest request)
        {
            DataSourceResult dsmainQuery = await _repositoryWrapper.Admin.GetAdmins(request); // admin table + state 
            return dsmainQuery;  //Note kendo datasource fetchgridpostJson need data is capital Data
        }

        [HttpGet("GetAdminListByLevel/{level}", Name = "GetAdminListByLevel")]
        public async Task<dynamic> GetAdminListByLevel(int level)
        {
            dynamic objresult = await _repositoryWrapper.Admin.GetAdminListByLevel(level);
            dynamic objresponse = new { data = objresult };
            return objresponse;
        }

        [HttpGet("GetAdminComboData", Name = "GetAdminComboData")]
        public async Task<dynamic> GetAdminComboData()
        {
            var adminLevel = await _repositoryWrapper.AdminLevel.FindAllAsync();
            dynamic objresponse = new { adminLevel };
            return objresponse;
        }

        [HttpPost("UpdateAdminSetup", Name = "UpdateAdminSetup")]
        public async Task<dynamic> UpdateAdminSetup(EditAdmin admin)
        {
            try {
                
                if(admin.AdminID == 0) {
                    throw new ValidationException("Invalid Parameter");
                }

                int AdminID = admin.AdminID;
                Admin objAdmin = _repositoryWrapper.Admin.FindByID(AdminID);
                if (objAdmin == null)  //edit admin
                    throw new ValidationException("Invalid Admin ID");
                
                string dupmessage = "";
                if(await _repositoryWrapper.Admin.CheckDuplicateAdminName(admin.AdminID, admin.AdminName))
                    dupmessage += " Name is duplicated.";

                if(await _repositoryWrapper.Admin.CheckDuplicateAdminLoginName(admin.AdminID, admin.LoginName))
                    dupmessage += " Login Name is duplicated.";


                if(dupmessage != "")//duplicated
                {
                    throw new ValidationException(dupmessage);
                }
                else 
                {
                    objAdmin.AdminName = admin.AdminName;
                    objAdmin.AdminLevelId = admin.AdminLevelID;
                    objAdmin.ModifiedDate = System.DateTime.Now;
                    objAdmin.LoginName = admin.LoginName;
                    objAdmin.Email = admin.Email;
                    Validator.ValidateObject(objAdmin, new ValidationContext(objAdmin), true);
                    
                    if(admin.AdminPhoto!= null && admin.AdminPhoto != "")
                    {
                        FileService.DeleteFileNameOnly("AdminPhoto", admin.AdminID.ToString());
                        FileService.MoveTempFile("AdminPhoto", admin.AdminID.ToString(), admin.AdminPhoto.ToString());
                    }
                    _repositoryWrapper.Admin.Update(objAdmin);
                    return new { data = admin.AdminID };
                }
            }      
            catch (ValidationException vex)
            {
                Log.Error("Admin Controller/ UpdateAdminSetup" + vex.Message);
                return new { data = 0, error = vex.ValidationResult.ErrorMessage };
            }
            catch (Exception ex)
            {
                Log.Error("Admin Controller/ UpdateAdminSetup" + ex.Message);
                return new { data = 0, error = "Update Admin Fail." };
            }
        }

        [HttpPost("AddAdminSetup", Name = "AddAdminSetup")]
        public async Task<dynamic> AddAdminSetup(AddAdmin admin)  //[FromBody] will be needed if you do not include .AddMvc in Startup.cs adn [ApiController] in class definition
        {
            try {
                int AdminID = 0;
               
                string dupmessage = "";
                if(await _repositoryWrapper.Admin.CheckDuplicateAdminName(0, admin.AdminName))
                    dupmessage += " Name is duplicated.";

                if(await _repositoryWrapper.Admin.CheckDuplicateAdminLoginName(0, admin.LoginName))
                    dupmessage += " Login Name is duplicated.";

                if(dupmessage != "")//duplicated
                {
                    throw new ValidationException(dupmessage);
                }
                else {
                    var newobj = new Admin
                    {
                        AdminName = admin.AdminName,
                        AdminLevelId = admin.AdminLevelID,
                        LoginName = admin.LoginName,
                        Email = admin.Email,
                        Inactive = false,
                        IsBlock = false,
                        CreateDate = System.DateTime.Now,
                        ModifiedDate = System.DateTime.Now
                    };
                    var password = admin.Password;
                    
                    if (password.ToString().Length < _minPasswordLength)
                    {
                        throw new ValidationException("Invalid Password");
                    }
                    else
                    {
                        string salt = Util.SaltedHash.GenerateSalt();
                        password = Util.SaltedHash.ComputeHash(salt, password.ToString());
                        newobj.Password = password;
                        newobj.Salt = salt;
                        Validator.ValidateObject(newobj, new ValidationContext(newobj), true);
                        _repositoryWrapper.Admin.Create(newobj);
                        AdminID = newobj.Id;

                        if(admin.AdminPhoto != null && admin.AdminPhoto != "")
                        {
                            FileService.MoveTempFile("AdminPhoto", newobj.Id.ToString(), admin.AdminPhoto.ToString());
                        }
                        return new { data = AdminID };
                    }
                }
            }
            catch (ValidationException vex)
            {
                Log.Error("Admin Controller/ AddAdminSetup" + vex.Message);
                return new { data = 0, error = vex.ValidationResult.ErrorMessage };
            }
            catch (Exception ex)
            {
                Log.Error("Admin Controller/ AddAdminSetup" + ex.Message);
                return new { data = 0, error = "Add Admin Fail." };
            }
        }        

        [HttpGet("ResetPassword/{AdminID}", Name = "AdminResetPassword")]
        public async Task<dynamic> ResetPassword(int AdminID)
        {
            Admin objAdmin = await _repositoryWrapper.Admin.FindByIDAsync(AdminID);
            string Password;
            string salt;
            string PWD;
            dynamic objresponse;
            objresponse = new { data = false };

            if (objAdmin != null)
            {
                
                Password = Util.RandomPassword.Generate(_minPasswordLength);

                if (Password != "" && Password.Length >= _minPasswordLength)
                {
                    objAdmin.Salt = Util.SaltedHash.GenerateSalt();
                    salt = objAdmin.Salt;
                    PWD = Util.SaltedHash.ComputeHash(salt, Password);
                    objAdmin.Password = PWD;
                    objAdmin.ModifiedDate = System.DateTime.Now;
                    await _repositoryWrapper.Admin.UpdateAsync(objAdmin);

                    objresponse = new { data = Password };
                }
            }

            return objresponse;
        }

        [HttpGet("UnBlock/{AdminID}", Name = "UnBlock")]
        public async Task<dynamic> UnBlock(int AdminID)
        {
            var objAdmin = await _repositoryWrapper.Admin.FindByIDAsync(AdminID);
            dynamic objresponse;

            if (objAdmin != null)
            {
                objAdmin.LoginFailCount = 0;
                objAdmin.IsBlock = false;
                await _repositoryWrapper.Admin.UpdateAsync(objAdmin);
                objresponse = new { data = true };
            }
            else
                objresponse = new { data = false };
            return objresponse;
        }
        
        [HttpGet("Block/{AdminID}", Name = "Block")]
        public async Task<dynamic> Block(int AdminID)
        {
            var objAdmin = await _repositoryWrapper.Admin.FindByIDAsync(AdminID);
            dynamic objresponse;

            if (objAdmin != null)
            {
                objAdmin.LoginFailCount = 6;
                objAdmin.IsBlock = true;
                await _repositoryWrapper.Admin.UpdateAsync(objAdmin);
                objresponse = new { data = true };
            }
            else
                objresponse = new { data = false };
            return objresponse;
        }

        
        [HttpGet("InactivateAdmin/{AdminID}", Name = "InactivateAdmin")]
        public async Task<dynamic> InactivateAdmin(int AdminID)
        {
            dynamic objresponse;
            try {
                var objAdmin = await _repositoryWrapper.Admin.FindByIDAsync(AdminID);
                if (objAdmin != null)
                {
                    objAdmin.Inactive = true;
                    await _repositoryWrapper.Admin.UpdateAsync(objAdmin);
                    objresponse = new { data = true };
                }
                else
                    objresponse = new { data = false };
            }
            catch(Exception ex) {
                Log.Error(ex.Message);
                objresponse = new { data = false };
            }
            return objresponse;
        }
        
        [HttpGet("ActivateAdmin/{AdminID}", Name = "ActivateAdmin")]
        public async Task<dynamic> ActivateAdmin(int AdminID)
        {
            dynamic objresponse;
            try {
                var objAdmin = await _repositoryWrapper.Admin.FindByIDAsync(AdminID);
                if (objAdmin != null)
                {
                    objAdmin.Inactive = false;
                    await _repositoryWrapper.Admin.UpdateAsync(objAdmin);
                    objresponse = new { data = true };
                }
                else
                    objresponse = new { data = false };
            }
            catch(Exception ex) {
                Log.Error(ex.Message);
                objresponse = new { data = false };
            }
            return objresponse;
        }

        [HttpDelete("DeleteAdminSetup/{AdminID}", Name = "DeleteAdminSetup")]
        public async Task<dynamic> DeleteAdminSetup(int AdminID)
        {
            bool retBool = false;

            var objAdmin = await _repositoryWrapper.Admin.FindByIDAsync(AdminID);
            if (objAdmin != null)
            {
                FileService.DeleteFileNameOnly("AdminPhoto", objAdmin.Id.ToString());
                await _repositoryWrapper.Admin.DeleteAsync(objAdmin);
                retBool = true;
            }
            dynamic objresponse = new { data = retBool };
            return objresponse;
        }
    
        
        // [HttpPost("PassChange", Name = "PassChange")]
        // public async Task<dynamic> PassChange(ChangePasswordPayload PasswordChangeInfo)
        // {
        //     try {
        //         int AdminID = int.Parse(GetTokenData("UserID"));
        //         string oldPassword = PasswordChangeInfo.OldPassword;
        //         string Password = PasswordChangeInfo.NewPassword;

        //         dynamic objresponse;
        //         objresponse = new { data = false };

        //         if (Password.ToString().Length >= _minPasswordLength)
        //         {
        //             Admin objAdmin = await _repositoryWrapper.Admin.FindByIDAsync(AdminID);
        //             string salt = "";
        //             string PWD = "";

        //             if (objAdmin != null)
        //             {
        //                 salt = objAdmin.Salt;
        //                 string oldhash = objAdmin.Password;
        //                 bool flag = Util.SaltedHash.Verify(salt, oldhash, oldPassword);
        //                 if (flag)
        //                 {
        //                     PWD = Util.SaltedHash.ComputeHash(salt, Password);
        //                     objAdmin.Password = PWD;
        //                     await _repositoryWrapper.Admin.UpdateAsync(objAdmin, true);

        //                     Log.Information("Successful change password : " + AdminID.ToString());
        //                     return new { data = 1 };
        //                 }
        //                 else {
        //                     throw new ValidationException("Incorrect Old Password!");  
        //                 }
        //             }
        //             else {
        //                 throw new ValidationException("Invalid Admin!");    
        //             }
        //         }
        //         else {
        //             throw new ValidationException("Invalid Password!");
        //         }
        //     }
        //     catch (ValidationException vex)
        //     {
        //         Log.Warning("Admin Controller/ Change Password " + vex.Message);
        //         return new { data = 0, error = vex.ValidationResult.ErrorMessage };
        //     }
        //     catch (Exception ex)
        //     {
        //         Log.Error("Admin Controller/ Change Password " + ex.Message);
        //         return new { data = 0, error = "Change Password Fail." };
        //     }
        }

    }
