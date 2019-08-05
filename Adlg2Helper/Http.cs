using System.Net.Http;

//This static http client exists as a fallback if no http client is provided via DI
namespace Adlg2Helper
{
    public static class Http
    {
        public static readonly HttpClient Client = new HttpClient();
    }
}