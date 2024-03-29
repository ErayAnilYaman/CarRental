﻿using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Jwt;
using Entities.DTO_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAuthService
    {
        IDataResult<User> Login(UserForLoginDto userForLoginDto);

        IDataResult<AccessToken> Register(UserForRegisterDto userForRegisterDto,string password);

        IResult UserExists(string email);

        IDataResult<AccessToken> CreateAccessToken(User user);

        
    }
}
