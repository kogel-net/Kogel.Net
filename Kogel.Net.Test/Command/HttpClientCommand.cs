using Kogel.Net.Http;
using Kogel.Net.Http.Interfaces;
using Kogel.Net.Test.Entites;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;

namespace Kogel.Net.Test.Command
{
    /// <summary>
    /// http请求执行
    /// </summary>
    public class HttpClientCommand : ICommand
    {
        IHttpClient httpClient;
        IFileClient fileClient;
        public HttpClientCommand(IHttpClient httpClient, IFileClient fileClient)
        {
            this.httpClient = httpClient;
            this.fileClient = fileClient;
        }

        string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOiIxNjI5OTQxNTM5IiwiZXhwIjoxNjMyNTMzNTM5LCJVc2VySWQiOiI0IiwiVXNlckNvZGUiOiJwZXRlciIsIlVzZXJOYW1lIjoicGV0ZXIiLCJHcm91cENvZGUiOiJBZG1pbiIsIkdyb3VwTmFtZSI6IueuoeeQhuWRmCIsImlzcyI6ImxvY2FsaG9zdCIsImF1ZCI6ImxvY2FsaG9zdCJ9.5KcKQYg0zq0ySo0CslcJB38fsMGVGhNcrRU1R-OM5Sc";

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            //aop监听请求(只会作用于当前上下文)
            //执行前
            HttpBase.Aop.OnExecuting += Aop_OnExecuting;
            //执行后
            HttpBase.Aop.OnExecuted += Aop_OnExecuted;

            //get请求
            Get();

            //post请求
            Post();

            //自定义请求
            Request();

            //文件下载
            DownLoad();

            //文件上传
            Upload();
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="request"></param>
        private void Aop_OnExecuting(KogelRequest request)
        {

        }

        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="request"></param>
        private void Aop_OnExecuted(KogelRequest request)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        private void Get()
        {
            var responseText = httpClient.Get("https://www.baidu.com/");
            Console.WriteLine(responseText);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Post()
        {
            var response = httpClient.Post("http://159.75.232.135:81/api/basic/cost_info/get_list", new { cost_code = "837" }, accessToken);
            //response.StatusCode//状态码
            Console.WriteLine(response.Result);

            //指定类型返回
            var resultResponse = httpClient.Post<ResultResponse<PageList<GetCostInfoListReponse>>>("http://159.75.232.135:81/api/basic/cost_info/get_list", new { cost_code = "837" }, accessToken);
            Console.WriteLine(JsonConvert.SerializeObject(resultResponse));
        }

        /// <summary>
        /// 自定义请求
        /// </summary>
        private void Request()
        {
            //参数
            var jsonData = JsonConvert.SerializeObject(new { cost_code = "837" });
            var byteArr = Encoding.UTF8.GetBytes(jsonData);

            //请求头
            WebHeaderCollection header = new WebHeaderCollection();
            header.Add("Authorization", $"Bearer {accessToken}");

            //开始请求
            var response = httpClient.Request(new KogelRequest
            {
                Method = "post",
                Url = "http://159.75.232.135:81/api/basic/cost_info/get_list",
                ContentType = "application/json",
                PostDataType = PostDataType.Byte,
                PostDataByte = byteArr,
                Header = header
            });
            //response.StatusCode//状态码
            Console.WriteLine(response.Result);
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        private void DownLoad()
        {
            string path = $"{Directory.GetCurrentDirectory()}\\abc.png";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            fileClient.Download("http://159.75.232.135:81/files/abc.png", path);
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        private void Upload()
        {
            string path = $"{Directory.GetCurrentDirectory()}\\abc.png";
            var resultResponse = fileClient.Upload("https://localhost:44370/api/file/uplpad?suffix=png", path, accessToken);
            Console.WriteLine(JsonConvert.SerializeObject(resultResponse));
        }
    }
}
