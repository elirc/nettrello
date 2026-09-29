import type { DragEvent } from "react";
import type { Card } from "../types";

interface KanbanCardProps {
  card: Card;
  onOpen: (card: Card) => void;
  onDropCard: (draggedCardId: string, targetPosition: number) => void;
}

export function KanbanCard({ card, onOpen, onDropCard }: KanbanCardProps) {
  return (
    <article
      className={`kanban-card priority-${card.priority.toLowerCase()}`}
      draggable
      onDragStart={(event) => {
        event.dataTransfer.effectAllowed = "move";
        event.dataTransfer.setData("application/json", JSON.stringify({ cardId: card.id }));
      }}
      onDragOver={(event) => event.preventDefault()}
      onDrop={(event) => {
        event.preventDefault();
        event.stopPropagation();
        const draggedCardId = readDraggedCardId(event);
        if (draggedCardId) {
          onDropCard(draggedCardId, card.position);
        }
      }}
    >
      <button type="button" onClick={() => onOpen(card)}>
        <span>{card.title}</span>
        {card.description ? <small>{card.description}</small> : null}
      </button>
      <div className="card-meta">
        <span>{card.priority}</span>
        {card.assigneeName ? <span>{card.assigneeName}</span> : null}
      </div>
      {card.labels.length > 0 ? (
        <div className="label-list">
          {card.labels.map((label) => (
            <span key={label}>{label}</span>
          ))}
        </div>
      ) : null}
    </article>
  );
}

function readDraggedCardId(event: DragEvent<HTMLElement>): string | null {
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
