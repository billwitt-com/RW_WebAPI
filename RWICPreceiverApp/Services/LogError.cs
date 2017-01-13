using RWICPreceiverApp.Models;
using System;

namespace RWICPreceiverApp.Services
{
    public class LogError
    {
        public void WriteToErrorLog(string msg, string fromPage, string stackTrace, string loggedInUser, string comment)
        {
            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                ErrorLog eL = new ErrorLog();
                eL.Date = DateTime.Now;
                eL.Message = msg;
                eL.Comment = comment;
                eL.StackTrace = stackTrace;
                eL.FromPage = fromPage;
                eL.LoggedInUser = loggedInUser;

                try
                {
                    _db.ErrorLogs.Add(eL);
                    int rows = _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string ms = ex.Message;
                }
            }
        }
    }
}