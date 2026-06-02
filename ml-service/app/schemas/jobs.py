from __future__ import annotations

from typing import Literal
from typing import Any

from pydantic import Field

from app.schemas.datasets import ApiModel


JobStatus = Literal["queued", "running", "completed", "failed"]


class TriggerResponse(ApiModel):
    job_id: str
    status: JobStatus
    message: str


class PipelineRunResult(ApiModel):
    status: JobStatus
    processed_symbols: list[str] = Field(default_factory=list)
    errors: dict[str, str] = Field(default_factory=dict)


class JobResponse(ApiModel):
    job_id: str
    type: str
    status: JobStatus
    created_at: str
    started_at: str | None = None
    finished_at: str | None = None
    processed_symbols: list[str] = Field(default_factory=list)
    errors: dict[str, Any] = Field(default_factory=dict)
