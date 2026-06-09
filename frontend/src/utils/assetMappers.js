function toNullableNumber(value) {
  if (value === null || value === undefined || value === "") {
    return null;
  }

  const parsedValue = Number(value);
  return Number.isNaN(parsedValue) ? null : parsedValue;
}

function toSafeString(value, fallback = "") {
  return typeof value === "string" ? value : fallback;
}

export function normalizeSnapshot(rawSnapshot) {
  if (!rawSnapshot || typeof rawSnapshot !== "object") {
    return null;
  }

  return {
    price: toNullableNumber(rawSnapshot.price),
    priceChange24h: toNullableNumber(rawSnapshot.priceChange24h),
    priceChangePercentage24h: toNullableNumber(rawSnapshot.priceChangePercentage24h),
    volume24h: toNullableNumber(rawSnapshot.volume24h),
    marketCap: toNullableNumber(rawSnapshot.marketCap),
    fullyDilutedValuation: toNullableNumber(rawSnapshot.fullyDilutedValuation),
    updatedAt: toSafeString(rawSnapshot.updatedAt)
  };
}

export function normalizeCandle(rawCandle) {
  if (!rawCandle || typeof rawCandle !== "object") {
    return null;
  }

  return {
    date: toSafeString(rawCandle.date),
    open: toNullableNumber(rawCandle.open),
    high: toNullableNumber(rawCandle.high),
    low: toNullableNumber(rawCandle.low),
    close: toNullableNumber(rawCandle.close),
    volume: toNullableNumber(rawCandle.volume)
  };
}

export function normalizeForecastPoint(rawPoint) {
  if (!rawPoint || typeof rawPoint !== "object") {
    return null;
  }

  return {
    dayOffset: toNullableNumber(rawPoint.dayOffset),
    targetDate: toSafeString(rawPoint.targetDate),
    predictedPrice: toNullableNumber(rawPoint.predictedPrice)
  };
}

export function normalizeForecast(rawForecast) {
  if (!rawForecast || typeof rawForecast !== "object") {
    return null;
  }

  return {
    modelType: toSafeString(rawForecast.modelType),
    modelVersion: toSafeString(rawForecast.modelVersion),
    generatedAt: toSafeString(rawForecast.generatedAt),
    forecastStartDate: toSafeString(rawForecast.forecastStartDate),
    forecastEndDate: toSafeString(rawForecast.forecastEndDate),
    horizonDays: toNullableNumber(rawForecast.horizonDays),
    points: Array.isArray(rawForecast.points)
      ? rawForecast.points.map(normalizeForecastPoint).filter(Boolean)
      : []
  };
}

export function normalizeAssetListItem(rawAsset) {
  return {
    id: toNullableNumber(rawAsset?.id),
    symbol: toSafeString(rawAsset?.symbol),
    name: toSafeString(rawAsset?.name),
    pairSymbol: toSafeString(rawAsset?.pairSymbol),
    imageUrl: toSafeString(rawAsset?.imageUrl),
    isMlEnabled: rawAsset?.isMlEnabled ?? false,
    isNewsEnabled: rawAsset?.isNewsEnabled ?? false,
    snapshot: null
  };
}

export function normalizeAssetDetails(rawAsset) {
  return {
    id: toNullableNumber(rawAsset?.id),
    symbol: toSafeString(rawAsset?.symbol),
    name: toSafeString(rawAsset?.name),
    pairSymbol: toSafeString(rawAsset?.pairSymbol),
    coinGeckoId: toSafeString(rawAsset?.coinGeckoId),
    imageUrl: toSafeString(rawAsset?.imageUrl),
    circulatingSupply: toNullableNumber(rawAsset?.circulatingSupply),
    totalSupply: toNullableNumber(rawAsset?.totalSupply),
    maxSupply: toNullableNumber(rawAsset?.maxSupply),
    snapshot: normalizeSnapshot(rawAsset?.snapshot)
  };
}

export function mergeAssetWithSnapshot(asset, snapshot) {
  return {
    ...asset,
    snapshot: snapshot || asset.snapshot || null
  };
}

export function getAssetRouteKey(asset) {
  return asset?.symbol || "";
}

export function getAssetDisplayLabel(asset, index = 0) {
  if (asset?.name) {
    return asset.name;
  }

  if (asset?.symbol) {
    return asset.symbol;
  }

  return `Asset ${index + 1}`;
}

export function getAssetSearchText(asset) {
  return [asset?.name, asset?.symbol, asset?.pairSymbol].filter(Boolean).join(" ").toLowerCase();
}
