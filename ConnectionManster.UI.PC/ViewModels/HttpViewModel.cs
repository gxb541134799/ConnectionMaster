using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public class HttpViewModel:ObservableObject
    {
        private CancellationTokenSource source;

        public HttpViewModel(FormatterViewModel formatterViewModel)
        {
            FormatterViewModel = formatterViewModel;
            Methods = new HttpMethod[]
            {
                HttpMethod.Get,
                HttpMethod.Post,
                HttpMethod.Put,
                HttpMethod.Delete,
                HttpMethod.Patch
            };
            Versions = new string[]
            {
                "1.1","2.0"
            };
            ContentTypes = new string[]
            {
                "application/json",
                "application/x-www-form-urlencoded",
                "text/xml"
            };
            ContentType = ContentTypes.First();

            SendCommand = new Command(Send,()=>!Sending);
            CancelCommand = new Command(Cancel, () => Sending);
        }

        private string _url = "http://";
        public string Url
        {
            get { return _url; }
            set { SetValue(ref _url, value, nameof(Url)); }
        }

        private HttpMethod _method = HttpMethod.Get;
        public HttpMethod Method
        {
            get { return _method; }
            set { SetValue(ref _method, value, nameof(Method)); }
        }

        public HttpMethod[] Methods { get; }

        public string ContentType { get; set; }

        public string[] ContentTypes { get; }

        public string Content { get; set; }

        public string Header { get; set; }

        private string _version = "1.1";
        public string Version
        {
            get { return _version; }
            set { SetValue(ref _version, value, nameof(Version)); }
        }

        public string[] Versions { get; }

        public FormatterViewModel FormatterViewModel { get; }

        private string _result;
        public string Result
        {
            get { return _result; }
            set { SetValue(ref _result, value, nameof(Result)); }
        }

        private bool _sending = false;
        public bool Sending
        {
            get { return _sending; }
            set { SetValue(ref _sending, value, nameof(Sending)); }
        }

        public Command SendCommand { get; }

        public Command CancelCommand { get; }

        private async void Send()
        {
            Uri uri;
            if(!Uri.TryCreate(Url,UriKind.Absolute,out uri))
            {
                Notify.ShowError("URL无效", "校验");
                return;
            }
            if(!Regex.IsMatch(Url, @"^https?://", RegexOptions.IgnoreCase))
            {
                Notify.ShowError("URL必须是http或https协议", "校验");
                return;
            }
            Sending = true;
            using(var http = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    Method = Method,
                    RequestUri = uri,
                    Version = System.Version.Parse(Version),
                };
                SetHeaders(request);
                SetContent(request);
                if(source != null)
                {
                    source.Dispose();
                }
                source = new CancellationTokenSource();
                HttpResponseMessage response;
                try
                {
                    response = await http.SendAsync(request,source.Token);
                }
                catch(OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Notify.ShowError(ex.Message, "请求出现异常");
                    return;
                }
                finally
                {
                    Sending = false;
                }
                var message = new StringBuilder();
                message.AppendLine($"{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
                message.AppendLine(response.Headers.ToString());
                if (response.Content != null)
                {
                    message.AppendLine();
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    message.AppendLine(FormatterViewModel.Formatter.FromBytes(bytes));
                }
               Result =message.ToString();
            }
        }

        private void Cancel()
        {
            source.Cancel();
        }

        private void SetContent(HttpRequestMessage request)
        {
            if (Method != HttpMethod.Get && Method != HttpMethod.Delete)
            {
                var bytes =string.IsNullOrEmpty(Content) ? new byte[0] : FormatterViewModel.Formatter.FromString(Content);
                request.Content = new StreamContent(new MemoryStream(bytes));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
            }
        }

        private void SetHeaders(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(Header))
            {
                var headerItems = Regex.Split(Header, @"[\r\n]");
                var pairRegex = new Regex(@"^(?<name>[^:：]+?)[\s:：]+(?<value>.*)$");
                foreach (var item in headerItems)
                {
                    var pairMatch = pairRegex.Match(item);
                    request.Headers.TryAddWithoutValidation(pairMatch.Groups["name"].Value, pairMatch.Groups["value"].Value);
                }
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(Sending))
            {
                SendCommand.OnCanExecuteChanged();
                CancelCommand.OnCanExecuteChanged();
            }
        }
    }
}
