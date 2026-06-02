import { useEffect, useMemo, useRef } from "react";
import { CandlestickSeries, ColorType, createChart, LineSeries } from "lightweight-charts";
import { formatCurrency, formatShortDate } from "../../utils/formatters";

const FORECAST_COLORS = {
  LinearRegression: "#f4c96b",
  Lstm: "#c48bff",
  XGBoost: "#ff8a65"
};

function getSnapshotDay(snapshot) {
  if (!snapshot?.updatedAt) {
    return "";
  }

  return snapshot.updatedAt.slice(0, 10);
}

function buildSyntheticSnapshotCandle(latestCandle, snapshot) {
  const snapshotDay = getSnapshotDay(snapshot);
  const snapshotPrice = snapshot?.price;

  if (!snapshotDay || snapshotPrice === null || snapshotPrice === undefined) {
    return null;
  }

  if (!latestCandle?.date || latestCandle.close === null || latestCandle.close === undefined) {
    return null;
  }

  if (snapshotDay <= latestCandle.date) {
    return null;
  }

  const open = latestCandle.close;
  const close = snapshotPrice;

  return {
    time: snapshotDay,
    open,
    high: Math.max(open, close),
    low: Math.min(open, close),
    close
  };
}

function CandleChart({ candles, snapshot, forecastModels = [], mode = "simple" }) {
  const chartRef = useRef(null);
  const containerRef = useRef(null);

  if (!candles.length) {
    return <p className="section-footnote">No historical candles are available for this asset.</p>;
  }

  const latestCandle = candles[candles.length - 1];
  const earliestCandle = candles[0];
  const snapshotDay = getSnapshotDay(snapshot);
  const latestClose = snapshot?.price ?? latestCandle.close;
  const chartData = useMemo(
    () =>
      candles
        .filter((candle) => candle.date)
        .map((candle) => ({
          time: candle.date,
          open: candle.open,
          high: candle.high,
          low: candle.low,
          close: candle.close,
          value: candle.close
        })),
    [candles]
  );
  const chartLineData = useMemo(() => {
    if (!snapshotDay || snapshot?.price === null || snapshot?.price === undefined) {
      return chartData.map((item) => ({
        time: item.time,
        value: item.value
      }));
    }

    const nextData = chartData.map((item) => ({
      time: item.time,
      value: item.value
    }));
    const lastIndex = nextData.length - 1;

    if (nextData[lastIndex]?.time === snapshotDay) {
      nextData[lastIndex] = {
        time: snapshotDay,
        value: snapshot.price
      };
    } else {
      nextData.push({
        time: snapshotDay,
        value: snapshot.price
      });
    }

    return nextData;
  }, [chartData, snapshot?.price, snapshotDay]);
  const syntheticSnapshotCandle = useMemo(
    () => buildSyntheticSnapshotCandle(latestCandle, snapshot),
    [latestCandle, snapshot]
  );
  const advancedCandlestickData = useMemo(() => {
    const historicalData = chartData.map((item) => ({
      time: item.time,
      open: item.open,
      high: item.high,
      low: item.low,
      close: item.close
    }));

    if (syntheticSnapshotCandle) {
      historicalData.push(syntheticSnapshotCandle);
    }

    return historicalData;
  }, [chartData, syntheticSnapshotCandle]);
  const snapshotOverlayData = useMemo(() => {
    if (!snapshotDay || snapshot?.price === null || snapshot?.price === undefined) {
      return [];
    }

    if (syntheticSnapshotCandle) {
      return [];
    }

    if (snapshotDay === latestCandle.date && snapshot.price === latestCandle.close) {
      return [];
    }

    return [
      {
        time: latestCandle.date,
        value: latestCandle.close
      },
      {
        time: snapshotDay,
        value: snapshot.price
      }
    ];
  }, [latestCandle.close, latestCandle.date, snapshot?.price, snapshotDay, syntheticSnapshotCandle]);
  const displayEndDate = snapshotDay || latestCandle.date;
  const forecastSeriesData = useMemo(() => {
    const anchorTime = displayEndDate;
    const anchorValue = latestClose;

    return forecastModels
      .map((forecastModel) => {
        const points = Array.isArray(forecastModel.points)
          ? forecastModel.points
              .filter((point) => point.targetDate && point.predictedPrice !== null && point.predictedPrice !== undefined)
              .map((point) => ({
                time: point.targetDate,
                value: point.predictedPrice
              }))
          : [];

        if (!points.length) {
          return null;
        }

        const firstPoint = points[0];
        const shouldPrependAnchor = firstPoint.time > anchorTime;

        return {
          modelType: forecastModel.modelType || "Forecast",
          color: FORECAST_COLORS[forecastModel.modelType] || "#8bd9ff",
          data: shouldPrependAnchor
            ? [{ time: anchorTime, value: anchorValue }, ...points]
            : points
        };
      })
      .filter(Boolean);
  }, [displayEndDate, forecastModels, latestClose]);

  useEffect(() => {
    if (!containerRef.current || !chartData.length) {
      return undefined;
    }

    const chart = createChart(containerRef.current, {
      autoSize: true,
      height: 360,
      layout: {
        background: { type: ColorType.Solid, color: "transparent" },
        textColor: "#97a7c3"
      },
      grid: {
        vertLines: { color: "rgba(255,255,255,0.05)" },
        horzLines: { color: "rgba(255,255,255,0.05)" }
      },
      timeScale: {
        borderColor: "rgba(255,255,255,0.08)"
      },
      rightPriceScale: {
        borderColor: "rgba(255,255,255,0.08)"
      },
      crosshair: {
        vertLine: { color: "rgba(91, 214, 172, 0.35)" },
        horzLine: { color: "rgba(91, 214, 172, 0.25)" }
      }
    });

    chartRef.current = chart;

    if (mode === "advanced") {
      const series = chart.addSeries(CandlestickSeries, {
        upColor: "#5bd6ac",
        downColor: "#ff7b7b",
        wickUpColor: "#5bd6ac",
        wickDownColor: "#ff7b7b",
        borderVisible: false
      });

      series.setData(advancedCandlestickData);

      if (snapshotOverlayData.length > 0) {
        const snapshotSeries = chart.addSeries(LineSeries, {
          color: "#6ac5ff",
          lineWidth: 2,
          lineStyle: 2,
          crosshairMarkerVisible: true,
          crosshairMarkerRadius: 4,
          lastValueVisible: true,
          priceLineVisible: true
        });

        snapshotSeries.setData(snapshotOverlayData);
      }
    } else {
      const series = chart.addSeries(LineSeries, {
        color: "#5bd6ac",
        lineWidth: 3,
        crosshairMarkerBackgroundColor: "#5bd6ac",
        lastValueVisible: true,
        priceLineVisible: true
      });

      series.setData(
        chartLineData
      );
    }

    forecastSeriesData.forEach((forecastSeries) => {
      const series = chart.addSeries(LineSeries, {
        color: forecastSeries.color,
        lineWidth: 2,
        lineStyle: 2,
        crosshairMarkerVisible: true,
        crosshairMarkerRadius: 3,
        lastValueVisible: true,
        priceLineVisible: false
      });

      series.setData(forecastSeries.data);
    });

    chart.timeScale().fitContent();

    return () => {
      chart.remove();
      chartRef.current = null;
    };
  }, [advancedCandlestickData, chartData, chartLineData, forecastSeriesData, mode, snapshotOverlayData]);

  return (
    <div className="chart-block">
      <div className="chart-summary">
        <div>
          <span className="chart-summary__label">Trend range</span>
          <strong>
            {formatShortDate(earliestCandle.date)} - {formatShortDate(displayEndDate)}
          </strong>
        </div>
        <div>
          <span className="chart-summary__label">
            {snapshot?.price ? "Current snapshot price" : mode === "advanced" ? "Latest candle close" : "Latest close"}
          </span>
          <strong>{formatCurrency(latestClose)}</strong>
        </div>
      </div>

      <div className="chart-frame" aria-label="Historical price chart">
        <div ref={containerRef} className="chart-canvas" role="img" aria-label="Asset historical chart" />
      </div>

      {forecastSeriesData.length > 0 ? (
        <div className="forecast-legend" aria-label="Forecast models">
          {forecastSeriesData.map((forecastSeries) => (
            <div key={forecastSeries.modelType} className="forecast-legend__item">
              <span
                className="forecast-legend__swatch"
                style={{ backgroundColor: forecastSeries.color }}
                aria-hidden="true"
              />
              <span>{forecastSeries.modelType}</span>
            </div>
          ))}
        </div>
      ) : null}

      <div className="chart-footer">
        <span>{formatShortDate(earliestCandle.date)}</span>
        <span>{formatShortDate(displayEndDate)}</span>
      </div>
    </div>
  );
}

export default CandleChart;
