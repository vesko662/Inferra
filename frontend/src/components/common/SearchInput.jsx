function SearchInput({ label, placeholder, value, onChange }) {
  return (
    <label className="search-field">
      <span className="search-field__label">{label}</span>
      <input
        className="search-field__input"
        type="search"
        placeholder={placeholder}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  );
}

export default SearchInput;
