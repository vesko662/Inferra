import { useMemo } from "react";
import { formatDateTime } from "../../utils/formatters";

// Gauge segments: bearish → neutral → bullish (left to right)
const SEGMENTS = [
  { color: "#e63946" },
  { color: "#f4722b" },
  { color: "#f4a825" },
  { color: "#a8c023" },
  { color: "#4caf50" }
];

const NUM_SEGMENTS = SEGMENTS.length;
const R = 80;          // arc radius
const CX = 110;        // center x
const CY = 100;        // center y (bottom of semicircle)
const NEEDLE_LEN = 68;
const NEEDLE_BASE = 6;

function polarToCartesian(cx, cy, r, angleDeg) {
  const rad = ((angleDeg - 180) * Math.PI) / 180;
  return {
    x: cx + r * Math.cos(rad),
    y: cy + r * Math.sin(rad)
  };
}

function segmentPath(cx, cy, r, startAngle, endAngle, thickness = 18) {
  const inner = r - thickness;
  const s1 = polarToCartesian(cx, cy, r, startAngle);
  const e1 = polarToCartesian(cx, cy, r, endAngle);
  const s2 = polarToCartesian(cx, cy, inner, endAngle);
  const e2 = polarToCartesian(cx, cy, inner, startAngle);
  return `M ${s1.x} ${s1.y} A ${r} ${r} 0 0 1 ${e1.x} ${e1.y} L ${s2.x} ${s2.y} A ${inner} ${inner} 0 0 0 ${e2.x} ${e2.y} Z`;
}

function needlePath(cx, cy, angleDeg, len, base) {
  const tip = polarToCartesian(cx, cy, len, angleDeg);
  const left = polarToCartesian(cx, cy, base, angleDeg - 90);
  const right = polarToCartesian(cx, cy, base, angleDeg + 90);
  return `M ${left.x} ${left.y} L ${tip.x} ${tip.y} L ${right.x} ${right.y} Z`;
}

function SentimentGauge({ sentiment }) {
  const { bullish = 0, bearish = 0, neutral = 0, generatedAt } = sentiment ?? {};

  const total = bullish + bearish + neutral;

  // Score 0–100: 0 = pure bearish, 50 = balanced, 100 = pure bullish
  const score = useMemo(() => {
    if (total === 0) return 50;
    return Math.round(((bullish + neutral * 0.5) / total) * 100);
  }, [bullish, neutral, total]);

  const label = useMemo(() => {
    if (score >= 75) return "Bullish";
    if (score >= 55) return "Slightly Bullish";
    if (score >= 45) return "Neutral";
    if (score >= 25) return "Slightly Bearish";
    return "Bearish";
  }, [score]);

  const labelColor = useMemo(() => {
    if (score >= 75) return "#4caf50";
    if (score >= 55) return "#a8c023";
    if (score >= 45) return "#f4a825";
    if (score >= 25) return "#f4722b";
    return "#e63946";
  }, [score]);

  // Map score (0–100) → angle (0°–180°)
  const needleAngle = (score / 100) * 180;

  const GAP = 2; // degrees gap between segments
  const totalAngle = 180;
  const segAngle = (totalAngle - GAP * (NUM_SEGMENTS - 1)) / NUM_SEGMENTS;

  const bullishPct = total > 0 ? Math.round((bullish / total) * 100) : 0;
  const bearishPct = total > 0 ? Math.round((bearish / total) * 100) : 0;
  const neutralPct = total > 0 ? Math.round((neutral / total) * 100) : 0;

  return (
    <div className="sentiment-gauge">
      <svg viewBox="0 0 220 115" className="sentiment-gauge__svg" aria-label={`Sentiment gauge: ${label}`}>
        {/* Segments */}
        {SEGMENTS.map((seg, i) => {
          const start = i * (segAngle + GAP);
          const end = start + segAngle;
          return (
            <path
              key={i}
              d={segmentPath(CX, CY, R, start, end, 20)}
              fill={seg.color}
              opacity={0.85}
            />
          );
        })}

        {/* Needle */}
        <path
          d={needlePath(CX, CY, needleAngle, NEEDLE_LEN, NEEDLE_BASE)}
          fill="white"
          filter="drop-shadow(0 1px 3px rgba(0,0,0,0.5))"
        />

        {/* Pivot dot */}
        <circle cx={CX} cy={CY} r={7} fill="#1e2a3a" stroke="white" strokeWidth={2} />

        {/* Score label */}
        <text x={CX} y={CY - 30} textAnchor="middle" fill="white" fontSize="22" fontWeight="700">
          {score}
        </text>
        <text x={CX} y={CY - 14} textAnchor="middle" fill={labelColor} fontSize="9" fontWeight="600" letterSpacing="0.5">
          {label.toUpperCase()}
        </text>
      </svg>

      {/* Stats row */}
      <div className="sentiment-gauge__stats">
        <div className="sentiment-stat sentiment-stat--bearish">
          <span className="sentiment-stat__dot" />
          <span className="sentiment-stat__label">Bearish</span>
          <span className="sentiment-stat__value">{bearishPct}%</span>
        </div>
        <div className="sentiment-stat sentiment-stat--neutral">
          <span className="sentiment-stat__dot" />
          <span className="sentiment-stat__label">Neutral</span>
          <span className="sentiment-stat__value">{neutralPct}%</span>
        </div>
        <div className="sentiment-stat sentiment-stat--bullish">
          <span className="sentiment-stat__dot" />
          <span className="sentiment-stat__label">Bullish</span>
          <span className="sentiment-stat__value">{bullishPct}%</span>
        </div>
      </div>

      {generatedAt && (
        <p className="sentiment-gauge__timestamp">
          Updated {formatDateTime(generatedAt)}
        </p>
      )}
    </div>
  );
}

export default SentimentGauge;
