using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Doctor")]
    public class DoctorController : ApiController
    {
        [HttpPost]
        [Route("doctorList")]
        public ExpandoObject DoctorList(RequestModel requestModel)
        { 

            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Doctor model = JsonConvert.DeserializeObject<Doctor>(decryptData);
                var list = (from d1 in dbContext.Doctors
                            join d3 in dbContext.Departments on d1.DepartmentId equals d3.DepartmentId
                            where (model.DoctorId == d1.DoctorId || model.DoctorId == 0) && (model.Status == d1.Status || model.Status == null)
                            orderby d1.DoctorName
                            select new
                            {
                                d1.DoctorId,
                                d1.DoctorName,
                                d1.Status,
                                d1.MobileNo,
                                d1.Email,
                                d1.Gender,
                                d1.Qualification,
                                d1.DepartmentId,
                                d1.Department.DepartmentName,
                                d1.ConsultFee,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.DateOfBirth,
                                d1.JoinDate

                            }).ToList();

                response.DoctorList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveDoctor")]
        public ExpandoObject SaveDoctor(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Doctor model = JsonConvert.DeserializeObject<Doctor>(decryptData);

                Doctor Doctor = null;
                if (model.DoctorId > 0)
                {
                    Doctor = dbContext.Doctors.Where(x => x.DoctorId == model.DoctorId).First();
                    Doctor.DoctorName = model.DoctorName;
                    Doctor.DepartmentId = model.DepartmentId;
                    Doctor.Qualification = model.Qualification;
                    Doctor.Gender = model.Gender;
                    Doctor.ConsultFee = model.ConsultFee;
                    Doctor.MobileNo = model.MobileNo;
                    Doctor.Email = model.Email;
                    Doctor.Status = model.Status;
                    Doctor.JoinDate = model.JoinDate;
                    Doctor.DateOfBirth = model.DateOfBirth;
                    Doctor.UpdatedBy = model.UpdatedBy;
                    Doctor.UpdatedOn = DateTime.Now;
                }
                else
                {
                    Doctor = model;
                    Doctor.CreatedOn = DateTime.Now;
                    Doctor.CreatedBy = model.CreatedBy;
                }

                if (Doctor.DoctorId == 0)
                    dbContext.Doctors.Add(Doctor);
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
        [Route("deleteDoctor")]
        public ExpandoObject DeleteDoctor(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Doctor model = JsonConvert.DeserializeObject<Doctor>(decryptData);
                var Doctor = dbContext.Doctors.Where(x => x.DoctorId == model.DoctorId).First();
                dbContext.Doctors.Remove(Doctor);
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
