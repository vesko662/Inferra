using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.News
{
    public class NewsArticleDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
