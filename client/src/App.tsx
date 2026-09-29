import { BoardSidebar } from "./components/BoardSidebar";
import { ErrorBoundary } from "./components/ErrorBoundary";
import { KanbanBoard } from "./components/KanbanBoard";
import { TopBar } from "./components/TopBar";
import { useBoard } from "./hooks/useBoard";

export default function App() {
  const { boards, selectedBoardId, board, isLoading, isSaving, error, actions } = useBoard();

  return (
    <ErrorBoundary>
      <div className="app">
        <TopBar isSaving={isSaving} />
        {error ? (
          <div className="error-banner" role="alert">
            <span>{error}</span>
            <button type="button" onClick={actions.dismissError}>
              Dismiss
            </button>
          </div>
        ) : null}

        <div className="layout">
          <BoardSidebar boards={boards} selectedBoardId={selectedBoardId} onSelectBoard={actions.selectBoard} onCreateBoard={actions.createBoard} />

          {isLoading && !board ? (
            <main className="empty-state">
              <h1>Loading board...</h1>
            </main>
          ) : board ? (
            <KanbanBoard
              board={board}
              onAddColumn={actions.addColumn}
              onAddCard={actions.addCard}
              onUpdateCard={actions.updateCard}
              onArchiveCard={actions.archiveCard}
              onMoveCard={actions.moveCard}
            />
          ) : (
            <main className="empty-state">
              <h1>Create your first board</h1>
              <p>Use the sidebar form to start a workspace.</p>
            </main>
          )}
        </div>
      </div>
    </ErrorBoundary>
  );
}
