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

                Patient patient = null;

                if (model.PatientId > 0)
                {
                    // Update existing record
                    patient = dbContext.Patients.FirstOrDefault(x => x.PatientId == model.PatientId);
                    if (patient != null)
                    {
                        patient.PatientName = model.PatientName;
                        patient.Gender = model.Gender;
                        patient.DateOfBirth = model.DateOfBirth;
                        patient.Age = model.Age;
                        patient.MobileNo = model.MobileNo;
                        patient.Email = model.Email;
                        patient.Address = model.Address;
                        patient.AadharNo = model.AadharNo;
                        patient.Status = model.Status;
                        patient.JoinDate = model.JoinDate;
                        patient.UpdatedBy = model.UpdatedBy;
                        patient.UpdatedOn = DateTime.Now;
                    }
                }
                else
                {
                    patient = model;
                    patient.CreatedOn = DateTime.Now;
                    patient.CreatedBy = model.CreatedBy;

                    // ✅ Generate UHIDNo like WIK00001, WIK00002, etc.
                    var lastPatient = dbContext.Patients
                                               .OrderByDescending(x => x.PatientId)
                                               .FirstOrDefault();
                    int nextNumber = 1;
                    if (lastPatient != null && !string.IsNullOrEmpty(lastPatient.UHIDNo))
                    {
                        // Extract numeric part from last UHIDNo (e.g., WIK00023 → 23)
                        string lastNumberStr = lastPatient.UHIDNo.Substring(3);
                        int.TryParse(lastNumberStr, out nextNumber);
                        nextNumber += 1;
                    }

                    // Generate new UHIDNo
                    patient.UHIDNo = "WIK" + nextNumber.ToString("D5");

                    dbContext.Patients.Add(patient);
                }

                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
                response.UHIDNo = patient.UHIDNo; // optional: return generated UHID
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IX"))
                    response.Message = "This record already exists.";
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
