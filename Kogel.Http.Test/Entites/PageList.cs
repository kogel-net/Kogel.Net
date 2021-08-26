using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Http.Test.Entites
{
    public class PageList<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalPage { get; set; }
        public bool HasPrev { get; set; }
        public bool HasNext { get; set; }
    }
}
