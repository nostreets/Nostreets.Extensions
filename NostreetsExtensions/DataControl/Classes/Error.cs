using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NostreetsExtensions.DataControl.Classes
{
    public class Error : DBObject
    {
        public Error(Exception ex)
        {
            DateCreated = DateTime.Now;
            Message = ex.Message;
            StackTrace = ex.StackTrace;
            Source = ex.Source;
            Message = ex.Message;
            HelpLink = ex.HelpLink;
            Method = ex.TargetSite.NameWithParams();
        }


        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string Method { get; set; }
        public string HelpLink { get; set; }

        [NotMapped]
        public override string ModifiedUserId { get; set; }
        [NotMapped]
        public override bool IsDeleted { get; set; }
        [NotMapped]
        public override DateTime? DateModified { get; set; }


    }
}
