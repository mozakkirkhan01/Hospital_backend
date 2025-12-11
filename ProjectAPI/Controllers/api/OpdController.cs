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
        [HttpPost]
        [Route("OpdList")]
        public ExpandoObject OpdList(RequestModel requestModel)
        {

            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ServiceSubCategory model = JsonConvert.DeserializeObject<ServiceSubCategory>(decryptData);
                var list = (from d1 in dbContext.Opds
                            select new
                            {
                                d1.OpdId,
                                d1.Patient.PatientName,
                                d1.OpdNo,
                                d1.TokenNo,
                                d1.OpdDate,
                                d1.OpdType,
                                d1.LineTotal,
                                d1.TotalDiscount,
                                d1.GrandTotal,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                                d1.PaymentId,
                                d1.PaymentStatus,
                                d1.Remarks

                            }).ToList();

                response.OpdList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
