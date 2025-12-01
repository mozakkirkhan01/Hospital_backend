using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/ServiceCharge")]
    public class ServiceChargeController : ApiController
    {
        [HttpPost]
        [Route("saveServiceCharge")]
        public ExpandoObject SaveServiceCharge(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceCharge model = JsonConvert.DeserializeObject<ServiceCharge>(decryptData);

                ServiceCharge ServiceCharge = null;
                if (model.ServiceChargeId > 0)
                {
                    ServiceCharge = dbContext.ServiceCharges.Where(x => x.ServiceChargeId == model.ServiceChargeId).First();
                    ServiceCharge.ServiceCategoryId = model.ServiceCategoryId;
                    ServiceCharge.ServiceSubCategoryId = model.ServiceSubCategoryId;
                    ServiceCharge.ServiceChargeAmount = model.ServiceChargeAmount;
                    ServiceCharge.Status = model.Status;
                    ServiceCharge.UpdatedBy = model.UpdatedBy;
                    ServiceCharge.UpdatedOn = model.UpdatedOn;
                }
                else
                {
                    ServiceCharge = model;
                    ServiceCharge.CreatedOn = DateTime.Now;
                    ServiceCharge.CreatedBy = model.CreatedBy;
                }

                if (ServiceCharge.ServiceChargeId == 0)
                    dbContext.ServiceCharges.Add(ServiceCharge);
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
