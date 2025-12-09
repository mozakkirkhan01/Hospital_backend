using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;
namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Opd")]
    public class OpdController : ApiController
    {
        [HttpPost]
        [Route("saveOpd")]
        public ExpandoObject SaveOpd(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Opd model = JsonConvert.DeserializeObject<Opd>(decryptData);

                Opd Opd = null;
                if (model.OpdId > 0)
                {
                    Opd = dbContext.Opds.Where(x => x.OpdId == model.OpdId).First();
                    Opd.PatientId = model.PatientId;
                    Opd.OpdNo = model.OpdNo;
                    Opd.TokenNo = model.TokenNo;
                    Opd.OpdDate = model.OpdDate;
                    Opd.OpdType = model.OpdType;
                    Opd.LineTotal = model.LineTotal;
                    Opd.TotalDiscount = model.TotalDiscount;
                    Opd.GrandTotal = model.GrandTotal;
                    Opd.UpdatedBy = model.UpdatedBy;
                    Opd.UpdatedOn = model.UpdatedOn;
                    Opd.PaymentId = model.PaymentId;
                    Opd.PaymentStatus = model.PaymentStatus;
                    Opd.Remarks = model.Remarks;
                }
                else
                {
                    Opd = model;
                    Opd.CreatedOn = DateTime.Now;
                    Opd.CreatedBy = model.CreatedBy;
                }

                if (Opd.OpdId == 0)
                    dbContext.Opds.Add(Opd);
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
