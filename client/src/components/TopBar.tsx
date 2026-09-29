interface TopBarProps {
  isSaving: boolean;
}

export function TopBar({ isSaving }: TopBarProps) {
  return (
    <header className="top-bar">
      <div>
        <strong>NetTrello</strong>
        <span>Enterprise-ready kanban starter</span>
      </div>
      <div className="save-state" aria-live="polite">
        {isSaving ? "Saving..." : "All changes saved"}
      </div>
    </header>
  );
}
