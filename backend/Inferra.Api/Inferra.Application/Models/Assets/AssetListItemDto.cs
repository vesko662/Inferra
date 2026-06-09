using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Assets
{
    public  class AssetListItemDto
    {
        public int Id { get; init; }
        public string Symbol { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string PairSymbol { get; init; } = null!;
        public string? ImageUrl { get; init; }
        public bool IsMlEnabled { get; init; }
        public bool IsNewsEnabled { get; init; }
    }
}
