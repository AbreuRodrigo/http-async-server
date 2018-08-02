using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace HttpAsyncServer
{
    public class RouteHandler
    {
        public string BasePath { get; set; }
        public bool ShowDirectories { get; set; }

        public HttpResponse Handle(HttpRequest request)
        {
            var url_part = request.Path;

            url_part = url_part.Replace("\\..\\", "\\");
            url_part = url_part.Replace("/../", "/");
            url_part = url_part.Replace("//", "/");
            url_part = url_part.Replace(@"\\", @"\");
            url_part = url_part.Replace(":", "");
            url_part = url_part.Replace("/", Path.DirectorySeparatorChar.ToString());

            if (url_part.Length > 0)
            {
                var first_char = url_part.ElementAt(0);
                if (first_char == '/' || first_char == '\\')
                {
                    url_part = "." + url_part;
                }
            }
            var local_path = Path.Combine(BasePath, url_part);

            if (ShowDirectories && Directory.Exists(local_path))
            {
                return HandleLocalDir(request, local_path);
            }
            else if (File.Exists(local_path))
            {
                return HandleLocalFile(request, local_path);
            }
            else
            {
                return new HttpResponse
                {
                    HttpResponseStatus = HttpStatus.NotFound
                };
            }
        }

        HttpResponse HandleLocalFile(HttpRequest request, string local_path)
        {
            var file_extension = Path.GetExtension(local_path);

            var response = new HttpResponse();
            response.HttpResponseStatus = HttpStatus.Ok;
            response.Headers[Consts.CONTENT_TYPE_TEXT] = QuickMimeTypeMapper.GetMimeType(file_extension);
            response.Content = File.ReadAllBytes(local_path);

            return response;
        }

        HttpResponse HandleLocalDir(HttpRequest request, string local_path)
        {
            var output = new StringBuilder();
            output.Append(string.Format("<h1> Directory: {0} </h1>", request.Url));

            foreach (var entry in Directory.GetFiles(local_path))
            {
                var file_info = new System.IO.FileInfo(entry);

                var filename = file_info.Name;
                output.Append(string.Format("<a href=\"{1}\">{1}</a> <br>", filename, filename));
            }

            return new HttpResponse()
            {
                HttpResponseStatus = HttpStatus.Ok,
                ContentAsUTF8 = output.ToString(),
            };
        }
    }
}