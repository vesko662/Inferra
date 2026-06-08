from __future__ import annotations

import logging

from fastapi import APIRouter, BackgroundTasks, Depends, HTTPException

from app.clients.dotnet_api_client import DotNetApiClient
from app.core import settings
from app.dependencies import verify_api_key
from app.schemas.jobs import JobResponse, TriggerResponse
from app.services.job_registry import job_registry
from app.services.model_store import ModelStore
from app.services.prediction_service import PredictionService
from app.services.training_service import TrainingService


logger = logging.getLogger(__name__)
router = APIRouter(tags=["ML pipelines"])


@router.post("/train", response_model=TriggerResponse, dependencies=[Depends(verify_api_key)])
async def train(background_tasks: BackgroundTasks) -> TriggerResponse:
    job_id, created = job_registry.create_or_get_active("training")
    if not created:
        job = job_registry.get(job_id)
        return TriggerResponse(
            job_id=job_id,
            status=job["status"] if job else "running",
            message="Training pipeline is already queued or running.",
        )

    if settings.run_pipelines_in_background:
        background_tasks.add_task(run_training_job, job_id)
        status = "queued"
        message = "Training pipeline started."
    else:
        await run_training_job(job_id)
        job = job_registry.get(job_id)
        status = job["status"] if job else "completed"
        message = "Training pipeline finished."

    return TriggerResponse(
        job_id=job_id,
        status=status,
        message=message,
    )


@router.post("/predict-daily", response_model=TriggerResponse, dependencies=[Depends(verify_api_key)])
async def predict_daily(background_tasks: BackgroundTasks) -> TriggerResponse:
    job_id, created = job_registry.create_or_get_active("daily-prediction")
    if not created:
        job = job_registry.get(job_id)
        return TriggerResponse(
            job_id=job_id,
            status=job["status"] if job else "running",
            message="Daily prediction pipeline is already queued or running.",
        )

    if settings.run_pipelines_in_background:
        background_tasks.add_task(run_prediction_job, job_id)
        status = "queued"
        message = "Daily prediction pipeline started."
    else:
        await run_prediction_job(job_id)
        job = job_registry.get(job_id)
        status = job["status"] if job else "completed"
        message = "Daily prediction pipeline finished."

    return TriggerResponse(
        job_id=job_id,
        status=status,
        message=message,
    )


@router.get("/jobs/{job_id}", response_model=JobResponse)
async def get_job(job_id: str) -> JobResponse:
    job = job_registry.get(job_id)
    if job is None:
        raise HTTPException(status_code=404, detail="Job not found.")
    return JobResponse.model_validate(job)


async def run_training_job(job_id: str) -> None:
    job_registry.mark_running(job_id)
    try:
        service = TrainingService(DotNetApiClient(), ModelStore())
        result = await service.train_all()
        job_registry.mark_finished(job_id, result)
    except Exception as exc:  # noqa: BLE001
        logger.exception("Training job failed.")
        job_registry.mark_failed(job_id, str(exc))


async def run_prediction_job(job_id: str) -> None:
    job_registry.mark_running(job_id)
    try:
        service = PredictionService(DotNetApiClient(), ModelStore())
        result = await service.predict_daily()
        job_registry.mark_finished(job_id, result)
    except Exception as exc:  # noqa: BLE001
        logger.exception("Prediction job failed.")
        job_registry.mark_failed(job_id, str(exc))
