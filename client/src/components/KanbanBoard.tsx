import { FormEvent, useState } from "react";
import type { BoardDetail, Card, CardInput, CreateColumnInput } from "../types";
import { CardEditor } from "./CardEditor";
import { KanbanColumn } from "./KanbanColumn";

interface KanbanBoardProps {
  board: BoardDetail;
  onAddColumn: (input: CreateColumnInput) => Promise<void>;
  onAddCard: (columnId: string, input: CardInput) => Promise<void>;
  onUpdateCard: (cardId: string, input: CardInput) => Promise<void>;
  onArchiveCard: (cardId: string) => Promise<void>;
  onMoveCard: (cardId: string, targetColumnId: string, position: number) => Promise<void>;
}

export function KanbanBoard({ board, onAddColumn, onAddCard, onUpdateCard, onArchiveCard, onMoveCard }: KanbanBoardProps) {
  const [newColumnTitle, setNewColumnTitle] = useState("");
  const [selectedCard, setSelectedCard] = useState<Card | null>(null);

  async function handleAddColumn(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!newColumnTitle.trim()) {
      return;
    }

    await onAddColumn({ title: newColumnTitle, cardLimit: 0 });
    setNewColumnTitle("");
  }

  return (
    <main className="board-shell">
      <section className="board-header">
        <div>
          <h1>{board.name}</h1>
          <p>{board.description || "No description yet."}</p>
        </div>
        <dl>
          <div>
            <dt>Version</dt>
            <dd>{board.version}</dd>
          </div>
          <div>
            <dt>Updated</dt>
            <dd>{new Date(board.updatedAt).toLocaleString()}</dd>
          </div>
        </dl>
      </section>

      <section className="kanban-board" aria-label={`${board.name} columns`}>
        {board.columns.map((column) => (
          <KanbanColumn
            key={column.id}
            column={column}
            members={board.members}
            onAddCard={onAddCard}
            onMoveCard={onMoveCard}
            onOpenCard={setSelectedCard}
          />
        ))}

        <form className="add-column" onSubmit={handleAddColumn}>
          <input placeholder="New column" value={newColumnTitle} maxLength={80} onChange={(event) => setNewColumnTitle(event.target.value)} />
          <button type="submit">Add column</button>
        </form>
      </section>

      {selectedCard ? (
        <CardEditor
          card={selectedCard}
          members={board.members}
          onClose={() => setSelectedCard(null)}
          onArchive={onArchiveCard}
          onUpdate={onUpdateCard}
        />
      ) : null}
    </main>
  );
}
