using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Helper
{
    public class ResponseMVC<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public bool Success => StatusCode >= 200 && StatusCode < 300;

        public int? TotalPages { get; set; }

        public ResponseMVC() { }

        public ResponseMVC(int statusCode, string message = null, T data = default)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }


        public static ResponseMVC<T> SuccessResponse(T data, string message = "Operation completed successfully", int statusCode = 200)
        {
            return new ResponseMVC<T>(statusCode, message, data);
        }

        public static ResponseMVC<T> ErrorResponse(string message, int statusCode = 400)
        {
            return new ResponseMVC<T>(statusCode, message, default);
        }
    }
}
