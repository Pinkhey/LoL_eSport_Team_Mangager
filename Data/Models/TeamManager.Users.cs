﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using EF Core template.
// Code is generated on: 2025. 05. 11. 11:20:28
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace TeamManagerContext
{
    public partial class Users {

        public Users()
        {
            OnCreated();
        }

        public virtual int UserId { get; set; }

        public virtual string Username { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual bool IsAdmin { get; set; }


        public virtual Teams Teams { get; set; }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
