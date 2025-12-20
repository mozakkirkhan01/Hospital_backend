using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Dynamic;
using System.Web;
using Newtonsoft.Json;
using Project;
namespace ProjectAPI.Controllers.api

{
    [RoutePrefix("api/Opd")]
    public class OpdController : ApiController
    {

        private string GenerateOpdNo(InstituteDbEntities dbContext)
        {
            // Read all OPD numbers that start with OPD
            var numbers = dbContext.Opds
                            .Where(x => x.OpdNo.StartsWith("OPD"))
                            .Select(x => x.OpdNo.Substring(3)) // remove "OPD"
                            .ToList();

            int lastNumber = 0;

            foreach (var num in numbers)
            {
                if (int.TryParse(num, out int n))
                {
                    if (n > lastNumber)
                        lastNumber = n;
                }
            }

            return "OPD" + (lastNumber + 1);
        }

        private int GenerateTokenNo(InstituteDbEntities dbContext)
        {
            int lastOrderNo = dbContext.Opds.Max(o => (int?)o.TokenNo) ?? 0;
            return lastOrderNo + 1;
        }



        public class OpdModel
        {
            public Opd GetOpd { get; set; }
            public List<OpdDetail> GetOpdDetail { get; set; }
            public List<Payment> GetPayment { get; set; }  // FIXED ✔
        }



