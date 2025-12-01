using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/ServiceSubCategory")]
    public class ServiceSubCategoryController : ApiController
    {
        [HttpPost]
        [Route("saveServiceSubCategory")]
        public ExpandoObject SaveServiceSubCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();

                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceSubCategory model = JsonConvert.DeserializeObject<ServiceSubCategory>(decryptData);

                bool isDuplicate = dbContext.ServiceSubCategories.Any(x =>
                    x.ServiceSubCategoryName == model.ServiceSubCategoryName &&
                    x.ServiceCategoryId == model.ServiceCategoryId &&
                    x.ServiceSubCategoryId != model.ServiceSubCategoryId
                );

                if (isDuplicate)
                {
                    response.Message = "This subcategory already exists in this service category.";
                    return response;
                }

                ServiceSubCategory entity;

                if (model.ServiceSubCategoryId > 0)
                {
                    entity = dbContext.ServiceSubCategories
                        .FirstOrDefault(x => x.ServiceSubCategoryId == model.ServiceSubCategoryId);

                    if (entity == null)
                    {
                        response.Message = "Record not found.";
                        return response;
                    }

                    entity.ServiceCategoryId = model.ServiceCategoryId;
                    entity.ServiceSubCategoryName = model.ServiceSubCategoryName;
                    entity.Status = model.Status;
                }
                else
                {
                    entity = model;
                    dbContext.ServiceSubCategories.Add(entity);
                }

                dbContext.SaveChanges();

                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.InnerException?.Message ?? ex.Message;

                if (errorMessage.Contains("IX_ServiceSubCategory"))
                    response.Message = "This subcategory already exists.";
                else if (errorMessage.Contains("FK"))
                    response.Message = "This record is in use, so it cannot be deleted.";
                else
                    response.Message = errorMessage;
            }

            return response;
        }


        [HttpPost]
        [Route("serviceSubCategoryList")]
        public ExpandoObject ServiceSubCategoryList(RequestModel requestModel)
        {

            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceSubCategory model = JsonConvert.DeserializeObject<ServiceSubCategory>(decryptData);
                var list = (from d1 in dbContext.ServiceSubCategories
                            join d3 in dbContext.ServiceCategories on d1.ServiceCategoryId equals d3.ServiceCategoryId
                            where (model.ServiceSubCategoryId == d1.ServiceSubCategoryId || model.ServiceSubCategoryId == 0) && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.ServiceSubCategoryName
                            select new
                            {
                                d1.ServiceSubCategoryId,
                                d1.ServiceSubCategoryName,
                                d1.Status,
                                d1.ServiceCategoryId,
                                d1.ServiceCategory.ServiceCategoryName

                            }).ToList();

                response.serviceSubcategoryList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("deleteserviceSubCategory")]
        public ExpandoObject DeleteserviceSubCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceSubCategory model = JsonConvert.DeserializeObject<ServiceSubCategory>(decryptData);
                var ServiceSubCategory = dbContext.ServiceSubCategories.Where(x => x.ServiceSubCategoryId == model.ServiceSubCategoryId).First();
                dbContext.ServiceSubCategories.Remove(ServiceSubCategory);
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
