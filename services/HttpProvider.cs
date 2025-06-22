using AutoBuilderlauncher.Model;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Util.Services
{
    public class HttpProvider
    {
        private static readonly AsyncLock mutex = new AsyncLock();
        private readonly HttpClient httpClient;   
        private int Timeout;
       
        public HttpProvider(int Timeout = 0)
        {
            var handler = new HttpClientHandler();

            // HTTPS 인증서 검증을 무시하는 콜백 설정 (보안 위험 있음)
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            if(Timeout == 0)
            {
                httpClient = new HttpClient();
            }
            else
            {
                httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMinutes(Timeout)
                };
            }
        }

        private async Task<HttpResultData<TContext>> HttpSendJsonStreamMessage<TContext>(string PostMsg, string url, string encode,
                                                                                         string Method = "POST")
            where TContext : class, new()
        {            
            HttpResultData<TContext> Result = new HttpResultData<TContext>();

            try
            {
                var request = new HttpRequestMessage(new HttpMethod(Method), url);

                if (PostMsg != null)
                {
                    var content = new StringContent(PostMsg, Encoding.GetEncoding(encode), "application/json");
                    request.Content = content;
                }

                var response = await httpClient.SendAsync(request);
                string ResponseGetString = await response.Content.ReadAsStringAsync();

                Result.HttpStatusCode = (int)response.StatusCode;
                Result.ResponseData = ResponseGetString;
            }
            catch (HttpRequestException ex)
            {
                Result.HttpStatusCode = -1;
                Result.ResponseData = ex.Message;
            }

            return Result;
        }

        public async Task<HttpResultData<TContext>> HttpSendMessage<TContext>(TContext SendMessage, string url, string encode,
                                                                              string Method = "POST")
            where TContext : class, new()
        {
            HttpResultData<TContext> Result = new HttpResultData<TContext>();

            using (await mutex.LockAsync())
            {
                try
                {
                    string PostMsg = string.Empty;

                    ConverterJson json = new ConverterJson();
                    PostMsg = json.SerializeToJson<TContext>(SendMessage);
                    Result = await HttpSendJsonStreamMessage<TContext>(PostMsg, url, encode, Method);
                }
                catch (Exception ex)
                {
                    Result.HttpStatusCode = -1;
                    Result.ResponseData = ex.Message;
                }
            }
            return Result;
        }

        private async Task<HttpResultData<TContext>> HttpGetReponseMessage<TContext>(string Url)
            where TContext : class, new()
        {
            HttpResultData<TContext> Result = new HttpResultData<TContext>();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, Url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.SendAsync(request);
                string ResponseGetString = await response.Content.ReadAsStringAsync();

                Result.HttpStatusCode = (int)response.StatusCode;
                Result.ResponseData = ResponseGetString;
            }
            catch (HttpRequestException ex)
            {
                Result.HttpStatusCode = -1;
                Result.ResponseData = ex.Message;
            }

            return Result;
        }

        public async Task<HttpResultData<TContext>> HTTPGetMessage<TContext>(string Url)
            where TContext : class, new()
        {
            using (await mutex.LockAsync())
            {
                HttpResultData<TContext> Result = new HttpResultData<TContext>();

                try
                {
                    Result = await HttpGetReponseMessage<TContext>(Url);

                    if (Result.HttpStatusCode == 200)
                    {
                        ConverterJson json = new ConverterJson();
                        Result.ModelContext = json.DeserializeFromJson<TContext>(Result.ResponseData);
                    }
                }
                catch (Exception ex)
                {
                    Result.HttpStatusCode = -1;
                    Result.ResponseData = ex.Message;
                }

                return Result;
            }
        }

        public async Task<HttpResultData<TContext>> HTTPGetMessages<TContext>(string Url)
            where TContext : class, new()
        {
            using (await mutex.LockAsync())
            {
                HttpResultData<TContext> Result = new HttpResultData<TContext>();
                try
                {
                    Result = await HttpGetReponseMessage<TContext>(Url);

                    if (Result.HttpStatusCode == 200)
                    {
                        ConverterJson json = new ConverterJson();
                        Result.ArrayModelContext = json.DeserializeFromArrayJson<TContext>(Result.ResponseData);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Result.HttpStatusCode = -1;
                    Result.ResponseData = ex.Message;
                }
                return Result;
            }
        }

        public string JoinUriSegments(string uri, params string[] segments)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return null;

            if (segments == null || segments.Length == 0)
                return uri;

            return segments.Aggregate(uri, (current, segment) => $"{current.TrimEnd('/')}/{segment.TrimStart('/')}");
        }
    }
}
