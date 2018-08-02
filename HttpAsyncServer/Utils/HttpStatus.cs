namespace HttpAsyncServer
{
    public class HttpStatus
    {
        public readonly static HttpResponseStatus Continue = new HttpResponseStatus("100", "Continue");
        public readonly static HttpResponseStatus Ok = new HttpResponseStatus("200", "Ok");
        public readonly static HttpResponseStatus Created = new HttpResponseStatus("201", "Created");
        public readonly static HttpResponseStatus Accepted = new HttpResponseStatus("202", "Accepted");
        public readonly static HttpResponseStatus NonAuthoritativeInformation = new HttpResponseStatus("203", "Non Authoritative Information");
        public readonly static HttpResponseStatus NoContent = new HttpResponseStatus("204", "No Content");
        public readonly static HttpResponseStatus ResetContent = new HttpResponseStatus("205", "ResetContent");
        public readonly static HttpResponseStatus MovedPermanently = new HttpResponseStatus("301", "Moved Permanently");
        public readonly static HttpResponseStatus Found = new HttpResponseStatus("302", "Found");
        public readonly static HttpResponseStatus NotModified = new HttpResponseStatus("304", "Not Modified");
        public readonly static HttpResponseStatus BadRequest = new HttpResponseStatus("400", "Bad Request");
        public readonly static HttpResponseStatus Unauthorized = new HttpResponseStatus("401", "Unauthorized");
        public readonly static HttpResponseStatus Forbidden = new HttpResponseStatus("403", "Forbidden");
        public readonly static HttpResponseStatus NotFound = new HttpResponseStatus("404", "Not Found");
        public readonly static HttpResponseStatus MethodNotAllowed = new HttpResponseStatus("405", "Method Not Allowed");
        public readonly static HttpResponseStatus TooManyRequests = new HttpResponseStatus("429", "Too Many Requests");
        public readonly static HttpResponseStatus InternalServerError = new HttpResponseStatus("500", "Internal ServerError");
        public readonly static HttpResponseStatus NotImplemented = new HttpResponseStatus("501", "Not Implemented");
        public readonly static HttpResponseStatus BadGateway = new HttpResponseStatus("502", "Bad Gateway");
        public readonly static HttpResponseStatus ServiceUnavailable = new HttpResponseStatus("503", "Service Unavailable");
        public readonly static HttpResponseStatus GatewayTimeout = new HttpResponseStatus("504", "Gateway Timeout");
        public readonly static HttpResponseStatus HTTPVersionNotSupported = new HttpResponseStatus("505", "HTTP Version Not Supported");
    }
}