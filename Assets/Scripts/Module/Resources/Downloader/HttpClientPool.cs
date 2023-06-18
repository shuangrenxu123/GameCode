using System;
using System.Collections.Generic;
using System.Net.Http;
public static class HttpClientPool
{
    private static readonly Queue<HttpClient> httpClientPool = new Queue<HttpClient>();
    private static System.Object HttpClientPoolLockRoot = new();

    public static HttpClient GetHttpClient()
    {
        HttpClient httpClient = null;
        lock (HttpClientPoolLockRoot)
        {
            try
            {
                if (httpClientPool.Count > 0)
                {
                    httpClient = httpClientPool.Dequeue();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        return httpClient ?? new HttpClient() { Timeout = TimeSpan.FromSeconds(10.0) };
    }
    public static void RecoverHttpClient(HttpClient httpClient)
    {
        lock (HttpClientPoolLockRoot)
        {
            try
            {
                httpClientPool.Enqueue(httpClient);
            }
            catch (Exception e)
            {
                throw new Exception("回收 httpclient 失败");
            }
        }
    }
}