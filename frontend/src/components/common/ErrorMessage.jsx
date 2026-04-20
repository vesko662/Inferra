function ErrorMessage({ title, message, actionLabel, onAction }) {
  return (
    <div className="feedback-state feedback-state--error" role="alert">
      <h2>{title}</h2>
      <p>{message}</p>
      {actionLabel && onAction ? (
        <button className="button button--primary" type="button" onClick={onAction}>
          {actionLabel}
        </button>
      ) : null}
    </div>
  );
}

export default ErrorMessage;
