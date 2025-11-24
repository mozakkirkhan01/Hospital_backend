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
        public ExpandoObject saveServiceCategory(RequestModel requestModel)
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

    }
}
