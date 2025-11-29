using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/ServiceCategory")]
    public class ServiceCategoryController : ApiController
    {
        [HttpPost]
        [Route("saveServiceCategory")]
        public ExpandoObject SaveServiceCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceCategory model = JsonConvert.DeserializeObject<ServiceCategory>(decryptData);

                ServiceCategory ServiceCategory = null;
                if (model.ServiceCategoryId > 0)
                {
                    ServiceCategory = dbContext.ServiceCategories.Where(x => x.ServiceCategoryId == model.ServiceCategoryId).First();
                    ServiceCategory.ServiceCategoryName = model.ServiceCategoryName;
                    ServiceCategory.Status = model.Status;
                }
                else
                {
                    ServiceCategory = model;
                }

                if (ServiceCategory.ServiceCategoryId == 0)
                    dbContext.ServiceCategories.Add(ServiceCategory);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IX"))
                    response.Message = "This record is already exist";
                else
                    response.Message = ex.Message;
            }
            return response;
        }



        [HttpPost]
        [Route("DepartmentList")]
        public ExpandoObject DepartmentList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Department model = JsonConvert.DeserializeObject<Department>(decryptData);

                var list = (from d1 in dbContext.Departments
                            where (model.DepartmentId == d1.DepartmentId || model.DepartmentId == 0)
                            && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.DepartmentName
                            select new
                            {
                                d1.DepartmentId,
                                d1.DepartmentName,
                                d1.Status,
                            }).ToList();

                response.DepartmentList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("serviceCategoryList")]
        public ExpandoObject ServiceCategoryList(RequestModel requestModel)
        {

            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceCategory model = JsonConvert.DeserializeObject<ServiceCategory>(decryptData);
                var list = (from d1 in dbContext.ServiceCategories
                            where (model.ServiceCategoryId == d1.ServiceCategoryId || model.ServiceCategoryId == 0) && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.ServiceCategoryName
                            select new
                            {
                                d1.ServiceCategoryId,
                                d1.ServiceCategoryName,
                                d1.Status,

                            }).ToList();

                response.serviceCategoryList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("deleteServiceCategory")]
        public ExpandoObject DeleteServiceCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceCategory model = JsonConvert.DeserializeObject<ServiceCategory>(decryptData);
                var ServiceCategory = dbContext.ServiceCategories.Where(x => x.ServiceCategoryId == model.ServiceCategoryId).First();
                dbContext.ServiceCategories.Remove(ServiceCategory);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                    response.Message = "This record is in use. so can't delete.";
                else
                    response.Message = ex.Message;
            }
            return response;
        }
    }
}

