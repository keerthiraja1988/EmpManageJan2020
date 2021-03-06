﻿namespace CompName.ManageStocks.Domain.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class UserLogin
    {
        public long UserId { get; set; }

        public long UserInCorrectAuthLogId { get; set; }

        public long UserLoggingLogId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        public long InCorrectLoggingCount { get; set; }

        public string LoggingIpAddress { get; set; }

        public string LoggingBrowser { get; set; }

        public bool IsUserAuthenticated { get; set; }

        public bool IsUserAccountLocked { get; set; }

        public bool IsUserAccountDisabled { get; set; }

        public bool IsUserAccountNotFound { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public long? CreatedBy { get; set; }

        public string CreatedByUserName { get; set; }

        public DateTime ModifiedOn { get; set; }

        public long? ModifiedBy { get; set; }

        public string ModifiedByUserName { get; set; }
    }
}