using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Patient")]
    public class PatientController : ApiController
    {
        [HttpPost]
        [Route("savePatient")]
        public ExpandoObject SavePatient(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Patient model = JsonConvert.DeserializeObject<Patient>(decryptData);

                Patient Patient = null;
                if (model.PatientId > 0)
                {
                    Patient = dbContext.Patients.Where(x => x.PatientId == model.PatientId).First();
                    Patient.PatientName = model.PatientName;
                    Patient.Gender = model.Gender;
                    Patient.DateOfBirth = model.DateOfBirth;
                    Patient.Age = model.Age;
                    Patient.MobileNo = model.MobileNo;
                    Patient.Email = model.Email;
                    Patient.Address = model.Address;
                    Patient.AadharNo = model.AadharNo;
                    Patient.Status = model.Status;
                    Patient.JoinDate = model.JoinDate;
                    Patient.UpdatedBy = model.UpdatedBy;
                    Patient.UpdatedOn = DateTime.Now;
                }
                else
                {
                    Patient = model;
                    Patient.CreatedOn = DateTime.Now;
                    Patient.CreatedBy = model.CreatedBy;
                }

                if (Patient.PatientId == 0)
                    dbContext.Patients.Add(Patient);
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
        [Route("patientList")]
        public ExpandoObject PatientList(RequestModel requestModel)
        {

            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Patient model = JsonConvert.DeserializeObject<Patient>(decryptData);
                var list = (from d1 in dbContext.Patients
                            where (model.PatientId == d1.PatientId || model.PatientId == 0) && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.PatientName
                            select new
                            {
                                d1.PatientId,
                                d1.UHIDNo,
                                d1.PatientName,
                                d1.BloodGroup,
                                d1.MaritalStatus,
                                d1.Gender,
                                d1.DateOfBirth,
                                d1.Status,
                                d1.Age,
                                d1.MobileNo,
                                d1.Email,
                                d1.Address,
                                d1.AadharNo,
                                d1.JoinDate,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                                d1.CreatedBy,
                                d1.CreatedOn

                            }).ToList();

                response.PatientList = list;
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
