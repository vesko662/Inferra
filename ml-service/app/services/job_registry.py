from __future__ import annotations

import uuid
from datetime import datetime, timezone
from threading import Lock
from typing import Any

from app.schemas.jobs import JobStatus, PipelineRunResult


class JobRegistry:
    def __init__(self) -> None:
        self._lock = Lock()
        self._jobs: dict[str, dict[str, Any]] = {}

    def create(self, job_type: str) -> str:
        job_id = str(uuid.uuid4())
        with self._lock:
            self._jobs[job_id] = {
                "job_id": job_id,
                "type": job_type,
                "status": "queued",
                "created_at": self._now(),
                "started_at": None,
                "finished_at": None,
                "processed_symbols": [],
                "errors": {},
            }
        return job_id

    def create_or_get_active(self, job_type: str) -> tuple[str, bool]:
        with self._lock:
            for job_id, job in self._jobs.items():
                if job["type"] == job_type and job["status"] in {"queued", "running"}:
                    return job_id, False

            job_id = str(uuid.uuid4())
            self._jobs[job_id] = {
                "job_id": job_id,
                "type": job_type,
                "status": "queued",
                "created_at": self._now(),
                "started_at": None,
                "finished_at": None,
                "processed_symbols": [],
                "errors": {},
            }
            return job_id, True

    def mark_running(self, job_id: str) -> None:
        self.update(job_id, status="running", started_at=self._now())

    def mark_finished(self, job_id: str, result: PipelineRunResult) -> None:
        self.update(
            job_id,
            status=result.status,
            finished_at=self._now(),
            processed_symbols=result.processed_symbols,
            errors=result.errors,
        )

    def mark_failed(self, job_id: str, error: str) -> None:
        self.update(job_id, status="failed", finished_at=self._now(), errors={"job": error})

    def update(self, job_id: str, **changes: Any) -> None:
        with self._lock:
            if job_id in self._jobs:
                self._jobs[job_id].update(changes)

    def get(self, job_id: str) -> dict[str, Any] | None:
        with self._lock:
            job = self._jobs.get(job_id)
            return dict(job) if job else None

    def _now(self) -> str:
        return datetime.now(timezone.utc).isoformat()


job_registry = JobRegistry()
