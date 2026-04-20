export function formatCurrency(value) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) {
    return "N/A";
  }

  const amount = Number(value);

  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    maximumFractionDigits: amount >= 1 ? 2 : 6
  }).format(amount);
}

export function formatCompactNumber(value) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) {
    return "N/A";
  }

  return new Intl.NumberFormat("en-US", {
    notation: "compact",
    maximumFractionDigits: 2
  }).format(Number(value));
}

export function formatPercentage(value) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) {
    return "N/A";
  }

  const amount = Number(value);
  const prefix = amount > 0 ? "+" : "";
  return `${prefix}${amount.toFixed(2)}%`;
}

export function formatDateTime(value) {
  if (!value) {
    return "N/A";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "N/A";
  }

  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(date);
}

export function formatShortDate(value) {
  if (!value) {
    return "N/A";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "N/A";
  }

  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric"
  }).format(date);
}

export function formatRelativeTime(value) {
  if (!value) {
    return "just now";
  }

  const date = new Date(value);
  const seconds = Math.round((date.getTime() - Date.now()) / 1000);

  if (Number.isNaN(seconds)) {
    return "just now";
  }

  const formatter = new Intl.RelativeTimeFormat("en", { numeric: "auto" });
  const divisions = [
    { amount: 60, unit: "second" },
    { amount: 60, unit: "minute" },
    { amount: 24, unit: "hour" },
    { amount: 7, unit: "day" },
    { amount: 4.34524, unit: "week" },
    { amount: 12, unit: "month" },
    { amount: Number.POSITIVE_INFINITY, unit: "year" }
  ];

  let duration = seconds;

  for (const division of divisions) {
    if (Math.abs(duration) < division.amount) {
      return formatter.format(Math.round(duration), division.unit);
    }

    duration /= division.amount;
  }

  return "just now";
}
