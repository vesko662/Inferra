function LoadingState({ label = "Loading..." }) {
  return (
    <div className="feedback-state" role="status" aria-live="polite">
      <div className="loading-spinner" aria-hidden="true" />
      <p>{label}</p>
    </div>
  );
}

export default LoadingState;
