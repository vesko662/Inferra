from __future__ import annotations

from datetime import date

from pydantic import BaseModel, ConfigDict


def to_camel(value: str) -> str:
    parts = value.split("_")
    return parts[0] + "".join(part.capitalize() for part in parts[1:])


class ApiModel(BaseModel):
    model_config = ConfigDict(
        alias_generator=to_camel,
        populate_by_name=True,
        from_attributes=True,
    )


class CandleDto(ApiModel):
    date: date
    open: float
    high: float
    low: float
    close: float
    volume: float


class AssetDatasetDto(ApiModel):
    symbol: str
    candles: list[CandleDto]

