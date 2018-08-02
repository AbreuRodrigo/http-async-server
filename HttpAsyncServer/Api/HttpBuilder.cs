namespace HttpAsyncServer
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError()
        {
            return new HttpResponse()
            {
                HttpResponseStatus = HttpStatus.InternalServerError,
                ContentAsUTF8 = HttpStatus.InternalServerError.Reason
            };
        }

        public static HttpResponse NotFound()
        {
            return new HttpResponse()
            {
                HttpResponseStatus = HttpStatus.NotFound,
                ContentAsUTF8 = HttpStatus.NotFound.Reason
            };
        }

        public static HttpResponse Forbidden()
        {
            return new HttpResponse()
            {
                HttpResponseStatus = HttpStatus.Forbidden,
                ContentAsUTF8 = HttpStatus.Forbidden.Reason
            };
        }

        public static HttpResponse Ok()
        {
            return new HttpResponse()
            {
                HttpResponseStatus = HttpStatus.Ok,
                ContentAsUTF8 = HttpStatus.Ok.Reason
            };
        }
    }
}