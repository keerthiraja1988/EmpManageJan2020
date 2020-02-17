﻿namespace CompName.ManageStocks.WebAppMVC.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using CompName.ManageStocks.CrossCutting.Configuration;
    using CompName.ManageStocks.Domain;
    using CompName.ManageStocks.Domain.Admin;
    using CompName.ManageStocks.Domain.Authentication;
    using CompName.ManageStocks.ServiceInterface;
    using CompName.ManageStocks.WebAppMVC.Areas.Admin.Models.UserManagement;
    using CompName.ManageStocks.WebAppMVC.Areas.Admin.Models.UserManagement.DTOs;
    using CompName.ManageStocks.WebAppMVC.Areas.Authentication.Models.Auth;
    using CompName.ManageStocks.WebAppMVC.Infrastructure.Extensions;
    using CompName.ManageStocks.WebAppMVC.Infrastructure.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    [AutoValidateAntiforgeryToken]
    [Area("Admin")]
    [ApplicationAuthorize]
    public class UserManagementController : Controller
    {
        private readonly IMapper _mapper;

        private readonly AppSetting _appSetting;

        private readonly IUserManagementService _userManagementService;

        public UserManagementController(
                        IMapper mapper,
                        AppSetting appSetting,
                        IUserManagementService userManagementService)
        {
            this._mapper = mapper;

            this._appSetting = appSetting;

            this._userManagementService = userManagementService;
        }

        [HttpGet]
        [Route("[controller]/GetUserAccounts")]
        public async ValueTask<IActionResult> GetUserAccountsAsync()
        {
            return await Task.Run(() => this.View());
        }

        [HttpGet]
        [Route("[controller]/GetAllUserAccounts")]
        public async ValueTask<IActionResult> GetAllUserAccountsAsync()
        {
            List<User> users = new List<User>();
            List<UserAccountViewModel> userAccountsViewModel = new List<UserAccountViewModel>();
            users = await this._userManagementService.GetAllUserAccountsAsync();

            userAccountsViewModel = this._mapper.Map<List<UserAccountViewModel>>(users);

            return this.Json(new { data = userAccountsViewModel });
        }

        [HttpGet]
        [Route("[controller]/GetUserAccountDetails")]
        public async ValueTask<IActionResult> GetUserAccountDetailsAsync(long userId)
        {
            User userDetails = new User();
            List<UserLogin> userInCorrectAuthLogs = new List<UserLogin>();
            List<UserLogin> userLoggingLogs = new List<UserLogin>();

            List<UserGender> userGenders = new List<UserGender>();
            List<UserGenderViewModel> userGendersViewModel = new List<UserGenderViewModel>();

            List<UserTitle> userTitles = new List<UserTitle>();
            List<UserTitleViewModel> userTitlesViewModel = new List<UserTitleViewModel>();

            List<UserRoles> userRoles = new List<UserRoles>();
            List<UserRolesViewModel> userRolesVM = new List<UserRolesViewModel>();

            userGenders = await this._userManagementService.GetAllUserGenderDetailsAsync();
            userGendersViewModel = this._mapper.Map<List<UserGenderViewModel>>(userGenders);

            userTitlesViewModel = this._mapper.Map<List<UserTitleViewModel>>(await this._userManagementService.GetAllUserTitleDetailsAsync());

            var userAccountDetails = await this._userManagementService.GetUserAccountDetailsAsync(userId);
            UserAccountDetailsDTO userAccountDetailsDTO = new UserAccountDetailsDTO();

            userAccountDetailsDTO.UserDetails = this._mapper.Map<UpdateUserAccountViewModel>(userAccountDetails.userDetails);
            userAccountDetailsDTO.UserDetails.UserGenders = userGendersViewModel;
            userAccountDetailsDTO.UserDetails.UserTitles = userTitlesViewModel;
            userAccountDetailsDTO.UserRolesVM = this._mapper.Map<List<UserRolesViewModel>>(userAccountDetails.userRoles);
            userAccountDetailsDTO.UserInCorrectAuthLogs = this._mapper.Map<List<UserLoginViewModel>>(userAccountDetails.userInCorrectAuthLogs);
            userAccountDetailsDTO.UserLoggingLogs = this._mapper.Map<List<UserLoginViewModel>>(userAccountDetails.userLoggingLogs);

            return await Task.Run(() => this.View(userAccountDetailsDTO));
        }

        [HttpPost]
        [Route("[controller]/UpdateUserAccountDetails")]
        public async ValueTask<IActionResult> UpdateUserAccountDetailsAsync(UpdateUserAccountViewModel updateUserAccountViewModel)
        {
            dynamic ajaxReturn = new JObject();
            User user = new User();
            var userAuthenticationModel = WebAppMVCExtensions.GetLoggedInUserDetails(this.User);

            user = this._mapper.Map<User>(updateUserAccountViewModel);
            user.ModifiedBy = userAuthenticationModel.UserId;

            var isUpdateSuccess = await this._userManagementService.UpdateUserAccountDetailsAsync(user);

            if (isUpdateSuccess)
            {
                ajaxReturn.Status = "Success";
                ajaxReturn.Message = "User details saved successfully";
            }
            else
            {
                ajaxReturn.Status = "Error";
                ajaxReturn.Message = "Error occured";
            }

            return this.Json(ajaxReturn);
        }

        [HttpPost]
        [Route("[controller]/UpdateUserAccountActiveStatus")]
        public async ValueTask<IActionResult> UpdateUserAccountActiveStatusAsync(long userId, bool isActive)
        {
            dynamic ajaxReturn = new JObject();

            UserAuthentication userAuthenticationModel = new UserAuthentication();
            userAuthenticationModel = WebAppMVCExtensions.GetLoggedInUserDetails(this.User);

            var isUpdateSuccess = await this._userManagementService.UpdateUserAccountActiveStatusAsync(userId, isActive, userAuthenticationModel.UserId);

            if (isUpdateSuccess && isActive)
            {
                ajaxReturn.Status = "Success";
                ajaxReturn.Message = "User account enabled successfully";
            }
            else if (isUpdateSuccess && !isActive)
            {
                ajaxReturn.Status = "Success";
                ajaxReturn.Message = "User account disabled successfully";
            }
            else
            {
                ajaxReturn.Status = "Error";
                ajaxReturn.Message = "Error occured";
            }

            return this.Json(ajaxReturn);
        }

        [HttpPost]
        [Route("[controller]/UpdateUserAccountLockedStatus")]
        public async ValueTask<IActionResult> UpdateUserAccountLockedStatusAsync(long userId, bool isLocked)
        {
            dynamic ajaxReturn = new JObject();

            UserAuthentication userAuthenticationModel = new UserAuthentication();
            userAuthenticationModel = WebAppMVCExtensions.GetLoggedInUserDetails(this.User);

            var isUpdateSuccess = await this._userManagementService.UpdateUserAccountLockedStatusAsync(userId, isLocked, userAuthenticationModel.UserId);

            if (isUpdateSuccess && isLocked)
            {
                ajaxReturn.Status = "Success";
                ajaxReturn.Message = "User account locked successfully";
            }
            else if (isUpdateSuccess && !isLocked)
            {
                ajaxReturn.Status = "Success";
                ajaxReturn.Message = "User account un-locked successfully";
            }
            else
            {
                ajaxReturn.Status = "Error";
                ajaxReturn.Message = "Error occured";
            }

            return this.Json(ajaxReturn);
        }

        [HttpGet]
        [Route("[controller]/LoadChangePasswordPartialView")]

        public async ValueTask<IActionResult> LoadChangePasswordPartialViewAsync(long userId, string userName)
        {
            ChangeUserAccountPasswordViewModel changeUserAccountPasswordVM = new ChangeUserAccountPasswordViewModel();
            changeUserAccountPasswordVM.UserId = userId;
            changeUserAccountPasswordVM.UserName = userName;

            return await Task.Run(() => this.PartialView("_ChangeUserAccountPassword", changeUserAccountPasswordVM));
        }

        [HttpPost]
        [Route("[controller]/ChangeUserAccountPassword")]
        public async ValueTask<IActionResult> ChangeUserAccountPasswordAsync([FromForm] ChangeUserAccountPasswordViewModel changeUserAccountPasswordVM)
        {
            dynamic ajaxReturn = new JObject();
            User user = new User();
            user = this._mapper.Map<User>(changeUserAccountPasswordVM);

            var isUpdateSuccess = await this._userManagementService.ChangeUserAccountPasswordAsync(user);

            if (isUpdateSuccess)
            {
                ajaxReturn.Status = "Success";
                ajaxReturn.Message = "Password changed successfully";
            }
            else
            {
                ajaxReturn.Status = "Error";
                ajaxReturn.Message = "Error occured";
            }

            return this.Json(ajaxReturn);
        }
    }
}