using System;

namespace PTTEP_Risk.Model
{
    public abstract class BaseMessage<T>
    {
        protected string _MessageId = string.Empty;
        public string CodeVersion { get { return "1.0.0.6"; } }
        public string MessageId { get { return _MessageId; } }
        public string TimeStamp { get { return DateTime.Now.ToString("yyyyMMddHHmmss"); } }
        public string SessionEmpID { get; set; }
        public string Email { get; set; }
        public string SecurityCode { get; set; }
        public string Module { get; set; }
        public string Token { get; set; }
        public T body { get; set; }
    }

    public class RequestMessage<T> : BaseMessage<T>
    {
        public RequestMessage()
        {
            this._MessageId = Guid.NewGuid().ToString();
        }

        public bool IsAllow()
        {
            //todo:access with permission
            return true;
        }
    }

    public class ResponseMessage<T> : BaseMessage<T>
    {
        public bool Status { get; set; }
        public int StatusId { get; set; }
        public int ReqId { get; set; }

        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public int Return_Id { get; set; }

        public ResponseMessage()
        {
            this._MessageId = Guid.NewGuid().ToString();
        }
        public ResponseMessage(string requestMessageId)
        {
            this._MessageId = requestMessageId;
        }
    }

    public class RequestGrid<T> : BaseMessage<T>
    {
        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public string SortFieldName { get; set; }
        public string SortDirection { get; set; }

        public RequestGrid()
        {
            this._MessageId = Guid.NewGuid().ToString();
        }

        public bool IsAllow()
        {
            //todo:access with permission
            return true;
        }
    }

    public class ResponseGrid<T> : BaseMessage<T>
    {
        public bool Status { get; set; }
        public string ErrorMessage { get; set; }
        public int Total { get; set; }

        public ResponseGrid()
        {
            this._MessageId = Guid.NewGuid().ToString();
        }
        public ResponseGrid(string requestMessageId)
        {
            this._MessageId = requestMessageId;
        }
    }
}
