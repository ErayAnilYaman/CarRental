using Business.Abstract;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ColorManager : IColorService
    {
        IColorDal _colorDal;
        public ColorManager(IColorDal colorDal)
        {
            _colorDal = colorDal;
        }
        public void Add(Color color)
        {
            _colorDal.Add(color);
        }

        public void Delete(Color color)
        {
            _colorDal.Delete(color);
        }

        public List<Color> GetAllColors()
        {
            return _colorDal.GetAll();
        }

        public Color GetColorById(int id)
        {
            return _colorDal.Get(c=>c.ColorId== id);
        }

        public void Update(Color color)
        {
            _colorDal.Update(color);
        }
    }
}
