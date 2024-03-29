﻿using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants.Messages;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Validation;
using Core.Entities.Concrete;
using Core.Utilities.BusinessRules;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTO_s;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        IUserDal _userDal;
        public UserManager(IUserDal userDal)
        {
            _userDal = userDal;
        }

        [ValidationAspect(typeof(UserValidator))]
        public IResult Add(User user)
        {
            _userDal.Add(user);
            return new SuccessResult(UserMessages.UserAdded);
        }

        [ValidationAspect(typeof(UserValidator))]
        //[CacheRemoveAspect("IUserService.Get")]
        //[SecuredOperation("delete,admin")]
        public IResult Delete(User user)
        {
            _userDal.Delete(user);
            return new SuccessResult(UserMessages.UserDeleted);
        }

        
        //[CacheAspect]
        //[SecuredOperation("list,admin")]
        public IDataResult<List<User>> GetAll()
        {
            if (DateTime.Now.Hour == 05)
            {
                return new ErrorDataResult<List<User>>(Messages.MaintenanceTime);
            }
            return new SuccessDataResult<List<User>>(_userDal.GetAll(), Messages.Succeed);
        }
        

        public IDataResult<User> GetByMail(string mail)
        {
            var result = _userDal.Get(u => u.Email == mail);
            if (result !=null)
            {
                return new SuccessDataResult<User>(result, Messages.Succeed);

            }

            return new ErrorDataResult<User>(UserMessages.MailAlreadyExists);
        }

        
        
        public IDataResult<List<OperationClaim>> GetOperationClaims(User user)
        {
            var operationResult = _userDal?.GetClaims(user);
            return new SuccessDataResult<List<OperationClaim>>(operationResult, OperationClaimsMessage.OperationClaimsListed);
        }

        

        public IDataResult<User> GetUserById(int id)
        {
            if (DateTime.Now.Hour == 05)
            {
                return new ErrorDataResult<User>(Messages.MaintenanceTime);
            }
            return new SuccessDataResult<User>(_userDal.Get(c => c.Id == id), Messages.Succeed);
        }

        public IResult Transaction(User user)
        {
            throw new NotImplementedException();
        }

        [ValidationAspect(typeof(UserValidator))]
        //[CacheRemoveAspect("IUserService.Get")]
        //[SecuredOperation("update,admin")]
        public IResult Update(UserForUpdateDto userUpdateDto)
        {
            
            var userToCheck = GetUserById(userUpdateDto.Id);
            var result = HashingHelper.VerifyPasswordHash(userUpdateDto.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt)!;
            if (result == false)
            {
                return new ErrorResult(UserMessages.PasswordError);
            }
            User user = new User
            {
                Id = userUpdateDto.Id,
                Email = userUpdateDto.Email,
                FirstName = userUpdateDto.FirstName,
                LastName = userUpdateDto.LastName,
                PasswordHash = userToCheck.Data.PasswordHash,
                PasswordSalt = userToCheck.Data.PasswordSalt,
                Status = true
            };

            _userDal.Update(user);
            return new SuccessResult(UserMessages.UserUpdated);
        }
        

        public IDataResult<User> GetUserByUserName(string userName)
        {
            var result = _userDal.Get(u => u.LastName + " " + u.FirstName == userName)!;
            if (result != null)
            {
                return new SuccessDataResult<User>(result);
            }
            return new ErrorDataResult<User>(Messages.InvalidNameError);
        }

        public IDataResult<List<UserDetailDto>> GetUserDetailsByUserId(int userId)
        {
            var result = _userDal.GetUserDtoByUserId(userId);
            if (result != null)
            {
                return new SuccessDataResult<List<UserDetailDto>>(result);
            }
            return new ErrorDataResult<List<UserDetailDto>>(UserMessages.UserNotFound);
        }

        public IDataResult<List<UserDetailDto>> GetAllUserDetails()
        {
            var result = _userDal.GetUserDtos();
            if (result != null)
            {
                return new SuccessDataResult<List<UserDetailDto>>(result);
            }
            return new ErrorDataResult<List<UserDetailDto>>(UserMessages.NotAvailable);
        }

        public IDataResult<List<UserDetailDto>> GetUserDetailsByCustomerId(int customerId)
        {
            var result = _userDal.GetUserDtoByCustomerId(customerId);
            if (result != null)
            {
                return new SuccessDataResult<List<UserDetailDto>>(result);
            }
            return new ErrorDataResult<List<UserDetailDto>>();
        }

        public IResult DeleteById(int id)
        {
            var result = _userDal.Get(u=>u.Id== id);
            if (result == null)
            {
                return new ErrorResult(UserMessages.UserNotFound);
            }
            return new SuccessResult(UserMessages.UserDeleted);
        }

        public IResult UpdatePassword(UserForChangePasswordDto userForChangePasswordDto)
        {
            byte[] passwordHash, passwordSalt;
            var userToCheck = GetUserById(userForChangePasswordDto.Id);
            var result = BusinessRules.Run(CheckThePasswordHash(userForChangePasswordDto.PasswordToChange, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt));
            if (result != null )
            {
                return new ErrorResult(result.Message);
            }
            HashingHelper.CreatePasswordHash(userForChangePasswordDto.Password,out passwordHash,out passwordSalt);
            User user = new User
            {
                Id = userToCheck.Data.Id,
                Email = userToCheck.Data.Email,
                FirstName = userToCheck.Data.FirstName,
                LastName = userToCheck.Data.LastName,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                Status = true

            };
            _userDal.Update(user);
            return new SuccessResult(UserMessages.UserUpdated);

        }
        private IResult CheckThePasswordHash(string password, byte[] passwordHash , byte[] passwordSalt)
        {
            var result = HashingHelper.VerifyPasswordHash(password, passwordHash, passwordSalt);
            if (result != true)
            {
                return new ErrorResult(UserMessages.PasswordError);
            }
            return new SuccessResult();
        }
    }
}
