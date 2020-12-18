using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MQuery.AspNetCore.Sample.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        public class Book
        {
            public Book(string name, string author, DateTime pubDate, double price)
            {
                Name = name;
                Author = author;
                PubDate = pubDate;
                Price = price;
            }

            public string Name { get; set; }

            public string Author { get; set; }

            public DateTime PubDate { get; set; }

            public double Price { get; set; }
        }

        public IEnumerable<Book> QueryBooks(Query<Book> query)
        {
            var books = new List<Book>
            {
                new Book("乘战车的人", "Riders in the Chariot",DateTime.Parse("2021-1"), 98),
                new Book("华盛顿广场", "Henry James",DateTime.Parse("2020-09"), 24.99),
                new Book("风暴眼", "〔澳〕帕特里克·怀特",DateTime.Parse("2020-01"), 32.99),
                new Book("马人", "〔美〕约翰·厄普代克（John Updike）",DateTime.Parse("2017-08"), 33.99),
                new Book("消失的艺术", "〔西〕恩里克·比拉-马塔斯",DateTime.Parse("2018-10"), 28.00),
                new Book("天竺葵", "〔美〕弗兰纳里·奥康纳",DateTime.Parse("2016-10"), 28.00),
            };
            return query.ApplyTo(books.AsQueryable());
        }
    }
}
