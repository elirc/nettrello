import type { DragEvent } from "react";
import type { BoardColumn, BoardMember, Card, CardInput } from "../types";
import { CardComposer } from "./CardComposer";
import { KanbanCard } from "./KanbanCard";

interface KanbanColumnProps {
  column: BoardColumn;
  members: BoardMember[];
  onAddCard: (columnId: string, input: CardInput) => Promise<void>;
  onMoveCard: (cardId: string, targetColumnId: string, position: number) => Promise<void>;
  onOpenCard: (card: Card) => void;
}

export function KanbanColumn({ column, members, onAddCard, onMoveCard, onOpenCard }: KanbanColumnProps) {
  function readDraggedCardId(event: DragEvent): string | null {
    const payload = event.dataTransfer.getData("application/json");
    if (!payload) {
      return null;
    }

    try {
      return (JSON.parse(payload) as { cardId?: string }).cardId ?? null;
    } catch {
      return null;
    }
  }

  function handleDrop(event: DragEvent, position: number) {
    event.preventDefault();
    const cardId = readDraggedCardId(event);
    if (cardId) {
      void onMoveCard(cardId, column.id, position);
    }
  }

  return (
    <section className="kanban-column" onDragOver={(event) => event.preventDefault()} onDrop={(event) => handleDrop(event, column.cards.length)}>
      <header className="column-header">
        <div>
          <h2>{column.title}</h2>
          <span>
            {column.cards.length}
            {column.cardLimit > 0 ? ` / ${column.cardLimit}` : ""} cards
          </span>
        </div>
      </header>

      <div className="card-stack">
        {column.cards.map((card) => (
          <KanbanCard key={card.id} card={card} onOpen={onOpenCard} onDropCard={(draggedCardId, position) => void onMoveCard(draggedCardId, column.id, position)} />
        ))}
      </div>

      <CardComposer members={members} onCreateCard={(input) => onAddCard(column.id, input)} />
    </section>
  );

}
