﻿using Business.Abstract;
using Business.Constants.Messages;
using Business.Constants.Path;
using Business.ValidationRules.FluentValidation;
using Castle.Core.Internal;
using Core.Aspects.Autofac.Performances;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.BusinessRules;
using Core.Utilities.Helpers.FileHelpers;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CarImageManager : ICarImageService
    {
        ICarImageDal _carImageDal;
        IFileHelper _fileHelper;

        public CarImageManager(ICarImageDal carImageDal, IFileHelper fileHelper)
        {
            _carImageDal = carImageDal;
            _fileHelper = fileHelper;

        }

        [ValidationAspect(typeof(CarImageValidator))]
        [PerformanceAspect(10)]

        public IResult Add(IFormFile file, CarImage carImage)
        {
            var result = BusinessRules.Run(CheckIfCarImagesLimit(carImage.CarId));
            if (result != null)
            {
                return new ErrorResult(result.Message);
            }
            carImage.ImagePath = _fileHelper.Upload(file, PathConstant.ImagePath);
            carImage.Date = DateTime.Now;
            _carImageDal.Add(carImage);
            return new SuccessResult(CarImagesMessage.CarImageAdded);

        }
        [ValidationAspect(typeof(CarImageValidator))]
        [PerformanceAspect(10)]

        public IResult Delete(CarImage carImage)
        {
            if (DateTime.Now.Hour == 05)
            {
                return new ErrorResult(Messages.MaintenanceTime);
            }
            _fileHelper.Delete(PathConstant.ImagePath + carImage.ImagePath);
            _carImageDal.Delete(carImage);
            return new SuccessResult(Messages.Succeed);
        }
        [ValidationAspect(typeof(CarImageValidator))]
        [PerformanceAspect(10)]

        public IDataResult<List<CarImage>> GetAll()
        {
            IResult result = BusinessRules.Run(CheckTheMaintenanceTime());
            if (result != null)
            {
                return new ErrorDataResult<List<CarImage>>();
            }
            return new SuccessDataResult<List<CarImage>>(_carImageDal.GetAll());

        }
        [ValidationAspect(typeof(CarImageValidator))]
        [PerformanceAspect(10)]

        public IDataResult<CarImage> GetById(int id)
        {

            if (DateTime.Now.Hour == 05)
            {
                return new ErrorDataResult<CarImage>();
            }
            return new SuccessDataResult<CarImage>(_carImageDal.Get(c => c.Id == id));
        }
        
        public IDataResult<List<CarImage>> GetDefaultImage(int carId)
        {
            List<CarImage> carImages = new List<CarImage>();
            carImages.Add(new CarImage
            {
                CarId = carId,
                ImagePath = PathConstant.ImagePath +  @"Images\\defaultCar.jpg",
                Date = DateTime.Now
            });
            return new SuccessDataResult<List<CarImage>>(carImages, Messages.Succeed);
        }

        

        public IDataResult<List<CarImage>> GetImagesByCarId(int carId)
        {
            
            List<CarImage> result = _carImageDal.GetAll(c => c.CarId == carId);
            if (result != null)
            {
                return new SuccessDataResult<List<CarImage>>(result, Messages.Succeed); ;
            }
            return new ErrorDataResult<List<CarImage>>(Messages.Error);
            

        }

        public IResult Transaction(CarImage carImage)
        {
            throw new NotImplementedException();
        }

        [ValidationAspect(typeof(CarImageValidator))]
        [PerformanceAspect(10)]

        public IResult Update(IFormFile file,CarImage carImage)
        {
            IResult result = BusinessRules.Run(CheckTheDateTime(carImage.Id),
                CheckTheMaintenanceTime());
            if (result != null)
            {
                return result;
            }
            _fileHelper.Update(file,PathConstant.ImagePath + carImage.ImagePath,PathConstant.ImagePath);
            _carImageDal.Update(carImage);
            return new SuccessResult(Messages.Succeed);
        }

        private IResult CheckIfCarImagesLimit(int carId)
        {
            var result = _carImageDal.GetAll(c=>c.CarId== carId).Count;
            if (result  < 2)
            {
                return new SuccessResult();
            }
            return new ErrorResult(CarImagesMessage.CarImagesLimitExceded);
        }
        
        private IResult CheckTheDateTime(int id)
        {
            var result = _carImageDal.Get(c=>c.Id== id);
            if (result.Date.Hour == 05)
            {
                return new ErrorResult(Messages.MaintenanceTime);
            }
            return new SuccessResult();
        }
        private IResult CheckTheMaintenanceTime()
        {
            if (DateTime.Now.Hour == 05)
            {
                return new ErrorResult(Messages.MaintenanceTime);
            }
            return new SuccessResult();
        }
    }
}
