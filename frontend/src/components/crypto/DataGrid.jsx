import { getPrimitiveEntries } from "../../utils/assetMappers";
import DetailField from "./DetailField";

function DataGrid({ data, emptyMessage }) {
  const entries = getPrimitiveEntries(data);

  if (entries.length === 0) {
    return <p className="section-footnote">{emptyMessage}</p>;
  }

  return (
    <div className="detail-fields">
      {entries.map(([key, value]) => (
        <DetailField key={key} label={key} value={String(value)} />
      ))}
    </div>
  );
}

export default DataGrid;