        [HttpPost]
        [Route("saveOpd")]
        public ExpandoObject SaveOpd(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
                InstituteDbEntities dbContext = new InstituteDbEntities();

                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {


                        string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                        AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                        var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                        OpdModel model = JsonConvert.DeserializeObject<OpdModel>(decryptData);
                        
                        Opd Opd = null;
                        
                        if (model.GetOpd.OpdId > 0)
                        {
                            
                            Opd = dbContext.Opds.Where(x => x.OpdId == model.GetOpd.OpdId).First();

                            Opd.PatientId = model.GetOpd.PatientId;
                            Opd.OpdDate = model.GetOpd.OpdDate;
                            Opd.OpdType = model.GetOpd.OpdType;
                            Opd.LineTotal = model.GetOpd.LineTotal;
                            Opd.TotalDiscount = model.GetOpd.TotalDiscount;
                            Opd.GrandTotal = model.GetOpd.GrandTotal;
                            Opd.PaymentStatus = model.GetOpd.PaymentStatus;
                            Opd.Remarks = model.GetOpd.Remarks;
                            Opd.TotalQty = model.GetOpd.TotalQty;
                            Opd.TotalDuesAmount = model.GetOpd.TotalDuesAmount;
                            Opd.TotalPaidAmount = model.GetOpd.TotalPaidAmount;
                            Opd.UpdatedBy = model.GetOpd.UpdatedBy;
                            Opd.UpdatedOn = DateTime.Now;
                        }
                        else
                        {
                            Opd = new Opd();

                            Opd.PatientId = model.GetOpd.PatientId;
                            Opd.OpdNo = GenerateOpdNo(dbContext);
                            Opd.TokenNo = GenerateTokenNo(dbContext);
                            Opd.OpdDate = model.GetOpd.OpdDate;
                            Opd.OpdType = model.GetOpd.OpdType;
                            Opd.LineTotal = model.GetOpd.LineTotal;
                            Opd.TotalDiscount = model.GetOpd.TotalDiscount;
                            Opd.GrandTotal = model.GetOpd.GrandTotal;
                            Opd.PaymentStatus = model.GetOpd.PaymentStatus;
                            Opd.Remarks = model.GetOpd.Remarks;
                            Opd.TotalQty = model.GetOpd.TotalQty;
                            Opd.TotalDuesAmount = model.GetOpd.TotalDuesAmount;
                            Opd.TotalPaidAmount = model.GetOpd.TotalPaidAmount;
                            Opd.CreatedBy = model.GetOpd.CreatedBy;
                            Opd.CreatedOn = DateTime.Now;

                            dbContext.Opds.Add(Opd);

                        }

                        dbContext.SaveChanges();
                        

                        // OpdDetail insert Code

                        if (model.GetOpdDetail != null)
                        {
                            model.GetOpdDetail.ForEach(s =>
                            {
                                OpdDetail OpdDetail = null;
                                if (s.OpdDetailId > 0)
                                {
                                    OpdDetail = dbContext.OpdDetails.Where(x => x.OpdDetailId == s.OpdDetailId).First();
                                    OpdDetail.OpdId = s.OpdId;
                                    OpdDetail.ServiceCategoryId = s.ServiceCategoryId;
                                    OpdDetail.OpdDetailId = OpdDetail.OpdDetailId;
                                    OpdDetail.ServiceChargeAmount = s.ServiceChargeAmount;
                                    OpdDetail.ServiceSubCategoryId = s.ServiceSubCategoryId;
                                    OpdDetail.Quantity = s.Quantity;
                                    OpdDetail.Discount = s.Discount;
                                    OpdDetail.Total = s.Total;

                                }
                                else
                                {
                                    OpdDetail = new OpdDetail();

                                    OpdDetail.OpdId = Opd.OpdId;
                                    OpdDetail.ServiceCategoryId = s.ServiceCategoryId;
                                    OpdDetail.ServiceChargeAmount = s.ServiceChargeAmount;
                                    OpdDetail.ServiceSubCategoryId = s.ServiceSubCategoryId;
                                    OpdDetail.Quantity = s.Quantity;
                                    OpdDetail.Discount = s.Discount;
                                    OpdDetail.Total = s.Total;
                                    dbContext.OpdDetails.Add(OpdDetail);
                                }
                                dbContext.SaveChanges();
                            });
                        }

                    //Payment Details 
                    // Payment Details 
                    if (model.GetPayment != null && model.GetPayment.Count > 0)
                    {
                        foreach (var pay in model.GetPayment)
                        {
                            Payment payment = new Payment();

                            payment.OpdId = Opd.OpdId;
                            payment.PaymentDate = pay.PaymentDate;
                            payment.Amount = pay.Amount;
                            payment.PaymentMode = pay.PaymentMode;
                            payment.PaymentType = pay.PaymentType;

                            dbContext.Payments.Add(payment);
                        }

                        dbContext.SaveChanges();
                    }



                    // Commit the transaction
                    transaction.Commit();
                        response.Message = ConstantData.SuccessMessage;
                        response.OpdId = Opd.OpdId;
                    }

                    catch (Exception ex)
                    {
                        // Rollback on error
                        transaction.Rollback();
                        response.Status = "Error";
                        response.Message = ex.Message;
                    }
                }
                return response;
            }
        }
        //[HttpPost]
        //[Route("OpdList")]
        //public ExpandoObject OpdList(RequestModel requestModel)
        //{

        //    dynamic response = new ExpandoObject();
        //    try
        //    {
        //        InstituteDbEntities dbContext = new InstituteDbEntities();
        //        string AppKey = HttpContext.Current.Request.Headers["AppKey"];
        //        AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
        //        var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
        //        ServiceSubCategory model = JsonConvert.DeserializeObject<ServiceSubCategory>(decryptData);
        //        var list = (from d1 in dbContext.Opds
        //                    select new
        //                    {
        //                        d1.OpdId,
        //                        d1.Patient.PatientName,
        //                        d1.OpdNo,
        //                        d1.TokenNo,
        //                        d1.OpdDate,
        //                        d1.OpdType,
        //                        d1.LineTotal,
        //                        d1.TotalDiscount,
        //                        d1.GrandTotal,
        //                        d1.CreatedBy,
        //                        d1.CreatedOn,
        //                        d1.UpdatedBy,
        //                        d1.UpdatedOn,
        //                        d1.PaymentId,
        //                        d1.PaymentStatus,
        //                        d1.Remarks

        //                    }).ToList();

        //        response.OpdList = list;
        //        response.Message = ConstantData.SuccessMessage;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = ex.Message;
        //    }
        //    return response;
        //}
    }

