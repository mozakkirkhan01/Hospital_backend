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
        public ExpandoObject saveServiceSubCategory(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceSubCategory model = JsonConvert.DeserializeObject<ServiceSubCategory>(decryptData);

                ServiceSubCategory ServiceSubCategory = null;
                if (model.ServiceSubCategoryId > 0)
                {
                    ServiceSubCategory = dbContext.ServiceSubCategories.Where(x => x.ServiceSubCategoryId == model.ServiceSubCategoryId).First();
                    ServiceSubCategory.ServiceSubCategoryName = model.ServiceSubCategoryName;
                    ServiceSubCategory.ServiceCategoryId = model.ServiceCategoryId;
                    ServiceSubCategory.Status = model.Status;
                }
                else
                {
                    ServiceSubCategory = model;
                }

                if (ServiceSubCategory.ServiceSubCategoryId == 0)
                    dbContext.ServiceSubCategories.Add(ServiceSubCategory);
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

    }
}
