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


        public class CancelOpdModel
        {
            public int OpdId { get; set; }
            public string CancelReason { get; set; }
            public DateTime OpdCancelDate { get; set; }
            public int BillStatus { get; set; } // 2 = Cancelled
            public int UpdatedBy { get; set; }
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
                        Opd.TotalDuesAmount = model.GetOpd.TotalDuesAmount;
                        if (Opd.TotalDuesAmount == 0)
                            Opd.PaymentStatus = 1;
                        else
                            Opd.PaymentStatus = 2;
                        // ✅ Payment Status
                        //Opd.PaymentStatus = Opd.TotalDuesAmount <= 0 ? 1 : 2;
                        Opd.BillStatus = 1;
                        Opd.Remarks = model.GetOpd.Remarks;
                        Opd.TotalQty = model.GetOpd.TotalQty;
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
                        Opd.TotalDuesAmount = model.GetOpd.TotalDuesAmount;
                        if (Opd.TotalDuesAmount == 0)
                            Opd.PaymentStatus = 1;
                        else
                        {
                            Opd.PaymentStatus = 2;
                        }
                        Opd.BillStatus = 1;
                        Opd.Remarks = model.GetOpd.Remarks;
                        Opd.TotalQty = model.GetOpd.TotalQty;
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
                            join p1 in dbContext.Payments
                            on d1.OpdId equals p1.OpdId
                            into paymentGroup
                            from p in paymentGroup.DefaultIfEmpty()   // LEFT JOIN
                            select new
                            {
                                d1.OpdId,
                                d1.OpdNo,
                                d1.OpdDate,
                                d1.Patient.UHIDNo,
                                d1.Patient.PatientName,
                                d1.Patient.MobileNo,
                                d1.TokenNo,
                                d1.LineTotal,
                                d1.TotalDiscount,
                                d1.GrandTotal,
                                d1.TotalPaidAmount,
                                d1.TotalDuesAmount,
                                p.PaymentMode,
                                p.PaymentType,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                                d1.PaymentStatus,
                                d1.Remarks,
                                d1.BillStatus,
                                d1.OpdType

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
        [HttpPost]
        [Route("OpdDetailList")]
        public ExpandoObject OpdDetailList(RequestModel requestModel)
        {

            dynamic response = new ExpandoObject();
            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                OpdDetail model = JsonConvert.DeserializeObject<OpdDetail>(decryptData);
                var list = (from d1 in dbContext.Opds
                            join p1 in dbContext.OpdDetails
                            on d1.OpdId equals p1.OpdId
                            into DetailGroup
                            from p in DetailGroup.DefaultIfEmpty()   // LEFT JOIN
                            join pay in dbContext.Payments
                            on d1.OpdId equals pay.OpdId
                            into paymentGroup
                            from payment in paymentGroup.DefaultIfEmpty()
                            select new
                            {
                                p.OpdDetailId,
                                p.ServiceCategory.ServiceCategoryName,
                                p.ServiceSubCategory.ServiceSubCategoryName,
                                p.OpdId,
                                p.ServiceSubCategoryId,
                                p.ServiceCategoryId,
                                p.ServiceChargeAmount,
                                p.Quantity,
                                p.Discount,
                                p.Total,
                                payment.PaymentDate,
                                payment.Amount,
                                payment.PaymentMode,
                                payment.PaymentType,
                                


                            }).ToList();



                response.OpdDetailList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        //[HttpPost]
        //[Route("getOpdFullDetails")]
        //public ExpandoObject GetOpdFullDetails(RequestModel requestModel)
        //{
        //    dynamic response = new ExpandoObject();
        //    try
        //    {

        //    InstituteDbEntities dbContext = new InstituteDbEntities();
        //    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
        //    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
        //    var data = JsonConvert.DeserializeObject<dynamic>(CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv)
        //    );

        //    int opdId = data.OpdId;

        //    response.ServiceList = dbContext.OpdDetails
        //        .Where(x => x.OpdId == opdId)
        //        .ToList();

        //    response.PaymentList = dbContext.Payments
        //        .Where(x => x.OpdId == opdId)
        //        .ToList();

        //    response.Message = ConstantData.SuccessMessage;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = ex.Message;
        //    }
        //    return response;
        //}


        [HttpPost]
        [Route("CancelOpd")]
        public ExpandoObject CancelOpd(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();

            try
            {
                InstituteDbEntities dbContext = new InstituteDbEntities();

                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);

                var decryptData = CryptoJs.Decrypt(
                    requestModel.request,
                    CryptoJs.key,
                    CryptoJs.iv
                );

                CancelOpdModel model =
                    JsonConvert.DeserializeObject<CancelOpdModel>(decryptData);

                var opd = dbContext.Opds
                    .FirstOrDefault(x => x.OpdId == model.OpdId);

                if (opd == null)
                {
                    response.Message = "OPD not found";
                    return response;
                }

                // ✅ Update cancellation info
                opd.BillStatus = (byte)model.BillStatus;                // 2 = Cancelled
                opd.CancelReason = model.CancelReason;
                opd.OpdCancelDate = model.OpdCancelDate;
                opd.UpdatedBy = model.UpdatedBy;
                opd.UpdatedOn = DateTime.Now;

                dbContext.SaveChanges();

                response.Message = ConstantData.SuccessMessage;
                response.OpdId = opd.OpdId;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

    }
}

