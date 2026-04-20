import { useEffect, useMemo, useRef } from "react";
import { CandlestickSeries, ColorType, createChart, LineSeries } from "lightweight-charts";
import { formatCurrency, formatShortDate } from "../../utils/formatters";

function CandleChart({ candles, mode = "simple" }) {
  const chartRef = useRef(null);
  const containerRef = useRef(null);

  if (!candles.length) {
    return <p className="section-footnote">No historical candles are available for this asset.</p>;
  }

  const latestCandle = candles[candles.length - 1];
  const earliestCandle = candles[0];
  const latestClose = latestCandle.close;
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

      series.setData(
        chartData.map((item) => ({
          time: item.time,
          open: item.open,
          high: item.high,
          low: item.low,
          close: item.close
        }))
      );
    } else {
      const series = chart.addSeries(LineSeries, {
        color: "#5bd6ac",
        lineWidth: 3,
        crosshairMarkerBackgroundColor: "#5bd6ac",
        lastValueVisible: true,
        priceLineVisible: true
      });

      series.setData(
        chartData.map((item) => ({
          time: item.time,
          value: item.value
        }))
      );
    }

    chart.timeScale().fitContent();

    return () => {
      chart.remove();
      chartRef.current = null;
    };
  }, [chartData, mode]);

  return (
    <div className="chart-block">
      <div className="chart-summary">
        <div>
          <span className="chart-summary__label">Trend range</span>
          <strong>
            {formatShortDate(earliestCandle.date)} - {formatShortDate(latestCandle.date)}
          </strong>
        </div>
        <div>
          <span className="chart-summary__label">{mode === "advanced" ? "Latest candle close" : "Latest close"}</span>
          <strong>{formatCurrency(latestClose)}</strong>
        </div>
      </div>

      <div className="chart-frame" aria-label="Historical price chart">
        <div ref={containerRef} className="chart-canvas" role="img" aria-label="Asset historical chart" />
      </div>

      <div className="chart-footer">
        <span>{formatShortDate(earliestCandle.date)}</span>
        <span>{formatShortDate(latestCandle.date)}</span>
      </div>
    </div>
  );
}

export default CandleChart;
